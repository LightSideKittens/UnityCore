//-----------------------------------------------------------------------
// <copyright file="GUITextureDrawingUtil.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Utilities.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    public static class GUITextureDrawingUtil
    {
        private static Material material;
        private static float _greyScale;
        private static Color _guiColor;
        private static Vector4 _uv;
        private static Color _hueColor;

        private const string SHADER_NAME = "Hidden/Sirenix/OdinGUIShader";

        public static Material Material
        {
            get
            {
                if (material == null)
                {
                    const string key = "odin_gui_material1";
                    int matInstanceId = SessionState.GetInt(key, 0); // survive assembly reloads.

                    if (matInstanceId != 0)
                    {
                        material = EditorUtility.InstanceIDToObject(matInstanceId) as Material;
                    }

                    if (material == null)
                    {
                        var s = Shader.Find(SHADER_NAME);
								
                        material = new Material(s);

                        UnityEngine.Object.DontDestroyOnLoad(material);
                        material.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        SessionState.SetInt(key, material.GetInstanceID());
                    }

                    // Note that we are using global shader properties because changing
                    // shader properties of a material instance, causes the scene view to repaint.
                    Shader.SetGlobalColor("_SirenixOdin_GUIColor", new Color(1, 1, 1, 1));
                    Shader.SetGlobalColor("_SirenixOdin_HueColor", new Color(1, 1, 1, 0));
                    Shader.SetGlobalVector("_SirenixOdin_GUIUv", new Vector4(0, 0, 1, 1));
                    Shader.SetGlobalFloat("_SirenixOdin_GreyScale", 0);
                    Shader.SetGlobalFloat("_SirenixOdin_GreyScale", 0);
                    _uv = new Vector4(0, 0, 1, 1);
                    _hueColor = new Color(1, 1, 1, 0);
                    _greyScale = 0f;
                    _guiColor = new Color(1, 1, 1, 1);
                }

                return material;
            }
        }

        internal static Rect CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, out Rect uvRect)
        {
            float num = position.width / position.height;
            switch (scaleMode)
            {
                case ScaleMode.StretchToFill:
                    uvRect = new Rect(0f, 0f, 1f, 1f);
                    return position;
                case ScaleMode.ScaleAndCrop:
                    if (num > imageAspect)
                    {
                        float num4 = imageAspect / num;
                        uvRect = new Rect(0f, (1f - num4) * 0.5f, 1f, num4);
                    }
                    else
                    {
                        float num5 = num / imageAspect;
                        uvRect = new Rect(0.5f - num5 * 0.5f, 0f, num5, 1f);
                    }
                    return position;
                case ScaleMode.ScaleToFit:
                    if (num > imageAspect)
                    {
                        float num2 = imageAspect / num;
                        uvRect = new Rect(0f, 0f, 1f, 1f);
                        return new Rect(position.xMin + position.width * (1f - num2) * 0.5f, position.yMin, num2 * position.width, position.height);
                    }
                    else
                    {
                        float num3 = num / imageAspect;
                        uvRect = new Rect(0f, 0f, 1f, 1f);
                        return new Rect(position.xMin, position.yMin + position.height * (1f - num3) * 0.5f, position.width, num3 * position.height);
                    }
            }

            throw new NotImplementedException();
        }

        public static void DrawTexture(Rect rect, Texture2D texture, ScaleMode scaleMode, Color color, Color hueColor, float greyScale = 1)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (rect.width <= 0 || rect.height <= 0)
                return;

            rect = CalculateScaledTextureRects(rect, scaleMode, (float)texture.width / texture.height, out var uvRect);

            var unclipped = GUIClipInfo.Unclip(rect);
            var clipRect = GUIClipInfo.TopMostRect;

            if (!unclipped.Overlaps(clipRect))
                return;

            var uvXOrig = uvRect.x;
            var uvYOrig = uvRect.y;
            var uvWidthOrig = uvRect.width;
            var uvHeightOrig = uvRect.height;

            var uvX = uvXOrig;
            var uvY = uvYOrig;
            var uvWidth = uvWidthOrig;
            var uvHeight = uvHeightOrig;
            var clipped = rect;

            var clipTop = clipRect.y - unclipped.y;
            if (clipTop > 0)
            {
                var amount = clipTop / rect.height;
                uvY += uvHeightOrig * amount;
                uvHeight -= uvHeightOrig * amount;
                clipped.y += clipTop;
                clipped.height -= clipTop;
            }

            var clipBottom = unclipped.yMax - clipRect.yMax;
            if (clipBottom > 0)
            {
                var amount = clipBottom / rect.height;
                uvHeight -= uvHeight * amount;
                clipped.height -= clipBottom;
            }

            var clipLeft = clipRect.x - unclipped.x;
            if (clipLeft > 0)
            {
                var amount = clipLeft / rect.width;
                uvX += uvWidthOrig * amount;
                uvWidth -= uvWidthOrig * amount;
                clipped.x += clipLeft;
                clipped.width -= clipLeft;
            }

            var clipRight = unclipped.xMax - clipRect.xMax;
            if (clipRight > 0)
            {
                var amount = clipRight / rect.width;
                uvWidth -= uvWidth * amount;
                clipped.width -= clipRight;
            }

            color *= GUI.color;

            if (!GUI.enabled)
                color.a *= 0.4f;

            SetHueColor(hueColor);
            SetGUIColor(color);
            SetGreyScale(greyScale);
            SetUv(new Vector4(uvX, uvY, uvWidth, uvHeight));
            Graphics.DrawTexture(clipped, texture, Material);
        }

        public static void SetProperties(Color guiColor, Color hueColor, Vector4 uv, float greyScale)
        {
            SetGUIColor(guiColor);
            SetHueColor(hueColor);
            SetUv(uv);
            SetGreyScale(greyScale);
        }

        private static void SetGUIColor(Color color)
        {
            if (_guiColor != color)
            {
                Shader.SetGlobalColor("_SirenixOdin_GUIColor", color);
                _guiColor = color;
            }
        }

        private static void SetHueColor(Color color)
        {
            if (_hueColor != color)
            {
                Shader.SetGlobalColor("_SirenixOdin_HueColor", color);
                _hueColor = color;
            }
        }

        private static void SetGreyScale(float factor)
        {
            if (_greyScale != factor)
            {
                Shader.SetGlobalFloat("_SirenixOdin_GreyScale", factor);
                _greyScale = factor;
            }
        }

        private static void SetUv(Vector4 uv)
        {
            if (_uv != uv)
            {
                Shader.SetGlobalVector("_SirenixOdin_GUIUv", uv);
                _uv = uv;
            }
        }
    }
}
#endif