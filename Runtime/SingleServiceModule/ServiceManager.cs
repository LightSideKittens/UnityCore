using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore
{
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
                }
            }
        }

        protected virtual void OnDestroy() => Services.Clear();

        internal static TService GetService<TService>() where TService : BaseSingleService
        {
            CheckIsServiceExist<TService>();
            return (TService)Services[typeof(TService)];
        }

        [Conditional("UNITY_EDITOR")]
        private static void CheckIsServiceExist<TService>()
        {
            if (Services.ContainsKey(typeof(TService)) == false)
            {
                var exeption = new Exception(
                    $"[{typeof(TService)}] Check if the prefab with {typeof(TService)} type is exist in the ServiceManager prefab" +
                    $" which should be on the scene.");

                exeption.Source = Type.Name;
                
                throw exeption;
            }
        }
        
        [Conditional("UNITY_EDITOR")]
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
        public static event Action Destroyed;
        protected static T Instance { get; private set; }


        protected override void Awake()
        {
            Instance = (T)this;
            Type = typeof(T);
            Burger.Log($"[{GetType().Name}] Awake. Scene: {SceneManager.GetActiveScene().name}");
            base.Awake();
        }

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();
            Destroyed = null;
            base.OnDestroy();
        }
    }
}
