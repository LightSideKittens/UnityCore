using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class ColorModifier : BaseModifier
{
    private ArrayPoolBuffer<uint> instanceBuffer;
    private static ArrayPoolBuffer<uint> buffer;

    protected override void CreateBuffers()
    {
        instanceBuffer = new ArrayPoolBuffer<uint>(256);
        buffer = instanceBuffer;
    }

    protected override void Subscribe()
    {
        cachedUniText.Rebuilding += OnRebuilding;
        cachedUniText.MeshGenerator.OnGlyph += OnGlyph;
    }

    protected override void Unsubscribe()
    {
        cachedUniText.Rebuilding -= OnRebuilding;
        cachedUniText.MeshGenerator.OnGlyph -= OnGlyph;
    }

    protected override void ReleaseBuffers()
    {
        instanceBuffer.ReturnToPool();
        instanceBuffer = null;
    }

    protected override void ClearBuffers() => instanceBuffer.Clear();

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return;

        if (!TryParseColor(parameter, out Color32 color))
            return;

        int cpCount = CommonData.Current.codepointCount;
        buffer.EnsureCapacity(cpCount);

        uint packed = PackColor(color);
        buffer.SetValueRange(start, Math.Min(end, cpCount), packed);
    }

    private void OnRebuilding() => buffer = instanceBuffer;

    private static void OnGlyph()
    {
        int cluster = UniTextMeshGenerator.currentCluster;
        uint packed = buffer.GetValueOrDefault(cluster);
        if (packed == 0)
            return;

        Color32 color = UnpackColor(packed);

        int baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var colors = UniTextMeshGenerator.Colors;

        colors[baseIdx] = color;
        colors[baseIdx + 1] = color;
        colors[baseIdx + 2] = color;
        colors[baseIdx + 3] = color;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PackColor(Color32 c)
    {
        byte a = c.a == 0 ? (byte)1 : c.a;
        return ((uint)a << 24) | ((uint)c.r << 16) | ((uint)c.g << 8) | c.b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color32 UnpackColor(uint packed)
    {
        return new Color32(
            (byte)((packed >> 16) & 0xFF),
            (byte)((packed >> 8) & 0xFF),
            (byte)(packed & 0xFF),
            (byte)((packed >> 24) & 0xFF)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasColor(int cluster) => buffer != null && buffer.HasValue(cluster);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color32 GetColor(int cluster)
    {
        if (buffer == null || (uint)cluster >= (uint)buffer.Capacity)
            return new Color32(255, 255, 255, 255);
        uint packed = buffer.Data[cluster];
        if (packed == 0)
            return new Color32(255, 255, 255, 255);
        return UnpackColor(packed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetColor(int cluster, out Color32 color)
    {
        if (buffer == null || (uint)cluster >= (uint)buffer.Capacity)
        {
            color = default;
            return false;
        }
        uint packed = buffer.Data[cluster];
        if (packed == 0)
        {
            color = default;
            return false;
        }
        color = UnpackColor(packed);
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;

    private static bool TryParseColor(string value, out Color32 color)
    {
        color = new Color32(255, 255, 255, 255);
        if (string.IsNullOrEmpty(value))
            return false;
        if (value[0] == '#')
            return TryParseHexColor(value, out color);
        return TryParseNamedColor(value, out color);
    }

    private static bool TryParseHexColor(string hex, out Color32 color)
    {
        color = new Color32(255, 255, 255, 255);
        int len = hex.Length - 1;

        if (len == 3)
        {
            byte r = ParseHexDigit(hex[1]);
            byte g = ParseHexDigit(hex[2]);
            byte b = ParseHexDigit(hex[3]);
            color = new Color32((byte)(r * 17), (byte)(g * 17), (byte)(b * 17), 255);
            return true;
        }
        if (len == 6)
        {
            color = new Color32(ParseHexByte(hex[1], hex[2]), ParseHexByte(hex[3], hex[4]), ParseHexByte(hex[5], hex[6]), 255);
            return true;
        }
        if (len == 8)
        {
            color = new Color32(ParseHexByte(hex[1], hex[2]), ParseHexByte(hex[3], hex[4]), ParseHexByte(hex[5], hex[6]), ParseHexByte(hex[7], hex[8]));
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

    private static byte ParseHexByte(char high, char low) => (byte)(ParseHexDigit(high) * 16 + ParseHexDigit(low));

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
