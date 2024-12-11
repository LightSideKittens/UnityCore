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
        public static OnOffPool<EmojiImage> EmojiImagePool => OnOffPool<EmojiImage>.GetOrCreatePool(emojiImagePrefab);
        private static EmojiImage emojiImagePrefab;
        private static Transform currentParent;
        private EmojiRange[] emojis;
        private Texture2D[] textures;
        private Action releaseEmojiImages;

#if UNITY_EDITOR
        static LSText()
        {
            World.Creating += () =>
            {
                EmojiImagePool.Created -= OnCreate;
                emojiImagePrefab = null;
            };
            
            World.Destroyed += () =>
            {
                EmojiImagePool.Created -= OnCreate;
                emojiImagePrefab = null;
            };
        }
#endif
        
        private static void OnCreate(EmojiImage rawImage)
        {
            rawImage.gameObject.hideFlags = HideFlags.HideAndDontSave;
            rawImage.transform.SetParent(currentParent);
        }
        
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ReleaseAllEmojiImages();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ReleaseAllEmojiImages();
        }

        private void Init()
        {
            if (emojiImagePrefab is null)
            {
                emojiImagePrefab = new GameObject("EmojiImage").AddComponent<EmojiImage>();
                emojiImagePrefab.gameObject.hideFlags = HideFlags.HideAndDontSave;
                EmojiImagePool.Created += OnCreate;
                if (World.IsPlaying)
                {
                    DontDestroyOnLoad(emojiImagePrefab.gameObject);
                }
            }
            
            OnPreRenderText += HandleOnPreRenderText;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText -= HandleOnPreRenderText;
            if (World.IsEditMode)
            {
                Init();
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
                int charIndexToHide = emoji.index;
                
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
            currentParent = transform;
            var rawImage = EmojiImagePool.Get();
            releaseEmojiImages += () => EmojiImagePool.Release(rawImage);

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

        protected void DrawTextPropertiesAsFoldout()
        {
            EditorUtils.DrawInBoxFoldout("Text Properties", base.OnInspectorGUI, text, false);
        }
        
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