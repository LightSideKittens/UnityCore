using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Парсит ссылки: &lt;link="url"&gt;text&lt;/link&gt; + автоматическое распознавание URL.
/// Поддерживает RFC 3986 (URI), RFC 3987 (IRI), WHATWG URL Standard.
/// </summary>
[Serializable]
public sealed class LinkParseRule : TagParseRule
{
    /// <summary>
    /// Автоматически распознавать URL в тексте
    /// </summary>
    public bool parseLinkInRawText = true;

    protected override string TagName => "link";
    protected override bool HasParameter => true;

    // Схемы для автоматического распознавания
    private static readonly string[] schemes = { "https://", "http://", "ftp://", "ftps://", "mailto:", "tel:", "file://" };
    private const string WwwPrefix = "www.";

    public override int TryMatch(string text, int index, IList<ParsedRange> results)
    {
        // Сначала пробуем теги
        int tagResult = base.TryMatch(text, index, results);
        if (tagResult > index) return tagResult;

        // Затем автораспознавание URL
        if (parseLinkInRawText)
        {
            int urlResult = TryMatchRawUrl(text, index, results);
            if (urlResult > index) return urlResult;
        }

        return index;
    }

    private int TryMatchRawUrl(string text, int index, IList<ParsedRange> results)
    {
        char c = text[index];

        // Схемы (http://, https://, и т.д.)
        for (int i = 0; i < schemes.Length; i++)
        {
            string scheme = schemes[i];
            if (StartsWithScheme(text, index, scheme))
            {
                int urlEnd = FindUrlEnd(text, index + scheme.Length);
                if (urlEnd > index + scheme.Length)
                {
                    string url = text.Substring(index, urlEnd - index);
                    results.Add(new ParsedRange(index, urlEnd, url));
                    return urlEnd;
                }
            }
        }

        // www. (без схемы)
        if ((c == 'w' || c == 'W') && MatchesIgnoreCase(text, index, WwwPrefix))
        {
            if (index == 0 || !IsWordChar(text[index - 1]))
            {
                int urlEnd = FindUrlEnd(text, index + WwwPrefix.Length);
                if (urlEnd > index + WwwPrefix.Length)
                {
                    string url = text.Substring(index, urlEnd - index);
                    results.Add(new ParsedRange(index, urlEnd, "https://" + url));
                    return urlEnd;
                }
            }
        }

        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool StartsWithScheme(string text, int index, string scheme)
    {
        if (index + scheme.Length > text.Length) return false;
        if (index > 0 && IsWordChar(text[index - 1])) return false;
        return MatchesIgnoreCase(text, index, scheme);
    }

    private static int FindUrlEnd(string text, int start)
    {
        int i = start;
        int parenDepth = 0;

        while (i < text.Length)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c)) break;

            // Пунктуация в конце
            if (c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?')
            {
                if (i + 1 >= text.Length || char.IsWhiteSpace(text[i + 1]))
                    break;
            }

            // Балансировка скобок (Wikipedia-style URLs)
            if (c == '(') { parenDepth++; i++; continue; }
            if (c == ')')
            {
                if (parenDepth > 0) { parenDepth--; i++; continue; }
                break;
            }

            if (c == '"' || c == '\'' || c == '<' || c == '>' || c == '`' || c == '[' || c == ']') break;
            if (!IsValidUrlChar(c)) break;

            i++;
        }
        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidUrlChar(char c)
    {
        if (c >= 'a' && c <= 'z') return true;
        if (c >= 'A' && c <= 'Z') return true;
        if (c >= '0' && c <= '9') return true;
        switch (c)
        {
            case '-': case '.': case '_': case '~':
            case '!': case '$': case '&': case '\'': case '*': case '+': case ',': case ';': case '=':
            case ':': case '@': case '/': case '?': case '#': case '%': case '(': case ')':
                return true;
        }
        // RFC 3987 IRI Unicode
        if (c >= 0x00A0 && c <= 0xD7FF) return true;
        if (c >= 0xF900 && c <= 0xFDCF) return true;
        if (c >= 0xFDF0 && c <= 0xFFEF) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWordChar(char c) =>
        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';

    private static bool MatchesIgnoreCase(string text, int index, string pattern)
    {
        if (index + pattern.Length > text.Length) return false;
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = text[index + i];
            char p = pattern[i];
            if (c != p && char.ToLowerInvariant(c) != char.ToLowerInvariant(p))
                return false;
        }
        return true;
    }
}
