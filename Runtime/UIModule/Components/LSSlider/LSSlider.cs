using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static LSCore.LSSlider.TextMode;

namespace LSCore
{
    public partial class LSSlider : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        [Serializable]
        public enum TextMode
        {
            Value,
            Percent,
            NormalizedPercent,
            ValueToMax
        }

        [Serializable]
        public enum NumberMode
        {
            Default,
            WholeInText,
            WholeEverywhere
        }
        
        [field: SerializeField] public Image Icon { get; private set; }
        [SerializeField] private LSText text; 
        [SerializeField] private TextMode textMode;
        [SerializeField] private NumberMode numberMode;
        [SerializeField] private bool onlyDiff;
        [SerializeField] private bool clampValue;
        
        public float OffsetMaxValue => maxValue - minValue;
        public float OffsetValue => value - minValue;
        
        public float DisplayedMaxValue => GetDisplayedValue(maxValue);
        public float DisplayedValue => GetDisplayedValue(value);
        public float GetDisplayedValue(float val) => onlyDiff ? val - minValue : val;
        
        public NumberMode NumberModee
        {
            get => numberMode;
            set
            {
                if (numberMode != value)
                {
                    UpdateVisuals();
                    numberMode = value;
                    UpdateValueTextGetter();
                }
            }
        }

        private Func<float, string> textGetter;
        private Func<float, string> valueTextGetter;
        private StringBuilder stringBuilder = new();
        
        public TextMode TextModee
        {
            get => textMode;
            set
            {
                textMode = value;
                UpdateTextGetter();
                UpdateText();
            }
        }

        public bool OnlyDiff
        {
            get => onlyDiff;
            set
            {
                if (onlyDiff != value)
                {
                    onlyDiff = value;
                    UpdateText();
                }
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedReferences();
            Set(m_Value, false);
            UpdateVisuals();
            UpdateCachedReferences();
        }

        protected virtual void Init()
        {
            if (numberMode == NumberMode.WholeEverywhere)
            {
                m_MinValue = Mathf.Round(m_MinValue);
                m_MaxValue = Mathf.Round(m_MaxValue);
            }
            
            UpdateValueTextGetter();
            UpdateTextGetter();

            if (text != null)
            {
                onValueChanged.RemoveListener(UpdateValueText);
                onValueChanged.AddListener(UpdateValueText);
            }

            UpdateText();
        }

        private void UpdateText()
        {
            if (text != null)
            {
                UpdateValueText(value);
            }
        }

        private void UpdateValueText(float val)
        {
            text.text = textGetter(val);
        }

        private void UpdateTextGetter()
        {
            textGetter = GetTextGetter(textMode);
        }

        protected Func<float, string> GetTextGetter(TextMode mode)
        {
            return mode switch
            {
                Value => valueTextGetter,
                Percent => PercentGetter,
                NormalizedPercent => NormalizedPercentGetter,
                ValueToMax => ValueToMaxGetter,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string PercentGetter(float val) => $"{valueTextGetter(val)}%";
        private string NormalizedPercentGetter(float val) => $"{(int)(normalizedValue * 100)}%";
        private string ValueToMaxGetter(float val1)
        {
            stringBuilder.Clear();
            var max = valueTextGetter(maxValue);
            var val = valueTextGetter(val1);
            stringBuilder.Append("<mspace=0.6em>");
            for (int i = max.Length - val.Length; i > 0; i--)
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(val);
            stringBuilder.Append('/');
            stringBuilder.Append(max);
            
            return stringBuilder.ToString();
        }

        private void UpdateValueTextGetter()
        {
            UpdateValueTextGetter(ValueText, IntValue, out valueTextGetter);
        }
        
        private void UpdateValueTextGetter(Func<float, string> defaultGetter, Func<float, string> intGetter, out Func<float, string> getter)
        {
            getter = numberMode > 0 ? intGetter : defaultGetter;
        }

        private string IntValue(float val) => $"{(int)GetDisplayedValue(val)}";
        private string ValueText(float val) => $"{GetDisplayedValue(val)}";
        
        /// <summary>
        /// Set the value of the slider.
        /// </summary>
        /// <param name="input">The new value for the slider.</param>
        /// <param name="sendCallback">If the OnValueChanged callback should be invoked.</param>
        /// <remarks>
        /// Process the input to ensure the value is between min and max value. If the input is different set the value and send the callback is required.
        /// </remarks>
        protected virtual void Set(float input, bool sendCallback = true)
        {
            float newValue = clampValue ? ClampValue(input) : input;
            
            if (m_Value == newValue)
                return;

            m_Value = newValue;
            UpdateVisuals();
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Slider.value", this);
                onValueChanged.Invoke(newValue);
            }
        }
    }
}