using System;
using System.Collections.Generic;

/// <summary>
/// Базовый класс для правил парсинга HTML-подобных тегов.
/// Обрабатывает открывающие/закрывающие теги и вложенность.
/// </summary>
[Serializable]
public abstract class TagParseRule : IParseRule
{
    private readonly Stack<OpenTag> openTags = new();

    private struct OpenTag
    {
        public int tagStart;
        public int tagEnd;
        public string parameter;
    }

    /// <summary>
    /// Имя тега (например, "color", "b", "size")
    /// </summary>
    protected abstract string TagName { get; }

    /// <summary>
    /// Поддерживает ли тег параметр (например, color=red)
    /// </summary>
    protected virtual bool HasParameter => false;

    /// <summary>
    /// Self-closing тег (не требует закрывающего тега, вставляет InsertString)
    /// </summary>
    protected virtual bool IsSelfClosing => false;

    /// <summary>
    /// Строка для вставки вместо тега (только для self-closing)
    /// </summary>
    protected virtual string InsertString => "\uFFFC";

    public void Reset()
    {
        openTags.Clear();
    }

    public void Finalize(int textLength, List<ParsedRange> results)
    {
        // Закрываем все незакрытые теги — их действие распространяется до конца текста
        while (openTags.Count > 0)
        {
            var open = openTags.Pop();
            results.Add(new ParsedRange(
                tagStart: open.tagStart,
                tagEnd: open.tagEnd,
                closeTagStart: textLength, // Нет закрывающего тега
                closeTagEnd: textLength,   // Пустой диапазон = ничего не удаляем
                parameter: open.parameter
            ));
        }
    }

    public int TryMatch(string text, int index, List<ParsedRange> results)
    {
        if (text[index] != '<')
            return index;

        int openResult = TryMatchOpenTag(text, index, results);
        if (openResult > index)
            return openResult;

        if (!IsSelfClosing)
        {
            int closeResult = TryMatchCloseTag(text, index, results);
            if (closeResult > index)
                return closeResult;
        }

        return index;
    }

    private int TryMatchOpenTag(string text, int index, List<ParsedRange> results)
    {
        int tagNameLen = TagName.Length;
        int minLen = HasParameter ? tagNameLen + 4 : tagNameLen + 2;
        if (index + minLen > text.Length)
            return index;

        if (!MatchesIgnoreCase(text, index + 1, TagName))
            return index;

        int afterName = index + 1 + tagNameLen;
        string parameter = null;
        int tagEnd;

        if (HasParameter)
        {
            if (afterName >= text.Length || text[afterName] != '=')
                return index;

            int paramStart = afterName + 1;
            int closePos = FindTagClose(text, paramStart);
            if (closePos < 0)
                return index;

            bool selfClose = closePos > paramStart && text[closePos - 1] == '/';
            int paramEnd = selfClose ? closePos - 1 : closePos;
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

            char c = text[afterName];
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
        for (int i = start; i < text.Length; i++)
            if (text[i] == '>') return i;
        return -1;
    }

    private int TryMatchCloseTag(string text, int index, List<ParsedRange> results)
    {
        int tagNameLen = TagName.Length;

        // </tag> = 3 + tagNameLen символов
        int closeLen = 3 + tagNameLen;
        if (index + closeLen > text.Length)
            return index;

        // Проверяем "</"
        if (text[index + 1] != '/')
            return index;

        // Проверяем имя тега
        if (!MatchesIgnoreCase(text, index + 2, TagName))
            return index;

        // Проверяем ">"
        if (text[index + 2 + tagNameLen] != '>')
            return index;

        if (openTags.Count == 0)
            return index;

        var open = openTags.Pop();
        int closeTagEnd = index + closeLen;

        results.Add(new ParsedRange(
            tagStart: open.tagStart,
            tagEnd: open.tagEnd,
            closeTagStart: index,
            closeTagEnd: closeTagEnd,
            parameter: open.parameter
        ));

        return closeTagEnd;
    }

    private static string ExtractParameter(string text, int start, int end)
    {
        var span = text.AsSpan(start, end - start);

        // Убираем пробелы
        span = span.Trim();

        // Убираем кавычки если есть
        if (span.Length >= 2)
        {
            char first = span[0];
            char last = span[span.Length - 1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
            {
                span = span.Slice(1, span.Length - 2);
            }
        }

        return span.ToString();
    }

    private static bool MatchesIgnoreCase(string text, int index, string pattern)
    {
        if (index + pattern.Length > text.Length)
            return false;

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