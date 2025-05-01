//-----------------------------------------------------------------------
// <copyright file="LazyEditorIcon.cs" company="Sirenix ApS">
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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Lazy loading Editor Icon.
    /// </summary>
    public class LazyEditorIcon : EditorIcon, IDisposable
    {
        private static Color inactiveColorPro = new Color(0.40f, 0.40f, 0.40f, 1);
        private static Color activeColorPro = new Color(0.55f, 0.55f, 0.55f, 1);
        private static Color highlightedColorPro = new Color(0.90f, 0.90f, 0.90f, 1);

        private static Color inactiveColor = new Color(0.72f, 0.72f, 0.72f, 1);
        private static Color activeColor = new Color(0.40f, 0.40f, 0.40f, 1);
        private static Color highlightedColor = new Color(0.20f, 0.20f, 0.20f, 1);

        private static Material iconMat;

        private Texture2D icon;
        private Texture inactive;
        private Texture active;
        private Texture highlighted;
        private string data;
        private int width;
        private int height;

        /// <summary>
        /// Loads an EditorIcon from the spritesheet.
        /// </summary>
        public LazyEditorIcon(int width, int height, string base64ImageDataPngOrJPG)
        {
            this.width = width;
            this.height = height;
            this.data = base64ImageDataPngOrJPG;
        }

        /// <summary>
        /// Gets the icon's highlight texture.
        /// </summary>
        public override Texture Highlighted
        {
            get
            {
                if (this.highlighted == null)
                {
                    this.highlighted = this.RenderIcon(EditorGUIUtility.isProSkin ? highlightedColorPro : highlightedColor);
                }

                return this.highlighted;
            }
        }

        /// <summary>
        /// Gets the icon's active texture.
        /// </summary>
        public override Texture Active
        {
            get
            {
                if (this.active == null)
                {
                    this.active = this.RenderIcon(EditorGUIUtility.isProSkin ? activeColorPro : activeColor);
                }
                return this.active;
            }
        }

        /// <summary>
        /// Gets the icon's inactive texture.
        /// </summary>
        public override Texture Inactive
        {
            get
            {
                if (this.inactive == null)
                {
                    this.inactive = this.RenderIcon(EditorGUIUtility.isProSkin ? inactiveColorPro : inactiveColor);
                }

                return this.inactive;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override Texture2D Raw
        {
            get
            {
                if (this.icon == null)
                {
                    var bytes = Convert.FromBase64String(this.data);
                    this.icon = TextureUtilities.LoadImage(this.width, this.height, bytes);
                }

                return this.icon;
            }
        }

        public void Dispose()
        {
            if (this.icon != null) UnityEngine.Object.DestroyImmediate(this.icon);
            if (this.inactive != null) UnityEngine.Object.DestroyImmediate(this.inactive);
            if (this.active != null) UnityEngine.Object.DestroyImmediate(this.active);
            if (this.highlighted != null) UnityEngine.Object.DestroyImmediate(this.highlighted);
        }

        private Texture RenderIcon(Color color)
        {
            if (iconMat == null || iconMat.shader == null)
            {
                const string key = "odin_inspector_lazy_icons";
                int matInstanceId = SessionState.GetInt(key, 0); // survive assembly reloads.

                if (matInstanceId != 0)
                {
                    iconMat = EditorUtility.InstanceIDToObject(matInstanceId) as Material;
                }

                if (iconMat == null)
                {
                    Shader shader = Shader.Find("Hidden/Sirenix/Editor/GUIIcon");
                    
                    iconMat = new Material(shader);

                    UnityEngine.Object.DontDestroyOnLoad(iconMat);
                    iconMat.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    SessionState.SetInt(key, iconMat.GetInstanceID());
                }
            }

            iconMat.SetColor("_Color", color);

            var prevSRGB = GL.sRGBWrite;
            GL.sRGBWrite = true;
            RenderTexture prev = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(this.width, this.height, 0);
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(1, 1, 1, 0));
            Graphics.Blit(this.Raw, rt, iconMat);

            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false, true);
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