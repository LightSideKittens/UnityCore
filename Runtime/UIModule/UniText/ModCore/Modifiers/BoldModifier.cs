using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class BoldModifier : GlyphModifier<byte>
{
    private static ArrayPoolBuffer<byte> buffer;

    protected override Action GetOnGlyphCallback() => OnGlyph;
    protected override void SetStaticBuffer(ArrayPoolBuffer<byte> buf) => buffer = buf;

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        int cpCount = CommonData.Current.codepointCount;
        buffer.EnsureCapacity(cpCount);
        buffer.SetFlagRange(start, Math.Min(end, cpCount));
    }

    private static void OnGlyph()
    {
        int cluster = UniTextMeshGenerator.currentCluster;
        if (!buffer.HasFlag(cluster))
            return;

        float negXScale = -UniTextMeshGenerator.xScale;
        int baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var uvs = UniTextMeshGenerator.Uvs0;

        uvs[baseIdx].w = negXScale;
        uvs[baseIdx + 1].w = negXScale;
        uvs[baseIdx + 2].w = negXScale;
        uvs[baseIdx + 3].w = negXScale;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBold(int cluster) => buffer != null && buffer.HasFlag(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;
}
