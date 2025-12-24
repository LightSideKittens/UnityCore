using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class BoldModifier : GlyphModifier<byte>
{
    private static byte[] buffer;

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
        EnsureBufferCapacity(cpCount);
        buffer.SetFlagRange(start, Math.Min(end, cpCount));
    }

    private static void OnGlyph()
    {
        var cluster = UniTextMeshGenerator.currentCluster;
        if (!buffer.HasFlag(cluster))
            return;

        var negXScale = -UniTextMeshGenerator.xScale;
        var baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var uvs = UniTextMeshGenerator.Uvs0;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        buffer = null;
    }
}
