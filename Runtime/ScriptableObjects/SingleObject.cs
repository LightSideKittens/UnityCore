using Object = UnityEngine.Object;

namespace LSCore
{
    public static class SingleObject<T> where T : Object
    {
        private static T instance;

        static SingleObject()
        {
            World.Destroyed += () => instance = null;
        }
        
        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode && instance == null)
                {
                    instance = AssetDatabaseUtils.LoadAny<T>();
                }
#endif
            
                return instance;
            }
            set => instance = value;
        }
    }
}