using UnityEngine;

public static class ApplicationUtils
{
    public static string ProjectPath = Application.dataPath[..^7];
    public static string ProjectSettingsPath = $"{ProjectPath}/ProjectSettings";
    public static string LibraryPath = $"{ProjectPath}/Library";
}
