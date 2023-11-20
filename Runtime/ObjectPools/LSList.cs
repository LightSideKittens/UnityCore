using System;
using System.Collections;
using System.Collections.Generic;

namespace LSCore
{
    public class LSList<T> : IEnumerable<T>
    {
        private T[] _items;
        private int _count;

        public LSList()
        {
            _items = new T[4];
            _count = 0;
        }

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException();

                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException();

                _items[index] = value;
            }
        }
        
        public ref T this[Index index]
        {
            get
            {
                int i = index.IsFromEnd ? _count - index.Value : index.Value;
                if (i < 0 || i >= _count)
                    throw new ArgumentOutOfRangeException();
                return ref _items[i];
            }
        }

        public void Add(T item)
        {
            if (_count == _items.Length)
            {
                Resize();
            }

            _items[_count++] = item;
        }
        
        public void Add(in T item)
        {
            if (_count == _items.Length)
            {
                Resize();
            }

            _items[_count++] = item;
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (Equals(_items[i], item))
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException();

            _count--;
            for (int i = index; i < _count; i++)
            {
                _items[i] = _items[i + 1];
            }

            _items[_count] = default(T); // Optional: to remove the reference
        }

        private void Resize()
        {
            int newSize = _items.Length * 2;
            T[] newArray = new T[newSize];
            Array.Copy(_items, newArray, _items.Length);
            _items = newArray;
        }

        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_items, 0, _count);
                _count = 0;
            }
        }
        
        public void Sort(Comparison<T> comparison)
        {
            if (_count > 1)
            {
                Array.Sort(_items, 0, _count, Comparer<T>.Create(comparison));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}