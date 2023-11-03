using System;
using Sirenix.Utilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LSCore
{
    public abstract class BaseCurrency<T> where T : BaseCurrency<T>, new()
    {
        [Serializable]
        public class Fund : BaseFund
        {
            public override void Earn()
            {
                BaseCurrency<T>.Earn(value);
            }

            public override bool Spend(out Action spend)
            {
                return BaseCurrency<T>.Spend(value, out spend);
            }
            
#if UNITY_EDITOR
            protected override void SetIcon(ref Texture2D icon)
            {
                icon ??= AssetDatabase.LoadAssetAtPath<Texture2D>(GetType().GetGenericArguments()[0].GetAttribute<IconAttribute>().path);
            }

            public override bool Equals(object obj)
            {
                if (obj is BaseFund currency)
                {
                    return Equals(currency);
                }

                return false;
            }
        
            public bool Equals(BaseFund other) => GetType() == other.GetType();

            public override int GetHashCode() => GetType().GetHashCode();
#endif
        }
        
        private static T instance = new T();
        private static string name = typeof(T).Name;

        public static void Earn(int value)
        {
            Currencies.Earn(name, value);
        }
    
        public static bool Spend(int value, out Action spend)
        {
            return Currencies.Spend(name, value, out spend);
        }
    
        /*public static bool TryConvertTo<T1>(int fromUnitCount, int toUnitCount) where T1 : BaseCurrency<T1>, new()
        {
            var isSpent = TrySpend(fromUnitCount);
    
            if (isSpent)
            {
                BaseCurrency<T1>.Earn(toUnitCount);
            }
    
            return isSpent;
        }*/
    }
}

