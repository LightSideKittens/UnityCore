using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class ItalicModifier : BaseModifier
{
    private ArrayPoolBuffer<float> instanceBuffer;
    private static ArrayPoolBuffer<float> buffer;

    protected override void CreateBuffers()
    {
        instanceBuffer = new ArrayPoolBuffer<float>(256);
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
        int cpCount = SharedTextBuffers.Current.codepointCount;
        buffer.EnsureCapacity(cpCount);
        buffer.SetValueRange(start, Math.Min(end, cpCount), 1f);
    }

    private void OnRebuilding() => buffer = instanceBuffer;

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
