using System;
using System.IO;
using LSCore.NativeUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using TMPro.EditorUtilities;
using UnityEditor;
#endif

namespace LSCore
{
    public class LSText : TextMeshProUGUI
    {
        private EmojiRange[] emojis;
        private Texture2D[] textures;
        private Action releaseEmojiImages;
        
        public override float preferredWidth
        {
            get
            {
                if (emojis is { Length: > 0 })
                {
                    var lastText = m_text;
                    m_text = Emoji.ReplaceWithEmojiRanges(m_text, emojis, "\ue000\u200b");
                    var width = base.preferredWidth;
                    m_text = lastText;
                    return width;
                }
                
                return base.preferredWidth;
            }
        }

        public override float preferredHeight
        {
            get
            {
                if (emojis is { Length: > 0 })
                {
                    var lastText = m_text;
                    m_text = Emoji.ReplaceWithEmojiRanges(m_text, emojis, "\ue000\u200b");
                    var height = base.preferredHeight;
                    m_text = lastText;
                    return height;
                }

                return base.preferredHeight;
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            Init();
            OnPreRenderText += HandleOnPreRenderText;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ReleaseAllEmojiImages();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            releaseEmojiImages = null;
        }

        private void Init()
        {
#if UNITY_EDITOR
            EditorApplication.update -= Init;
#endif
            EmojiImage.InitPool();
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText += HandleOnPreRenderText;
            
            if (World.IsEditMode)
            {
                EditorApplication.update += Init;
            }
        }
#endif

        public override void Rebuild(CanvasUpdate update)
        {
            var t = m_text;
            
            if (update == CanvasUpdate.PreRender)
            {
                ModifyText();
            }
            
            base.Rebuild(update);
            
            m_text = t;
        }

        public override void ForceMeshUpdate(bool ignoreActiveState = false, bool forceTextReparsing = false)
        {
            var t = m_text;
            ModifyText();
            base.ForceMeshUpdate(ignoreActiveState, forceTextReparsing);
            m_text = t;
        }

        public override void ClearMesh()
        {
            base.ClearMesh();
            ReleaseAllEmojiImages();
        }

        private void ModifyText()
        {
            emojis = Emoji.ParseEmojis(m_text, Path.Combine(Application.persistentDataPath, "Emojis"), out textures);
            m_text = Emoji.ReplaceWithEmojiRanges(m_text, emojis, "\ue000\u200b");
        }

        private void HandleOnPreRenderText(TMP_TextInfo textInfo)
        {
            ReleaseAllEmojiImages();
            var clear = new Color32(0, 0, 0, 0);

            for (int i = 0; i < emojis.Length; i++)
            {
                var emoji = emojis[i];
                int charIndexToHide = emoji.adjustedIndex;
                
                if (charIndexToHide < 0 || charIndexToHide >= textInfo.characterCount) return;
                
                var charInfo = textInfo.characterInfo[charIndexToHide];
                
                if (!charInfo.isVisible) return;
                
                int vertexIndex = charInfo.vertexIndex;
                var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
                
                var vertexColors = meshInfo.colors32;
                
                for (int x = 0; x < 4; x++)
                {
                    vertexColors[vertexIndex + x] = clear;
                }

                CreateRawImageForChar(charInfo, i);
            }
        }
        
        private void CreateRawImageForChar(TMP_CharacterInfo charInfo, int i)
        {
            var rawImage = EmojiImage.Get();
            rawImage.transform.SetParent(transform);
            releaseEmojiImages += () => EmojiImage.Release(rawImage);
            
            rawImage.texture = textures[i];
            Vector3 bottomLeft = charInfo.bottomLeft;
            Vector3 topRight = charInfo.topRight;
            
            RectTransform r = rawImage.rectTransform;
            r.localPosition = Vector3.zero;
            r.sizeDelta = new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
            
            Vector3 charPosition = (bottomLeft + topRight) / 2;
            r.anchoredPosition = new Vector2(charPosition.x, charPosition.y);
            
            r.localScale = Vector3.one;
        }

        private void ReleaseAllEmojiImages()
        {
            releaseEmojiImages?.Invoke();
            releaseEmojiImages = null;
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        protected SerializedProperty padding;
        protected PropertyTree propertyTree;
        protected LSText text;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            text = (LSText)target;
            padding = serializedObject.FindProperty("m_RaycastPadding");
            propertyTree = PropertyTree.Create(serializedObject);
            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SceneView.duringSceneGui -= DrawAnchorsOnSceneView;
        }

        private void OnDestroy()
        {
            propertyTree.Dispose();
        }

        private void DrawAnchorsOnSceneView(SceneView sceneView) => LSRaycastTargetEditor.DrawAnchorsOnSceneView(this, sceneView);

        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();
            
            if (Foldout.extraSettings)
            {
                EditorGUILayout.PropertyField(padding);
            }
        }
    }
#endif
}