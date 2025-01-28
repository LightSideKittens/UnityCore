using System;
using System.Collections;
using System.Collections.Generic;

namespace LSCore.DataStructs
{
    public readonly struct ListSpan<T> : IEnumerable<T>
    {
        private readonly IList<T> list;
        private readonly int start;
        private readonly int count;
    
        public ListSpan(IList<T> list, int start, int count)
        {
            if (start < 0 || start > list.Count || count < 0 || start + count > list.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
    
            this.list = list;
            this.start = start;
            this.count = count;
        }
    
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count) throw new ArgumentOutOfRangeException();
                return list[start + index];
            }
            set
            {
                if (index < 0 || index >= count) throw new ArgumentOutOfRangeException();
                list[start + index] = value;
            }
        }
    
        public int Count => count;
    
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < count; i++)
            {
                yield return list[start + i];
            }
        }
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    public static class IListExtensions
    {
        public static ListSpan<T> AsSpan<T>(this IList<T> list, Range range)
        {
            var count = list.Count;
            var start = range.Start.GetOffset(count);
            var end = range.End.GetOffset(count);
            count = end - start;
    
            return new ListSpan<T>(list, start, count);
        }
    }
}
