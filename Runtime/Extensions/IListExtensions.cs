using System;
using System.Collections.Generic;

namespace LSCore.Extensions
{
    public static class IListExtensions
    {
        public static T Min<T>(this IList<T> list, Func<T, float> arr, out int index)
        {
            float minValue = float.MaxValue;
            index = -1;
            T result = default;
            
            for (int i = 0; i < list.Count; i++)
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
            int width = array.GetLength(0);
            int height = array.GetLength(1);

            x = (x % width + width) % width;
            y = (y % height + height) % height;

            return array[x, y];
        }
        
        public static T Random<T>(this T[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);

            int randomX = UnityEngine.Random.Range(0, width);
            int randomY = UnityEngine.Random.Range(0, height);

            return array[randomX, randomY];
        }
        
        public static T Random<T>(this IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

        public static T ClosestBinarySearch<T>(this IList<T> list, Func<T, int> arr, int target) => list.ClosestBinarySearch(arr, target, out _);
        
        public static T ClosestBinarySearch<T>(this IList<T> list, Func<T, int> arr, int target, out int index)
        {
            int length = list.Count;
            int left = 0;
            int right = length - 1;

            if (length == 2)
            {
                index = target > arr(list[0]) ? 1 : 0;
                return list[index];
            }

            while (left < right - 1) // Keep looping until left and right are adjacent
            {
                int mid = left + (right - left) / 2;
                var midItem = list[mid];
                
                if (arr(midItem) == target)
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