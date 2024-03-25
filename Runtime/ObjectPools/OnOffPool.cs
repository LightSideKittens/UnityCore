using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class OnOffPool<T> : LSObjectPool<T> where T : Component
    {
        private readonly T prefab;
        
        public OnOffPool(
            T prefab,
            int defaultCapacity = 10,
            int maxSize = 10000) : base(null, defaultCapacity, maxSize)
        {
            this.prefab = prefab;
            createFunc = InstantiatePrefab;
            Got += OnGet;
            Released += OnRelease;
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

        public void OnGet(T obj)
        {
            obj.gameObject.SetActive(true);
        }
        
        private void OnRelease(T element)
        {
            element.gameObject.SetActive(false);
        }
    }
}
