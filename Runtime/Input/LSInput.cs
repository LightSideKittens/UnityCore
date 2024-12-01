using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

namespace LSCore
{
    public static partial class LSInput
    {
        private static IInputProvider provider = DefaultInputProvider.Instance;
        private static Vector3 lastPosition;

        public static bool IsManualControl
        {
            set => provider = value ? Simulator.Instance : DefaultInputProvider.Instance;
        }

        static LSInput()
        {
            World.Updated += Update;
        }

        private static void Update()
        {
            if (TouchDown != null && IsTouchDown)
            {
                TouchDown();
            }
            else if(Touching != null && IsTouching)
            {
                Touching();
            }
            else if(TouchUp != null && IsTouchUp)
            {
                TouchUp();
            }
        }

        public static event Action TouchDown;
        public static event Action Touching;
        public static event Action TouchUp;
        public static bool IsTouchDown => provider.IsTouchDown;
        public static bool IsTouching => provider.IsTouching;
        public static bool IsTouchUp => provider.IsTouchUp;
        public static Vector3 MousePosition => provider.MousePosition;
        
        public static Vector3 MouseDelta
        {
            get
            {
                var mousePosition = MousePosition;
                if (IsTouchDown) lastPosition = mousePosition;
                var delta = mousePosition - lastPosition;
                lastPosition = mousePosition;
                return delta;
            }
        }
        
        public static class UIExcluded
        {
            public static bool IsPointerOverUI => EventSystem.current.IsPointerOverGameObject(PointerId);

            private static int PointerId
            {
                get
                {
                    var pointerId = PointerInputModule.kMouseLeftId;
#if !UNITY_EDITOR
                    if (Input.touches.Length > 0)
                    {
                        pointerId = Input.touches[0].fingerId;
                    }
#endif
                    return pointerId;
                }
            }


            public static bool IsTouchDown => LSInput.IsTouchDown && !IsPointerOverUI;
            public static bool IsTouching => LSInput.IsTouching && !IsPointerOverUI;
            public static bool IsTouchUp => LSInput.IsTouchUp && !IsPointerOverUI;
        }
    }
}