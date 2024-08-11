using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class OnOffPool<T> : LSObjectPool<T> where T : Component
    {
#if UNITY_EDITOR
        static OnOffPool()
        {
            World.Destroyed += _poolsByPrefab.Clear;
        }
#endif
        private static readonly Dictionary<T, OnOffPool<T>> _poolsByPrefab = new();

        public static bool TryCreatePool(
            T prefab,
            out OnOffPool<T> pool,
            Transform parent = null,
            int capacity = 100,
            bool shouldStoreActive = false)
        {
            bool isNew = !_poolsByPrefab.TryGetValue(prefab, out pool);
            if (isNew)
            {
                pool = new OnOffPool<T>(prefab, parent, capacity, shouldStoreActive);
                _poolsByPrefab.Add(prefab, pool);
            }

            pool.Parent = parent;
            return isNew;
        }
        
        public static OnOffPool<T> GetOrCreatePool(
            T prefab,
            Transform parent = null,
            int capacity = 100,
            bool shouldStoreActive = false)
        {
            if (_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool.Parent = parent;
                return pool;
            }
            pool = new OnOffPool<T>(prefab, parent, capacity, shouldStoreActive);
            _poolsByPrefab.Add(prefab, pool);
            pool.Parent = parent;
            return pool;
        }
        
        private readonly T prefab;
        private Transform parent;

        public Transform Parent
        {
            get => parent;
            set
            {
                parent = value;
                Got -= OnGetWithParent;
                Got -= OnGet;
                
                if (value != null)
                {
                    createFunc = InstantiatePrefabWithParent;
                    Got += OnGetWithParent;
                }
                else
                {
                    createFunc = InstantiatePrefab;
                    Got += OnGet;
                }
            }
        }
        
        public OnOffPool(
            T prefab,
            Transform parent = null,
            int capacity = 100,
            bool shouldStoreActive = false) : base(null, capacity, shouldStoreActive)
        {
            this.prefab = prefab;
            Parent = parent;
            Released += OnRelease;
        }
        
        private T InstantiatePrefabWithParent()
        {
            return Object.Instantiate(prefab, parent);
        }

        private T InstantiatePrefab()
        {
            return Object.Instantiate(prefab);
        }

        public T Get(Vector3 position)
        {
            var obj = Get();
            obj.transform.position = position;
            return obj;
        }
        
        private void OnGetWithParent(T obj)
        {
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(parent, false);
        }

        private  void OnGet(T obj)
        {
            obj.gameObject.SetActive(true);
        }
        
        private void OnRelease(T element)
        {
            element.gameObject.SetActive(false);
        }
    }
}
