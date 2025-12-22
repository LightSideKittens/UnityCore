using System;
using System.Runtime.CompilerServices;

[Serializable]
public sealed class RawUrlParseRule : IParseRule
{
    public int TryMatch(string text, int index, PooledList<ParsedRange> results) => index;

    public void Finalize(string text, PooledList<ParsedRange> results)
    {
        var len = text.Length;
        var i = 0;

        // Ищем :// — это признак URL со схемой
        while (i < len - 2)
        {
            var colonPos = text.IndexOf(':', i);
            if (colonPos < 0) break;

            // Проверяем ://
            if (colonPos + 2 < len && text[colonPos + 1] == '/' && text[colonPos + 2] == '/')
            {
                var schemeStart = FindSchemeStart(text, colonPos);
                if (schemeStart >= 0)
                {
                    var urlEnd = FindUrlEnd(text, colonPos + 3);
                    if (urlEnd > colonPos + 3)
                    {
                        results.Add(new ParsedRange(schemeStart, urlEnd, text.Substring(schemeStart, urlEnd - schemeStart)));
                        i = urlEnd;
                        continue;
                    }
                }
            }
            // mailto: и tel: (без //)
            else if (colonPos >= 6)
            {
                var cl = ToLowerAscii(text[colonPos - 6]);
                if (cl == 'm' && MatchesScheme(text, colonPos - 6, 'm', 'a', 'i', 'l', 't', 'o', ':'))
                {
                    var schemeStart = colonPos - 6;
                    if (schemeStart == 0 || !IsWordChar(text[schemeStart - 1]))
                    {
                        var urlEnd = FindUrlEnd(text, colonPos + 1);
                        if (urlEnd > colonPos + 1)
                        {
                            results.Add(new ParsedRange(schemeStart, urlEnd, text.Substring(schemeStart, urlEnd - schemeStart)));
                            i = urlEnd;
                            continue;
                        }
                    }
                }
            }
            else if (colonPos >= 3)
            {
                var cl = ToLowerAscii(text[colonPos - 3]);
                if (cl == 't' && MatchesScheme(text, colonPos - 3, 't', 'e', 'l', ':'))
                {
                    var schemeStart = colonPos - 3;
                    if (schemeStart == 0 || !IsWordChar(text[schemeStart - 1]))
                    {
                        var urlEnd = FindUrlEnd(text, colonPos + 1);
                        if (urlEnd > colonPos + 1)
                        {
                            results.Add(new ParsedRange(schemeStart, urlEnd, text.Substring(schemeStart, urlEnd - schemeStart)));
                            i = urlEnd;
                            continue;
                        }
                    }
                }
            }

            i = colonPos + 1;
        }

        // Ищем www.
        i = 0;
        while (i < len - 3)
        {
            var wwwPos = text.IndexOf('w', i);
            if (wwwPos < 0) break;

            if (MatchesScheme(text, wwwPos, 'w', 'w', 'w', '.'))
            {
                if (wwwPos == 0 || !IsWordChar(text[wwwPos - 1]))
                {
                    var urlEnd = FindUrlEnd(text, wwwPos + 4);
                    if (urlEnd > wwwPos + 4)
                    {
                        results.Add(new ParsedRange(wwwPos, urlEnd, "https://" + text.Substring(wwwPos, urlEnd - wwwPos)));
                        i = urlEnd;
                        continue;
                    }
                }
            }

            i = wwwPos + 1;
        }
    }

    private static int FindSchemeStart(string text, int colonPos)
    {
        // https (5), http (4), ftps (4), ftp (3), file (4)
        if (colonPos >= 5)
        {
            var c = ToLowerAscii(text[colonPos - 5]);
            if (c == 'h' && MatchesScheme(text, colonPos - 5, 'h', 't', 't', 'p', 's', ':', '/', '/'))
            {
                var start = colonPos - 5;
                if (start == 0 || !IsWordChar(text[start - 1])) return start;
            }
        }
        if (colonPos >= 4)
        {
            var c = ToLowerAscii(text[colonPos - 4]);
            if (c == 'h' && MatchesScheme(text, colonPos - 4, 'h', 't', 't', 'p', ':', '/', '/'))
            {
                var start = colonPos - 4;
                if (start == 0 || !IsWordChar(text[start - 1])) return start;
            }
            if (c == 'f')
            {
                if (MatchesScheme(text, colonPos - 4, 'f', 't', 'p', 's', ':', '/', '/'))
                {
                    var start = colonPos - 4;
                    if (start == 0 || !IsWordChar(text[start - 1])) return start;
                }
                if (MatchesScheme(text, colonPos - 4, 'f', 'i', 'l', 'e', ':', '/', '/'))
                {
                    var start = colonPos - 4;
                    if (start == 0 || !IsWordChar(text[start - 1])) return start;
                }
            }
        }
        if (colonPos >= 3)
        {
            var c = ToLowerAscii(text[colonPos - 3]);
            if (c == 'f' && MatchesScheme(text, colonPos - 3, 'f', 't', 'p', ':', '/', '/'))
            {
                var start = colonPos - 3;
                if (start == 0 || !IsWordChar(text[start - 1])) return start;
            }
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char ToLowerAscii(char c)
    {
        return (c >= 'A' && c <= 'Z') ? (char)(c | 0x20) : c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesScheme(string text, int index, char c0, char c1, char c2, char c3)
    {
        if (index + 4 > text.Length) return false;
        return ToLowerAscii(text[index]) == c0 &&
               ToLowerAscii(text[index + 1]) == c1 &&
               ToLowerAscii(text[index + 2]) == c2 &&
               ToLowerAscii(text[index + 3]) == c3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesScheme(string text, int index, char c0, char c1, char c2, char c3, char c4, char c5)
    {
        if (index + 6 > text.Length) return false;
        return ToLowerAscii(text[index]) == c0 &&
               ToLowerAscii(text[index + 1]) == c1 &&
               ToLowerAscii(text[index + 2]) == c2 &&
               ToLowerAscii(text[index + 3]) == c3 &&
               ToLowerAscii(text[index + 4]) == c4 &&
               ToLowerAscii(text[index + 5]) == c5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesScheme(string text, int index, char c0, char c1, char c2, char c3, char c4, char c5, char c6)
    {
        if (index + 7 > text.Length) return false;
        return ToLowerAscii(text[index]) == c0 &&
               ToLowerAscii(text[index + 1]) == c1 &&
               ToLowerAscii(text[index + 2]) == c2 &&
               ToLowerAscii(text[index + 3]) == c3 &&
               ToLowerAscii(text[index + 4]) == c4 &&
               ToLowerAscii(text[index + 5]) == c5 &&
               ToLowerAscii(text[index + 6]) == c6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesScheme(string text, int index, char c0, char c1, char c2, char c3, char c4, char c5, char c6, char c7)
    {
        if (index + 8 > text.Length) return false;
        return ToLowerAscii(text[index]) == c0 &&
               ToLowerAscii(text[index + 1]) == c1 &&
               ToLowerAscii(text[index + 2]) == c2 &&
               ToLowerAscii(text[index + 3]) == c3 &&
               ToLowerAscii(text[index + 4]) == c4 &&
               ToLowerAscii(text[index + 5]) == c5 &&
               ToLowerAscii(text[index + 6]) == c6 &&
               ToLowerAscii(text[index + 7]) == c7;
    }

    private static int FindUrlEnd(string text, int start)
    {
        var i = start;
        var parenDepth = 0;

        while (i < text.Length)
        {
            var c = text[i];
            if (c <= ' ') break;

            if (c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?')
                if (i + 1 >= text.Length || text[i + 1] <= ' ')
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
    private static bool IsValidUrlChar(char c)
    {
        if ((c | 0x20) >= 'a' && (c | 0x20) <= 'z') return true;
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
        return ((c | 0x20) >= 'a' && (c | 0x20) <= 'z') || (c >= '0' && c <= '9') || c == '_';
    }
}
