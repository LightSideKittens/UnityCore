using System;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Bold текста.
/// Устанавливает stylePadding для расширения глифа в SDF рендеринге.
/// Подписывается на OnGlyph для модификации UV.w.
/// </summary>
[Serializable]
public class BoldModifier : IModifier
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

        float negXScale = -UniTextMeshGenerator.xScale;
        int baseIdx = UniTextMeshGenerator.vertexCount - 4;
        var uvs = UniTextMeshGenerator.Uvs0;

        uvs[baseIdx].w = negXScale;
        uvs[baseIdx + 1].w = negXScale;
        uvs[baseIdx + 2].w = negXScale;
        uvs[baseIdx + 3].w = negXScale;
    }

    void IModifier.Reset() => buffer.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBold(int cluster) => buffer.HasValue(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload() => buffer = null;
}