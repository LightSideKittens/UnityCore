using UnityEditor;
using UnityEngine;

namespace LSCore.Editor
{
    public class EditorHiddenObjectPool<T> : LSObjectPool<T>
    {
        public EditorHiddenObjectPool(int defaultCapacity = 10, bool shouldStoreActive = false) : base(null, defaultCapacity, shouldStoreActive)
        {
            createFunc = CreateObject;
        }

        public T CreateObject()
        {
            var go = EditorUtility.CreateGameObjectWithHideFlags("hidden", HideFlags.None, typeof(T));
            return go.GetComponent<T>();
        }
    }
}