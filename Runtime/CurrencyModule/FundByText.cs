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
        [SerializeField] private FundText fundText;

        private Id id;
        public override Id Id => id ??= fundText == null ? null : fundText.Id;

        public override int Value
        {
            get => fundText;
            set => fundText.text = value.ToString();
        }
        

#if UNITY_EDITOR
        private FundText Editor_Draw(FundText value, GUIContent content)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = DrawIcon();
            value = (FundText)EditorGUI.ObjectField(rect, " ", value, typeof(FundText), true);
            EditorGUILayout.EndHorizontal();
            return value;
        }
#endif
    }
}