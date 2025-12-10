using System;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Bold текста.
/// Устанавливает stylePadding для расширения глифа в SDF рендеринге.
/// Подписывается на OnGlyph для модификации UV.w.
/// </summary>
[Serializable]
public class BoldModifier : IRenderModifier
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
        // Use extension method for fast check (no EqualityComparer overhead)
        if (!ArrayPoolBufferFloatExtensions.HasValue(ref buffer, cluster))
            return;

        // Bold: set UV.w to negative xScale for last 4 vertices
        float negXScale = -UniTextMeshGenerator.xScale;
        int baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var uvs = UniTextMeshGenerator.Uvs0;

        uvs[baseIdx].w = negXScale;
        uvs[baseIdx + 1].w = negXScale;
        uvs[baseIdx + 2].w = negXScale;
        uvs[baseIdx + 3].w = negXScale;
    }

    public static void ResetStatic() => buffer.Clear();

    void IModifier.Reset() => ResetStatic();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBold(int cluster) => ArrayPoolBufferFloatExtensions.HasValue(ref buffer, cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer.Reset();
}
