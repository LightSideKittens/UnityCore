using System.Diagnostics;
using LSCore;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LSCore
{
    public static partial class LGInput
    {
        public class Simulator : IInputProvider
        {
            private bool isTouchDown;
            private bool isTouchUp;
            internal static Simulator Instance { get; }

            static Simulator()
            {
                Instance = new Simulator();
                World.Updated += Instance.Update;
            }

            public static void TouchDown() => Instance.IsTouchDown = true;
            public static void TouchUp() => Instance.IsTouchUp = true;
            public static void SetMousePosition(in Vector3 position) => Instance.MousePosition = position;
            
            [Conditional("DEBUG")]
            private void LogTouchWarning()
            {
                if (IsTouchDown && IsTouchUp)
                {
                    Debug.LogWarning($"[{nameof(LGInput)}] {nameof(IsTouchDown)} and {nameof(IsTouchUp)} cannot be enabled in the same frame.");
                }
            }

            private void Update()
            {
                IsTouchDown = false;
                IsTouchUp = false;
            }

            private Simulator(){}

            public bool IsTouchDown
            {
                get => isTouchDown;
                private set
                {
                    if (value)
                    {
                        IsTouching = true;
                    }

                    isTouchDown = value;
                    LogTouchWarning();
                }
            }

            public bool IsTouching { get; private set; }

            public bool IsTouchUp
            {
                get => isTouchUp;
                private set
                {
                    if (value)
                    {
                        IsTouching = false;
                    }
                    
                    isTouchUp = value;
                    LogTouchWarning();
                }
            }

            public Vector3 MousePosition { get; private set; }
        }
    }
}