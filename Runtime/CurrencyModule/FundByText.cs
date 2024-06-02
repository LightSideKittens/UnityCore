using System;
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
        [ShowIf("$changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id enoughColorId;

        public override Id Id => fundText == null ? null : fundText.Id;

        public override int Value
        {
            get => fundText;
            set => fundText.text = value.ToString();
        }

        public void OnBeforeSerialize() { }

        public async void OnAfterDeserialize()
        {
            await Task.Delay(1);
            
            if (!World.IsPlaying) return;
            if (!changeTextColorIfNotEnough) return;
            
            Funds.OnChanged(Id, UpdateColor, true);
        }

        private void UpdateColor(int a)
        {
            if (fundText == null)
            {
                Funds.RemoveOnChanged(Id, UpdateColor);
                return;
            }

            var id = notEnoughColorId;
            if (Spend(out _)) id = enoughColorId;
            
            if (Palette.TryGet(id, out var color))
            {
                fundText.color = color;
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