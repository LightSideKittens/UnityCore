#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ObjTracker : MonoBehaviour
    {
        public object obj;

        public static void Track(GameObject go, object obj)
        {
            if (!go.TryGetComponent(out ObjTracker tracker))
            {
                tracker = go.AddComponent<ObjTracker>();
            }
            
            tracker.obj = obj;
        }
        
        private void OnEnable()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }
        
        void OnUpdate()
        {
            EditorApplication.update -= OnUpdate;
            if (this != null && obj == null)
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