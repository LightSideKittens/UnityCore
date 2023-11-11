using System;
using System.Reflection;
using UnityEngine;

namespace UnityToolbarExtender
{
    public class LSColorPicker
    {
        private static Type type;
        private static MethodInfo show;
        private static PropertyInfo color;

        public static Color Color
        {
            get => (Color)color.GetValue(null);
            set => color.SetValue(null, value);
        }
        
        static LSColorPicker()
        {
            type = Type.GetType("UnityEditor.ColorPicker,UnityEditor");
            show = type.GetMethod("Show", BindingFlags.Static | BindingFlags.NonPublic);
            color = type.GetProperty("color", BindingFlags.Static | BindingFlags.Public);
        }

        public static void Show(
            Action<Color> colorChangedCallback,
            Color col,
            bool showAlpha = true,
            bool hdr = false)
        {
            show.Invoke(null, new []{LSGUIView.Current, colorChangedCallback, col, showAlpha, hdr});
        }
    }
}