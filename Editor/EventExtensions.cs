using Sirenix.Utilities.Editor;
using UnityEngine;

namespace LightSideCore.Editor
{
    public static class EventExtensions
    {
        public static bool IsCopy(this Event e)
        {
            var needCopy = e.control && e.OnKeyDown(KeyCode.C, false);
            return needCopy;
        }
        
        public static bool IsPaste(this Event e)
        {
            var needCopy = e.control && e.OnKeyDown(KeyCode.V, false);
            return needCopy;
        }
    }
}