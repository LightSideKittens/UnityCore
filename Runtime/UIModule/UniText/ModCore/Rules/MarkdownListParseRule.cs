using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[Serializable]
public sealed class MarkdownListParseRule : IParseRule
{
    public int spacesPerLevel = 2;

    private static readonly char[] BulletChars = { '-', '*', '+' };

    private static readonly string[] LevelStrings = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    private static readonly Dictionary<(int level, int number), string> OrderedParamCache = new(64);

    public int TryMatch(string text, int index, IList<ParsedRange> results)
    {
        if (index > 0 && text[index - 1] != '\n')
            return index;

        var pos = index;
        var textLen = text.Length;

        var indent = 0;
        while (pos < textLen && text[pos] == ' ')
        {
            indent++;
            pos++;
        }

        if (pos >= textLen)
            return index;

        var nestingLevel = spacesPerLevel > 0 ? indent / spacesPerLevel : 0;

        if (pos < textLen - 1 &&
            Array.IndexOf(BulletChars, text[pos]) >= 0 &&
            text[pos + 1] == ' ')
        {
            var contentStart = pos + 2;
            var contentEnd = FindEndOfListItem(text, contentStart, indent);

            results.Add(new ParsedRange
            {
                start = contentStart,
                end = contentEnd,
                parameter = GetLevelString(nestingLevel),
                tagStart = index,
                tagEnd = contentStart,
                closeTagStart = contentEnd,
                closeTagEnd = contentEnd
            });

            return contentStart;
        }

        if (TryParseOrderedMarker(text, pos, out var markerEnd, out var number))
        {
            var contentStart = markerEnd;
            var contentEnd = FindEndOfListItem(text, contentStart, indent);

            results.Add(new ParsedRange
            {
                start = contentStart,
                end = contentEnd,
                parameter = GetOrderedParam(nestingLevel, number),
                tagStart = index,
                tagEnd = contentStart,
                closeTagStart = contentEnd,
                closeTagEnd = contentEnd
            });

            return contentStart;
        }

        return index;
    }

    private static bool TryParseOrderedMarker(string text, int pos, out int end, out int number)
    {
        end = pos;
        number = 0;

        var textLen = text.Length;

        var numStart = pos;
        while (end < textLen && end - numStart < 9 && char.IsDigit(text[end]))
            end++;

        if (end == numStart)
            return false;

        if (end >= textLen - 1)
            return false;

        var terminator = text[end];
        if (terminator != '.' && terminator != ')')
            return false;

        if (text[end + 1] != ' ')
            return false;

        if (!int.TryParse(text.AsSpan(numStart, end - numStart), out number))
            return false;

        end += 2;
        return true;
    }

    private int FindEndOfListItem(string text, int start, int currentIndent)
    {
        var textLen = text.Length;
        var pos = start;

        while (pos < textLen)
        {
            var lineEnd = text.IndexOf('\n', pos);
            if (lineEnd < 0)
                return textLen;

            pos = lineEnd + 1;
            if (pos >= textLen)
                return textLen;

            var nextIndent = 0;
            var checkPos = pos;
            while (checkPos < textLen && text[checkPos] == ' ')
            {
                nextIndent++;
                checkPos++;
            }

            if (checkPos < textLen && text[checkPos] == '\n')
                return lineEnd + 1;

            if (nextIndent <= currentIndent && checkPos < textLen)
            {
                var c = text[checkPos];

                if (Array.IndexOf(BulletChars, c) >= 0 &&
                    checkPos + 1 < textLen && text[checkPos + 1] == ' ')
                    return lineEnd + 1;

                if (char.IsDigit(c))
                    if (TryParseOrderedMarker(text, checkPos, out _, out _))
                        return lineEnd + 1;
            }
        }

        return textLen;
    }

    public void Finalize(int textLength, IList<ParsedRange> results)
    {
    }

    public void Reset()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetLevelString(int level)
    {
        return (uint)level < (uint)LevelStrings.Length ? LevelStrings[level] : level.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetOrderedParam(int level, int number)
    {
        var key = (level, number);
        if (OrderedParamCache.TryGetValue(key, out var cached))
            return cached;

        var result = $"{level}:{number}";
        OrderedParamCache[key] = result;
        return result;
    }
}