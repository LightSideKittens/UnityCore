using UnityEngine.EventSystems;

namespace LSCore
{
    public static class PointerEventDataExtensions
    {
        public static bool IsFirstTouch(this PointerEventData eventData)
        {
#if UNITY_EDITOR
            return eventData.pointerId is -1 or 0;
#endif
            return eventData.pointerId == 0;
        }
    }
}