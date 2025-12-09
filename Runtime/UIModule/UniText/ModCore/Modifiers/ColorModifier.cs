using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор цвета текста.
/// Подписывается на OnGlyph и применяет цвет к последним 4 вершинам.
/// Данные хранятся в статическом буфере.
/// </summary>
[Serializable]
public class ColorModifier : IRenderModifier
{
    // Статический буфер цветов per-cluster (индексируется по codepoint)
    // Color32 packed как uint для экономии памяти, 0 = нет кастомного цвета
    // ВАЖНО: ArrayPool.Rent() НЕ гарантирует очищенный массив — очищаем сразу
    private static uint[] colorData = RentCleared(256);
    private static int capacity = 256;

    private static uint[] RentCleared(int size)
    {
        var arr = ArrayPool<uint>.Shared.Rent(size);
        arr.AsSpan(0, size).Clear();
        return arr;
    }

    void IModifier.Apply(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return;

        if (!TryParseColor(parameter, out Color32 color))
            return;

        int cpCount = SharedTextBuffers.codepointCount;

        // Ensure capacity
        if (cpCount > capacity)
        {
            var newBuffer = ArrayPool<uint>.Shared.Rent(cpCount);
            colorData.AsSpan(0, capacity).CopyTo(newBuffer);
            // ВАЖНО: очищаем новую часть буфера (ArrayPool не гарантирует нули)
            newBuffer.AsSpan(capacity, cpCount - capacity).Clear();
            ArrayPool<uint>.Shared.Return(colorData);
            colorData = newBuffer;
            capacity = cpCount;
        }

        // Clamp to valid range
        if (start < 0) start = 0;
        if (end > cpCount) end = cpCount;

        // Pack Color32 into uint with alpha in high byte to distinguish from 0
        // Format: ARGB (alpha always non-zero even for transparent colors)
        uint packed = PackColor(color);

        // Set color for range
        for (int i = start; i < end; i++)
            colorData[i] = packed;
    }

    void IModifier.Initialize(UniText uniText)
    {
        uniText.MeshGenerator.OnGlyph += OnGlyph;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        var gen = uniText.MeshGenerator;
        if (gen == null) return;
        gen.OnGlyph -= OnGlyph;
    }

    private static void OnGlyph()
    {
        int cluster = UniTextMeshGenerator.CurrentCluster;
        if (cluster < 0 || cluster >= capacity)
            return;

        uint packed = colorData[cluster];
        if (packed == 0)
            return; // No custom color

        Color32 color = UnpackColor(packed);

        // Apply color to last 4 vertices
        int baseIdx = UniTextMeshGenerator.VertexCount - 4;
        var colors = UniTextMeshGenerator.Colors;

        colors[baseIdx] = color;
        colors[baseIdx + 1] = color;
        colors[baseIdx + 2] = color;
        colors[baseIdx + 3] = color;
    }

    /// <summary>
    /// Сброс буфера. Вызывается перед новым текстом.
    /// </summary>
    public static void ResetStatic()
    {
        colorData.AsSpan(0, capacity).Clear();
    }

    void IModifier.Reset() => ResetStatic();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PackColor(Color32 c)
    {
        // Pack as ARGB, ensure alpha is at least 1 to distinguish from "no color"
        byte a = c.a == 0 ? (byte)1 : c.a;
        return ((uint)a << 24) | ((uint)c.r << 16) | ((uint)c.g << 8) | c.b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color32 UnpackColor(uint packed)
    {
        return new Color32(
            (byte)((packed >> 16) & 0xFF), // R
            (byte)((packed >> 8) & 0xFF),  // G
            (byte)(packed & 0xFF),         // B
            (byte)((packed >> 24) & 0xFF)  // A
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasColor(int cluster)
    {
        return cluster >= 0 && cluster < capacity && colorData[cluster] != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color32 GetColor(int cluster)
    {
        if (cluster < 0 || cluster >= capacity || colorData[cluster] == 0)
            return new Color32(255, 255, 255, 255);
        return UnpackColor(colorData[cluster]);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        if (colorData != null)
            ArrayPool<uint>.Shared.Return(colorData);
        colorData = ArrayPool<uint>.Shared.Rent(256);
        capacity = 256;
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
