using System.Collections.Generic;
using System.Text;
using LSCore.Extensions.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using TMPro.EditorUtilities;
using UnityEditor;
#endif

namespace LSCore
{
    public class LSText : TextMeshProUGUI
    {
        void AddRawImageForMissingCharacter(char character, int index)
        {
            EmojiRenderer.RenderEmoji(character.ToString(), fontSize, color, out var texture);
            
            foreach (Transform obj in transform)
            {
                if (obj.TryGetComponent<RawImage>(out _))
                {
                    DestroyImmediate(obj.gameObject);
                }
            }
            
            TMP_CharacterInfo charInfo = textInfo.characterInfo[index];

            // Получение размеров отсутствующего символа
            float width = charInfo.topRight.x - charInfo.bottomLeft.x;
            float height = charInfo.topRight.y - charInfo.bottomLeft.y;

            // Создание нового GameObject
            GameObject rawImageGameObject = new GameObject("MissingCharacter_" + character);
            rawImageGameObject.transform.SetParent(transform);

            // Добавление компонента RawImage
            RawImage rawImage = rawImageGameObject.AddComponent<RawImage>();
            rawImage.texture = texture;

            // Установка размеров и позиции RawImage согласно размерам отсутствующего символа
            RectTransform rectTransform = rawImageGameObject.GetComponent<RectTransform>();
            rectTransform.pivot = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.anchoredPosition = charInfo.bottomLeft;

            // Дополнительные настройки
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        SerializedProperty padding;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            padding = serializedObject.FindProperty("m_RaycastPadding");
            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SceneView.duringSceneGui -= DrawAnchorsOnSceneView;
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
    
    public class EmojiTextPreprocessor : ITextPreprocessor
    {
        private LSText text;

        public EmojiTextPreprocessor(LSText text)
        {
            this.text = text;
        }
        
        public string PreprocessText(string text)
        {
            var fontAsset = this.text.font;
            StringBuilder builder = new StringBuilder();
            StringBuilder missing = new StringBuilder();
            List<string> emojis = new List<string>();
            var wasMissing = false;
            var lastWidth = float.PositiveInfinity;
            Vector2 spaceSize = this.text.GetPreferredValues(" ");
            
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (fontAsset.HasCharacter(c))
                {
                    if (wasMissing)
                    {
                        missing.Clear();
                        wasMissing = false;
                        lastWidth = float.PositiveInfinity;
                        emojis.Clear();
                        
                        EmojiRenderer.RenderEmoji(missing.ToString(), this.text.fontSize, this.text.color, out var texture);
                        var size = texture.Size();
                        var factor = spaceSize.y / size.y;
                        size *= factor;

                        for (float j = 0; j < size.x; j += spaceSize.x)
                        {
                            builder.Append(' ');
                        }
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
                else
                {
                    wasMissing = true;
                    var width = EmojiRenderer.GetWidth(c.ToString(), this.text.fontSize);
                    
                    if (width > lastWidth)
                    {
                        emojis.Add(missing.ToString());
                        missing.Clear();
                    }
                    
                    lastWidth = width;
                    missing.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}