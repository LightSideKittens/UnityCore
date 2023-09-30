using UnityEditor;
using UnityEngine;

namespace LSCore.Editor
{
    public static class LSIcons
    {
        public static Texture2D Get(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{LSConsts.Path.Icons}/{iconName}");
        }
    }
}
