using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Italic текста.
/// Помечает глифы как italic. Подписывается на OnGlyph для применения shear к вершинам.
/// Данные хранятся в статическом буфере.
/// </summary>
[Serializable]
public class ItalicModifier : IRenderModifier
{
    // Статический буфер italic flags per-cluster (индексируется по codepoint)
    // ВАЖНО: ArrayPool.Rent() НЕ гарантирует очищенный массив — очищаем сразу
    private static float[] italicFlags = RentCleared(256);
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
            italicFlags.AsSpan(0, capacity).CopyTo(newBuffer);
            // ВАЖНО: очищаем новую часть буфера (ArrayPool не гарантирует нули)
            newBuffer.AsSpan(capacity, cpCount - capacity).Clear();
            ArrayPool<float>.Shared.Return(italicFlags);
            italicFlags = newBuffer;
            capacity = cpCount;
        }

        // Clamp to valid range
        if (start < 0) start = 0;
        if (end > cpCount) end = cpCount;

        // Set italic flag for range
        for (int i = start; i < end; i++)
            italicFlags[i] = 1f;
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
        if (cluster < 0 || cluster >= capacity || italicFlags[cluster] == 0)
            return;

        // Get italic style from current font
        float italicStyle = UniTextMeshGenerator.CurrentFontAsset?.ItalicStyle ?? 12f;
        float shearValue = italicStyle * 0.01f;

        // Get last 4 vertices
        int baseIdx = UniTextMeshGenerator.VertexCount - 4;
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

    /// <summary>
    /// Сброс буфера. Вызывается перед новым текстом.
    /// </summary>
    public static void ResetStatic()
    {
        italicFlags.AsSpan(0, capacity).Clear();
    }

    void IModifier.Reset() => ResetStatic();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItalic(int cluster)
    {
        return cluster >= 0 && cluster < capacity && italicFlags[cluster] != 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        if (italicFlags != null)
            ArrayPool<float>.Shared.Return(italicFlags);
        italicFlags = ArrayPool<float>.Shared.Rent(256);
        italicFlags.AsSpan(0, 256).Clear(); // ArrayPool doesn't guarantee zeroed arrays
        capacity = 256;
    }
}
