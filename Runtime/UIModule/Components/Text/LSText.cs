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

        protected override void Awake()
        {
            base.Awake();
            OnPreRenderText += HandleOnPreRenderText;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText -= HandleOnPreRenderText;
            OnPreRenderText += HandleOnPreRenderText;
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
            emojis = Emoji.ParseEmojis(m_text, Path.Combine(Application.persistentDataPath, "Emojis"));
            m_text = Emoji.ReplaceWithEmojiRanges(m_text, emojis, "\ue000\u200b");
        }

        private void HandleOnPreRenderText(TMP_TextInfo textInfo)
        {
            var clear = new Color32(0, 0, 0, 0);
            foreach (var emoji in emojis)
            {
                int charIndexToHide = emoji.index;
                
                if (charIndexToHide < 0 || charIndexToHide >= textInfo.characterCount) return;
                
                var charInfo = textInfo.characterInfo[charIndexToHide];
                
                if (!charInfo.isVisible) return;
                
                int vertexIndex = charInfo.vertexIndex;
                var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
                
                var vertexColors = meshInfo.colors32;
                
                for (int i = 0; i < 4; i++)
                {
                    vertexColors[vertexIndex + i] = clear;
                }

                CreateRawImageForChar(charInfo);
            }
        }
        
        private void CreateRawImageForChar(TMP_CharacterInfo charInfo)
        {
            // Создаем новый объект RawImage внутри текущего GameObject
            GameObject rawImageObj = new GameObject("Emoji");
            rawImageObj.transform.SetParent(transform);

            // Добавляем компонент RawImage
            RawImage rawImage = rawImageObj.AddComponent<EmojiImage>();
            //rawImage.texture = emojiTexture;

            // Получаем размеры символа
            Vector3 bottomLeft = charInfo.bottomLeft;
            Vector3 topRight = charInfo.topRight;

            // Настраиваем RectTransform RawImage
            RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero; // Локальная позиция относительно родителя
            rectTransform.sizeDelta = new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y); // Устанавливаем размер RawImage

            // Смещаем RawImage на позицию символа
            Vector3 charPosition = (bottomLeft + topRight) / 2;
            rectTransform.anchoredPosition = new Vector2(charPosition.x, charPosition.y);

            rectTransform.localScale = Vector3.one; // Убедитесь, что масштаб нормальный
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