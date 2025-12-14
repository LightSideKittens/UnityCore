#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class PoolTracker : MonoBehaviour
    {
        public object pool;

        public static void Track(GameObject go, object pool)
        {
            if (!go.TryGetComponent(out PoolTracker tracker))
            {
                tracker = go.AddComponent<PoolTracker>();
            }
            
            tracker.pool = pool;
        }
        
        private void OnEnable()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }
        
        void OnUpdate()
        {
            EditorApplication.update -= OnUpdate;
            if (this != null && pool == null)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }
    }
}
#endif