#if UNITY_EDITOR
using System;
using HarmonyLib;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace LSCore
{
    [InitializeOnLoad]
    public static class PropertyTreePatcher
    {
        static PropertyTreePatcher()
        {
            var patcher = new Harmony("com.lightsidecore.propertytreepatcher");
            patcher.PatchAll();
        }

        [HarmonyPatch(typeof(PropertyTree), "InvokeOnPropertyValueChanged")]
        public static class InvokeOnPropertyValueChanged
        {
            public static event Action<InspectorProperty> Called;
            
            [HarmonyPostfix]
            private static void Postfix(InspectorProperty property, int selectionIndex)
            {
                Called?.Invoke(property);
            }
        }
    }
}
#endif