using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace LSCore.Extensions
{
    public static class IListExtensions
    {
        public static unsafe NativeArray<T> ToNativeArray<T>(this IList<T> list, Allocator allocator) where T : struct
        {
            var arr = new NativeArray<T>(list.Count, allocator);
            void* basePtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(arr);
            
            for (int i = 0; i < list.Count; i++)
            {
                UnsafeUtility.WriteArrayElement(basePtr, i, list[i]);
            }
            
            return arr;
        }
        
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
            var index = array.RandomIndex();
            return array.Get(index);
        }
        
        public static Vector2Int RandomIndex<T>(this T[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            var randomX = UnityEngine.Random.Range(0, width);
            var randomY = UnityEngine.Random.Range(0, height);

            return new(randomX, randomY);
        }
        
        public static T Random<T>(this IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];
        public static T Random<T>(this IList<T> list, int min, int max) => list[UnityEngine.Random.Range(min, max)];
        public static T Random<T>(this IList<T> list, int min, int max, out int index)
        {
            index = UnityEngine.Random.Range(min, max);
            return list[index];
        }

        public static T RandomSafe<T>(this IList<T> list, int min, int max, out int index)
        {
            index = UnityEngine.Random.Range(min, Math.Min(max, list.Count));
            return list[index];
        }

        public static T Random<T>(this IList<T> list, out int index)
        {
            index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        public static bool HasIndex<T>(this T[,] array, int x, int y)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public static bool HasIndex<T>(this T[,] array, Vector2Int index) => array.HasIndex(index.x, index.y);

        public static Vector2Int GetSize<T>(this T[,] array) => new(array.GetLength(0), array.GetLength(1));
        public static T Get<T>(this T[,] array, Vector2Int index) => array[index.x, index.y];
        public static void Set<T>(this T[,] array, Vector2Int index, T value) => array[index.x, index.y] = value;

        public static IEnumerable<T> Enumerate<T>(this T[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    yield return array[x, y];
                }
            }
        }
        
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