using System;
using System.Collections.Generic;

namespace LSCore.Extensions
{
    public static class IListExtensions
    {
        public static T Random<T>(this IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

        public static int ClosestBinarySearch(Func<int, int> arr, int length, int target)
        {
            int left = 0;
            int right = length - 1;

            if (length == 2)
            {
                return target > arr(0) ? 1 : 0;
            }

            while (left < right - 1) // Keep looping until left and right are adjacent
            {
                int mid = left + (right - left) / 2;

                if (arr(mid) == target)
                {
                    return mid; // Found the exact target
                }
                else if (arr(mid) < target)
                {
                    left = mid; // Move left pointer to mid
                }
                else
                {
                    right = mid; // Move right pointer to mid
                }
            }

            if (target > arr(left))
            {
                return right;
            }

            return left;
        }
    }
}