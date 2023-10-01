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
    }
}