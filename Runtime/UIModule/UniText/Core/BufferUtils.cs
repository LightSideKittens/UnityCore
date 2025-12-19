using System;
using System.Runtime.CompilerServices;


public static class BufferUtils
{
    /// <param name="buffer">Buffer reference (will be replaced if grown)</param>
    /// <param name="count">Current element count to preserve</param>
    /// <param name="required">Required capacity</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureCapacity<T>(ref T[] buffer, int count, int required)
    {
        if (buffer.Length < required)
            Grow(ref buffer, count, required);
    }


    public static int growCount;
    private static System.Collections.Generic.Dictionary<string, int> growByType = new();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Grow<T>(ref T[] buffer, int count, int required)
    {
        growCount++;
        var typeName = typeof(T).Name;
        growByType.TryGetValue(typeName, out var c);
        growByType[typeName] = c + 1;

        var newSize = Math.Max(required, buffer.Length * 2);
        var newBuffer = UniTextArrayPool<T>.Rent(newSize);
        buffer.AsSpan(0, count).CopyTo(newBuffer);
        UniTextArrayPool<T>.Return(buffer);
        buffer = newBuffer;
    }

    public static void LogGrowStats()
    {
        if (growCount > 0)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[BufferUtils] Total Grow: {growCount}");
            foreach (var kv in growByType)
                sb.AppendLine($"  {kv.Key}: {kv.Value}");
            UnityEngine.Debug.Log(sb.ToString());
        }

        growCount = 0;
        growByType.Clear();
    }


    public static void GrowCodepointBuffers(
        ref int[] codepoints,
        ref byte[] bidiLevels,
        ref UnicodeScript[] scripts,
        ref float[] startMargins,
        int count,
        int required)
    {
        var newSize = Math.Max(required, codepoints.Length * 2);

        var newCodepoints = UniTextArrayPool<int>.Rent(newSize);
        codepoints.AsSpan(0, count).CopyTo(newCodepoints);
        UniTextArrayPool<int>.Return(codepoints);
        codepoints = newCodepoints;

        var newBidiLevels = UniTextArrayPool<byte>.Rent(newSize);
        bidiLevels.AsSpan(0, Math.Min(count, bidiLevels.Length)).CopyTo(newBidiLevels);
        UniTextArrayPool<byte>.Return(bidiLevels);
        bidiLevels = newBidiLevels;

        var newScripts = UniTextArrayPool<UnicodeScript>.Rent(newSize);
        scripts.AsSpan(0, Math.Min(count, scripts.Length)).CopyTo(newScripts);
        UniTextArrayPool<UnicodeScript>.Return(scripts);
        scripts = newScripts;

        var newMargins = UniTextArrayPool<float>.Rent(newSize);
        startMargins.AsSpan(0, Math.Min(count, startMargins.Length)).CopyTo(newMargins);
        startMargins.AsSpan().Clear();
        UniTextArrayPool<float>.Return(startMargins);
        startMargins = newMargins;
    }


    public static void GrowPositionedGlyphBuffers(
        ref PositionedGlyph[] positionedGlyphs,
        int count,
        int required)
    {
        var newSize = Math.Max(required, positionedGlyphs.Length * 2);

        var newGlyphs = UniTextArrayPool<PositionedGlyph>.Rent(newSize);
        positionedGlyphs.AsSpan(0, count).CopyTo(newGlyphs);
        UniTextArrayPool<PositionedGlyph>.Return(positionedGlyphs);
        positionedGlyphs = newGlyphs;
    }
}