using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public class RaycastHole : MonoBehaviour, ICanvasRaycastFilter
    {
        public Graphic hole;
        
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (hole == null) return true;
            return !RectTransformUtility.RectangleContainsScreenPoint(hole.rectTransform, sp, eventCamera, hole.raycastPadding);
        }
    }
}