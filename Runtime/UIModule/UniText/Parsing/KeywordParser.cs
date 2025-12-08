using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Parser that finds keywords in text and applies modifiers without removing text.
/// Useful for syntax highlighting, log coloring, etc.
/// Optimized: single pass with first-character grouping.
/// </summary>
public sealed class KeywordParser : TextParser
{
    // Keywords grouped by first character (lowercase) for O(1) lookup
    // Index 0-25 = a-z, 26-35 = 0-9, 36 = underscore, 37 = other
    private const int BucketCount = 38;
    private KeywordEntry[][] buckets = new KeywordEntry[BucketCount][];
    private int[] bucketCounts = new int[BucketCount];
    private int keywordCount;

    public override string ParserName => "KeywordParser";

    /// <summary>
    /// Case sensitivity for keyword matching.
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// If true, only match whole words (surrounded by non-word characters).
    /// </summary>
    public bool WholeWordsOnly { get; set; } = false;

    /// <summary>
    /// Register a keyword to find and associate with a modifier.
    /// </summary>
    public void RegisterKeyword(string keyword, int modifierId)
    {
        if (string.IsNullOrEmpty(keyword))
            return;

        int bucket = GetBucketIndex(keyword[0]);

        // Ensure bucket capacity
        if (buckets[bucket] == null)
            buckets[bucket] = new KeywordEntry[4];
        else if (bucketCounts[bucket] >= buckets[bucket].Length)
        {
            var newArr = new KeywordEntry[buckets[bucket].Length * 2];
            Array.Copy(buckets[bucket], newArr, buckets[bucket].Length);
            buckets[bucket] = newArr;
        }

        buckets[bucket][bucketCounts[bucket]++] = new KeywordEntry
        {
            keyword = keyword,
            modifierId = modifierId
        };
        keywordCount++;

        Log($"Registered keyword '{keyword}' -> modifier {modifierId} (bucket {bucket})");
    }

    /// <summary>
    /// Remove all registered keywords.
    /// </summary>
    public void ClearKeywords()
    {
        for (int i = 0; i < BucketCount; i++)
            bucketCounts[i] = 0;
        keywordCount = 0;
    }

    public override ParseResult Parse(
        ReadOnlySpan<char> input,
        Span<ParsedSpan> spanBuffer,
        Span<char> charBuffer)
    {
        if (input.IsEmpty || keywordCount == 0)
        {
            return new ParseResult
            {
                displayText = input,
                originalText = input,
                spans = ReadOnlySpan<ParsedSpan>.Empty,
                textModified = false
            };
        }

        Log($"Parsing input ({input.Length} chars) for {keywordCount} keywords");

        int spanCount = 0;

        // Single pass through text
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            int bucket = GetBucketIndex(c);

            var bucketArr = buckets[bucket];
            if (bucketArr == null)
                continue;

            int count = bucketCounts[bucket];

            // Check all keywords in this bucket
            for (int k = 0; k < count; k++)
            {
                ref var entry = ref bucketArr[k];
                var keyword = entry.keyword;

                // Quick length check
                if (i + keyword.Length > input.Length)
                    continue;

                // Compare keyword
                if (!MatchesAt(input, i, keyword))
                    continue;

                // Check whole word boundary if required
                if (WholeWordsOnly && !IsWholeWord(input, i, keyword.Length))
                    continue;

                // Add span
                if (spanCount < spanBuffer.Length)
                {
                    var span = new ParsedSpan(i, i + keyword.Length, entry.modifierId);
                    spanBuffer[spanCount++] = span;
                    LogSpan(span, input, input);
                }
            }
        }

        // Spans are already sorted by position (single pass left-to-right)
        // But overlapping keywords might need sorting by length
        if (spanCount > 1)
            SortSpans(spanBuffer.Slice(0, spanCount));

        var result = new ParseResult
        {
            displayText = input,
            originalText = input,
            spans = spanBuffer.Slice(0, spanCount),
            textModified = false
        };

        LogResult(result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBucketIndex(char c)
    {
        // Lowercase letter
        if (c >= 'a' && c <= 'z') return c - 'a';
        if (c >= 'A' && c <= 'Z') return c - 'A';
        // Digit
        if (c >= '0' && c <= '9') return 26 + (c - '0');
        // Underscore
        if (c == '_') return 36;
        // Other
        return 37;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchesAt(ReadOnlySpan<char> text, int position, string keyword)
    {
        if (CaseSensitive)
        {
            for (int i = 0; i < keyword.Length; i++)
            {
                if (text[position + i] != keyword[i])
                    return false;
            }
        }
        else
        {
            for (int i = 0; i < keyword.Length; i++)
            {
                if (char.ToLowerInvariant(text[position + i]) != char.ToLowerInvariant(keyword[i]))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the match at position is a whole word.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWholeWord(ReadOnlySpan<char> text, int position, int length)
    {
        // Check character before
        if (position > 0 && IsWordChar(text[position - 1]))
            return false;

        // Check character after
        int endPos = position + length;
        if (endPos < text.Length && IsWordChar(text[endPos]))
            return false;

        return true;
    }

    /// <summary>
    /// Check if character is a word character (letter, digit, underscore).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    /// <summary>
    /// Simple insertion sort for small span arrays.
    /// </summary>
    private static void SortSpans(Span<ParsedSpan> spans)
    {
        for (int i = 1; i < spans.Length; i++)
        {
            var current = spans[i];
            int j = i - 1;

            while (j >= 0 && spans[j].start > current.start)
            {
                spans[j + 1] = spans[j];
                j--;
            }

            spans[j + 1] = current;
        }
    }

    private struct KeywordEntry
    {
        public string keyword;
        public int modifierId;
    }
}
