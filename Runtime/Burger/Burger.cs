using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class Burger
{
    [Conditional("DEBUG")]
    public static void Log(object log) => Debug.Log(log);

    [Conditional("DEBUG")]
    public static void Error(object log) => Debug.LogError(log);

    [Conditional("DEBUG")]
    public static void Warning(object log) => Debug.LogWarning(log);
}