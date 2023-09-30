using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using LSCore.Extensions;
using GameCore.Attributes;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Attributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReferenceFromAttribute))]
    public class ReferenceFromPropertyDrawer : PropertyDrawer
    {
        private int selectedName;
        private List<Transform> list = new List<Transform>();
        private Rect buttonRect;
        private Texture2D tex;
        private Object currentObject;
        private string currentObjectName;
        private Texture2D componentIconTex;
        private Texture2D dropdownIconTex;
        private ReferenceFromAttribute Attribute;
        private Object prefabObject;
        private Type propertyType;
        private GUIStyle backStyle;
        private bool isInitialized;
        private BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private GUIStyle objectButtonStyle;
        private Color color;
        private Component prefabComponent;
        private GameObject prefabGameObject;
        private Transform transform;
        private SerializedProperty prefabProperty;
        private Rect dropdownRect;
        private Rect componentIconRect;
        private GUIStyle dropdownButtonStyle;
        private string labelText;

        public Object CurrentObject
        {
            get => currentObject;
            set
            {
                currentObject = value;
                currentObjectName = currentObject != null ? currentObject.name : "null";
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -EditorGUIUtility.standardVerticalSpacing + 8;
        }

        private void OnInitialize(SerializedProperty property, GUIContent label)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                Attribute = (ReferenceFromAttribute) attribute;
                tex = EditorUtils.GetTextureByColor(Attribute.ColorData.color);
                backStyle = new GUIStyle {normal = {background = tex}};
                
                objectButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    richText = true,
                    normal = {textColor = Color.white.CloneAndChangeBrightness(0.9f)},
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true
                };

                dropdownButtonStyle = new GUIStyle(objectButtonStyle);
                dropdownButtonStyle.alignment = TextAnchor.MiddleCenter;
                dropdownButtonStyle.padding.left = 40;
                dropdownButtonStyle.padding.right = 20;

                CurrentObject = property.objectReferenceValue; 
                var prefabName = Attribute.PrefabName;
                var prefabPath = property.propertyPath;
                var propertyTypePath = Regex.Replace(property.propertyPath, @"\.Array\.data\[\d+\]$", string.Empty);

                if (propertyTypePath != property.propertyPath)
                {
                    prefabPath = propertyTypePath;
                }
                
                prefabPath = Regex.Replace(prefabPath, @"\w+$", prefabName);
                prefabProperty = property.serializedObject.FindProperty(prefabPath);
                
                var parentType = property.serializedObject.targetObject.GetType();
                propertyTypePath = Regex.Replace(propertyTypePath, @"\.Array\.data\[\d+\]", string.Empty);
                propertyType = parentType.GetFieldViaPath(propertyTypePath, flags).FieldType;

                if (propertyType.IsArray)
                {
                    propertyType = propertyType.GetElementType();
                }
                else if(typeof(IList).IsAssignableFrom(propertyType))
                {
                    var isList = false;

                    while (isList == false)
                    {
                        while (propertyType.IsGenericType == false)
                        {
                            propertyType = propertyType.BaseType;
                        }
                            
                        isList = propertyType.GetGenericTypeDefinition() == typeof(List<>);
                    }

                    propertyType = propertyType.GenericTypeArguments[0];
                }
                
                labelText = $"{property.name.SplitPascalCase()} <b>({prefabName.SplitPascalCase()})</b>";

                var method = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic);

                componentIconTex = (Texture2D)method.Invoke(null, new object[]{$"{propertyType.Name} Icon"});

                if (componentIconTex == null)
                {
                    componentIconTex = (Texture2D) method.Invoke(null, new object[] {"cs Script Icon"});
                }
                
                dropdownIconTex = (Texture2D) method.Invoke(null, new object[] {"d_icon dropdown@2x"});
                
                color = Color.gray / 1.8f;
                color.a = 1;
            }
        }

        private void UpdatePrefabObject()
        {
            if (prefabProperty.objectReferenceValue != prefabObject)
            {
                prefabObject = prefabProperty.objectReferenceValue;
                prefabComponent = prefabObject as Component;
                prefabGameObject = prefabObject as GameObject;
                transform = prefabGameObject != null ? prefabGameObject.transform : prefabComponent.transform;

                var isMatch = false;

                if (currentObject != null)
                {
                    var type = propertyType;

                    if (type == typeof(GameObject))
                    {
                        type = typeof(Transform);
                    }
                    
                    var components = transform.GetComponentsInChildren(type, true);
                    Transform currentTransform = null;
                    
                    if (currentObject is GameObject currentGameObject)
                    {
                        currentTransform = currentGameObject.transform;
                    }
                    else if (currentObject is Component currentComponent)
                    {
                        currentTransform = currentComponent.transform;
                    }
                    
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i].transform.GetInstanceID() == currentTransform.GetInstanceID())
                        {
                            isMatch = true;
                            break;
                        }
                    }
                    
                    if (!isMatch)
                    {
                        CurrentObject = null;
                    }
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnInitialize(property, label);
            UpdatePrefabObject();
            
            if (prefabObject == null)
            {
                DrawHelpBox();
                return;
            }

            var lastIsRich = EditorStyles.label.richText = true;
            var lastColor = EditorStyles.helpBox.normal.textColor = Color.white.CloneAndChangeBrightness(0.9f);

            EditorGUILayout.BeginVertical(backStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(labelText, objectButtonStyle, GUILayout.MinWidth(110)))
                    {
                        EditorGUIUtility.PingObject(currentObject);
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        buttonRect = GUILayoutUtility.GetLastRect();
                    }

                    if (GUILayout.Button(currentObjectName, dropdownButtonStyle, GUILayout.MinWidth(110)))
                    {
                        var popupRect = buttonRect;
                        popupRect.x += buttonRect.width;
                        PopupWindow.Show(popupRect, new ReferenceFromPopup(this, transform, propertyType));
                    }

                    dropdownRect = new Rect();
                    componentIconRect = new Rect();

                    if (Event.current.type == EventType.Repaint)
                    {
                        componentIconRect = GUILayoutUtility.GetLastRect();
                        dropdownRect = componentIconRect;

                        dropdownRect.width = dropdownIconTex.width - 2;
                        dropdownRect.height = dropdownIconTex.height - 2;
                        dropdownRect.x += componentIconRect.width - dropdownRect.width;
                        componentIconRect.width = componentIconRect.height;
                        componentIconRect.x += 5;
                        dropdownRect.y -= 12;
                        dropdownRect.y += componentIconRect.height / 2;
                    }

                    GUI.Box(componentIconRect, componentIconTex, new GUIStyle());
                    GUI.Box(dropdownRect, dropdownIconTex, new GUIStyle());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

            var boxRect = GUILayoutUtility.GetLastRect();
            SirenixEditorGUI.DrawBorders(boxRect, 1, 1, 1, 4, color / 2);
            SirenixEditorGUI.DrawBorders(boxRect, 1);
            
            EditorGUILayout.Space(8);
            
            EditorStyles.label.richText = lastIsRich;
            EditorStyles.label.normal.textColor = lastColor;
            
            property.objectReferenceValue = CurrentObject;
        }

        private void DrawHelpBox()
        {
            var lastIsRichBack = EditorStyles.helpBox.normal.background;
            var lastIsRich = EditorStyles.helpBox.richText;
            var lastColor = EditorStyles.helpBox.normal.textColor;
            EditorStyles.helpBox.normal.background = EditorUtils.GetTextureByColor(color);
            EditorStyles.helpBox.richText = true;
            EditorStyles.helpBox.normal.textColor = Color.white.CloneAndChangeBrightness(0.9f);

            EditorGUILayout.HelpBox($"<b>{Attribute.PrefabName.SplitPascalCase()}</b> is null.", MessageType.Error);
                
            EditorStyles.helpBox.richText = lastIsRich;
            EditorStyles.helpBox.normal.textColor = lastColor;
            EditorStyles.helpBox.normal.background = lastIsRichBack;

            var lastRect = GUILayoutUtility.GetLastRect();
            SirenixEditorGUI.DrawBorders(lastRect, 1, 1, 1, 4, color / 2);
            SirenixEditorGUI.DrawBorders(lastRect, 1);
        }
    }
}