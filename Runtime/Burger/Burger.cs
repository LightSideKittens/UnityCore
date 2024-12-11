using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Burger
{
    public static bool logToFile;
    
    [Conditional("DEBUG")]
    public static void Log(object log) => Debug.Log(log);

    [Conditional("DEBUG")]
    public static void Error(object log) => Debug.LogError(log);

    [Conditional("DEBUG")]
    public static void Warning(object log) => Debug.LogWarning(log);
    
    public static string ToTag(this string text, Color color)
    {
        return ToBold(ToColor(text, color));
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

public struct DisableLog : System.IDisposable
{
    private bool lastValue;
    private ILogger logger;

    public DisableLog(ILogger logger)
    {
        lastValue = logger.logEnabled;
        logger.logEnabled = false;
        this.logger = logger;
    }
    
    public void Dispose()
    {
        logger.logEnabled = lastValue;
    }
}