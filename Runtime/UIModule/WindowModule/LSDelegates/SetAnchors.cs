using UnityEngine;

namespace LSCore
{
    public class SetAnchors : LSAction
    {
        public RectTransform rectTransform;
        public Vector2 min;
        public Vector2 max;
        public bool savePosition = true;
        
        public override void Invoke()
        {
            if (savePosition)
            { 
                var oldPosition = rectTransform.position;
                rectTransform.anchorMin = min;
                rectTransform.anchorMax = max;
                rectTransform.position = oldPosition;
            }
            else
            {
                rectTransform.anchorMin = min;
                rectTransform.anchorMax = max;
            }
        }
    }
}