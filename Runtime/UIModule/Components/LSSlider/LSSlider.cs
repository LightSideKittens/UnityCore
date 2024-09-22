using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static LSCore.LSSlider.TextMode;

namespace LSCore
{
    public partial class LSSlider : Slider
    {
        [Serializable]
        public enum TextMode
        {
            Value,
            Percent,
            NormalizedPercent,
            ValueToMax
        }
        
        [field: SerializeField] public Image Icon { get; private set; }
        [SerializeField] private LSText text; 
        [SerializeField] private TextMode textMode;
        [SerializeField] private bool wholeNumberInText;
        [SerializeField] private bool onlyDiff;
        [SerializeField] private bool clampValue;

        public float OffsetMaxValue => maxValue - minValue;
        public float OffsetValue => value - minValue;
        
        public float DisplayedMaxValue => onlyDiff ? maxValue - minValue : maxValue;
        public float DisplayedValue => onlyDiff ? value - minValue : value;
        
        public new bool wholeNumbers
        {
            get => base.wholeNumbers;
            set
            {
                if (base.wholeNumbers != value)
                {
                    base.wholeNumbers = value;
                    UpdateValueTextGetters();
                }
            }
        }
        
        public bool WholeNumberInText
        {
            get => wholeNumberInText;
            set
            {
                if (wholeNumberInText != value)
                {
                    base.wholeNumbers = !base.wholeNumbers; //HACK: need for calling "private void UpdateVisuals()" in base
                    base.wholeNumbers = !base.wholeNumbers;
                    wholeNumberInText = value;
                    UpdateValueTextGetters();
                }
            }
        }

        private Func<string> textGetter;
        private Func<string> valueTextGetter;
        private Func<string> maxValueTextGetter;
        private StringBuilder stringBuilder = new();
        
        public TextMode TextModee
        {
            get => textMode;
            set
            {
                textMode = value;
                UpdateTextGetter();
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

#if UNITY_EDITOR
        protected override void Reset()
        {
            wholeNumbers = true;
            base.Reset();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            onValueChanged.RemoveAllListeners();
            Init();
        }
#endif
        
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            UpdateValueTextGetters();
            UpdateTextGetter();
            
            if (text != null)
            {
                onValueChanged.AddListener(UpdateText);
                UpdateText();
            }
        }

        private void UpdateText() => UpdateText(0);

        private void UpdateText(float _)
        {
            text.text = textGetter();
        }

        private void UpdateTextGetter()
        {
            textGetter = textMode switch
            {
                Value => valueTextGetter,
                Percent => PercentGetter,
                NormalizedPercent => NormalizedPercentGetter,
                ValueToMax => ValueToMaxGetter,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string PercentGetter() => $"{valueTextGetter()}%";
        private string NormalizedPercentGetter() => $"{(int)(normalizedValue * 100)}%";
        private string ValueToMaxGetter()
        {
            stringBuilder.Clear();
            var max = maxValueTextGetter();
            var val = valueTextGetter();
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

        private void UpdateValueTextGetters()
        {
            UpdateValueTextGetter(ValueText, IntValue, out valueTextGetter);
            UpdateValueTextGetter(MaxValue, IntMaxValue, out maxValueTextGetter);
        }
        
        private void UpdateValueTextGetter(Func<string> defaultGetter, Func<string> intGetter, out Func<string> getter)
        {
            getter = wholeNumbers || WholeNumberInText ? intGetter : defaultGetter;
        }

        private string IntValue() => $"{(int)DisplayedValue}";
        private string IntMaxValue() => $"{(int)DisplayedMaxValue}";
        
        private string ValueText() => $"{DisplayedValue}";
        private string MaxValue() => $"{DisplayedMaxValue}";


        protected override void Set(float input, bool sendCallback = true)
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