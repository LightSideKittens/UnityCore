using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class RectTransformExtensions
    {
        public static RectTransform Parent(this RectTransform target) => target.parent.GetComponent<RectTransform>();
        
        public static Vector2 GetLocalPositionByScreenPoint(this RectTransform parent, Vector2 screenPoint, Canvas canvas = null)
        {
            Camera cam = null;
            
            if (canvas)
            {
                cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            }
            
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, cam, out var localPoint);
            return localPoint;
        }
        
        public static void SetPositionByScreenPoint(this RectTransform target, RectTransform parent, Vector2 screenPoint, Canvas canvas = null)
        {
            target.localPosition = parent.GetLocalPositionByScreenPoint(screenPoint, canvas);
        }

        public static void SetPivot(this RectTransform target, in Vector2 pivot)
        {
            Vector2 size = target.rect.size;
            Vector2 pivotDelta = pivot - target.pivot;
            Vector2 offset = new Vector2(pivotDelta.x * size.x, pivotDelta.y * size.y);
            target.anchoredPosition += offset;
            target.pivot = pivot;
        }
        
        public static void SetSizeDeltaX(this RectTransform target, float x)
        {
            var size = target.sizeDelta;
            size.x = x;
            target.sizeDelta = size;
        }
        
        public static void FitToWorldRect(this RectTransform uiRect,
            Bounds worldBounds,
            Camera cam,
            Canvas canvas)
        {
            Vector3[] worldCorners = new Vector3[4]
            {
                new(worldBounds.min.x, worldBounds.min.y, worldBounds.center.z),
                new(worldBounds.min.x, worldBounds.max.y, worldBounds.center.z),
                new(worldBounds.max.x, worldBounds.max.y, worldBounds.center.z),
                new(worldBounds.max.x, worldBounds.min.y, worldBounds.center.z) 
            };

            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
            
            bool overlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;

            foreach (Vector3 w in worldCorners)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, w);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)uiRect.parent, screen, overlay ? null : cam, out Vector2 local);

                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            
            var lastPivot = uiRect.pivot;
            uiRect.pivot = new Vector2(0, 0);
            uiRect.localPosition = min;
            uiRect.sizeDelta = max - min;
            uiRect.SetPivot(lastPivot);
        }
    }
}