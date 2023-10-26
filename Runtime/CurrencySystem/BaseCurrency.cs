using System;

namespace LSCore
{
    public abstract class BaseCurrency<T> where T : BaseCurrency<T>, new()
    {
        [Serializable]
        public class Price : BasePrice
        {
            public override void Earn()
            {
                BaseCurrency<T>.Earn(value);
            }
    
            public override bool TrySpend()
            {
                return BaseCurrency<T>.TrySpend(value);
            }
        }

        public static event Action Changing;
        public static event Action Changed;
        private static T instance = new T();

        public static int Value
        {
            get => Currencies.GetValue<T>();
            private set
            {
                var currentValue = Currencies.GetValue<T>();
                if (currentValue != value)
                {
                    Changing?.Invoke();
                    Currencies.SetValue<T>(value);
                    Changed?.Invoke();
                }
            }
        }

        public static void Earn(int value)
        {
            Value += value;
        }
    
        public static bool TrySpend(int value)
        {
            var canSpend = value <= Value;
    
            if (canSpend)
            {
                Value -= value;
            }
    
            return canSpend;
        }
    
        public static bool TryConvertTo<T1>(int fromUnitCount, int toUnitCount) where T1 : BaseCurrency<T1>, new()
        {
            var isSpent = TrySpend(fromUnitCount);
    
            if (isSpent)
            {
                BaseCurrency<T1>.Earn(toUnitCount);
            }
    
            return isSpent;
        }
    }
}

