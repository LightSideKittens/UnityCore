using UnityEditor;
using UnityEngine;

public static class LSIcons
{
    public static Texture2D Get(string iconName, string extension = "png")
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{LSPaths.Icons}/{iconName}.{extension}");
    }
    
    public static Sprite GetSprite(string iconName, string extension = "png")
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{LSPaths.Icons}/{iconName}.{extension}");
    }
}
