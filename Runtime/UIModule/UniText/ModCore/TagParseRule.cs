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

        // Пробуем открывающий тег
        int openResult = TryMatchOpenTag(text, index);
        if (openResult > index)
            return openResult;

        // Пробуем закрывающий тег
        int closeResult = TryMatchCloseTag(text, index, results);
        if (closeResult > index)
            return closeResult;

        return index;
    }

    private int TryMatchOpenTag(string text, int index)
    {
        int tagNameLen = TagName.Length;

        // Минимум: <tag> или <tag=x>
        int minLen = HasParameter ? tagNameLen + 4 : tagNameLen + 2;
        if (index + minLen > text.Length)
            return index;

        // Проверяем "<tagname"
        if (!MatchesIgnoreCase(text, index + 1, TagName))
            return index;

        int afterName = index + 1 + tagNameLen;

        string parameter = null;

        if (HasParameter)
        {
            // Ожидаем "=" после имени тега
            if (afterName >= text.Length || text[afterName] != '=')
                return index;

            int paramStart = afterName + 1;
            int tagEnd = text.IndexOf('>', paramStart);
            if (tagEnd < 0)
                return index;

            // Извлекаем параметр
            parameter = ExtractParameter(text, paramStart, tagEnd);

            openTags.Push(new OpenTag
            {
                tagStart = index,
                tagEnd = tagEnd + 1,
                parameter = parameter
            });

            return tagEnd + 1;
        }
        else
        {
            // Простой тег без параметра: <tag>
            if (afterName >= text.Length || text[afterName] != '>')
                return index;

            openTags.Push(new OpenTag
            {
                tagStart = index,
                tagEnd = afterName + 1,
                parameter = null
            });

            return afterName + 1;
        }
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