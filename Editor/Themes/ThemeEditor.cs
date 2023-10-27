using System.IO;
using System.Text.RegularExpressions;
using DG.DemiEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Path = LSPaths;

namespace LSCore.Editor.EditorThemes
{
    internal class ThemeEditor : OdinEditorWindow
    {
        private static string path;
        private static string pathTemplate;
        [SerializeField] private Color color;
        [Range(0f, 2f)] [SerializeField] private float brightness = 1f;
        [Range(0f, 2f)] [SerializeField] private float saturation = 1f;

        [MenuItem(LSPaths.Windows.ThemeEditor)]
        private static void OpenWindow()
        {
            GetWindow<ThemeEditor>().Show();
        }

        [OnInspectorInit]
        private void OnInit()
        {
            path = (Path.Editor + "/Themes/StyleSheets/Extensions/dark.uss").ToFull();
            pathTemplate = (Path.Editor + "/Themes/dark-template.uss").ToFull();
        }
        
        public static string LoadUss()
        {
            return File.ReadAllText(pathTemplate);
        }
        
        public static void WriteUss(string ussText)
        {
            File.WriteAllText(path, ussText);
            AssetDatabase.Refresh();
        }

        [Button]
        public void Apply()
        {
            color = color.CloneAndChangeSaturation(100, 1);
            color = color.CloneAndChangeBrightness(100, 1);
            Color.RGBToHSV(color, out var hue, out _, out _);
            var uss = LoadUss();
            var rgbs = Regex.Matches(uss, @"rgb(a)?\((\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(,\s*(\d+)\s*)?\)\s*;");

            foreach (Match match in rgbs)
            {
                var r = byte.Parse(match.Groups[2].Value);
                var g = byte.Parse(match.Groups[3].Value);
                var b = byte.Parse(match.Groups[4].Value);
                var hasAlpha = byte.TryParse(match.Groups[6].Value, out var a);
                a = hasAlpha ? a : (byte)1;
                
                var color = new Color32(r, g, b, a);
                SetHue(ref color, hue);
                uss = uss.Replace(match.Value, $"rgba({color.r}, {color.g}, {color.b}, {a});");
            }

            WriteUss(uss);
        }

        private void SetHue(ref Color32 color, float hue)
        {
            Color.RGBToHSV(color, out _, out var s, out var v);
            color = Color.HSVToRGB(hue, s * saturation, v * brightness);
        }
    }
}