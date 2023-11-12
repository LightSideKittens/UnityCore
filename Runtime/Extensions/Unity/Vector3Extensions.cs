using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Vector3Extensions
    {
        public static void ClampX(this ref Vector3 target, float min, float max)
        {
            target.x = Mathf.Clamp(target.x, min, max);
        }
        
        public static void ClampY(this ref Vector3 target, float min, float max)
        {
            target.y = Mathf.Clamp(target.y, min, max);
        }
        
        public static void ClampZ(this ref Vector3 target, float min, float max)
        {
            target.z = Mathf.Clamp(target.z, min, max);
        }
        
        public static float GetAspect(this Vector2 target)
        {
            if (ScreenExt.IsPortrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
        
        public static float GetAspect(this Vector2 target, ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Portrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
        
        public static float UnclampedInverseLerp(this in Vector3 value, in Vector3 a, in Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 av = value - a;

            if (ab == Vector3.zero)
                return 0f;
            
            Vector3 projected = Vector3.Project(av, ab);
            float t = projected.magnitude / ab.magnitude;
            
            if (Vector3.Dot(ab, projected) < 0)
                t *= -1;
            
            return t;
        }
        
        public static float InverseLerp(this in Vector3 value, in Vector3 a, in Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 av = value - a;
            
            if (ab == Vector3.zero)
                return 0f;
            
            Vector3 projected = Vector3.Project(av, ab);
            float t = projected.magnitude / ab.magnitude;
            
            if (Vector3.Dot(ab, projected) < 0)
                t *= -1;

            // Normalize t to be between 0 and 1
            return Mathf.InverseLerp(0, ab.magnitude, t * ab.magnitude);
        }
    }
}