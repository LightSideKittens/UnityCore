using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Parser for HTML-like markup: &lt;tag&gt;content&lt;/tag&gt; or &lt;tag=value&gt;content&lt;/tag&gt;
/// Removes tags from display text and creates spans for the content.
/// </summary>
public sealed class HtmlParser : TextParser
{
    private readonly Dictionary<int, int> tagHashToModifier = new();

    public override string ParserName => "HtmlParser";

    /// <summary>
    /// Register a tag name and associate it with a modifier ID.
    /// </summary>
    /// <param name="tagName">Tag name without brackets (e.g., "b", "color")</param>
    /// <param name="modifierId">Modifier ID from ModifierRegistry</param>
    public void RegisterTag(string tagName, int modifierId)
    {
        int hash = GetTagHash(tagName.AsSpan());
        tagHashToModifier[hash] = modifierId;
        Log($"Registered tag '{tagName}' (hash={hash}) -> modifier {modifierId}");
    }

    /// <summary>
    /// Check if a tag is registered.
    /// </summary>
    public bool IsTagRegistered(ReadOnlySpan<char> tagName)
    {
        return tagHashToModifier.ContainsKey(GetTagHash(tagName));
    }

    public override ParseResult Parse(
        ReadOnlySpan<char> input,
        Span<ParsedSpan> spanBuffer,
        Span<char> charBuffer)
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

        Log($"Parsing input: {(input.Length > 50 ? input.Slice(0, 47).ToString() + "..." : input.ToString())}");

        int spanCount = 0;
        int charCount = 0;

        // Tag stack for nested tags
        var tagStack = ParseBuffers.GetTagStackBuffer();
        int stackDepth = 0;

        int i = 0;
        while (i < input.Length)
        {
            // Fast path: find next '<' using optimized search
            int nextTag = input.Slice(i).IndexOf('<');
            if (nextTag < 0)
            {
                // No more tags - copy remaining text
                var remaining = input.Slice(i);
                for (int j = 0; j < remaining.Length && charCount < charBuffer.Length; j++)
                    charBuffer[charCount++] = remaining[j];
                break;
            }

            // Copy text before tag
            if (nextTag > 0)
            {
                var textBefore = input.Slice(i, nextTag);
                for (int j = 0; j < textBefore.Length && charCount < charBuffer.Length; j++)
                    charBuffer[charCount++] = textBefore[j];
                i += nextTag;
            }

            int tagStart = i;

            // Find closing bracket
            int bracketEnd = FindClosingBracket(input, i);
            if (bracketEnd < 0)
            {
                // No closing bracket - treat as regular character
                if (charCount < charBuffer.Length)
                    charBuffer[charCount++] = input[i];
                i++;
                continue;
            }

            var tagContent = input.Slice(i + 1, bracketEnd - i - 1);

            if (tagContent.IsEmpty)
            {
                // Empty tag <> - copy as is
                if (charCount < charBuffer.Length)
                    charBuffer[charCount++] = input[i];
                i++;
                continue;
            }

            if (tagContent[0] == '/')
            {
                // Closing tag </tag>
                var closingTagName = tagContent.Slice(1).Trim();
                int closingHash = GetTagHash(closingTagName);

                Log($"Found closing tag </{closingTagName.ToString()}> at position {tagStart}");

                // Find matching opening tag in stack
                int matchIndex = -1;
                for (int s = stackDepth - 1; s >= 0; s--)
                {
                    if (tagStack[s].tagHash == closingHash)
                    {
                        matchIndex = s;
                        break;
                    }
                }

                if (matchIndex >= 0)
                {
                    ref var openTag = ref tagStack[matchIndex];

                    // Create span from opening tag's content start to current position
                    if (spanCount < spanBuffer.Length)
                    {
                        var span = new ParsedSpan(
                            openTag.displayStart,
                            charCount,
                            openTag.modifierId,
                            openTag.valueStart,
                            openTag.valueLength);

                        spanBuffer[spanCount++] = span;
                        LogSpan(span, charBuffer.Slice(0, charCount), input);
                    }

                    stackDepth = matchIndex;  // Pop this and all unclosed nested tags
                }
                else
                {
                    Log($"Unmatched closing tag </{closingTagName.ToString()}>, copying as text");
                    // Copy unmatched closing tag as text
                    for (int j = tagStart; j <= bracketEnd && charCount < charBuffer.Length; j++)
                        charBuffer[charCount++] = input[j];
                }

                i = bracketEnd + 1;
            }
            else
            {
                // Opening tag <tag> or <tag=value>
                ParseTagContent(tagContent, out var tagName, out var tagValue);
                int tagHash = GetTagHash(tagName);

                Log($"Found opening tag <{tagName.ToString()}> at position {tagStart}" +
                    (tagValue.IsEmpty ? "" : $" with value \"{tagValue.ToString()}\""));

                if (tagHashToModifier.TryGetValue(tagHash, out int modId))
                {
                    if (stackDepth < tagStack.Length)
                    {
                        // Calculate value position in original text
                        int valueStart = 0;
                        int valueLen = 0;
                        if (!tagValue.IsEmpty)
                        {
                            // Find value position relative to input
                            valueStart = i + 1 + (int)(tagName.Length + 1);  // <tag= position
                            valueLen = tagValue.Length;
                        }

                        tagStack[stackDepth++] = new OpenTagInfo
                        {
                            tagHash = tagHash,
                            modifierId = modId,
                            displayStart = charCount,
                            valueStart = valueStart,
                            valueLength = valueLen,
                            originalStart = tagStart
                        };

                        Log($"Pushed tag to stack (depth={stackDepth})");
                    }
                    else
                    {
                        Log($"Warning: Tag stack overflow at <{tagName.ToString()}>");
                    }
                }
                else
                {
                    Log($"Unknown tag <{tagName.ToString()}>, copying as text");
                    // Unknown tag - copy as text
                    for (int j = tagStart; j <= bracketEnd && charCount < charBuffer.Length; j++)
                        charBuffer[charCount++] = input[j];
                }

                i = bracketEnd + 1;
            }
        }

        // Handle unclosed tags - create spans anyway
        for (int s = stackDepth - 1; s >= 0; s--)
        {
            ref var unclosed = ref tagStack[s];
            if (spanCount < spanBuffer.Length)
            {
                Log($"Warning: Unclosed tag at position {unclosed.originalStart}, creating span to end");

                var span = new ParsedSpan(
                    unclosed.displayStart,
                    charCount,
                    unclosed.modifierId,
                    unclosed.valueStart,
                    unclosed.valueLength);

                spanBuffer[spanCount++] = span;
                LogSpan(span, charBuffer.Slice(0, charCount), input);
            }
        }

        var result = new ParseResult
        {
            displayText = charBuffer.Slice(0, charCount),
            originalText = input,
            spans = spanBuffer.Slice(0, spanCount),
            textModified = true
        };

        LogResult(result);
        return result;
    }

    /// <summary>
    /// Find the position of '>' starting from openBracket position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindClosingBracket(ReadOnlySpan<char> text, int openBracket)
    {
        for (int i = openBracket + 1; i < text.Length; i++)
        {
            if (text[i] == '>')
                return i;
            // Don't search too far (malformed tags)
            if (i - openBracket > 128)
                return -1;
        }
        return -1;
    }

    /// <summary>
    /// Parse tag content to extract name and optional value.
    /// "color=#FF0000" -> name="color", value="#FF0000"
    /// "b" -> name="b", value=empty
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseTagContent(
        ReadOnlySpan<char> content,
        out ReadOnlySpan<char> tagName,
        out ReadOnlySpan<char> tagValue)
    {
        int eqIndex = content.IndexOf('=');
        if (eqIndex >= 0)
        {
            tagName = content.Slice(0, eqIndex).Trim();
            tagValue = content.Slice(eqIndex + 1).Trim();

            // Remove quotes if present
            if (tagValue.Length >= 2)
            {
                char first = tagValue[0];
                char last = tagValue[^1];
                if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                {
                    tagValue = tagValue.Slice(1, tagValue.Length - 2);
                }
            }
        }
        else
        {
            tagName = content.Trim();
            tagValue = ReadOnlySpan<char>.Empty;
        }
    }

    /// <summary>
    /// Compute case-insensitive hash for tag name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetTagHash(ReadOnlySpan<char> tag)
    {
        int hash = 17;
        for (int i = 0; i < tag.Length; i++)
        {
            char c = tag[i];
            // Case-insensitive: convert to lowercase
            if (c >= 'A' && c <= 'Z')
                c = (char)(c + 32);
            hash = hash * 31 + c;
        }
        return hash;
    }
}
