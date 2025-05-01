//-----------------------------------------------------------------------
// <copyright file="TextureUtilities.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Collection of texture functions.
    /// </summary>
    public static class TextureUtilities
    {
        private static Material extractSpriteMaterial;

        private const string SHADER_NAME = "Hidden/Sirenix/Editor/ExtractSprite";

        /// <summary>
        /// Creates a new texture with no mimapping, linier colors, and calls texture.LoadImage(bytes), DontDestroyOnLoad(tex) and sets hideFlags = DontUnloadUnusedAsset | DontSaveInEditor.
        /// 
        /// Old description no longer relevant as we've moved past version 2017.
        /// Loads an image from bytes with the specified width and height. Use this instead of someTexture.LoadImage() if you're compiling to an assembly. Unity has moved the method in 2017, 
        /// and Unity's assembly updater is not able to fix it for you. This searches for a proper LoadImage method in multiple locations, and also handles type name conflicts.
        /// </summary>
        public static Texture2D LoadImage(int width, int height, byte[] bytes)
        {
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            tex.LoadImage(bytes);
            UnityEngine.Object.DontDestroyOnLoad(tex);
            tex.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSaveInEditor;
            return tex;
        }


        /// <summary>
        /// Crops a Texture2D into a new Texture2D.
        /// </summary>
        public static Texture2D CropTexture(this Texture texture, Rect source)
        {
            RenderTexture prev = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8);
            RenderTexture.active = rt;
            var prevsRGB = GL.sRGBWrite;
            GL.sRGBWrite = false;
            GL.Clear(false, true, new Color(1, 1, 1, 0));
            Graphics.Blit(texture, rt);
            Texture2D clone = new Texture2D((int)source.width, (int)source.height, TextureFormat.ARGB32, true, false);
            clone.filterMode = FilterMode.Point;
            clone.ReadPixels(source, 0, 0);
            clone.Apply();
            GL.sRGBWrite = prevsRGB;
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return clone;
        }
        
        /// <summary>
        /// Resizes a texture by blitting, this allows you to resize unreadable textures.
        /// </summary>
        public static Texture2D ResizeByBlit(this Texture texture, int width, int height, FilterMode filterMode = FilterMode.Bilinear)
        {
            RenderTexture prev = RenderTexture.active;
            var           rt   = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            rt.filterMode        = FilterMode.Bilinear;
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(1, 1, 1, 0));
            var prevSRGB = GL.sRGBWrite;
            GL.sRGBWrite = false;
            Graphics.Blit(texture, rt);
            Texture2D clone = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, true, false);
            clone.filterMode = filterMode;
            clone.ReadPixels(new Rect(0,0, width, height), 0, 0);
            clone.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            GL.sRGBWrite = prevSRGB;
            return clone;
        }


        /// <summary>
        /// Converts a Sprite to a Texture2D.
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static Texture2D ConvertSpriteToTexture(Sprite sprite)
        {
            var rect = sprite.rect;

            if (extractSpriteMaterial == null || extractSpriteMaterial.shader == null)
            {
                extractSpriteMaterial = new Material(Shader.Find(SHADER_NAME));
            }

            extractSpriteMaterial.SetVector("_TexelSize", new Vector2(1f / sprite.texture.width, 1f / sprite.texture.height));
            extractSpriteMaterial.SetVector("_Rect", new Vector4(
                rect.x / sprite.texture.width,
                rect.y / sprite.texture.height,
                rect.width / sprite.texture.width,
                rect.height / sprite.texture.height
            ));

            var prevSRGB = GL.sRGBWrite;
            GL.sRGBWrite = false;
            RenderTexture prev = RenderTexture.active;
            var rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 0);
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(1, 1, 1, 0));
            Graphics.Blit(sprite.texture, rt, extractSpriteMaterial);
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.alphaIsTransparency = true;
            texture.Apply();
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = prev;
            GL.sRGBWrite = prevSRGB;
            return texture;
        }
    }
}
#endif