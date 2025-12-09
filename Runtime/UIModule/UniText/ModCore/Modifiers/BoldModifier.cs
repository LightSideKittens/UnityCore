using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Bold текста.
/// Устанавливает stylePadding для расширения глифа в SDF рендеринге.
/// Данные хранятся в статическом буфере. Подписывается на OnGlyph для модификации UV.w.
/// </summary>
[Serializable]
public class BoldModifier : IRenderModifier
{
    // Статический буфер stylePadding per-cluster (индексируется по codepoint)
    // ВАЖНО: ArrayPool.Rent() НЕ гарантирует очищенный массив — очищаем сразу
    private static float[] stylePadding = RentCleared(256);
    private static int capacity = 256;

    private static float[] RentCleared(int size)
    {
        var arr = ArrayPool<float>.Shared.Rent(size);
        arr.AsSpan(0, size).Clear();
        return arr;
    }

    void IModifier.Apply(int start, int end, string parameter)
    {
        int cpCount = SharedTextBuffers.codepointCount;

        // Ensure capacity
        if (cpCount > capacity)
        {
            var newBuffer = ArrayPool<float>.Shared.Rent(cpCount);
            stylePadding.AsSpan(0, capacity).CopyTo(newBuffer);
            // ВАЖНО: очищаем новую часть буфера (ArrayPool не гарантирует нули)
            newBuffer.AsSpan(capacity, cpCount - capacity).Clear();
            ArrayPool<float>.Shared.Return(stylePadding);
            stylePadding = newBuffer;
            capacity = cpCount;
        }

        // Clamp to valid range
        if (start < 0) start = 0;
        if (end > cpCount) end = cpCount;

        // Set bold flag for range
        for (int i = start; i < end; i++)
            stylePadding[i] = 1f; // any non-zero value = bold
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
        int cluster = UniTextMeshGenerator.CurrentCluster;
        if (cluster < 0 || cluster >= capacity || stylePadding[cluster] == 0)
            return;

        // Bold: set UV.w to negative xScale for last 4 vertices
        float negXScale = -UniTextMeshGenerator.XScale;
        int baseIdx = UniTextMeshGenerator.VertexCount - 4;
        var uvs = UniTextMeshGenerator.Uvs0;

        uvs[baseIdx].w = negXScale;
        uvs[baseIdx + 1].w = negXScale;
        uvs[baseIdx + 2].w = negXScale;
        uvs[baseIdx + 3].w = negXScale;
    }

    /// <summary>
    /// Сброс буфера. Вызывается перед новым текстом.
    /// </summary>
    public static void ResetStatic()
    {
        stylePadding.AsSpan(0, capacity).Clear();
    }

    void IModifier.Reset() => ResetStatic();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBold(int cluster)
    {
        return cluster >= 0 && cluster < capacity && stylePadding[cluster] != 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        if (stylePadding != null)
            ArrayPool<float>.Shared.Return(stylePadding);
        stylePadding = ArrayPool<float>.Shared.Rent(256);
        stylePadding.AsSpan(0, 256).Clear(); // ArrayPool doesn't guarantee zeroed arrays
        capacity = 256;
    }
}
