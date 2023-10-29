using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using LSCore.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Video;
using Random = UnityEngine.Random;

namespace Attributes.Editor.Drawers
{
    public class ReferenceFromPopup : PopupWindowContent
    {
        private readonly Texture2D[] textures;
        private readonly Texture2D backTexture;
        private readonly GUIStyle boxStyle;
        private readonly Transform transform;
        private readonly ReferenceFromPropertyDrawer property;
        private readonly Type type;
        private Texture2D heartTex;
        private Texture2D cookieTex;
        private VideoClip videoClip;
        private Transform previousTransform;

        private readonly Dictionary<Transform, int> intentLevels = new Dictionary<Transform, int>();
        private readonly Dictionary<int, Rect> levelsMap = new Dictionary<int, Rect>();
        private readonly HashSet<Transform> needSpaceSet = new HashSet<Transform>();
        private readonly List<KeyValuePair<int, Rect>> rectsToDraw = new List<KeyValuePair<int, Rect>>();
        private readonly List<int> toRemove = new List<int>();
        private readonly List<KeyValuePair<int, Rect>> toModify = new List<KeyValuePair<int, Rect>>();

        private readonly int typeCount;
        private readonly Vector2 size = new Vector2(200, 400);
        private readonly Vector2 sizeOffset = new Vector2(20, 10);
        private Vector2 scrollPos;
        private Rect popupRect;
        private Rect boxRect;
        private Rect lastBoxRect;
        private Rect lastButtonRect;
        private float intentWidth;
        private bool isRepaint;
        private bool isInited;

        private bool IsTransformOrGameObject => type == typeof(Transform) || type == typeof(GameObject);
        
        public ReferenceFromPopup(ReferenceFromPropertyDrawer property, Transform transform, Type type)
        {
            this.transform = transform;
            this.property = property;
            this.type = type;

            if (IsTransformOrGameObject)
            {
                typeCount = transform.childCount;
            }
            else
            {
                typeCount = transform.GetComponentsInChildren(type, true).Length;

                if (transform.TryGetComponent(type, out _))
                {
                    typeCount--;
                }
            }

            popupRect.size = size;
            textures = new Texture2D[EditorUtils.Colors.Length - 1];
            var colors = new EditorUtils.ColorData[textures.Length];

            for (int i = 1; i < EditorUtils.Colors.Length; i++)
            {
                colors[i - 1] = EditorUtils.Colors[i];
            }

            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = EditorUtils.GetTextureByColor(colors[i % colors.Length].color);
            }
            
            backTexture = EditorUtils.GetTextureByColor(EditorUtils.GetColorData(0).color);
            boxRect.width = size.x;
            boxStyle = new GUIStyle();
            heartTex = Resources.Load<Texture2D>($"ColoredField/heart");
            cookieTex = Resources.Load<Texture2D>($"cookie");
            cookieTex = cookieTex.ResizeByBlit(heartTex.height, heartTex.height);
        }

        public override Vector2 GetWindowSize()
        {
            return popupRect.size;
        }

        public override void OnGUI(Rect rect)
        {
            lastBoxRect = rect;
            
            if (typeCount == 0)
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    var labelBoxStyle = new GUIStyle();
                    labelBoxStyle.richText = true;
                    labelBoxStyle.normal.textColor = Color.white;
                    labelBoxStyle.wordWrap = true;
                
                    GUILayout.Space(10);
                    
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Box(
                            $"<b>{transform.name}</b> does not contain children with type: <b>{type.Name}</b>",
                            labelBoxStyle);
                        GUILayout.Space(10);
                        GUILayout.Box($"Don't worry eat a cookie", labelBoxStyle);
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Box(heartTex, labelBoxStyle);
                            GUILayout.Box(cookieTex, labelBoxStyle);
                            GUILayout.Box(heartTex, labelBoxStyle);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    
                    popupRect.height = 140;
                }

                GUILayout.EndHorizontal();

                return;
            }

            previousTransform = null;
            boxStyle.normal.background = backTexture;
            
            var backRect = popupRect; backRect.x += 10; backRect.y += 10;
            GUI.Box(backRect, "", boxStyle);
            SirenixEditorGUI.DrawBorders(backRect, 1, 1, 1, 4);

            EditorGUILayout.Space(sizeOffset.y);
            
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(sizeOffset.x);
                EditorGUILayout.BeginVertical();
                
                var isScroll = popupRect.height > size.y;
                isRepaint = Event.current.type == EventType.Repaint;

                if (isScroll)
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                }
                
                rectsToDraw.Sort((a, b) => a.Value.yMin > b.Value.yMin ? 1 : -1);

                for (int i = 0; i < rectsToDraw.Count; i++)
                {
                    var texture = textures[i % textures.Length];

                    boxStyle.normal.background = texture;
                    var rectToDraw = rectsToDraw[i].Value; 
        
                    GUI.Box(rectToDraw, "", boxStyle);

                    SirenixEditorGUI.DrawBorders(rectToDraw, 1, 1, 1, 4);
                }
                
                if (isRepaint)
                {
                    levelsMap.Clear();
                    rectsToDraw.Clear();
                }

                intentLevels.Clear();
                
                transform.GetAllChild(OnChild, OnParent);
                UpdateRectsToDrawBefore(0);

                if (isScroll)
                {
                    EditorGUILayout.EndScrollView();
                
                    if (isRepaint)
                    {
                        boxRect.width += 20;
                    }
                }
            
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            if (!isInited)
            {
                boxRect.width += intentWidth;

                if (boxRect.width > size.x)
                {
                    isInited = true;
                    popupRect = boxRect;
                    popupRect.width = boxRect.width + sizeOffset.x;

                    UpdatePopupHeight();
                }
            }
        }

        private void UpdatePopupHeight()
        {
            if (popupRect.height > size.y)
            {
                popupRect.height = size.y;
                popupRect.height += 10;
            }
            else
            {
                popupRect.height = lastButtonRect.yMax + 10;
            }
        }

        private void OnParent(Transform parent)
        {
            if (IsTransformOrGameObject)
            {
                DrawOnParent(parent);
                return;
            }
            
            var component = parent.GetComponent(type);

            if (component != null)
            {
                var componentType = component.GetType();
                
                if (type == componentType || type.IsAssignableFrom(componentType))
                {
                    DrawOnParent(parent);
                }
            }
        }

        private void DrawOnParent(Transform parent)
        {
            EditorGUILayout.BeginHorizontal();

            if (intentLevels.TryGetValue(parent, out _) == false)
            {
                var myParent = parent.parent;

                if (myParent == null)
                {   
                    intentLevels.Add(parent, 1);
                }
                else if (intentLevels.TryGetValue(parent.parent, out var level))
                {
                    intentLevels.Add(parent, level + 1);
                }
                else
                {
                    intentLevels.Add(parent, 1);
                }
            }

            DrawButton(parent);
            EditorGUILayout.EndHorizontal();
        }
        
        private void OnChild(Transform child)
        {
            if (IsTransformOrGameObject)
            {
                DrawOnChild(child);
                return;
            }
            
            var component = child.GetComponent(type);

            if (component != null)
            {
                var componentType = component.GetType();

                if (type == componentType || type.IsAssignableFrom(componentType))
                {
                    DrawOnChild(child);
                }
            }
        }

        private void DrawOnChild(Transform child)
        {
            EditorGUILayout.BeginHorizontal();

            intentLevels.TryGetValue(child.parent, out var level);

            GUILayout.Space(level * 10);

            if (isRepaint)
            {
                lastBoxRect = GUILayoutUtility.GetLastRect();
                var width = lastBoxRect.width + 20;

                if (width > intentWidth)
                {
                    intentWidth = width;
                }
            }

            if (isRepaint)
            {
                UpdateRectsToDrawBefore(level);
            }
            
            DrawButton(child);
            
            if (isRepaint)
            {
                UpdateRectsToDrawAfter(child);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawButton(Transform child)
        {
            if (child != previousTransform)
            {
                var buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.wordWrap = true;

                if (child.childCount > 0)
                {
                    buttonStyle.margin = new RectOffset(0, 0, 10, 0);
                }
                else if (needSpaceSet.Contains(child))
                {
                    buttonStyle.margin = new RectOffset(0, 0, 22, 0);
                }

                if (GUILayout.Button(child.name, buttonStyle, GUILayout.Width(200)))
                {
                    property.CurrentObject = child.gameObject;
                    editorWindow.Close();
                    StickersEditorVideoPlayer.Play(Random.Range(0, StickersEditorVideoPlayer.ClipsCount));
                }

                if (isRepaint)
                {
                    var yMax = lastButtonRect.yMax;
                    lastButtonRect = GUILayoutUtility.GetLastRect();
                    boxRect.height += lastButtonRect.height + 4.5f;

                    if (yMax < lastButtonRect.yMax)
                    {
                        UpdatePopupHeight();
                    }
                }
            }

            previousTransform = child;
        }

        private void UpdateRectsToDrawBefore(int level)
        {
            toRemove.Clear();
            toModify.Clear();

            if (levelsMap.ContainsKey(level) == false)
            {
                var rect = lastBoxRect;
                var x = lastBoxRect.width - 10;
                rect.x += x;
                rect.yMin = lastButtonRect.yMin;
                rect.width = popupRect.width - 20 - sizeOffset.x - x;

                levelsMap[level] = rect;
            }

            foreach (var rect in levelsMap)
            {
                if (level < rect.Key)
                {
                    rectsToDraw.Add(rect);
                    toRemove.Add(rect.Key);
                }
                else
                {
                    toModify.Add(rect);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                levelsMap.Remove(toRemove[i]);
            }
        }

        private void UpdateRectsToDrawAfter(Transform child)
        {
            for (int i = 0; i < toModify.Count; i++)
            {
                var data = toModify[i];
                var rect = data.Value;

                rect.height = lastButtonRect.yMin - rect.yMin + lastButtonRect.height + 8;
                levelsMap[data.Key] = rect;
            }

            if (toRemove.Count > 0)
            {
                needSpaceSet.Add(child);
            }

            toRemove.Clear();
            toModify.Clear();
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
        }
    }
}