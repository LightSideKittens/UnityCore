using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore
{
    public class Intervals : ScriptableObject
    {
        [SerializeField] private int[] intervals;
        
        public int Count => intervals.Length;
        public int this[int index] => intervals[index];
        
        public (T data, int interval) Get<T, TList>(int value, TList list) where TList : IList<T>
        {
            intervals.ClosestBinarySearch(x => x, value, out var index);
            return (list[index], intervals[index]);
        }
    }
}