using System;
using System.Collections;
using UnityEngine;

namespace LSCore
{
    [DefaultExecutionOrder(-1000)]
    public class World : MonoBehaviour
    {
        public static event Action ApplicationPaused;
        public static event Action Creating;
        public static event Action Created;
        public static event Action Updated;
        private static bool isCreated;
        private static World instance;
        public static Camera Camera { get; private set; }
        public static bool IsPlaying { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Creating?.Invoke();
            
            instance = new GameObject(nameof(World)).AddComponent<World>();
            DontDestroyOnLoad(instance);
            IsPlaying = true;

            Created?.Invoke();
            Debug.Log("[World] Created");
        }

        private void Awake()
        {
            Camera = Camera.main;
        }

        private void Update()
        {
            Updated?.Invoke();
        }

        private void OnDestroy()
        {
            IsPlaying = false;
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
    }
}