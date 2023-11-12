using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class VectorExtensions
    {
        public static Vector2 Project(this in Vector2 a, in Vector2 b)
        {
            var dotProduct = Vector2.Dot(a, b);
            var magnitudeSquared = Vector2.Dot(b, b);
            return (dotProduct / magnitudeSquared) * b;
        }
        
        public static float UnclampedInverseLerp(this in Vector2 value, in Vector2 a, in Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 av = value - a;

            if (ab == Vector2.zero)
                return 0f;
            
            Vector2 projected = Project(av, ab);
            float t = projected.magnitude / ab.magnitude;
            
            if (Vector2.Dot(ab, projected) < 0)
                t *= -1;
            
            return t;
        }
        
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
        
        public static float InverseLerp(this in Vector2 value, in Vector2 a, in Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 av = value - a;
            
            if (ab == Vector2.zero)
                return 0f;
            
            Vector2 projected = Project(av, ab);
            float t = projected.magnitude / ab.magnitude;
            
            if (Vector2.Dot(ab, projected) < 0)
                t *= -1;

            // Normalize t to be between 0 and 1
            return Mathf.InverseLerp(0, ab.magnitude, t * ab.magnitude);
        }
        
        public static float GetAspect(this in Vector2 target)
        {
            if (ScreenExt.IsPortrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
        
        public static float GetAspect(this in Vector2 target, ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Portrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
    }
}