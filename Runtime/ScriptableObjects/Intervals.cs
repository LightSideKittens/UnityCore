using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore
{
    public class Intervals : ScriptableObject
    {
        public struct Data
        {
            public int index;
            public int from;
            public int to;

            public Data(int index, int from, int to)
            {
                this.index = index;
                this.from = from;
                this.to = to;
            }
        }
        
        [SerializeField] private int[] intervals;
        
        public int Count => intervals.Length;
        public int this[int index] => intervals[index];
        
        public Data Get(int value)
        {
            intervals.ClosestBinarySearch(x => x, value, out var index);
            return new Data(index, index > 0 ? intervals[index - 1] : 0, intervals[index]);
        }
        
        public (T element, Data data) Get<T, TList>(int value, TList list) where TList : IList<T>
        {
            var data = Get(value);
            return (list[data.index], data);
        }
    }
}