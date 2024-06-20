using System;
using System.Collections.Generic;
using System.Text;
using LightSideCore.Runtime.UIModule;
using LSCore.Extensions.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using TMPro.EditorUtilities;
using UnityEditor;
#endif

namespace LSCore
{
    public class LSText : TextMeshProUGUI, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
    {
        [SerializeField] private ClickAnim anim;
        public ref ClickAnim Anim => ref anim;

        protected override void Awake()
        {
            base.Awake();
            anim.Init(transform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

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
        
        public void OnPointerClick(PointerEventData eventData)
        {
            anim.OnPointerClick();
            Clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) => anim.OnPointerDown();
        public void OnPointerUp(PointerEventData eventData) => anim.OnPointerUp();

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }
        
        public Transform Transform => transform;
        public Action Clicked { get; set; }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        SerializedProperty padding;
        LSText text;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            text = (LSText)target;
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
            text.Anim.Editor_Draw();
            if (Foldout.extraSettings)
            {
                EditorGUILayout.PropertyField(padding);
            }
        }
        
        [MenuItem("GameObject/LSCore/Text")]
        private static void CreateButton()
        {
            new GameObject("LSText").AddComponent<LSText>();
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