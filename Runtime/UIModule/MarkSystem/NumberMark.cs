using UnityEngine;

namespace LSCore
{
    public class NumberMark : Mark<int>
    {
        [SerializeField] private LSText text;

        protected override void HandleView()
        {
            var total = GetTotal();
            gameObject.SetActive(total > 0);
            text.text = total.ToString();
        }

        private int GetTotal()
        {
            var total = 0;
            
            foreach (var value in Get())
            {
                total += value;
            }

            return total;
        }
        
        public void Increase()
        {
            var value = GetTotal();
            DoMark(value + 1);
        }
        
        public void Decrease()
        {
            var value = GetTotal();
            if (value - 1 <= 0)
            {
                DoUnMark();
                return;
            }
            DoMark(value - 1);
        }
    }
}