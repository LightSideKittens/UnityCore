using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore
{
    [DefaultExecutionOrder(-9)]
    public abstract class ServiceManager : MonoBehaviour
    {
        [SerializeField] private List<BaseSingleService> services;
        private static readonly Dictionary<Type, BaseSingleService> Services = new Dictionary<Type, BaseSingleService>();
        protected static Type Type { get; set; }
        
        protected virtual void Awake()
        {
            Services.Clear();
            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                var isError = false;
                CheckServiceNull(service, ref isError);
                if (!isError)
                {
                    Services.Add(service.Type, service);
                    if (service.CreateImmediately)
                    {
                        Instantiate(service);
                    }
                }
            }
        }

        protected virtual void OnDestroy() => Services.Clear();

        internal static TService GetService<TService>() where TService : BaseSingleService
        {
            CheckIsServiceExist<TService>();
            return (TService)Services[typeof(TService)];
        }

        internal static bool IsExists<TService>()
        {
            return Services.ContainsKey(typeof(TService));
        }
        
        private static void CheckIsServiceExist<TService>()
        {
            if (IsExists<TService>() == false)
            {
                var exeption = new Exception(
                    $"[{typeof(TService)}] Check if the prefab with {typeof(TService)} type is exist in the ServiceManager prefab" +
                    $" which should be on the scene.");

                exeption.Source = Type.Name;
                
                throw exeption;
            }
        }
        
        private static void CheckServiceNull(BaseSingleService service, ref bool isError)
        {
            isError = service == null;
            if (isError)
            {
                var exeption = new NullReferenceException($"Service is null")
                {
                    Source = Type.Name
                };

                Burger.Error(exeption);
            }
        }
    }

    public abstract class ServiceManager<T> : ServiceManager where T : ServiceManager<T>
    {
        [SerializeReference] public List<DoIt> onAwake;
        private static T instance;
        public static event Action Destroyed;

        protected static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if(World.IsEditMode) return FindAnyObjectByType<T>(FindObjectsInactive.Include);
#endif
                return instance;
            }
            private set => instance = value;
        }


        protected override void Awake()
        {
            Instance = (T)this;
            Type = typeof(T);
            Burger.Log($"[{GetType().Name}] Awake. Scene: {SceneManager.GetActiveScene().name}");
            base.Awake();
            onAwake.Do();
        }

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();
            Destroyed = null;
            base.OnDestroy();
        }

        protected static void SetInstance<TService>(TService instance) where TService : SingleService<TService>
        {
            SingleService<TService>.instance = instance;
        }
    }
}
