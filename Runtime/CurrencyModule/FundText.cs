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
                Text.color = color;
            }
        }

#if UNITY_EDITOR
        protected internal event System.Action Validated;
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
}
