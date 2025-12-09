using System;
using UnityEngine;

/// <summary>
/// Модификатор цвета текста.
/// Использует logicalToGlyph для эффективного применения цвета к диапазону символов.
/// </summary>
[Serializable]
public sealed class ColorModifier : IRenderModifier
{
    void IModifier.Apply(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return;

        if (!TryParseColor(parameter, out Color32 color))
            return;

        var glyphs = SharedTextBuffers.positionedGlyphs;
        var map = SharedTextBuffers.logicalToGlyph;
        int cpCount = SharedTextBuffers.codepointCount;
        int glyphCount = SharedTextBuffers.positionedGlyphCount;

        // Clamp end to valid range
        if (end > cpCount) end = cpCount;

        for (int cp = start; cp < end; cp++)
        {
            if (cp < 0 || cp >= cpCount)
                continue;

            int glyphIndex = map[cp];
            if (glyphIndex < 0 || glyphIndex >= glyphCount)
                continue;

            // Окрасить основной глиф
            glyphs[glyphIndex].color = color;

            // Окрасить соседние глифы с тем же cluster (для combining marks/diacritics)
            // Глифы с одним cluster обычно расположены рядом после shaping
            for (int g = glyphIndex - 1; g >= 0 && glyphs[g].cluster == cp; g--)
                glyphs[g].color = color;
            for (int g = glyphIndex + 1; g < glyphCount && glyphs[g].cluster == cp; g++)
                glyphs[g].color = color;
        }
    }

    private static bool TryParseColor(string value, out Color32 color)
    {
        color = new Color32(255, 255, 255, 255);

        if (string.IsNullOrEmpty(value))
            return false;

        // Hex: #RGB, #RRGGBB, #RRGGBBAA
        if (value[0] == '#')
            return TryParseHexColor(value, out color);

        // Именованные цвета
        return TryParseNamedColor(value, out color);
    }

    private static bool TryParseHexColor(string hex, out Color32 color)
    {
        color = new Color32(255, 255, 255, 255);

        int len = hex.Length - 1; // Без #

        if (len == 3)
        {
            // #RGB → #RRGGBB
            byte r = ParseHexDigit(hex[1]);
            byte g = ParseHexDigit(hex[2]);
            byte b = ParseHexDigit(hex[3]);
            color = new Color32((byte)(r * 17), (byte)(g * 17), (byte)(b * 17), 255);
            return true;
        }

        if (len == 6)
        {
            // #RRGGBB
            byte r = ParseHexByte(hex[1], hex[2]);
            byte g = ParseHexByte(hex[3], hex[4]);
            byte b = ParseHexByte(hex[5], hex[6]);
            color = new Color32(r, g, b, 255);
            return true;
        }

        if (len == 8)
        {
            // #RRGGBBAA
            byte r = ParseHexByte(hex[1], hex[2]);
            byte g = ParseHexByte(hex[3], hex[4]);
            byte b = ParseHexByte(hex[5], hex[6]);
            byte a = ParseHexByte(hex[7], hex[8]);
            color = new Color32(r, g, b, a);
            return true;
        }

        return false;
    }

    private static byte ParseHexDigit(char c)
    {
        if (c >= '0' && c <= '9') return (byte)(c - '0');
        if (c >= 'a' && c <= 'f') return (byte)(c - 'a' + 10);
        if (c >= 'A' && c <= 'F') return (byte)(c - 'A' + 10);
        return 0;
    }

    private static byte ParseHexByte(char high, char low)
    {
        return (byte)(ParseHexDigit(high) * 16 + ParseHexDigit(low));
    }

    private static bool TryParseNamedColor(string name, out Color32 color)
    {
        color = name.ToLowerInvariant() switch
        {
            "white" => new Color32(255, 255, 255, 255),
            "black" => new Color32(0, 0, 0, 255),
            "red" => new Color32(255, 0, 0, 255),
            "green" => new Color32(0, 128, 0, 255),
            "blue" => new Color32(0, 0, 255, 255),
            "yellow" => new Color32(255, 255, 0, 255),
            "cyan" => new Color32(0, 255, 255, 255),
            "magenta" => new Color32(255, 0, 255, 255),
            "orange" => new Color32(255, 165, 0, 255),
            "purple" => new Color32(128, 0, 128, 255),
            "gray" or "grey" => new Color32(128, 128, 128, 255),
            "lime" => new Color32(0, 255, 0, 255),
            "brown" => new Color32(165, 42, 42, 255),
            "pink" => new Color32(255, 192, 203, 255),
            "navy" => new Color32(0, 0, 128, 255),
            "teal" => new Color32(0, 128, 128, 255),
            "olive" => new Color32(128, 128, 0, 255),
            "maroon" => new Color32(128, 0, 0, 255),
            "silver" => new Color32(192, 192, 192, 255),
            "gold" => new Color32(255, 215, 0, 255),
            _ => new Color32(255, 255, 255, 255)
        };

        return name.ToLowerInvariant() is "white" or "black" or "red" or "green" or "blue"
            or "yellow" or "cyan" or "magenta" or "orange" or "purple"
            or "gray" or "grey" or "lime" or "brown" or "pink"
            or "navy" or "teal" or "olive" or "maroon" or "silver" or "gold";
    }
}