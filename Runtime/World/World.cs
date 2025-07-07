using System;
using System.Collections.Concurrent;
using System.Threading;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    [DefaultExecutionOrder(-1000)]
    public class World : MonoBehaviour
    {
        public static event Action ApplicationPaused;
        public static event Action Creating;
        public static event Action Created;
        public static event Action Updated;
        public static event Action FixedUpdated;
        public static event Action Destroyed;
        private static readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private static bool isCreated;
        private static World instance;
        
        public static int InstanceId => instance.GetInstanceID(); 
        public static Camera Camera { get; private set; }
        public static bool IsPlaying { get; private set; }
        public static bool IsEditMode => !IsPlaying;
        public static float FrameRate => 1f / Time.unscaledDeltaTime;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            DOTween.SetTweensCapacity(1000, 1000);
            Creating.SafeInvoke();
            var go = new GameObject(nameof(World));
            instance = go.AddComponent<World>();
            DontDestroyOnLoad(go);
            IsPlaying = true;

            Created.SafeInvoke();
            Application.targetFrameRate = 120;
            Burger.Log("[World] Created");
        }

        public static void CallOnCreated(Action action)
        {
            if (IsPlaying)
            {
                action();
            }
            else
            {
                Created += action;
            }
        }
        
        public static void CallOnCreatedOnce(Action action)
        {
            CallOnCreated(Call);
            void Call()
            {
                action();
                Created -= Call;
            }
        }
        
        private void Awake()
        {
            Camera = Camera.main;
        }

        private void Update()
        {
            Updated?.Invoke();
            CallActions();
        }
        
        private void FixedUpdate()
        {
            FixedUpdated?.Invoke();
        }
        
        private void OnDestroy()
        {
            IsPlaying = false;
            Burger.logToFile = false;
            Destroyed.SafeInvoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ApplicationPaused.SafeInvoke();
            }
        }

        private void OnApplicationQuit()
        {
            IsPlaying = false;
            OnApplicationPause(true);
        }
        
        
        private static readonly ConcurrentQueue<Action> executionQueue = new();
        
        public static void CallInMainThread(Action action)
        {
            executionQueue.Enqueue(action);
        }
        
        private static void CallActions()
        {
            while (executionQueue.Count > 0)
            {
                if (executionQueue.TryDequeue(out var action))
                {
                    action();
                }
            }
        }
    }
}