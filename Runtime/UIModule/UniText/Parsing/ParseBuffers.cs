using System;

/// <summary>
/// Thread-local pooled buffers for zero-allocation parsing.
/// </summary>
public static class ParseBuffers
{
    [ThreadStatic] private static ParsedSpan[] spanBuffer;
    [ThreadStatic] private static char[] charBuffer;
    [ThreadStatic] private static OpenTagInfo[] tagStackBuffer;

    private const int DefaultSpanCapacity = 64;
    private const int DefaultCharCapacity = 4096;
    private const int DefaultTagStackCapacity = 32;

    /// <summary>
    /// Get or create span buffer with at least minCapacity.
    /// </summary>
    public static Span<ParsedSpan> GetSpanBuffer(int minCapacity)
    {
        if (spanBuffer == null || spanBuffer.Length < minCapacity)
        {
            int newSize = Math.Max(minCapacity, DefaultSpanCapacity);
            newSize = Math.Max(newSize, spanBuffer?.Length * 2 ?? 0);
            spanBuffer = new ParsedSpan[newSize];
        }
        return spanBuffer;
    }

    /// <summary>
    /// Get or create char buffer with at least minCapacity.
    /// </summary>
    public static Span<char> GetCharBuffer(int minCapacity)
    {
        if (charBuffer == null || charBuffer.Length < minCapacity)
        {
            int newSize = Math.Max(minCapacity, DefaultCharCapacity);
            newSize = Math.Max(newSize, charBuffer?.Length * 2 ?? 0);
            charBuffer = new char[newSize];
        }
        return charBuffer;
    }

    /// <summary>
    /// Get or create tag stack buffer for nested tag parsing.
    /// </summary>
    public static Span<OpenTagInfo> GetTagStackBuffer(int minCapacity = DefaultTagStackCapacity)
    {
        if (tagStackBuffer == null || tagStackBuffer.Length < minCapacity)
        {
            int newSize = Math.Max(minCapacity, DefaultTagStackCapacity);
            tagStackBuffer = new OpenTagInfo[newSize];
        }
        return tagStackBuffer;
    }

    /// <summary>
    /// Reset buffers (for testing/debugging).
    /// </summary>
    public static void Reset()
    {
        spanBuffer = null;
        charBuffer = null;
        tagStackBuffer = null;
    }
}

/// <summary>
/// Information about an open tag during parsing.
/// </summary>
public struct OpenTagInfo
{
    public int tagHash;
    public int modifierId;
    public int displayStart;
    public int valueStart;
    public int valueLength;
    public int originalStart;  // Position in original text for debugging
}
