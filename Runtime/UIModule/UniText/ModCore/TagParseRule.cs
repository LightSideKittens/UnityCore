using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[Serializable]
public abstract class TagParseRule : IParseRule
{
    private readonly Stack<OpenTag> openTags = new(8);

    private static readonly FastIntDictionary<string> parameterCache = new(128);

    private struct OpenTag
    {
        public int tagStart;
        public int tagEnd;
        public string parameter;
    }


    protected abstract string TagName { get; }


    protected virtual bool HasParameter => false;


    protected virtual bool IsSelfClosing => false;


    protected virtual string InsertString => "\uFFFC";

    public void Reset()
    {
        openTags.Clear();
    }

    public void Finalize(string text, PooledList<ParsedRange> results)
    {
        while (openTags.Count > 0)
        {
            var open = openTags.Pop();
            results.Add(new ParsedRange(
                open.tagStart,
                open.tagEnd,
                text.Length, text.Length, open.parameter
            ));
        }
    }

    public virtual int TryMatch(string text, int index, PooledList<ParsedRange> results)
    {
        if (text[index] != '<')
            return index;

        var openResult = TryMatchOpenTag(text, index, results);
        if (openResult > index)
            return openResult;

        if (!IsSelfClosing)
        {
            var closeResult = TryMatchCloseTag(text, index, results);
            if (closeResult > index)
                return closeResult;
        }

        return index;
    }

    private int TryMatchOpenTag(string text, int index, PooledList<ParsedRange> results)
    {
        var tagNameLen = TagName.Length;
        var minLen = HasParameter ? tagNameLen + 4 : tagNameLen + 2;
        if (index + minLen > text.Length)
            return index;

        if (!MatchesIgnoreCase(text, index + 1, TagName))
            return index;

        var afterName = index + 1 + tagNameLen;
        string parameter = null;
        int tagEnd;

        if (HasParameter)
        {
            if (afterName >= text.Length || text[afterName] != '=')
                return index;

            var paramStart = afterName + 1;
            var closePos = FindTagClose(text, paramStart);
            if (closePos < 0)
                return index;

            var selfClose = closePos > paramStart && text[closePos - 1] == '/';
            var paramEnd = selfClose ? closePos - 1 : closePos;
            parameter = ExtractParameter(text, paramStart, paramEnd);
            tagEnd = closePos + 1;

            if (IsSelfClosing || selfClose)
            {
                results.Add(ParsedRange.SelfClosing(index, tagEnd, InsertString, parameter));
                return tagEnd;
            }
        }
        else
        {
            if (afterName >= text.Length)
                return index;

            var c = text[afterName];
            if (c == '/')
            {
                if (afterName + 1 >= text.Length || text[afterName + 1] != '>')
                    return index;
                tagEnd = afterName + 2;
                if (IsSelfClosing)
                {
                    results.Add(ParsedRange.SelfClosing(index, tagEnd, InsertString, null));
                    return tagEnd;
                }
            }
            else if (c == '>')
            {
                tagEnd = afterName + 1;
                if (IsSelfClosing)
                {
                    results.Add(ParsedRange.SelfClosing(index, tagEnd, InsertString, null));
                    return tagEnd;
                }
            }
            else
            {
                return index;
            }
        }

        openTags.Push(new OpenTag { tagStart = index, tagEnd = tagEnd, parameter = parameter });
        return tagEnd;
    }

    private static int FindTagClose(string text, int start)
    {
        for (var i = start; i < text.Length; i++)
            if (text[i] == '>')
                return i;
        return -1;
    }

    private int TryMatchCloseTag(string text, int index, PooledList<ParsedRange> results)
    {
        var tagNameLen = TagName.Length;

        var closeLen = 3 + tagNameLen;
        if (index + closeLen > text.Length)
            return index;

        if (text[index + 1] != '/')
            return index;

        if (!MatchesIgnoreCase(text, index + 2, TagName))
            return index;

        if (text[index + 2 + tagNameLen] != '>')
            return index;

        if (openTags.Count == 0)
            return index;

        var open = openTags.Pop();
        var closeTagEnd = index + closeLen;

        results.Add(new ParsedRange(
            open.tagStart,
            open.tagEnd,
            index,
            closeTagEnd,
            open.parameter
        ));

        return closeTagEnd;
    }

    private static string ExtractParameter(string text, int start, int end)
    {
        var span = text.AsSpan(start, end - start);

        span = span.Trim();

        if (span.Length >= 2)
        {
            var first = span[0];
            var last = span[span.Length - 1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\'')) span = span.Slice(1, span.Length - 2);
        }

        return GetOrCreateCachedString(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetOrCreateCachedString(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty) return string.Empty;

        var hash = ComputeSpanHash(span);

        if (parameterCache.TryGetValue(hash, out var cached))
            if (cached.Length == span.Length && span.SequenceEqual(cached.AsSpan()))
                return cached;

        var result = span.ToString();
        parameterCache[hash] = result;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeSpanHash(ReadOnlySpan<char> span)
    {
        unchecked
        {
            var hash = -2128831035;
            for (var i = 0; i < span.Length; i++)
            {
                hash ^= span[i];
                hash *= 16777619;
            }

            return hash;
        }
    }

    private static bool MatchesIgnoreCase(string text, int index, string pattern)
    {
        if (index + pattern.Length > text.Length)
            return false;

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = text[index + i];
            var p = pattern[i];

            if (c == p) continue;

            var cLower = (uint)((c | 0x20) - 'a');
            if (cLower >= 26 || (c | 0x20) != p)
                return false;
        }

        return true;
    }
}