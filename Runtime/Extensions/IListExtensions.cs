using System;
using System.Collections;
using System.Collections.Generic;

namespace LSCore.Extensions
{
    public static class IListExtensions
    {
        public static IEnumerable<T> BySelectEx<T>(this IList<T> list, string expression)
        {
            foreach (var index in SelectEx.GetIndexes(expression, list.Count))
            {
                yield return list[index];
            }
        }
        
        public static T Min<T>(this IList<T> list, Func<T, float> arr, out int index)
        {
            var minValue = float.MaxValue;
            index = -1;
            T result = default;
            
            for (var i = 0; i < list.Count; i++)
            {
                var candidate = list[i];
                var value = arr(candidate);
                if (value < minValue) {
                    minValue = value;
                    index = i;
                    result = candidate;
                }
            }
            
            return result;
        }
        
        public static T GetCyclic<T>(this T[,] array, int x, int y)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            x = (x % width + width) % width;
            y = (y % height + height) % height;

            return array[x, y];
        }
        
        public static T GetCyclic<T>(this IList<T> list, int i)
        {
            var c = list.Count;
            return list[(i % c + c) % c];
        }
        
        public static T Random<T>(this T[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            var randomX = UnityEngine.Random.Range(0, width);
            var randomY = UnityEngine.Random.Range(0, height);

            return array[randomX, randomY];
        }
        
        public static T Random<T>(this IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

        public static T ClosestBinarySearch<T>(this IList<T> list, Func<T, float> arr, float target, float tolerance = 0.0001f) => list.ClosestBinarySearch(arr, target, out _, tolerance);
        
        public static T ClosestBinarySearch<T>(this IList<T> list, Func<T, float> arr, float target, out int index, float tolerance = 0.0001f)
        {
            var length = list.Count;
            
            if (length == 1)
            {
                index = 0;
                return list[index];
            }

            var left = 0;
            var right = length - 1;

            if (length == 2)
            {
                index = target > arr(list[0]) ? 1 : 0;
                return list[index];
            }

            while (left < right - 1) // Keep looping until left and right are adjacent
            {
                var mid = left + (right - left) / 2;
                var midItem = list[mid];
                
                if (Math.Abs(arr(midItem) - target) < tolerance)
                {
                    index = mid;
                    return midItem; // Found the exact target
                }

                if (arr(midItem) < target)
                {
                    left = mid; // Move left pointer to mid
                }
                else
                {
                    right = mid; // Move right pointer to mid
                }
            }

            var leftItem = list[left];
            index = left;
            
            if (target > arr(leftItem))
            {
                index = right;
                return list[right];
            }
            
            return leftItem;
        }
    }
}