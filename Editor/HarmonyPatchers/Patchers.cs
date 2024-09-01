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
}