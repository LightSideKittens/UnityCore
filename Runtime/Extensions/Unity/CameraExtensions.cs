using System;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class CameraExtensions
    {
        public static Vector2 ScreenToWorldDelta(this Camera camera, Vector2 delta)
        {
            Vector3 worldPoint0 = camera.ScreenToWorldPoint(new Vector2(0, 0));
            Vector3 worldPointDelta = camera.ScreenToWorldPoint(new Vector3(delta.x, delta.y));
            var worldDelta = worldPointDelta - worldPoint0;
            return worldDelta;
        }
        
        public static Bounds GetBounds(this Camera camera)
        {
            return new Bounds(camera.transform.position, camera.GetSize());
        }

        public static Vector2 GetSize(this Camera camera)
        {
            return GetSize(camera.orthographicSize, camera.aspect);
        }
        
        public static Vector2 GetSize(float size, float aspect)
        {
            var cameraHeight = size * 2;
            var cameraWidth = cameraHeight * aspect;
            return new Vector2(cameraWidth, cameraHeight);
        }
        
        public static void SetSizeByWidth(this Camera camera, float width)
        {
            camera.orthographicSize = (width / camera.aspect) / 2;
        }
        
        public static void SetSizeByHeight(this Camera camera, float height)
        {
            camera.orthographicSize = height / 2;
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
           var halfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
           var halfWidth = halfHeight * camera.aspect;
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
            var pointOnPlane = tr.position + forward * distance;
            var plane = new Plane(forward, pointOnPlane);
            
            return plane;
        }
        
        public static Plane PlaneAt(this Canvas canvas)
        {
            var tr = canvas.transform;
            var forward = tr.forward;
            var pointOnPlane = tr.position;
            var plane = new Plane(forward, pointOnPlane);
            
            return plane;
        }
    }
}
