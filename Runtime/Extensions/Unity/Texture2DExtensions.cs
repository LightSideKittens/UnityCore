using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Texture2DExtensions
    {
        public static Vector2 Size(this Texture2D texture) => new Vector2(texture.width, texture.height);
        public static float AspectRatio(this Texture2D texture) => (float)texture.width / texture.height;
    }
}