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
        public class Price : BasePrice
        {
            public override void Earn()
            {
                BaseCurrency<T>.Earn(value);
            }

            public override void Spend(Func<bool> confirmation)
            {
                BaseCurrency<T>.Spend(value, confirmation);
            }
            
#if UNITY_EDITOR
            protected override Texture2D Icon => AssetDatabase.LoadAssetAtPath<Texture2D>(GetType().GetGenericArguments()[0].GetAttribute<IconAttribute>().path);

            public override bool Equals(object obj)
            {
                if (obj is BasePrice drawer)
                {
                    return Equals(drawer);
                }

                return false;
            }
        
            public bool Equals(BasePrice other) => GetType() == other.GetType();

            public override int GetHashCode() => GetType().GetHashCode();
#endif
        }
        
        private static T instance = new T();
        private static string name = typeof(T).Name;

        public static void Earn(int value)
        {
            Currencies.Earn(name, value);
        }
    
        public static void Spend(int value, Func<bool> confirmation)
        {
            Currencies.Spend(name, value, confirmation);
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

