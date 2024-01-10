using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class FundByText : BaseIntFund, ISerializationCallbackReceiver
    {
        [CustomValueDrawer("Editor_Draw")]
        [SerializeField] private FundText fundText;
        [SerializeField] private bool changeTextColorIfNotEnough;
        [ShowIf("$changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id notEnoughColorId;

        public override int Value
        {
            get => fundText;
            set => fundText.text = value.ToString();
        }

        public void OnBeforeSerialize(){}

        [Conditional("UNITY_EDITOR")]
        private void UpdateId()
        {
#if UNITY_EDITOR
            isControls = true;
#endif
            if (fundText != null && fundText.Id != null)
            {
                id = fundText.Id;
            }
        }
        
        public void OnAfterDeserialize()
        {
            UpdateId();
            
            if (World.IsPlaying)
            {
                if (changeTextColorIfNotEnough)
                {
                    if (!Spend(out _))
                    {
                        if (Palette.TryGet(notEnoughColorId, out var color))
                        {
                            fundText.color = color;
                        }
                    }
                }
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