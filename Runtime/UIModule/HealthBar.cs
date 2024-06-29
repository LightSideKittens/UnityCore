using DG.Tweening;
using LSCore.Async;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace LSCore
{
    public class HealthBar : LSSlider
    {
        [SerializeField] private Image backFill;
        [SerializeField] private float delay = 0.3f;
        [SerializeField] private float duration = 0.4f;

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(AnimateBackFill);
            backFill.fillAmount = normalizedValue;
        }

        private void AnimateBackFill(float val)
        {
            Wait.Delay(delay, () => backFill.DOFillAmount(normalizedValue, duration));
        }
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(HealthBar), true)]
    [CanEditMultipleObjects]
    public class HealthBarEditor : LSSliderEditor
    {
        SerializedProperty backFill;
        SerializedProperty delay;
        SerializedProperty duration;


        protected override void OnEnable()
        {
            base.OnEnable();
            backFill = serializedObject.FindProperty("backFill");
            delay = serializedObject.FindProperty("delay");
            duration = serializedObject.FindProperty("duration");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(backFill);
            EditorGUILayout.PropertyField(delay);
            EditorGUILayout.PropertyField(duration);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    
#endif
}