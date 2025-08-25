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
        
        public static Rect GetScreenRect(this RectTransform rectTransform)
        {
            if (!rectTransform) return new Rect();

            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (!canvas) return new Rect();

            var root = canvas.rootCanvas;
            Camera cam = null;
            var mainCam = Camera.main;
            
            if (root.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = root.worldCamera ?? mainCam;

            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[0]);
            Vector2 max = min;

            for (int i = 1; i < 4; i++)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
                min = Vector2.Min(min, sp);
                max = Vector2.Max(max, sp);
            }

            var rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            
            return rect;
        }

        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            if (!rectTransform) return new Rect();

            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (!canvas) return new Rect();

            var root = canvas.rootCanvas;
            Camera cam = null;
            var mainCam = Camera.main;
            
            if (root.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = root.worldCamera ?? mainCam;

            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[0]);
            Vector2 max = min;

            for (int i = 1; i < 4; i++)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
                min = Vector2.Min(min, sp);
                max = Vector2.Max(max, sp);
            }

            var rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            
            return ScreenRectToWorldRectXY(rect, cam ?? mainCam);
        }
        
        private static Vector3 ScreenToWorldOnPlane(Vector2 screenPoint, Camera cam, Plane plane)
        {
            var ray = cam.ScreenPointToRay(screenPoint);
            return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
        }
        
        public static Vector3[] ScreenRectToWorldQuad(Rect screenRect, Camera cam, Plane plane)
        {
            var bl = ScreenToWorldOnPlane(new Vector2(screenRect.xMin, screenRect.yMin), cam, plane);
            var tl = ScreenToWorldOnPlane(new Vector2(screenRect.xMin, screenRect.yMax), cam, plane);
            var tr = ScreenToWorldOnPlane(new Vector2(screenRect.xMax, screenRect.yMax), cam, plane);
            var br = ScreenToWorldOnPlane(new Vector2(screenRect.xMax, screenRect.yMin), cam, plane);
            return new[] { bl, tl, tr, br };
        }
        
        public static Rect ScreenRectToWorldRectXY(Rect screenRect, Camera cam, float zWorld = 1f)
        {
            var camT = cam.transform;
            var pos = camT.position;
            var forward = camT.forward;
            pos += zWorld * forward;
            var plane = new Plane(camT.forward, pos);
            var quad = ScreenRectToWorldQuad(screenRect, cam, plane);

            float xMin = Mathf.Min(quad[0].x, quad[1].x, quad[2].x, quad[3].x);
            float xMax = Mathf.Max(quad[0].x, quad[1].x, quad[2].x, quad[3].x);
            float yMin = Mathf.Min(quad[0].y, quad[1].y, quad[2].y, quad[3].y);
            float yMax = Mathf.Max(quad[0].y, quad[1].y, quad[2].y, quad[3].y);

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
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