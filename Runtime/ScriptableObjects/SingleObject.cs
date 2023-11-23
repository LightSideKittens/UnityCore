#if UNITY_EDITOR
using System;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class SingleObject<T> where T : Object
    {
        private T instance;
        public T Get(Action<T> onComplete = null)
        {
            if (instance == null)
            {
                instance = AssetDatabaseUtils.LoadAny<T>();
            }
            
            onComplete?.Invoke(instance);
            return instance;
        }
    }
}
#endif