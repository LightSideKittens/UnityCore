/*
using HarmonyLib;
using System;
using LSCore.Async;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[InitializeOnLoad]
internal static class Patcher
{
    static Patcher()
    {
        var harmony = new Harmony("com.lscore.patcher");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(UnityWebRequest))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new[] { typeof(Uri), typeof(string), typeof(DownloadHandler), typeof(UploadHandler) })]
internal class UnityWebRequestPatcher
{
    private static string requestCreatedLog = "Request created".ToColor(new Color(1f, 0.55f, 0.12f));
    private static string requestCompletedLog = "Request completed".ToColor(new Color(0.4f, 1f, 0.26f));
    
    /*[HarmonyPrefix]
    public static void Prefix(Uri uri, ref string method, DownloadHandler downloadHandler, UploadHandler uploadHandler) { }#1#
    
    [HarmonyPostfix]
    public static void Postfix(UnityWebRequest __instance)
    {
        Log(__instance);
    }

    public static void LogOnCreated(UnityWebRequest webRequest)
    {
        Burger.Log($"{requestCreatedLog}:\nURI: {webRequest.uri}\nMethod: {webRequest.method}"); 
        Wait.WaitWhile(() => !webRequest.isDone, Log);

        return;
        void Log()
        {
            UnityWebRequestPatcher.Log(webRequest);
        }
    }

    public static void Log(UnityWebRequest webRequest)
    {
        string data = "";
        if (webRequest.downloadHandler is not DownloadHandlerAssetBundle)
        {
            data = $"\nData: {webRequest.downloadHandler.text}";
        }
            
        Debug.Log($"{requestCompletedLog}:\nURI: {webRequest.uri}\nMethod: {webRequest.method}{data}");
    }
}

[HarmonyPatch(typeof(UnityWebRequest))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new[] { typeof(string), typeof(string), typeof(DownloadHandler), typeof(UploadHandler) })]
internal class UnityWebRequestPatcher2
{
    [HarmonyPostfix]
    public static void Postfix(UnityWebRequest __instance)
    {
        UnityWebRequestPatcher.LogOnCreated(__instance);
    }
}

[HarmonyPatch(typeof(UnityWebRequest))]
[HarmonyPatch("DisposeHandlers")]
public class UnityWebRequestPatcher_Dispose
{
    [HarmonyPrefix]
    public static void Prefix(UnityWebRequest __instance)
    {
        if (__instance.isDone)
        {
            UnityWebRequestPatcher.Log(__instance);
        }
    }
}
*/
