using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
        [ShowIf("changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id notEnoughColorId;
        [ShowIf("changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id enoughColorId;

        private Id id;
        public override Id Id => id ??= fundText == null ? null : fundText.Id;

        public override int Value
        {
            get => fundText;
            set => fundText.text = value.ToString();
        }

        private void UpdateColor(int a)
        {
            var colorId = CanSpend ? enoughColorId : notEnoughColorId;
            
            if (Palette.TryGet(colorId, out var color))
            {
                fundText.color = color;
            }
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
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (!changeTextColorIfNotEnough) return;
            fundText.Enabled += Sub;
            fundText.Disabled += UnSub;
        }

        private void Sub()
        {
            Funds.AddOnChanged(Id, UpdateColor, true);
        }

        private void UnSub()
        {
            Funds.RemoveOnChanged(Id, UpdateColor);
        }
    }
}