using System;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Italic текста.
/// Помечает глифы как italic. Подписывается на OnGlyph для применения shear к вершинам.
/// </summary>
[Serializable]
public class ItalicModifier : IModifier
{
    private static ArrayPoolBuffer<float> buffer = new(256);

    void IModifier.Apply(int start, int end, string parameter)
    {
        int cpCount = SharedTextBuffers.codepointCount;
        buffer.EnsureCapacity(cpCount);
        buffer.SetValueRange(start, Math.Min(end, cpCount), 1f);
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
        int cluster = UniTextMeshGenerator.currentCluster;
        if (!buffer.HasValue(cluster))
            return;

        // Get italic style from current font
        float italicStyle = UniTextMeshGenerator.currentFontAsset?.ItalicStyle ?? 12f;
        float shearValue = italicStyle * 0.01f;

        // Get last 4 vertices
        int baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var verts = UniTextMeshGenerator.Vertices;

        // BL=0, TL=1, TR=2, BR=3
        float blY = verts[baseIdx].y;
        float tlY = verts[baseIdx + 1].y;
        float midY = (tlY + blY) * 0.5f;

        float topShearX = shearValue * (tlY - midY);
        float bottomShearX = shearValue * (blY - midY);

        // Apply shear to vertices
        verts[baseIdx].x += bottomShearX;     // BL
        verts[baseIdx + 1].x += topShearX;    // TL
        verts[baseIdx + 2].x += topShearX;    // TR
        verts[baseIdx + 3].x += bottomShearX; // BR
    }

    public static void ResetStatic() => buffer.Clear();

    void IModifier.Reset() => ResetStatic();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItalic(int cluster) => buffer.HasValue(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer.Reset();
}
