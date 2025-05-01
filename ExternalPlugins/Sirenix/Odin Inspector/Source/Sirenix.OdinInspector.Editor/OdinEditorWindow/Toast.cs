//-----------------------------------------------------------------------
// <copyright file="Toast.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using UnityEngine;

    public class Toast
    {
        public Rect? CurrentRect;
        public float TimePassed;
        
        public readonly ToastPosition ToastPosition;
        public readonly SdfIconType Icon;
        public readonly string Text;
        public Color Color;
        public readonly float Duration;
        
        private static GUIStyle style;
        public static GUIStyle Style => style = style ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            normal = { textColor = Color.white },
        };

        public Toast(ToastPosition position, SdfIconType icon, string text, Color color, float duration)
        {
            this.ToastPosition = position;
            this.Icon = icon;
            this.Text = text;
            this.Color = color;
            this.Duration = duration;
        }
    }

    public enum ToastPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    } 
}
#endif