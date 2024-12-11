using UnityEngine.EventSystems;

namespace LSCore
{
    public static class PointerEventDataExtensions
    {
        public static bool IsFirstTouch(this PointerEventData eventData) => eventData.pointerId == 0;
    }
}