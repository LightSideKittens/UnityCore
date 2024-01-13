using System;
using System.Drawing;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LSCore.Extensions.Unity
{
    public static class BoundsExtensions
    {
        public static Vector3 GetRandomPointInside(this Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z));
        }
        
        public static float CircumscribedCircleRadius(in this Bounds bounds)
        {
            var size = bounds.size;
            float width = size.x;
            float height = size.y;
            float diagonal = (float)Math.Sqrt(width * width + height * height);
            return diagonal / 2;
        }
    }
}