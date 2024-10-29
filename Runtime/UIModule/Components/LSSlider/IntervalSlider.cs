using System;
using System.Collections.Generic;
using LSCore;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public class IntervalSlider : LSSlider
    {
        [Serializable]
        public struct IntervalData
        {
            public RectTransform transform;
            public LSText text;
            public float value;
            public float proportion;
        }
        
        
        [SerializeField] private float total;
        [SerializeField] private TextMode intervalTextMode;
        [SerializeField] private List<IntervalData> intervals;
        [SerializeField] private AnimationCurve curve = new();
        [SerializeField] private AnimationCurve inverseCurve = new();
        private Func<float, string> textGetter;
        
        public override float normalizedValue
        {
            get => inverseCurve.Evaluate(value);
            set => this.value = curve.Evaluate(value);
        }

        protected override void Init()
        {
            base.Init();
            textGetter = GetTextGetter(intervalTextMode);
            UpdateRectPositions();
            UpdateText();
        }

        private void UpdateRectPositions()
        {
            for (int i = 0; i < intervals.Count; i++)
            {
                var interval = intervals[i];
                if (interval.transform != null)
                {
                    var t = interval.transform;
                    var val = Vector2.one * inverseCurve.Evaluate(interval.value);
                    t.anchorMin = val;
                    t.anchorMax = val;
                    t.anchoredPosition = Vector2.zero;
                }
            }
        }

        private void UpdateText()
        {
            for (int i = 0; i < intervals.Count; i++)
            {
                var interval = intervals[i];
                if (interval.text != null)
                {
                    interval.text.text = textGetter(interval.value);
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            total = 0;

            for (int i = intervals.Count; i < 2; i++)
            {
                intervals.Add(new IntervalData()
                {
                    proportion = 1,
                    value = i
                });
            }

            float min = intervals[0].value;
            float max = intervals[^1].value;
            if (min > max)
            {
                max = min;
            }

            for (int i = 0; i < intervals.Count; i++)
            {
                var data = intervals[i];
                data.value = Mathf.Clamp(data.value, min, max);
                min = data.value;
                total += data.proportion;
                intervals[i] = data;
            }

            float time = 0;
            curve.ClearKeys();
            inverseCurve.ClearKeys();
            
            curve.AddKey(GetKey(0, intervals[0].value));
            inverseCurve.AddKey(GetKey(intervals[0].value, 0));
            
            for (int i = 1; i < intervals.Count; i++)
            {
                var data = intervals[i];
                time += data.proportion / total;
                curve.AddKey(GetKey(time, data.value));
                inverseCurve.AddKey(GetKey(data.value, time));
            }
        }

        private Keyframe GetKey(float time, float value)
        {
            var kf = new Keyframe(time, value, 0, 0, 0, 0);
            return kf;
        }
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(IntervalSlider), true)]
[CanEditMultipleObjects]
public class IntervalSliderEditor : LSSliderEditor
{
    SerializedProperty intervalTextMode;
    SerializedProperty intervals;

    protected override void OnEnable()
    {
        base.OnEnable();

        intervalTextMode = serializedObject.FindProperty("intervalTextMode");
        intervals = serializedObject.FindProperty("intervals");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(intervalTextMode);
        EditorGUILayout.PropertyField(intervals);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif