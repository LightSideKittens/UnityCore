using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace LSCore.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T RandomElement<T>(this IEnumerable source)
        {
            int count = 0;
            var enumerator = source.GetEnumerator();
            using var enumerator1 = enumerator as IDisposable;
            object selectedElement = default;

            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                
                count++;
                
                if (Random.Range(0, count) == 0)
                {
                    selectedElement = element;
                }
            }

            
            return (T)selectedElement;
        }
        
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            int count = 0;
            using var enumerator = source.GetEnumerator();
            T selectedElement = default;

            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                
                count++;
                
                if (Random.Range(0, count) == 0)
                {
                    selectedElement = element;
                }
            }

            return selectedElement;
        }
    }
}