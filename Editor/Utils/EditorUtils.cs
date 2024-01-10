using DG.DemiEditor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public static class EditorUtils
{
    public struct ColorData
    {
        public Color color;
        public bool isDark;

        public ColorData(Color color, float brightFactor = 0.8f, float saturationFactor = 0.5f)
        {
            this.color = color.CloneAndChangeSaturation(saturationFactor);
            this.color = this.color.CloneAndChangeBrightness(brightFactor);
            this.isDark = this.color.grayscale <= 0.5f;
        }
    }

    public static ColorData[] Colors { get; } = new[]
    {
        new ColorData(Color.red),
        new ColorData(Color.yellow),
        new ColorData(Color.blue),
        new ColorData(Color.green),
        new ColorData(Color.cyan),
        new ColorData(Color.magenta, 1, 0.3f),
        new ColorData(Color.gray),
        new ColorData(Color.white),
    };

    public static ColorData GetColorData(int index)
    {
        return Colors[index % Colors.Length];
    }

    public static SerializedProperty FindBackingField(this SerializedObject obj, string name) => obj.FindProperty($"<{name}>k__BackingField");

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
    
    public static void SetSirenixButtonWhiteColor()
    {
        var tex = GetTextureByColor(Color.white);
        var grayTex = GetTextureByColor(new Color(0.83f, 0.83f, 0.83f));
        var gray2Tex = GetTextureByColor(new Color(0.71f, 0.71f, 0.71f));
        var textColor = new Color(0.17f, 0.17f, 0.17f);

        var normal = SirenixGUIStyles.Button.normal;
        normal.textColor = textColor;
        normal.background = tex;

        var hover = SirenixGUIStyles.Button.hover;
        hover.textColor = textColor;
        hover.background = grayTex;

        var active = SirenixGUIStyles.Button.active;
        active.textColor = textColor;
        active.background = gray2Tex;
    }
}
