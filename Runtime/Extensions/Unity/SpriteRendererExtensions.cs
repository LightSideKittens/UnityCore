using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class SpriteRendererExtensions
    {
        public static void Alpha(this SpriteRenderer renderer, float alpha)
        {
            var color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}