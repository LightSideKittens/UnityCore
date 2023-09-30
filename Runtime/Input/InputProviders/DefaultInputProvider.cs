using UnityEngine;

namespace LSCore
{
    internal class DefaultInputProvider : IInputProvider
    {
        public static DefaultInputProvider Instance { get; }
        private Vector3 lastPosition;
        
        static DefaultInputProvider()
        {
            Instance = new DefaultInputProvider();
        }

        private DefaultInputProvider(){}

        public bool IsTouchDown => Input.GetMouseButtonDown(0);
        public bool IsTouching => Input.GetMouseButton(0);
        public bool IsTouchUp => Input.GetMouseButtonUp(0);
        public Vector3 MousePosition => Input.mousePosition;
    }
}