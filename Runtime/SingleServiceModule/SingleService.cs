using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LSCore
{
    public abstract class SingleService<T> : BaseSingleService where T : SingleService<T>
    {
        private static T instance;
        private static Func<T> staticConstructor;
        private static Action onInitializing;
        private static bool isInited;
        public override Type Type => typeof(T);

        protected static T Instance => staticConstructor();
        public static bool IsExistsInManager => ServiceManager.IsExists<T>();

        static SingleService()
        {
            Editor_Init();
            staticConstructor = StaticConstructor;
            
            [Conditional("UNITY_EDITOR")]
            static void Editor_Init()
            {
                World.Destroyed += ResetStatic;
            }
        }

        private static T StaticConstructor()
        {
            onInitializing?.Invoke();
            staticConstructor = TrowException;

            if (instance == null)
            {
                isInited = true;
                instance = Instantiate(ServiceManager.GetService<T>());
            }

            staticConstructor = GetInstance;

            instance.Init();

            return instance;
        }

        private static T GetInstance() => instance;

        private static T TrowException() => throw new Exception(
            $"You try get {nameof(Instance)} before initializing." +
            $" Use {nameof(Init)} method by override in {typeof(T)} class.");
        
        private static T TrowDeInitException() => throw new Exception(
            $"You try get {nameof(Instance)} at {nameof(DeInit)} method.");

        public static void AddChild(Transform child, bool worldPositionStays = false)
        {
            child.SetParent(Instance.transform, worldPositionStays);
        }
        
        public static void SetParent<TParent>(bool worldPositionStays = false) where TParent : SingleService<TParent>
        {
            SingleService<TParent>.AddChild(Instance.transform, worldPositionStays);
        }
        
        protected static void OnInitializing(Action action)
        {
            onInitializing = action;
        }
    
        protected static void AddOnInitializing(Action action)
        {
            onInitializing += action;
        }

        private void Awake()
        {
            if (!isInited)
            {
                isInited = true;
                instance = (T)this;
                staticConstructor = GetInstance;
                instance.Init();
            }
            
            Debug.Log($"[{GetType().Name}] Awake");
        }

        protected virtual void Init() { }
        protected virtual void DeInit() { }

        private void OnDestroy()
        {
            staticConstructor = TrowDeInitException;
            DeInit();
            ResetStatic();
        }

        private static void ResetStatic()
        {
            isInited = false;
            onInitializing = null;
            staticConstructor = StaticConstructor;
        }
    }
}