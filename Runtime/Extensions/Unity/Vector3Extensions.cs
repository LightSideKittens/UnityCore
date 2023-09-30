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
    }
}