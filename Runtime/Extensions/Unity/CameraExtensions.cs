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
            var pos = camera.transform.position;
            var rect = new Rect(pos, camera.GetSize());
            rect.center = pos;
            return rect;
        }
        
        public static Vector2 GetSize(this Camera camera, float distance)
        {
           float halfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
           float halfWidth = halfHeight * camera.aspect;
           return new Vector2(halfWidth * 2, halfHeight * 2);
        }
        
        public static Rect GetRect(this Camera camera, float distance)
        {
            var pos = camera.transform.position;
            var rect = new Rect(pos, camera.GetSize(distance));
            rect.center = pos;
            return rect;
        }
        
        public static Plane PlaneAt(this Camera camera, float distance)
        {
            var tr = camera.transform;
            var forward = tr.forward;
            Vector3 pointOnPlane = tr.position + forward * distance;
            Plane plane = new Plane(forward, pointOnPlane);
            
            return plane;
        }
        
        public static Plane PlaneAt(this Canvas canvas)
        {
            var tr = canvas.transform;
            var forward = tr.forward;
            Vector3 pointOnPlane = tr.position;
            Plane plane = new Plane(forward, pointOnPlane);
            
            return plane;
        }
    }
}
