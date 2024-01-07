using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class OnOffPool<T> where T : Component
    {
        internal readonly Stack<T> stack;
        internal readonly HashSet<T> set;
        private readonly Func<T> createFunc;
        public event Action<T> Created;
        public event Action<T> Got;
        public event Action<T> Released;
        public event Action<T> Destroyed;
        public int maxSize;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => stack.Count;

        private readonly T prefab;
        
        public OnOffPool(
            T prefab,
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            }

            stack = new Stack<T>(defaultCapacity);
            set = new HashSet<T>(defaultCapacity);
            this.maxSize = maxSize;
            this.prefab = prefab;
            createFunc = InstantiatePrefab;
        }

        private T InstantiatePrefab()
        {
            return Object.Instantiate(prefab);
        }
        
        public T Get()
        {
            T obj;
            if (stack.Count == 0)
            {
                obj = createFunc();
                Created?.Invoke(obj);
                ++CountAll;
            }
            else
            {
                obj = stack.Pop();
                set.Remove(obj);
            }

            obj.gameObject.SetActive(true);
            Got?.Invoke(obj);
            return obj;
        }

        public T Get(Vector3 position)
        {
            var obj = Get();
            obj.transform.position = position;
            return obj;
        }

        public void Release(T element)
        {
            if (set.Contains(element))
            {
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
            }

            element.gameObject.SetActive(false);
            Released?.Invoke(element);
            if (CountInactive < maxSize)
            {
                stack.Push(element);
            }
            else
            {
                Destroyed?.Invoke(element);
            }
        }

        public void Destroy()
        {
            if (Destroyed != null)
            {
                foreach (T obj in stack)
                {
                    Destroyed(obj);
                }
            }

            stack.Clear();
            set.Clear();
            CountAll = 0;
        }
    }
}
