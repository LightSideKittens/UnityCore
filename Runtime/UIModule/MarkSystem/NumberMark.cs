using UnityEngine;

namespace LSCore
{
    public class NumberMark : Mark<int>
    {
        [SerializeField] private LSText text;

        protected override void HandleView()
        {
            var has = TryGet(out var value) && value > 0;
            gameObject.SetActive(has);
            text.text = value.ToString();
        }

        public void Increase()
        {
            TryGet(out var value);
            DoMark(++value);
        }
        
        public void Decrease()
        {
            TryGet(out var value);
            if (value - 1 <= 0)
            {
                DoUnMark();
                return;
            }
            DoMark(--value);
        }
    }
}