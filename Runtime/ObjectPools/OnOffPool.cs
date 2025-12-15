using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class OnOffPool<T> : LSObjectPool<T> where T : Component
    {

        static OnOffPool()
        {
#if UNITY_EDITOR
            World.Destroyed += _poolsByPrefab.Clear;
#endif
            World.PreRendering += OnPrerender;
        }

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

        public static bool RemovePool(T prefab)
        {
            return _poolsByPrefab.Remove(prefab);
        }
        
        private readonly T prefab;
        private Transform parent;

        public Transform Parent
        {
            get => parent;
            set
            {
                parent = value;
                CreatedOrGot -= OnGetWithParent;
                CreatedOrGot -= OnGet;
                
                if (value != null)
                {
                    createFunc = InstantiatePrefabWithParent;
                    CreatedOrGot += OnGetWithParent;
                }
                else
                {
                    createFunc = InstantiatePrefab;
                    CreatedOrGot += OnGet;
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

        private static void OnPrerender()
        {
            foreach (var (target, data) in batches)
            {
                if (target)
                {
                    target.gameObject.SetActive(data.active);
                    target.transform.SetParent(data.parent);
                }
            }
            
            batches.Clear();
        }

        private T InstantiatePrefabWithParent()
        {
            prefab.gameObject.SetActive(false);
            var result=  Object.Instantiate(prefab, parent);
            return result;
        }

        private T InstantiatePrefab()
        {
            prefab.gameObject.SetActive(false);
            return Object.Instantiate(prefab);
        }

        public T Get(Vector3 position)
        {
            var obj = Get();
            obj.transform.position = position;
            return obj;
        }
        
        private static Dictionary<T, (bool active, Transform parent)> batches = new();
        
        private void OnGetWithParent(T obj)
        {
            batches[obj] = (true, parent);
        }

        private  void OnGet(T obj)
        {
            batches[obj] = (true, parent);
        }
        
        private void OnRelease(T obj)
        {
            batches[obj] = (false, parent);
        }
    }
}
