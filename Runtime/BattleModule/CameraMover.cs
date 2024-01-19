using UnityEngine;

namespace Battle
{
    public static class CameraMover
    {
        public static Camera Camera { get; private set; }
        public static Transform Hero { get; private set; }
        private static Vector3 offset;
        
        public static void Init(Camera camera, Transform hero, Vector3 offset)
        {
            Camera = camera;
            Hero = hero;
            CameraMover.offset = offset;
            Move();
        }

        public static void Move()
        {
            Camera.transform.position = Hero.position + offset;
        }
    }
}