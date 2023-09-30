using UnityEngine.Events;
using UnityEngine.UI;

namespace LSCore.Extensions.Unity
{
    public static class ButtonExtensions
    {
        public static void AddListener(this Button button, UnityAction action)
        {
            button.onClick.AddListener(action);
        }
        
        public static void RemoveListener(this Button button, UnityAction action)
        {
            button.onClick.RemoveListener(action);
        }
        
        public static void RemoveAllListeners(this Button button)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}