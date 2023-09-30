using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public static partial class LGInput
    {
        private static IInputProvider provider = DefaultInputProvider.Instance;
        private static Vector3 lastPosition;

        public static bool IsManualControl
        {
            set => provider = value ? Simulator.Instance : DefaultInputProvider.Instance;
        }
        
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


            public static bool IsTouchDown => LGInput.IsTouchDown && !IsPointerOverUI;
            public static bool IsTouching => LGInput.IsTouching && !IsPointerOverUI;
            public static bool IsTouchUp => LGInput.IsTouchUp && !IsPointerOverUI;
        }
    }
}