using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class GUIScene
    {
        public static float SnapX(float x, float step)
        {
            if (step > 0)
            {
                var rem = x % step;
                if (rem > step / 2)
                {
                    x += step - rem;
                }
                else
                {
                    x -= rem;
                }
            }
            
            return x;
        }
        
        public static Rect CalculateBounds(IEnumerable<Vector2> points)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var point in points)
            {
                if (point.x < minX) minX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.x > maxX) maxX = point.x;
                if (point.y > maxY) maxY = point.y;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}