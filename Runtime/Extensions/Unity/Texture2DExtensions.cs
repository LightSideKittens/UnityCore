using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Texture2DExtensions
    {
        public static Vector2 Size(this Texture2D texture) => new Vector2(texture.width, texture.height);
        public static float AspectRatio(this Texture2D texture) => (float)texture.width / texture.height;
        
        public static Texture2D GetTextureByColor(Color color)
        {
            var texture = new Texture2D(2, 2);
            var pixels = 2 * 2;
            var colors = new Color[pixels];

            for (int i = 0; i < pixels; i++)
            {
                colors[i] = color;
            }

            texture.SetPixels(colors);
            texture.Apply();
        
            return texture;
        }
    }
}