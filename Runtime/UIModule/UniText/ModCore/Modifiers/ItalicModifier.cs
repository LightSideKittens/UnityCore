using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class ItalicModifier : GlyphModifier<float>
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

        float italicStyle = UniTextMeshGenerator.currentFontAsset?.ItalicStyle ?? 12f;
        float shearValue = italicStyle * 0.01f;

        int baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var verts = UniTextMeshGenerator.Vertices;

        float blY = verts[baseIdx].y;
        float tlY = verts[baseIdx + 1].y;
        float midY = (tlY + blY) * 0.5f;

        float topShearX = shearValue * (tlY - midY);
        float bottomShearX = shearValue * (blY - midY);

        verts[baseIdx].x += bottomShearX;
        verts[baseIdx + 1].x += topShearX;
        verts[baseIdx + 2].x += topShearX;
        verts[baseIdx + 3].x += bottomShearX;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItalic(int cluster) => buffer != null && buffer.HasValue(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;
}
