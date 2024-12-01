using System;
using System.Diagnostics;
using LSCore;
using UnityEngine;

namespace LSCore
{
    public static partial class LSInput
    {
        public static class AndroidBackButton
        {
            static AndroidBackButton()
            {
                World.Updated += OnUpdate;
            }

            private static void OnUpdate()
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    action?.Invoke();
                }
            }

            private static Action action;
            [Conditional("UNITY_ANDROID")]
            public static void Subscribe(Action onBack) => action += onBack;
            [Conditional("UNITY_ANDROID")]
            public static void UnSubscribe(Action onBack) => action -= onBack;
        }
    }
}