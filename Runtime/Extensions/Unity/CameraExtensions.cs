using System;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class CameraExtensions
    {
        public static Bounds GetBounds(this Camera camera)
        {
            return new Bounds(camera.transform.position, camera.GetSize());
        }

        public static Vector2 GetSize(this Camera camera)
        {
            float cameraHeight = camera.orthographicSize * 2;
            float cameraWidth = cameraHeight * camera.aspect;
            return new Vector2(cameraWidth, cameraHeight);
        }
        
        public static Rect GetRect(this Camera camera)
        {
            var rect = new Rect(camera.transform.position, camera.GetSize());
            rect.center = camera.transform.position;
            return rect;
        }
    }
}
