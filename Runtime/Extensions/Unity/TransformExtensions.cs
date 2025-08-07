using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class TransformExtensions
    {
        public static void X(this Transform transform, float x)
        {
            var pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }
        
        public static void Y(this Transform transform, float y)
        {
            var pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }
        
        public static void XY(this Transform transform, float x, float y)
        {
            var pos = transform.position;
            pos.x = x;
            pos.y = y;
            transform.position = pos;
        }
        
        public static void LocalX(this Transform transform, float x)
        {
            var pos = transform.localPosition;
            pos.x = x;
            transform.localPosition = pos;
        }
        
        public static void LocalY(this Transform transform, float y)
        {
            var pos = transform.localPosition;
            pos.y = y;
            transform.localPosition = pos;
        }
        
        public static void LocalXY(this Transform transform, float x, float y)
        {
            var pos = transform.localPosition;
            pos.x = x;
            pos.y = y;
            transform.localPosition = pos;
        }
        
        public static void SetLossyScale(this Transform tr, Vector3 worldScale)
        {
            Vector3 parentScale = tr.parent ? tr.parent.lossyScale : Vector3.one;
            
            tr.localScale = new Vector3(
                parentScale.x == 0 ? 0 : worldScale.x / parentScale.x,
                parentScale.y == 0 ? 0 : worldScale.y / parentScale.y,
                parentScale.z == 0 ? 0 : worldScale.z / parentScale.z);
        }
        
        public static void SetLossyScaleUnsafe(this Transform tr, Vector3 worldScale)
        {
            Vector3 parentScale = tr.parent ? tr.parent.lossyScale : Vector3.one;
            
            tr.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z);
        }
        
        public static void SetScale(this Transform tr, Vector3 localScale)
        {
            tr.localScale = localScale;
        }

        public static void SetScale(this Transform tr, float f)
        {
            tr.localScale = new Vector3(f, f, f);
        }
        
        public static T FindComponentInChildren<T>(this Transform transform)
        {
            T result = default;
            foreach (Transform child in transform)
            {
                result = child.GetComponentInChildren<T>(true);
                if (result != null) break;
            }
            return result;
        }
    }
}