using System;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class CameraExtensions
    {
        private static Func<Camera, Vector3> sizeGetter;

        static CameraExtensions()
        {
            if (ScreenExt.IsPortrait)
            {
                sizeGetter = GetSizeOnPortraitMode;
            }
            else
            {
                sizeGetter = GetSizeOnAlbumMode;
            }
        }
    
        public static Bounds GetBounds(this Camera camera)
        {
            return new Bounds(camera.transform.position, sizeGetter(camera));
        }

        public static Vector3 GetSize(this Camera camera)
        {
            return sizeGetter(camera);
        }
        
        public static Rect GetRect(this Camera camera)
        {
            return new Rect(camera.transform.position, sizeGetter(camera) * 2);
        }
        

        private static Vector3 GetSizeOnPortraitMode(Camera camera)
        {
            var orthographicSize = camera.orthographicSize;

            return new Vector3(orthographicSize, orthographicSize / ScreenExt.Aspect);
        }
    
        private static Vector3 GetSizeOnAlbumMode(Camera camera)
        {
            var orthographicSize = camera.orthographicSize;

            return new Vector3(orthographicSize / ScreenExt.Aspect, orthographicSize);
        }
    }
}
