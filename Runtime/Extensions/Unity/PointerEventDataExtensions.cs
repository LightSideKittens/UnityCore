using UnityEngine.EventSystems;

namespace LSCore
{
    public static class PointerEventDataExtensions
    {
        public static bool IsFirstTouch(this PointerEventData eventData)
        {
#if UNITY_EDITOR
            return eventData.pointerId == -1;
#endif
            return eventData.pointerId == 0;
        }
    }
}