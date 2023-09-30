using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class VectorExtensions
    {
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