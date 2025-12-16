using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Парсит списки в стиле Markdown (близко к CommonMark):
/// - Bullet: строка начинается с "- ", "* ", "+ "
/// - Ordered: строка начинается с "N. " или "N) " где N — число (1-999999999)
/// - Вложенность: каждые spacesPerLevel пробелов = следующий уровень
/// </summary>
[Serializable]
public sealed class MarkdownListParseRule : IParseRule
{
    // Настраиваемое количество пробелов на уровень
    public int spacesPerLevel = 2;

    // DEBUG
    public static bool DebugLogging = false;

    // Bullet markers по CommonMark: -, *, +
    private static readonly char[] BulletChars = { '-', '*', '+' };

    // Cached level strings to avoid allocation
    private static readonly string[] LevelStrings = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    // Cache for ordered list parameters "level:number"
    private static readonly Dictionary<(int level, int number), string> OrderedParamCache = new(64);

    public int TryMatch(string text, int index, IList<ParsedRange> results)
    {
        // Должны быть в начале строки (после \n или index=0)
        if (index > 0 && text[index - 1] != '\n')
            return index;

        int pos = index;
        int textLen = text.Length;

        // Подсчитать отступ (пробелы)
        int indent = 0;
        while (pos < textLen && text[pos] == ' ')
        {
            indent++;
            pos++;
        }

        if (pos >= textLen)
            return index;

        int nestingLevel = spacesPerLevel > 0 ? indent / spacesPerLevel : 0;

        // Попробовать bullet list: "- ", "* ", "+ "
        if (pos < textLen - 1 &&
            Array.IndexOf(BulletChars, text[pos]) >= 0 &&
            text[pos + 1] == ' ')
        {
            int contentStart = pos + 2;
            int contentEnd = FindEndOfListItem(text, contentStart, indent);

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

            if (DebugLogging)
                UnityEngine.Debug.Log($"[MarkdownListParseRule] BULLET: indent={indent}, level={nestingLevel}, contentStart={contentStart}, contentEnd={contentEnd}");

            // Возвращаем contentStart чтобы парсер продолжил парсить внутри контента
            // (найдёт вложенные теги типа <b>, <i> и т.д.)
            return contentStart;
        }

        // Попробовать ordered list: "N. " или "N) "
        if (TryParseOrderedMarker(text, pos, out int markerEnd, out int number))
        {
            int contentStart = markerEnd;
            int contentEnd = FindEndOfListItem(text, contentStart, indent);

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

            // Возвращаем contentStart чтобы парсер продолжил парсить внутри контента
            return contentStart;
        }

        return index;
    }

    private static bool TryParseOrderedMarker(string text, int pos, out int end, out int number)
    {
        end = pos;
        number = 0;

        int textLen = text.Length;

        // Читаем цифры (макс 9 по CommonMark spec)
        int numStart = pos;
        while (end < textLen && end - numStart < 9 && char.IsDigit(text[end]))
            end++;

        if (end == numStart)
            return false;  // Нет цифр

        // Должен быть . или ) и пробел после
        if (end >= textLen - 1)
            return false;

        char terminator = text[end];
        if (terminator != '.' && terminator != ')')
            return false;

        if (text[end + 1] != ' ')
            return false;

        // Парсим число
        if (!int.TryParse(text.AsSpan(numStart, end - numStart), out number))
            return false;

        end += 2;  // Пропустить ". " или ") "
        return true;
    }

    private int FindEndOfListItem(string text, int start, int currentIndent)
    {
        // List item заканчивается когда:
        // 1. Конец текста
        // 2. Пустая строка (blank line)
        // 3. Строка с меньшим или равным отступом + маркер (новый item)

        int textLen = text.Length;
        int pos = start;

        while (pos < textLen)
        {
            // Найти конец текущей строки
            int lineEnd = text.IndexOf('\n', pos);
            if (lineEnd < 0)
                return textLen;  // Конец текста

            pos = lineEnd + 1;
            if (pos >= textLen)
                return textLen;

            // Проверить следующую строку
            int nextIndent = 0;
            int checkPos = pos;
            while (checkPos < textLen && text[checkPos] == ' ')
            {
                nextIndent++;
                checkPos++;
            }

            // Пустая строка — конец item
            if (checkPos < textLen && text[checkPos] == '\n')
                return lineEnd + 1;

            // Меньший или равный отступ — проверить на маркер
            if (nextIndent <= currentIndent && checkPos < textLen)
            {
                char c = text[checkPos];

                // Bullet marker?
                if (Array.IndexOf(BulletChars, c) >= 0 &&
                    checkPos + 1 < textLen && text[checkPos + 1] == ' ')
                {
                    return lineEnd + 1;  // Новый bullet item
                }

                // Ordered marker?
                if (char.IsDigit(c))
                {
                    if (TryParseOrderedMarker(text, checkPos, out _, out _))
                        return lineEnd + 1;  // Новый ordered item
                }
            }

            // Continuation line — продолжаем поиск
        }

        return textLen;
    }

    public void Finalize(int textLength, IList<ParsedRange> results)
    {
        // Списки не требуют финализации (нет unclosed tags)
    }

    public void Reset()
    {
        // Нет состояния для сброса
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