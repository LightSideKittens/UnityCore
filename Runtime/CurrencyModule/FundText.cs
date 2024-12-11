#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class FundText : LSNumber
    {
        [field: SerializeField, Id(typeof(CurrencyIdGroup))] public Id Id { get; private set; }
        
        [SerializeField] private bool changeTextColorIfNotEnough;
        [ShowIf("changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id notEnoughColorId;
        [ShowIf("changeTextColorIfNotEnough")]
        [Id(typeof(PaletteIdGroup))] [SerializeField] private Id enoughColorId;
        
        public bool CanSpend => Currencies.Spend(Id, Number, out _);

        public override string text
        {
            get => base.text;
            set
            {
                var oldNumber = Number;
                base.text = value;
                if (oldNumber != Number && changeTextColorIfNotEnough)
                {
                    UpdateColor(Number);
                }
            }
        }

        private void UpdateColor(int a)
        {
            var colorId = CanSpend ? enoughColorId : notEnoughColorId;
            
            if (Palette.TryGet(colorId, out var color))
            {
                this.color = color;
            }
        }
        
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

        protected override void OnDisable()
        {
            base.OnDisable();
            propertyTree.Dispose();
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