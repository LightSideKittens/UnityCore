using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Texture2DExt
    {
        public static Vector2 Size(this Texture2D texture) => new Vector2(texture.width, texture.height);
        public static float AspectRatio(this Texture2D texture) => (float)texture.width / texture.height;
        
        public static Texture2D LoadAndLimitTexture(byte[] bytes, int maxSideLength)
        {
            Texture2D originalTex = new Texture2D(2, 2);
            originalTex.LoadImage(bytes);

            int originalWidth = originalTex.width;
            int originalHeight = originalTex.height;
            
            float scale = 1f;
            if (Mathf.Max(originalWidth, originalHeight) > maxSideLength)
            {
                scale = (float)maxSideLength / Mathf.Max(originalWidth, originalHeight);
            }

            if (scale == 1f)
            {
                return originalTex;
            }

            int newWidth = Mathf.RoundToInt(originalWidth * scale);
            int newHeight = Mathf.RoundToInt(originalHeight * scale);

            Texture2D resizedTex = new Texture2D(newWidth, newHeight, originalTex.format, false);

            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            RenderTexture.active = rt;

            Graphics.Blit(originalTex, rt);

            resizedTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resizedTex.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            Object.Destroy(originalTex);

            return resizedTex;
        }
        
        public static Texture2D GetTextureByColor(Color color, int width = 2, int height = 2)
        {
            var texture = new Texture2D(width, height);
            var pixels = width * height;
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