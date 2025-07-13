using System;
using System.Linq;
using LSCore.DataStructs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public struct LSTouch
    {
        public int fingerId;
        public Vector2 position;
        public Vector2 deltaPosition;
        public TouchPhase phase;
        
        public bool IsPointerOverUI
        {
            get
            {
#if UNITY_EDITOR
                int id = PointerInputModule.kMouseLeftId;
#else
                int id = fingerId;
#endif
                return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(id);
            }
        }
    }

    public static partial class LSInput
    {
        private static readonly LSTouch[] touchesBuffer = new LSTouch[20];
        private static IInputProvider provider = DefaultInputProvider.Instance;
        private static ArraySlice<LSTouch> currentTouches = ArraySlice<LSTouch>.empty;

        public static ArraySlice<LSTouch> Touches => currentTouches;

        public static int TouchCount => currentTouches.Length;

        public static LSTouch GetTouch(int index) => currentTouches[index];

        public static bool IsManualControl
        {
            set => provider = value ? Simulator.Instance : DefaultInputProvider.Instance;
        }

        static LSInput()
        {
            World.Updated += Update;
            
#if UNITY_EDITOR
            World.Creating += () =>
            {
                Simulator.Instance = new Simulator();
                DefaultInputProvider.Instance = new DefaultInputProvider();
                IsManualControl = false;
            };
#endif
        }

        private static void Update()
        {
            var touches = provider.GetTouches();
            currentTouches = touches;
            
            for (var i = 0; i < touches.Length; i++)
            {
                ref var touch = ref currentTouches[i];
                
                switch (touch.phase)
                {
                    case TouchPhase.Moved:
                        TouchMoved?.Invoke(touch);
                        break;
                    case TouchPhase.Stationary:
                        TouchStaying?.Invoke(touch);
                        break;
                    case TouchPhase.Began:
                        TouchBegan?.Invoke(touch);
                        break;
                    case TouchPhase.Ended:
                        TouchEnded?.Invoke(touch);
                        break;
                    case TouchPhase.Canceled:
                        TouchCanceled?.Invoke(touch);
                        break;
                }
            }
        }

        public static event Action<LSTouch> TouchBegan;
        public static event Action<LSTouch> TouchMoved;
        public static event Action<LSTouch> TouchStaying;
        public static event Action<LSTouch> TouchEnded;
        public static event Action<LSTouch> TouchCanceled;

        public static class UIExcluded
        {
            public static bool IsPointerOverUI
            {
                get
                {
#if UNITY_EDITOR
                    int id = PointerInputModule.kMouseLeftId;
#else
                    int id = Input.touchCount > 0 ? Input.touches[0].fingerId : PointerInputModule.kMouseLeftId;
#endif
                    return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(id);
                }
            }
            
            public static bool IsTouchDown => TouchDown && !IsPointerOverUI;
            public static bool IsTouching => Touching && !IsPointerOverUI;
            public static bool IsTouchUp => TouchUp && !IsPointerOverUI;

            private static bool TouchDown => Touches.Any(t => t.phase == TouchPhase.Began);
            private static bool Touching => Touches.Any(t => t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary);
            private static bool TouchUp => Touches.Any(t => t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled);
        }
    }
}
