using UnityEditor;

[InitializeOnLoad]
public static class RainbowAssetResolver
{
    static RainbowAssetResolver()
    {
        EditorPrefs.SetString("Borodar.RainbowFolders.HomeFolder." +  Borodar.RainbowFolders.ProjectEditorUtility.ProjectName, $"Assets/{LSPaths.Root}/Editor/Plugins/RainbowAssets/RainbowFolders/");
    }
}