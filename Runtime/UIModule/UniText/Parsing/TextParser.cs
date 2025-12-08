using System;

/// <summary>
/// Abstract base class for text parsers.
/// Parsers find ranges in text and associate them with modifiers.
/// Each implementation defines its own rules for finding ranges.
/// </summary>
public abstract class TextParser
{
    /// <summary>
    /// Enable debug logging for this parser.
    /// </summary>
    public bool DebugLogging { get; set; }

    /// <summary>
    /// Parser name for logging.
    /// </summary>
    public abstract string ParserName { get; }

    /// <summary>
    /// Parse text and return spans with modifier references.
    /// Uses automatic buffers from ParseBuffers.
    /// </summary>
    public ParseResult Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
        {
            return new ParseResult
            {
                displayText = input,
                originalText = input,
                spans = ReadOnlySpan<ParsedSpan>.Empty,
                textModified = false
            };
        }

        // Estimate buffer sizes
        int estimatedSpans = Math.Max(16, input.Length / 20);  // ~5% of text might be tags
        int charBufferSize = input.Length;

        var spanBuffer = ParseBuffers.GetSpanBuffer(estimatedSpans);
        var charBuffer = ParseBuffers.GetCharBuffer(charBufferSize);

        return Parse(input, spanBuffer, charBuffer);
    }

    /// <summary>
    /// Parse text using provided buffers for zero-allocation.
    /// </summary>
    /// <param name="input">Input text to parse</param>
    /// <param name="spanBuffer">Buffer for storing parsed spans</param>
    /// <param name="charBuffer">Buffer for modified display text (if parser removes markup)</param>
    /// <returns>Parse result with slices of the buffers</returns>
    public abstract ParseResult Parse(
        ReadOnlySpan<char> input,
        Span<ParsedSpan> spanBuffer,
        Span<char> charBuffer);

    /// <summary>
    /// Log debug message if logging is enabled.
    /// </summary>
    protected void Log(string message)
    {
        if (DebugLogging)
        {
            UnityEngine.Debug.Log($"[{ParserName}] {message}");
        }
    }

    /// <summary>
    /// Log a parsed span if logging is enabled.
    /// </summary>
    protected void LogSpan(in ParsedSpan span, ReadOnlySpan<char> displayText, ReadOnlySpan<char> originalText)
    {
        if (!DebugLogging) return;

        var content = span.GetContent(displayText);
        var value = span.GetValue(originalText);

        string contentStr = content.Length > 30
            ? $"{content.Slice(0, 27).ToString()}..."
            : content.ToString();

        string valueStr = value.IsEmpty
            ? "none"
            : (value.Length > 20 ? $"{value.Slice(0, 17).ToString()}..." : value.ToString());

        UnityEngine.Debug.Log($"[{ParserName}] Span: [{span.start}..{span.end}] mod={span.modifierId} content=\"{contentStr}\" value=\"{valueStr}\"");
    }

    /// <summary>
    /// Log parse result summary.
    /// </summary>
    protected void LogResult(in ParseResult result)
    {
        if (!DebugLogging) return;

        string displayPreview = result.displayText.Length > 50
            ? $"{result.displayText.Slice(0, 47).ToString()}..."
            : result.displayText.ToString();

        UnityEngine.Debug.Log($"[{ParserName}] Result: {result.SpanCount} spans, textModified={result.textModified}, display=\"{displayPreview}\"");
    }
}
