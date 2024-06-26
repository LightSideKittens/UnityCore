using System.Diagnostics;
using System.IO;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Burger
{
    [Conditional("DEBUG")]
    public static void Log(object log) => Debug.Log(log);

    [Conditional("DEBUG")]
    public static void Error(object log) => Debug.LogError(log);

    [Conditional("DEBUG")]
    public static void Warning(object log) => Debug.LogWarning(log);

    [MenuItem(LSPaths.MenuItem.Root + "/Update Burger")]
    private static void Update()
    {
        var dllSrcPath = Assembly.GetExecutingAssembly().GetAssemblyFilePath();
        var dllDstPath = LSPaths.Root.ToFull();
        var newFileName = "LSCore.Burger.dll";
        var newFilePath = Path.Combine(dllDstPath, newFileName);
        
        if (File.Exists(newFilePath))
        {
            File.Delete(newFilePath);
        }
        
        File.Copy(dllSrcPath, newFilePath);
    }
    
    public static string ToTag(this string text, Color color)
    {
        return text.ToColor(color).ToBold();
    }
    
    public static string ToBold(this string text)
    {
        return $"<b>{text}</b>";
    }
    
    public static string ToColor(this string text, Color color)
    {
        string colorHexText = ColorUtility.ToHtmlStringRGB(color);
        return text.ToColor(colorHexText);
    }

    private static string ToColor(this string text, string colorHtmlText)
    {
#if UNITY_EDITOR
        text = $"<color=#{colorHtmlText}>{text}</color>";
#endif
        return text;
    }
}