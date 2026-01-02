using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class BoldModifier : GlyphModifier<byte>
{
    [ThreadStatic] private static byte[] buffer;

    protected override string AttributeKey => AttributeKeys.Bold;

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
        EnsureBufferCount(cpCount);
        buffer.SetFlagRange(start, Math.Min(end, cpCount));
    }

    private static void OnGlyph()
    {
        var gen = UniTextMeshGenerator.Current;
        var cluster = gen.currentCluster;
        if (!buffer.HasFlag(cluster))
            return;

        var negXScale = -gen.xScale;
        var baseIdx = gen.vertexCount - 4;
        var uvs = gen.Uvs0;

        uvs[baseIdx].w = negXScale;
        uvs[baseIdx + 1].w = negXScale;
        uvs[baseIdx + 2].w = negXScale;
        uvs[baseIdx + 3].w = negXScale;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBold(int cluster)
    {
        return buffer.HasFlag(cluster);
    }
}
