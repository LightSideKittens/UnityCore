using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class ItalicModifier : GlyphModifier<byte>
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
    public static bool IsItalic(int cluster) => buffer != null && buffer.HasFlag(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;
}
