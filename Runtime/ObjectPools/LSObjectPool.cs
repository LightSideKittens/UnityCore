using System;
using System.Collections.Generic;

namespace LSCore
{
    public class LSObjectPool<T>
    {
        internal readonly HashSet<T> releasedSet;
        public readonly HashSet<T> activeSet;
        protected Func<T> createFunc;
        public event Action<T> Created;
        public event Action<T> Got;
        public event Action<T> Released;
        public event Action<T> Destroyed;
        public int maxSize;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;
        public int CountInactive => releasedSet.Count;
        
        public LSObjectPool(
            Func<T> createFunc,
            int defaultCapacity = 10,
            int maxSize = 10000,
            bool shouldStoreActive = false)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            }
            
            releasedSet = new HashSet<T>(defaultCapacity);
            this.maxSize = maxSize;
            this.createFunc = createFunc;

            if (shouldStoreActive)
            {
                activeSet = new HashSet<T>();
                Got += AddActive;
                Released += RemoveActive;
            }
        }

        private void AddActive(T obj) => activeSet.Add(obj);
        private  void RemoveActive(T obj) => activeSet.Remove(obj);
        
        public T Get()
        {
            T obj;
            if (releasedSet.Count == 0)
            {
                obj = createFunc();
                Created?.Invoke(obj);
                ++CountAll;
            }
            else
            {
                using var enumerator = releasedSet.GetEnumerator();
                enumerator.MoveNext();
                obj = enumerator.Current;
                releasedSet.Remove(obj);
            }
            
            Got?.Invoke(obj);
            return obj;
        }
        

        public void Release(T element)
        {
            if (releasedSet.Contains(element))
            {
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
            }
            
            Released?.Invoke(element);
            if (CountInactive < maxSize)
            {
                releasedSet.Add(element);
            }
            else
            {
                Destroyed?.Invoke(element);
            }
        }

        public void ReleaseAll()
        {
            if (activeSet != null)
            {
                Released -= RemoveActive;
                
                foreach (var element in activeSet)
                {
                    Released?.Invoke(element);
                    if (CountInactive < maxSize)
                    {
                        releasedSet.Add(element);
                    }
                    else
                    {
                        Destroyed?.Invoke(element);
                    }
                }
                
                Released += RemoveActive;
                activeSet.Clear();
            }
        }

        public void Destroy()
        {
            if (Destroyed != null)
            {
                foreach (T obj in releasedSet)
                {
                    Destroyed(obj);
                }
            }
            
            releasedSet.Clear();
            CountAll = 0;
        }
    }
}