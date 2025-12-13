using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class BoldModifier : GlyphModifier<float>
{
    private static ArrayPoolBuffer<float> buffer;

    protected override Action GetOnGlyphCallback() => OnGlyph;
    protected override void SetStaticBuffer(ArrayPoolBuffer<float> buf) => buffer = buf;

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        int cpCount = CommonData.Current.codepointCount;
        buffer.EnsureCapacity(cpCount);
        buffer.SetValueRange(start, Math.Min(end, cpCount), 1f);
    }

    private static void OnGlyph()
    {
        int cluster = UniTextMeshGenerator.currentCluster;
        if (!buffer.HasValue(cluster))
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
    public static bool IsBold(int cluster) => buffer != null && buffer.HasValue(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;
}
