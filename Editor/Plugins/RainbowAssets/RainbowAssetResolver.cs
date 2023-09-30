using UnityEditor;

[InitializeOnLoad]
public static class RainbowAssetResolver
{
    static RainbowAssetResolver()
    {
        EditorPrefs.SetString("Borodar.RainbowFolders.HomeFolder." +  Borodar.RainbowFolders.ProjectEditorUtility.ProjectName, $"Assets/{LSConsts.Path.Root}/Editor/Plugins/RainbowAssets/RainbowFolders/");
    }
}