#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class FundText : LSNumber
    {
        [field: SerializeField, Id(typeof(CurrencyIdGroup))] public Id Id { get; protected set; }
        
        [SerializeField] private bool changeTextColorIfNotEnough;
        [ShowIf("changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id notEnoughColorId;
        [ShowIf("changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id enoughColorId;
        
        public bool CanSpend => Currencies.Spend(Id, (int)Number, out _);

        public override string text
        {
            get => base.text;
            set
            {
                var oldNumber = (int)Number;
                base.text = value;
                if (oldNumber != (int)Number && changeTextColorIfNotEnough)
                {
                    UpdateColor(default);
                }
            }
        }

        protected override void OnNumberChanged()
        {
            base.OnNumberChanged();
            if (changeTextColorIfNotEnough)
            {
                UpdateColor(default);
            }
        }

        private void UpdateColor((int, int) _)
        {
            var colorId = CanSpend ? enoughColorId : notEnoughColorId;
            
            if (Palette.TryGet(colorId, out var color))
            {
                this.color = color;
            }
        }
        
#if UNITY_EDITOR
        protected internal event Action Validated;
        protected override void OnValidate()
        {
            base.OnValidate();
            Validated?.Invoke();
        }
#endif
        
        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            if(World.IsEditMode) return;
#endif
            if (changeTextColorIfNotEnough)
            {
                Funds.AddOnChanged(Id, UpdateColor, true);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if UNITY_EDITOR
            if(World.IsEditMode) return;
#endif
            Funds.RemoveOnChanged(Id, UpdateColor);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(FundText), true), CanEditMultipleObjects]
    public class FundTextEditor : LSNumberEditor
    {
        private InspectorProperty id;
        private InspectorProperty changeTextColorIfNotEnough;
        private InspectorProperty enoughColorId;
        private InspectorProperty notEnoughColorId;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            var children = propertyTree.RootProperty.Children;
            
            id = children["<Id>k__BackingField"];
            changeTextColorIfNotEnough = children["changeTextColorIfNotEnough"];
            enoughColorId = children["enoughColorId"];
            notEnoughColorId = children["notEnoughColorId"];
        }

        protected override void Draw()
        {
            id.Draw();
            changeTextColorIfNotEnough.Draw();
            enoughColorId.Draw();
            notEnoughColorId.Draw();
            id.Tree.UpdateTree();
            base.Draw();
        }
    }
#endif
}