#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [InlineProperty]
    [HideLabel]
    public class DataByInterval<T>
    {
        [SerializeField] [LabelText("$Label")] protected Intervals intervals;
        [HideIf("$hideData")] [SerializeField] 
        [ListDrawerSettings(OnBeginListElementGUI = "DrawLabel")]
        private T[] data;

        public (T data, int interval) Get(int value) => intervals.Get<T, T[]>(value, data);

#if UNITY_EDITOR
        private bool hideData => intervals == null || intervals.Count == 0;
        protected virtual string Label => "Intervals";
        private void DrawLabel(int index)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            
            if (index >= intervals.Count)
            {
                EditorGUILayout.LabelField($"<b>Don't Exist</b>", style);
                return;
            }

            var fromVal = index > 0 ? intervals[index - 1] : 0;
            EditorGUILayout.LabelField($"<b>{fromVal} — {intervals[index]}</b>", style);
        }
#endif
    }
}