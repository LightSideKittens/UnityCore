using UnityEditor;
using UnityEngine;

namespace LSCore.Editor
{
    public static class LSIcons
    {
        public static Texture2D Get(string iconName, string extension = "png")
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{LSPaths.Icons}/{iconName}.{extension}");
        }
    }
}
