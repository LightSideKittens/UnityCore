using HarmonyLib;
using UnityEditor;

[InitializeOnLoad]
internal static class Patcher
{
    static Patcher()
    {
        var harmony = new Harmony("com.lscore.patcher");
        harmony.PatchAll();
    }
}