using LightSide;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(UniText))]
    public class LSNumber : MonoBehaviour
    {
        [SerializeField] private string textFormat = "{0}";
        [SerializeField] private string numberFormat;
        [SerializeField] private float number;

        private UniText text;
        protected UniText Text => text ? text : text = GetComponent<UniText>();

        public string TextFormat
        {
            get => textFormat;
            set { textFormat = value; UpdateText(); }
        }

        public string NumberFormat
        {
            get => numberFormat;
            set { numberFormat = value; UpdateText(); }
        }

        public float Number
        {
            get => number;
            set { number = value; UpdateText(); OnNumberChanged(); }
        }

        private void UpdateText()
        {
            Text.Text = string.Format(textFormat, number.ToString(numberFormat));
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnNumberChanged() { }

        public static implicit operator float(LSNumber number) => number.Number;
        public static implicit operator int(LSNumber number) => (int)number.Number;

        public static LSNumber operator +(LSNumber a, float b)
        {
            a.Number += b;
            return a;
        }

        public static LSNumber operator -(LSNumber a, float b)
        {
            a.Number -= b;
            return a;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            UpdateText();
        }
#endif
    }
}
