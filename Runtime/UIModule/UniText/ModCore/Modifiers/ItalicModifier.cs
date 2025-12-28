using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class ItalicModifier : GlyphModifier<byte>
{
    [ThreadStatic] private static byte[] buffer;

    protected override string AttributeKey => AttributeKeys.Italic;

    protected override Action GetOnGlyphCallback()
    {
        return OnGlyph;
    }

    protected override void SetStaticBuffer(byte[] buf)
    {
        buffer = buf;
    }

    protected override void OnApply(int start, int end, string parameter)
    {
        var cpCount = buffers.codepoints.count;
        EnsureBufferCapacity(cpCount);
        buffer.SetFlagRange(start, Math.Min(end, cpCount));
    }

    private static void OnGlyph()
    {
        var gen = UniTextMeshGenerator.Current;
        var cluster = gen.currentCluster;
        if (!buffer.HasFlag(cluster))
            return;

        var italicStyle = gen.currentFont?.ItalicStyle ?? 12f;
        var shearValue = italicStyle * 0.01f;

        var baseIdx = gen.vertexCount - 4;
        var verts = gen.Vertices;

        var blY = verts[baseIdx].y;
        var tlY = verts[baseIdx + 1].y;
        var midY = (tlY + blY) * 0.5f;

        var topShearX = shearValue * (tlY - midY);
        var bottomShearX = shearValue * (blY - midY);

        verts[baseIdx].x += bottomShearX;
        verts[baseIdx + 1].x += topShearX;
        verts[baseIdx + 2].x += topShearX;
        verts[baseIdx + 3].x += bottomShearX;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItalic(int cluster)
    {
        return buffer.HasFlag(cluster);
    }
}
