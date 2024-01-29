using System;
using Firebase;
using UnityEngine;

namespace LSCore.Firebase
{
    public static class Disposer
    {
        public static event Action Disposed;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StaticConstructor()
        {
            World.Destroyed -= Dispose;
            World.Destroyed += Dispose;
        }
        
        private static void Dispose()
        {
            FirebaseApp.DefaultInstance.Dispose();
            Disposed?.Invoke();
        }
    }
}