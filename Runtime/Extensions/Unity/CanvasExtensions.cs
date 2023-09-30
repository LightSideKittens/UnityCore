using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class CanvasExtensions
    {
        public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition, float scaleFactor)
        {
            var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                screenPosition.y / Screen.height,
                0);
            return canvas.ViewportToCanvasPosition(viewportPosition, scaleFactor) * scaleFactor;
        }
    
        public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition, float scaleFactor)
        {
            var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
            var canvasRect = canvas.GetComponent<RectTransform>();
            var scale = canvasRect.sizeDelta / scaleFactor;
            return Vector3.Scale(viewportPosition, scale);
        }
    }
}
