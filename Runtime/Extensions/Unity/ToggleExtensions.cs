using UnityEngine.Events;
using UnityEngine.UI;

namespace LSCore.Extensions.Unity
{
    public static class ToggleExtensions
    {
        public static void AddListener(this Toggle button, UnityAction<bool> action)
        {
            button.onValueChanged.AddListener(action);
        }
        
        public static void RemoveListener(this Toggle button, UnityAction<bool> action)
        {
            button.onValueChanged.RemoveListener(action);
        }
        
        public static void RemoveAllListeners(this Toggle button)
        {
            button.onValueChanged.RemoveAllListeners();
        }
    }
}