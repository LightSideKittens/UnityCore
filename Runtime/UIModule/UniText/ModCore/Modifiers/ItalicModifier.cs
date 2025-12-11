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
    private ArrayPoolBuffer<float> instanceBuffer;
    private static ArrayPoolBuffer<float> buffer;

    void IModifier.Apply(int start, int end, string parameter)
    {
        int cpCount = SharedTextBuffers.Current.codepointCount;
        buffer.EnsureCapacity(cpCount);
        buffer.SetValueRange(start, Math.Min(end, cpCount), 1f);
    }

    void IModifier.Initialize(UniText uniText)
    {
        instanceBuffer = new ArrayPoolBuffer<float>(256);
        uniText.Rebuilding += OnRebuilding;
        uniText.MeshGenerator.OnGlyph += OnGlyph;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        uniText.Rebuilding -= OnRebuilding;
        var gen = uniText.MeshGenerator;
        if (gen != null)
            gen.OnGlyph -= OnGlyph;
        instanceBuffer?.ReturnToPool();
        instanceBuffer = null;
    }

    private void OnRebuilding()
    {
        buffer = instanceBuffer;
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

    void IModifier.Reset() => buffer.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItalic(int cluster) => buffer.HasValue(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;
}
