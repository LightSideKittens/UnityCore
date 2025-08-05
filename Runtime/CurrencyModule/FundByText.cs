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
        public FundText fundText;
        
        public override Id Id => fundText == null ? null : fundText.Id;

        public override int Value
        {
            get => fundText == null ? 0 : (int)fundText;
            set
            {
                if(fundText == null) return;
                fundText.text = value.ToString();
            }
        }


#if UNITY_EDITOR
        private FundText Editor_Draw(FundText value, GUIContent content)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = DrawIcon();
            value = (FundText)EditorGUI.ObjectField(rect, GUIContent.none, value, typeof(FundText), true);
            EditorGUILayout.EndHorizontal();
            return value;
        }
#endif
    }
}