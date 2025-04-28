using System.Reflection;
using HarmonyLib;
using UnityEditor;

[InitializeOnLoad]
public static partial class Patchers
{
    static Patchers()
    {
        var harmony = new Harmony("com.lscore.patcher");
        harmony.PatchAll();
    }
    
    [HarmonyPatch]//TODO: Refactor OdinInspector SourceCode
    public static class GUIHelper
    {
        public static float IndentWidth { get; set; } = 15;
        
        static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Sirenix.Utilities.Editor.GUIHelper), "CurrentIndentAmount");
        }

        private static void Postfix(ref float __result)
        {
            __result = UnityEditor.EditorGUI.indentLevel * IndentWidth;
        }
    }
}