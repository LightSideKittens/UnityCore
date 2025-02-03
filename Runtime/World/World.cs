using System;
using System.Collections.Generic;
using System.Threading;
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
        public static event Action CanvasUpdateCompeted;
        private static readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private static bool isCreated;
        private static World instance;
        
        public static int InstanceId => instance.GetInstanceID(); 
        public static Camera Camera { get; private set; }
        public static bool IsPlaying { get; private set; }
        public static bool IsEditMode => !IsPlaying;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Creating?.Invoke();
            var go = new GameObject(nameof(World));
            go.hideFlags = HideFlags.HideInHierarchy;
            instance = go.AddComponent<World>();
            DontDestroyOnLoad(instance);
            IsPlaying = true;

            Created?.Invoke();
            Burger.Log("[World] Created");
        }
        
        private void Awake()
        {
            var i = CanvasUpdateRegistry.instance;
            Canvas.willRenderCanvases += OnCanvasUpdateCompeted;
            Camera = Camera.main;
        }

        private void OnCanvasUpdateCompeted()
        {
            CanvasUpdateCompeted?.Invoke();
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
            Canvas.willRenderCanvases -= OnCanvasUpdateCompeted;
            Destroyed?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ApplicationPaused?.Invoke();
            }
        }

        private void OnApplicationQuit()
        {
            IsPlaying = false;
            OnApplicationPause(true);
        }
        
        
        private static readonly Queue<Action> executionQueue = new();
        
        public static void CallInMainThread(Action action)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(action);
            }
        }
        
        private static void CallActions()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}