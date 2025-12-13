using System;
using System.Buffers;
using System.Runtime.CompilerServices;

/// <summary>
/// Generic utilities for ArrayPool buffer management.
/// Eliminates code duplication in CommonData Ensure/Grow methods.
/// </summary>
public static class BufferUtils
{
    /// <summary>
    /// Ensure buffer has at least 'required' capacity. Grows if needed.
    /// </summary>
    /// <param name="buffer">Buffer reference (will be replaced if grown)</param>
    /// <param name="count">Current element count to preserve</param>
    /// <param name="required">Required capacity</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureCapacity<T>(ref T[] buffer, int count, int required)
    {
        if (buffer.Length < required)
            Grow(ref buffer, count, required);
    }

    /// <summary>
    /// Grow buffer to at least 'required' capacity, doubling current size.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Grow<T>(ref T[] buffer, int count, int required)
    {
        int newSize = Math.Max(required, buffer.Length * 2);
        var newBuffer = ArrayPool<T>.Shared.Rent(newSize);
        buffer.AsSpan(0, count).CopyTo(newBuffer);
        ArrayPool<T>.Shared.Return(buffer);
        buffer = newBuffer;
    }

    /// <summary>
    /// Grow multiple codepoint-indexed buffers together.
    /// Used when codepoints, bidiLevels, scripts, margins must stay in sync.
    /// </summary>
    public static void GrowCodepointBuffers(
        ref int[] codepoints,
        ref byte[] bidiLevels,
        ref UnicodeScript[] scripts,
        ref float[] startMargins,
        int count,
        int required)
    {
        int newSize = Math.Max(required, codepoints.Length * 2);

        var newCodepoints = ArrayPool<int>.Shared.Rent(newSize);
        codepoints.AsSpan(0, count).CopyTo(newCodepoints);
        ArrayPool<int>.Shared.Return(codepoints);
        codepoints = newCodepoints;

        var newBidiLevels = ArrayPool<byte>.Shared.Rent(newSize);
        bidiLevels.AsSpan(0, Math.Min(count, bidiLevels.Length)).CopyTo(newBidiLevels);
        ArrayPool<byte>.Shared.Return(bidiLevels);
        bidiLevels = newBidiLevels;

        var newScripts = ArrayPool<UnicodeScript>.Shared.Rent(newSize);
        scripts.AsSpan(0, Math.Min(count, scripts.Length)).CopyTo(newScripts);
        ArrayPool<UnicodeScript>.Shared.Return(scripts);
        scripts = newScripts;

        var newMargins = ArrayPool<float>.Shared.Rent(newSize);
        startMargins.AsSpan(0, Math.Min(count, startMargins.Length)).CopyTo(newMargins);
        ArrayPool<float>.Shared.Return(startMargins);
        startMargins = newMargins;
    }

    /// <summary>
    /// Grow positioned glyphs and parallel color buffer together.
    /// </summary>
    public static void GrowPositionedGlyphBuffers(
        ref PositionedGlyph[] positionedGlyphs,
        ref UnityEngine.Color32[] glyphColors,
        int count,
        int required)
    {
        int newSize = Math.Max(required, positionedGlyphs.Length * 2);

        var newGlyphs = ArrayPool<PositionedGlyph>.Shared.Rent(newSize);
        positionedGlyphs.AsSpan(0, count).CopyTo(newGlyphs);
        ArrayPool<PositionedGlyph>.Shared.Return(positionedGlyphs);
        positionedGlyphs = newGlyphs;

        var newColors = ArrayPool<UnityEngine.Color32>.Shared.Rent(newSize);
        glyphColors.AsSpan(0, count).CopyTo(newColors);
        ArrayPool<UnityEngine.Color32>.Shared.Return(glyphColors);
        glyphColors = newColors;
    }
}
