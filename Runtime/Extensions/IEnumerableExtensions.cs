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
        
        public static T RandomElement<T>(this IEnumerable<T> source, Func<T, bool> predicate = null, bool invertPredicate = false)
        {
            int count = 0;
            using var enumerator = source.GetEnumerator();
            T selectedElement = default;

            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                
                count++;
                
                if (Random.Range(0, count) == 0 && (predicate == null || predicate(element) ^ invertPredicate ))
                {
                    selectedElement = element;
                }
            }

            return selectedElement;
        }

        public static T RandomElement<T>(this IEnumerable<T> source, HashSet<T> exclude)
        {
            return source.RandomElement(exclude.Contains, true);
        }
    }
}