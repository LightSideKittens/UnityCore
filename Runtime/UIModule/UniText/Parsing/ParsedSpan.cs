using System;

/// <summary>
/// A parsed span representing a range in text with an associated modifier.
/// Compact struct for zero-allocation parsing.
/// </summary>
public readonly struct ParsedSpan : IEquatable<ParsedSpan>
{
    /// <summary>
    /// Start index in display text (after markup removal).
    /// </summary>
    public readonly int start;

    /// <summary>
    /// End index in display text (exclusive).
    /// </summary>
    public readonly int end;

    /// <summary>
    /// Modifier ID from ModifierRegistry.
    /// </summary>
    public readonly int modifierId;

    /// <summary>
    /// Start of value in original text (for parsing "color=#FF0000" -> value starts at #).
    /// </summary>
    public readonly int valueStart;

    /// <summary>
    /// Length of value in original text. 0 if no value.
    /// </summary>
    public readonly int valueLength;

    public ParsedSpan(int start, int end, int modifierId, int valueStart = 0, int valueLength = 0)
    {
        this.start = start;
        this.end = end;
        this.modifierId = modifierId;
        this.valueStart = valueStart;
        this.valueLength = valueLength;
    }

    /// <summary>
    /// Length of the span.
    /// </summary>
    public int Length => end - start;

    /// <summary>
    /// Whether this span has a value.
    /// </summary>
    public bool HasValue => valueLength > 0;

    /// <summary>
    /// Get the Range representation.
    /// </summary>
    public Range Range => start..end;

    /// <summary>
    /// Get the value from original text.
    /// </summary>
    public ReadOnlySpan<char> GetValue(ReadOnlySpan<char> originalText)
    {
        if (valueLength <= 0 || valueStart < 0 || valueStart + valueLength > originalText.Length)
            return ReadOnlySpan<char>.Empty;
        return originalText.Slice(valueStart, valueLength);
    }

    /// <summary>
    /// Get the text content this span covers.
    /// </summary>
    public ReadOnlySpan<char> GetContent(ReadOnlySpan<char> displayText)
    {
        if (start < 0 || end > displayText.Length || start >= end)
            return ReadOnlySpan<char>.Empty;
        return displayText.Slice(start, end - start);
    }

    public bool Equals(ParsedSpan other)
    {
        return start == other.start &&
               end == other.end &&
               modifierId == other.modifierId &&
               valueStart == other.valueStart &&
               valueLength == other.valueLength;
    }

    public override bool Equals(object obj) => obj is ParsedSpan other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(start, end, modifierId, valueStart, valueLength);

    public override string ToString()
    {
        if (HasValue)
            return $"ParsedSpan[{start}..{end}, mod={modifierId}, value@{valueStart}:{valueLength}]";
        return $"ParsedSpan[{start}..{end}, mod={modifierId}]";
    }

    public static bool operator ==(ParsedSpan left, ParsedSpan right) => left.Equals(right);
    public static bool operator !=(ParsedSpan left, ParsedSpan right) => !left.Equals(right);
}

/// <summary>
/// Result of text parsing. Ref struct for zero-allocation.
/// </summary>
public ref struct ParseResult
{
    /// <summary>
    /// Text for display (may be original or with markup removed).
    /// </summary>
    public ReadOnlySpan<char> displayText;

    /// <summary>
    /// Original input text (for extracting values).
    /// </summary>
    public ReadOnlySpan<char> originalText;

    /// <summary>
    /// Parsed spans with modifier references.
    /// </summary>
    public ReadOnlySpan<ParsedSpan> spans;

    /// <summary>
    /// True if displayText differs from originalText.
    /// </summary>
    public bool textModified;

    /// <summary>
    /// Number of spans found.
    /// </summary>
    public int SpanCount => spans.Length;

    /// <summary>
    /// Check if parsing found any spans.
    /// </summary>
    public bool HasSpans => spans.Length > 0;
}
