using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    public class SetPivot : LSAction
    {
        public RectTransform rectTransform;
        public Vector2 pivot;
        public bool savePosition = true;
    
        public override void Invoke()
        {
            if (savePosition)
            {
                rectTransform.SetPivot(pivot);
            }
            else
            {
                rectTransform.pivot = pivot;
            }
        }
    }

}