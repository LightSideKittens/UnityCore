using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class FundByText : BaseIntFund
    {
        [CustomValueDrawer("Editor_Draw")]
        [SerializeField] private LSNumber number;

        public override int Value
        {
            get => number;
            set => number.text = value.ToString();
        }

#if UNITY_EDITOR
        private LSNumber Editor_Draw(LSNumber value, GUIContent content)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = DrawIcon();
            value = (LSNumber)EditorGUI.ObjectField(rect, GUIContent.none, value, typeof(LSNumber), true);
            EditorGUILayout.EndHorizontal();
            return value;
        }
#endif
    }
}