using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class OnOffPool<T> where T : Component
    {
        internal readonly Stack<T> stack;
        internal readonly HashSet<T> set;
        private readonly Func<T> createFunc;
        private readonly Action<T> actionOnGet;
        private readonly Action<T> actionOnRelease;
        private readonly Action<T> actionOnDestroy;
        private readonly int maxSize;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => stack.Count;

        public OnOffPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            }

            stack = new Stack<T>(defaultCapacity);
            set = new HashSet<T>(defaultCapacity);
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.maxSize = maxSize;
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.actionOnDestroy = actionOnDestroy;
        }

        public T Get()
        {
            T obj;
            if (stack.Count == 0)
            {
                obj = createFunc();
                ++CountAll;
            }
            else
            {
                obj = stack.Pop();
                set.Remove(obj);
            }

            obj.gameObject.SetActive(true);
            actionOnGet?.Invoke(obj);
            return obj;
        }

        public void Release(T element)
        {
            if (set.Contains(element))
            {
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
            }

            element.gameObject.SetActive(false);
            actionOnRelease?.Invoke(element);
            if (CountInactive < maxSize)
            {
                stack.Push(element);
            }
            else
            {
                actionOnDestroy?.Invoke(element);
            }
        }

        public void Clear()
        {
            if (actionOnDestroy != null)
            {
                foreach (T obj in stack)
                {
                    actionOnDestroy(obj);
                }
            }

            stack.Clear();
            set.Clear();
            CountAll = 0;
        }
    }
}
