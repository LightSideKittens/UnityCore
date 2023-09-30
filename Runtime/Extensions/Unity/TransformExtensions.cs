using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class TransformExtensions
    {
        public static void X(this Transform transform, float x)
        {
            var pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }
        
        public static void Y(this Transform transform, float y)
        {
            var pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }
        
        public static void XY(this Transform transform, float x, float y)
        {
            var pos = transform.position;
            pos.x = x;
            pos.y = y;
            transform.position = pos;
        }
    }
}