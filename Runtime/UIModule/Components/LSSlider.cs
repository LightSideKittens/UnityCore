using System;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
using UnityEngine;
using UnityEngine.UI;
using static LSCore.LSSlider.TextMode;

namespace LSCore
{
    public class LSSlider : Slider
    {
        [Serializable]
        public enum TextMode
        {
            Value,
            Percent,
            NormalizedPercent,
            ValueToMax
        }
        
        [field: SerializeField] public Image Icon { get; private set; }
        [SerializeField] private LSText text; 
        [SerializeField] private TextMode textMode;
        [SerializeField] private bool wholeNumberInText;
        [SerializeField] private bool onlyDiff;
        [SerializeField] private bool clampValue;

        public float OffsetMaxValue => maxValue - minValue;
        public float OffsetValue => value - minValue;
        
        public float DisplayedMaxValue => onlyDiff ? maxValue - minValue : maxValue;
        public float DisplayedValue => onlyDiff ? value - minValue : value;
        
        public new bool wholeNumbers
        {
            get => base.wholeNumbers;
            set
            {
                if (base.wholeNumbers != value)
                {
                    base.wholeNumbers = value;
                    UpdateValueTextGetters();
                }
            }
        }
        
        public bool WholeNumberInText
        {
            get => wholeNumberInText;
            set
            {
                if (wholeNumberInText != value)
                {
                    base.wholeNumbers = !base.wholeNumbers; //HACK: need for calling "private void UpdateVisuals()" in base
                    base.wholeNumbers = !base.wholeNumbers;
                    wholeNumberInText = value;
                    UpdateValueTextGetters();
                }
            }
        }

        private Func<string> textGetter;
        private Func<string> valueTextGetter;
        private Func<string> maxValueTextGetter;
        private StringBuilder stringBuilder = new();
        
        public TextMode TextModee
        {
            get => textMode;
            set
            {
                textMode = value;
                UpdateTextGetter();
            }
        }

        public bool OnlyDiff
        {
            get => onlyDiff;
            set
            {
                if (onlyDiff != value)
                {
                    onlyDiff = value;
                    UpdateText();
                }
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            wholeNumbers = true;
            base.Reset();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            onValueChanged.RemoveAllListeners();
            Init();
        }
#endif
        
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            UpdateValueTextGetters();
            UpdateTextGetter();
            
            if (text != null)
            {
                onValueChanged.AddListener(UpdateText);
                UpdateText();
            }
        }

        private void UpdateText() => UpdateText(0);

        private void UpdateText(float _)
        {
            text.text = textGetter();
        }

        private void UpdateTextGetter()
        {
            textGetter = textMode switch
            {
                Value => valueTextGetter,
                Percent => PercentGetter,
                NormalizedPercent => NormalizedPercentGetter,
                ValueToMax => ValueToMaxGetter,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string PercentGetter() => $"{valueTextGetter()}%";
        private string NormalizedPercentGetter() => $"{(int)(normalizedValue * 100)}%";
        private string ValueToMaxGetter()
        {
            stringBuilder.Clear();
            var max = maxValueTextGetter();
            var val = valueTextGetter();
            stringBuilder.Append("<mspace=0.6em>");
            for (int i = max.Length - val.Length; i > 0; i--)
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(val);
            stringBuilder.Append('/');
            stringBuilder.Append(max);
            
            return stringBuilder.ToString();
        }

        private void UpdateValueTextGetters()
        {
            UpdateValueTextGetter(ValueText, IntValue, out valueTextGetter);
            UpdateValueTextGetter(MaxValue, IntMaxValue, out maxValueTextGetter);
        }
        
        private void UpdateValueTextGetter(Func<string> defaultGetter, Func<string> intGetter, out Func<string> getter)
        {
            getter = wholeNumbers || WholeNumberInText ? intGetter : defaultGetter;
        }

        private string IntValue() => $"{(int)DisplayedValue}";
        private string IntMaxValue() => $"{(int)DisplayedMaxValue}";
        
        private string ValueText() => $"{DisplayedValue}";
        private string MaxValue() => $"{DisplayedMaxValue}";
        
    }
#if UNITY_EDITOR

    [CustomEditor(typeof(LSSlider), true)]
    [CanEditMultipleObjects]
    public class LSSliderEditor : SelectableEditor
    {
        SerializedProperty m_Direction;
        SerializedProperty m_FillRect;
        SerializedProperty m_HandleRect;
        SerializedProperty m_MinValue;
        SerializedProperty m_MaxValue;
        SerializedProperty m_Value;
        SerializedProperty m_OnValueChanged;
        
        SerializedProperty Icon;
        SerializedProperty text;
        SerializedProperty textMode;
        SerializedProperty wholeNumberInText;
        SerializedProperty onlyDiff;
        SerializedProperty clampValue;
        
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_FillRect = serializedObject.FindProperty("m_FillRect");
            m_HandleRect = serializedObject.FindProperty("m_HandleRect");
            m_Direction = serializedObject.FindProperty("m_Direction");
            m_MinValue = serializedObject.FindProperty("m_MinValue");
            m_MaxValue = serializedObject.FindProperty("m_MaxValue");
            m_Value = serializedObject.FindProperty("m_Value");
            m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            
            Icon = serializedObject.FindBackingField("Icon");
            text = serializedObject.FindProperty("text");
            textMode = serializedObject.FindProperty("textMode");
            wholeNumberInText = serializedObject.FindProperty("wholeNumberInText");
            onlyDiff = serializedObject.FindProperty("onlyDiff");
            clampValue = serializedObject.FindProperty("clampValue");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawBaseSlider();
            
            EditorGUILayout.PropertyField(Icon);
            EditorGUILayout.PropertyField(text);
            EditorGUILayout.PropertyField(textMode);
            EditorGUILayout.PropertyField(wholeNumberInText);
            EditorGUILayout.PropertyField(onlyDiff);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBaseSlider()
        {
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_FillRect);
            EditorGUILayout.PropertyField(m_HandleRect);

            if (m_FillRect.objectReferenceValue != null || m_HandleRect.objectReferenceValue != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_Direction);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(serializedObject.targetObjects, "Change Slider Direction");

                    Slider.Direction direction = (Slider.Direction)m_Direction.enumValueIndex;
                    foreach (var obj in serializedObject.targetObjects)
                    {
                        Slider slider = obj as Slider;
                        slider.SetDirection(direction, true);
                    }
                }

                EditorGUI.BeginChangeCheck();
                float newMin = EditorGUILayout.FloatField("Min Value", m_MinValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (clampValue.boolValue && newMin < m_MaxValue.floatValue)
                    {
                        m_MinValue.floatValue = newMin;
                        if (m_Value.floatValue < newMin)
                            m_Value.floatValue = newMin;
                    }
                }

                EditorGUI.BeginChangeCheck();
                float newMax = EditorGUILayout.FloatField("Max Value", m_MaxValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (clampValue.boolValue && newMax > m_MinValue.floatValue)
                    {
                        m_MaxValue.floatValue = newMax;
                        if (m_Value.floatValue > newMax)
                            m_Value.floatValue = newMax;
                    }
                }

                bool areMinMaxEqual = (m_MinValue.floatValue == m_MaxValue.floatValue);

                if (areMinMaxEqual)
                    EditorGUILayout.HelpBox("Min Value and Max Value cannot be equal.", MessageType.Warning);

                EditorGUI.BeginDisabledGroup(areMinMaxEqual);
                EditorGUI.BeginChangeCheck();
                
                if (clampValue.boolValue)
                {
                    EditorGUILayout.Slider(m_Value, m_MinValue.floatValue, m_MaxValue.floatValue);
                }
                else
                {
                    EditorGUILayout.PropertyField(m_Value);
                }

                EditorGUILayout.PropertyField(clampValue);
                
                if (EditorGUI.EndChangeCheck())
                {
                    // Apply the change before sending the event
                    serializedObject.ApplyModifiedProperties();

                    foreach (var t in targets)
                    {
                        if (t is Slider slider)
                        {
                            slider.onValueChanged?.Invoke(slider.value);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                bool warning = false;
                foreach (var obj in serializedObject.targetObjects)
                {
                    Slider slider = obj as Slider;
                    Slider.Direction dir = slider.direction;
                    if (dir == Slider.Direction.LeftToRight || dir == Slider.Direction.RightToLeft)
                        warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnLeft() != null || slider.FindSelectableOnRight() != null));
                    else
                        warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnDown() != null || slider.FindSelectableOnUp() != null));
                }

                if (warning)
                    EditorGUILayout.HelpBox("The selected slider direction conflicts with navigation. Not all navigation options may work.", MessageType.Warning);

                // Draw the event notification options
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_OnValueChanged);
            }
            else
            {
                EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    
#endif
}