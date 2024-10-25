#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace LSCore
{
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

                    LSSlider.Direction direction = (LSSlider.Direction)m_Direction.enumValueIndex;
                    foreach (var obj in serializedObject.targetObjects)
                    {
                        LSSlider slider = obj as LSSlider;
                        slider.SetDirection(direction, true);
                    }
                }

                EditorGUI.BeginChangeCheck();
                float newMin = EditorGUILayout.FloatField("Min Value", m_MinValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newMin < m_MaxValue.floatValue)
                    {
                        m_MinValue.floatValue = newMin;
                    }
                }

                EditorGUI.BeginChangeCheck();
                float newMax = EditorGUILayout.FloatField("Max Value", m_MaxValue.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newMax > m_MinValue.floatValue)
                    {
                        m_MaxValue.floatValue = newMax;
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
                    LSSlider slider = obj as LSSlider;
                    LSSlider.Direction dir = slider.direction;
                    if (dir == LSSlider.Direction.LeftToRight || dir == LSSlider.Direction.RightToLeft)
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
}
#endif