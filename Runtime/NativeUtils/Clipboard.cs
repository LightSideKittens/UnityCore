using UnityEngine;

namespace LSCore.Runtime
{
    public static class Clipboard
    {
        public static string Value
        {
            get => GUIUtility.systemCopyBuffer;
            set
            {
                Burger.Log($"[{nameof(Clipboard)}] Copy text: {value}");
                GUIUtility.systemCopyBuffer = value;
            }
        }
    }
}