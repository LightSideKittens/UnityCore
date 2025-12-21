using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Serializable]
public sealed class RawUrlParseRule : IParseRule
{
    private static readonly string[] schemes =
        { "https://", "http://", "ftp://", "ftps://", "mailto:", "tel:", "file://" };

    private const string WwwPrefix = "www.";

    public int TryMatch(string text, int index, PooledList<ParsedRange> results)
    {
        var c = text[index];

        for (var i = 0; i < schemes.Length; i++)
        {
            var scheme = schemes[i];
            if (StartsWithScheme(text, index, scheme))
            {
                var urlEnd = FindUrlEnd(text, index + scheme.Length);
                if (urlEnd > index + scheme.Length)
                {
                    var url = text.Substring(index, urlEnd - index);
                    results.Add(new ParsedRange(index, urlEnd, url));
                    return urlEnd;
                }
            }
        }

        if ((c == 'w' || c == 'W') && MatchesIgnoreCase(text, index, WwwPrefix))
            if (index == 0 || !IsWordChar(text[index - 1]))
            {
                var urlEnd = FindUrlEnd(text, index + WwwPrefix.Length);
                if (urlEnd > index + WwwPrefix.Length)
                {
                    var url = text.Substring(index, urlEnd - index);
                    results.Add(new ParsedRange(index, urlEnd, "https://" + url));
                    return urlEnd;
                }
            }

        return index;
    }

    public void Finalize(int textLength, PooledList<ParsedRange> results) { }

    public void Reset() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool StartsWithScheme(string text, int index, string scheme)
    {
        if (index + scheme.Length > text.Length) return false;
        if (index > 0 && IsWordChar(text[index - 1])) return false;
        return MatchesIgnoreCase(text, index, scheme);
    }

    internal static int FindUrlEnd(string text, int start)
    {
        var i = start;
        var parenDepth = 0;

        while (i < text.Length)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c)) break;

            if (c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?')
                if (i + 1 >= text.Length || char.IsWhiteSpace(text[i + 1]))
                    break;

            if (c == '(')
            {
                parenDepth++;
                i++;
                continue;
            }

            if (c == ')')
            {
                if (parenDepth > 0)
                {
                    parenDepth--;
                    i++;
                    continue;
                }

                break;
            }

            if (c == '"' || c == '\'' || c == '<' || c == '>' || c == '`' || c == '[' || c == ']') break;
            if (!IsValidUrlChar(c)) break;

            i++;
        }

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsValidUrlChar(char c)
    {
        if (c >= 'a' && c <= 'z') return true;
        if (c >= 'A' && c <= 'Z') return true;
        if (c >= '0' && c <= '9') return true;
        switch (c)
        {
            case '-':
            case '.':
            case '_':
            case '~':
            case '!':
            case '$':
            case '&':
            case '\'':
            case '*':
            case '+':
            case ',':
            case ';':
            case '=':
            case ':':
            case '@':
            case '/':
            case '?':
            case '#':
            case '%':
            case '(':
            case ')':
                return true;
        }

        if (c >= 0x00A0 && c <= 0xD7FF) return true;
        if (c >= 0xF900 && c <= 0xFDCF) return true;
        if (c >= 0xFDF0 && c <= 0xFFEF) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWordChar(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
    }

    private static bool MatchesIgnoreCase(string text, int index, string pattern)
    {
        if (index + pattern.Length > text.Length) return false;
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = text[index + i];
            var p = pattern[i];
            if (c != p && char.ToLowerInvariant(c) != char.ToLowerInvariant(p))
                return false;
        }

        return true;
    }
}
