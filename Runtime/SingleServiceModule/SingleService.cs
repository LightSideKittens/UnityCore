using System;
using System.Diagnostics;
using UnityEngine;

namespace LSCore
{
    public abstract class SingleService<T> : BaseSingleService where T : SingleService<T>
    {
        internal static T instance;
        private static Func<T> staticConstructor;
        public override Type Type => typeof(T);
        
        protected static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    if(instance != null) return instance;
                    return FindAnyObjectByType<T>(FindObjectsInactive.Include);
                }
#endif
                return staticConstructor();
            }
        }
        
        public static bool IsNull
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    if(instance != null) return false;
                    return FindAnyObjectByType<T>(FindObjectsInactive.Include) == null;
                }
#endif
                return instance == null;
            }
        }

        public static bool IsExistsInManager => ServiceManager.IsExists<T>();

        static SingleService()
        {
#if UNITY_EDITOR
            World.Destroyed += ResetStatic;
#endif
            staticConstructor = StaticConstructor;
        }

        private static T StaticConstructor()
        {
            staticConstructor = TrowException;
            var obj = Instantiate(ServiceManager.GetService<T>());
            obj.Awake();
            return obj;
        }

        private static T GetInstance() => instance;

        private static T TrowException() => throw new Exception(
            $"You try get {nameof(Instance)} before initializing." +
            $" Use {nameof(Init)} method by override in {typeof(T)} class.");
        
        private bool isInited;
        private string logTag => $"[{GetType().Name}]".ToTag(new Color(0.64f, 0.35f, 1f));
        
        private void Awake()
        {
            if(isInited) return;
            isInited = true;
            instance = (T)this;
            staticConstructor = GetInstance;
            instance.Init();
            Burger.Log($"{logTag} {name} Awake");
        }

        protected virtual void Init() { }
        protected virtual void DeInit() { }

        private void OnDestroy()
        {
            DeInit();
            ResetStatic();
        }

        private static void ResetStatic()
        {
            instance = null;
            staticConstructor = StaticConstructor;
        }
    }
}