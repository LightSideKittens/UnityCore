using System;
using System.Collections.Generic;
using LSCore;
using LSCore.Extensions;
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

        public struct Data
        {
            public int index;
            public IntervalData from;
            public IntervalData to;

            public Data(int index, IntervalData from, IntervalData to)
            {
                this.index = index;
                this.from = from;
                this.to = to;
            }
        }
        
        public Data GetIntervalData(float value)
        {
            intervals.ClosestBinarySearch(x => x.value, value, out int index);
            return new Data(index, index > 0 ? intervals[index - 1] : intervals[0], intervals[index]);
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
            
            TrySort();
        }

        internal void TrySort()
        {
            if(EditorGUIUtility.editingTextField) return;
            
            intervals.Sort((a, b) => (int)Mathf.Sign(a.value - b.value));

            for (int i = 0; i < intervals.Count; i++)
            {
                var data = intervals[i];
                total += data.proportion;
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
            
            UpdateRectPositions();
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
    private IntervalSlider slider;
    private bool lastIsEditing;
    
    protected override void OnEnable()
    {
        base.OnEnable();

        intervalTextMode = serializedObject.FindProperty("intervalTextMode");
        intervals = serializedObject.FindProperty("intervals");
        slider = (IntervalSlider)target;
        lastIsEditing = true;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(intervalTextMode);
        EditorGUILayout.PropertyField(intervals);
        var isEditing = EditorGUIUtility.editingTextField;
        if (!isEditing && lastIsEditing)
        {
            slider.TrySort();
        }
        lastIsEditing = EditorGUIUtility.editingTextField;
        serializedObject.ApplyModifiedProperties();
    }
}
#endif