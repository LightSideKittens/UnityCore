using System;
using System.Buffers;
using System.Collections.Generic;
using LSCore;
using UnityEngine.Profiling;

/// <summary>
/// Парсер атрибутов текста.
/// Single-pass парсинг с регистрацией правил и модификаторов по уровням.
/// </summary>
public sealed class AttributeParser
{
    // Уровни модификаторов
    public readonly List<(IParseRule rule, BaseModifier modifier)> ruleModPairs = new();
    // Using LSList for ref access to AttributeSpan (avoids struct copying in RemapSpanIndices)
    internal readonly LSList<AttributeSpan> spans = new();

    // Все правила для итерации (кешируем для производительности)
    private readonly List<IParseRule> allRules = new();

    // Временный буфер для ParsedRange от каждого правила (LSList to avoid allocations)
    private readonly LSList<ParsedRange> tempRanges = new(32);

    // Reusable buffer for tag removals (avoids allocation per Parse call)
    private readonly LSList<(int start, int end)> tagRemovals = new();
    // Buffer for self-closing tag insertions: (tagStart, tagEnd, insertString)
    private readonly LSList<(int start, int end, string insert)> tagInsertions = new();

    // Static cached IComparers to avoid FunctorComparer allocation in Sort()
    private static readonly RemovalComparerImpl RemovalComparer = new();
    private static readonly InsertionComparerImpl InsertionComparer = new();

    private sealed class RemovalComparerImpl : IComparer<(int start, int end)>
    {
        public int Compare((int start, int end) a, (int start, int end) b) => a.start.CompareTo(b.start);
    }

    private sealed class InsertionComparerImpl : IComparer<(int start, int end, string insert)>
    {
        public int Compare((int start, int end, string insert) a, (int start, int end, string insert) b) => a.start.CompareTo(b.start);
    }

    // Static shared buffers - parsing is single-threaded so safe to share
    private static int[] sharedIndexMap = new int[1024];
    private static char[] sharedCleanTextBuffer = new char[1024];

    // Current clean text length (static buffer is shared)
    private int cleanTextLength;

    /// <summary>
    /// Clean text после парсинга (без тегов) as ReadOnlySpan.
    /// Zero-allocation - use this instead of CleanText property.
    /// </summary>
    public ReadOnlySpan<char> CleanTextSpan => sharedCleanTextBuffer.AsSpan(0, cleanTextLength);

    // Cached string version - only allocated when accessed
    private string cachedCleanTextString;

    /// <summary>
    /// Clean text as string (allocates on first access, then cached).
    /// Prefer CleanTextSpan for zero-allocation access.
    /// </summary>
    public string CleanText
    {
        get
        {
            if (cachedCleanTextString == null && cleanTextLength > 0)
                cachedCleanTextString = new string(sharedCleanTextBuffer, 0, cleanTextLength);
            return cachedCleanTextString ?? string.Empty;
        }
    }

    /// <summary>
    /// Зарегистрировать правило с модификатором уровня Itemization
    /// </summary>
    public void Register(IParseRule rule, BaseModifier modifier)
    {
        ruleModPairs.Add((rule, modifier));
        allRules.Add(rule);
    }

    /// <summary>
    /// Инициализировать все модификаторы. Вызывается после создания MeshGenerator.
    /// Модификаторы подписываются на события MeshGenerator.
    /// </summary>
    public void InitializeModifiers(UniText uniText)
    {
        for (int i = 0; i < ruleModPairs.Count; i++)
        {
            ruleModPairs[i].modifier.Initialize(uniText);
        }
    }

    /// <summary>
    /// Деинициализировать все модификаторы. Вызывается при удалении или в OnValidate.
    /// Модификаторы отписываются от событий MeshGenerator.
    /// </summary>
    public void DeinitializeModifiers()
    {
        for (int i = 0; i < ruleModPairs.Count; i++)
        {
            ruleModPairs[i].modifier.Deinitialize();
        }
    }
    
    public void DestroyModifiers()
    {
        for (int i = 0; i < ruleModPairs.Count; i++)
        {
            ruleModPairs[i].modifier.Destroy();
        }
    }

    /// <summary>
    /// Сбросить буферы всех модификаторов перед новым текстом.
    /// </summary>
    public void ResetModifiers()
    {
        for (int i = 0; i < ruleModPairs.Count; i++)
        {
            ruleModPairs[i].modifier.Reset();
        }
    }

    public void Apply()
    {
        // Обратный порядок: внутренние теги применяются последними и переопределяют внешние
        for (int i = spans.Count - 1; i >= 0; i--)
        {
            // Use ref to avoid struct copy
            ref readonly var span = ref spans[i];
            span.modifier.Apply(span.start, span.end, span.parameter);
        }
    }
    
    /// <summary>
    /// Парсить текст и собрать все атрибуты
    /// </summary>
    /// <param name="text">Исходный текст с разметкой</param>
    /// <returns>Clean text без тегов</returns>
    public string Parse(string text)
    {
        spans.FakeClear();
        tagRemovals.FakeClear();
        tagInsertions.FakeClear();
        cleanTextLength = 0;
        cachedCleanTextString = null;

        for (int r = 0; r < allRules.Count; r++)
            allRules[r].Reset();

        if (string.IsNullOrEmpty(text))
        {
            cleanTextLength = 0;
            return string.Empty;
        }
        
        int index = 0;
        while (index < text.Length)
        {
            int newIndex = index;

            for (int r = 0; r < allRules.Count; r++)
            {
                var rule = allRules[r];
                tempRanges.FakeClear();
                int result = rule.TryMatch(text, index, tempRanges);

                if (result > index)
                {
                    newIndex = result;
                    for (int j = 0; j < tempRanges.Count; j++)
                    {
                        ref readonly var range = ref tempRanges[j];
                        ProcessRange(in range);
                        CreateSpanForRule(rule, in range);
                    }
                    break;
                }
            }

            if (newIndex > index)
                index = newIndex;
            else
                index++;
        }
        
        for (int r = 0; r < allRules.Count; r++)
        {
            var rule = allRules[r];
            tempRanges.FakeClear();
            rule.Finalize(text.Length, tempRanges);

            for (int j = 0; j < tempRanges.Count; j++)
            {
                ref readonly var range = ref tempRanges[j];
                ProcessRange(in range);
                CreateSpanForRule(rule, in range);
            }
        }
        
        BuildCleanTextAndRemapIndices(text);

        return cachedCleanTextString;
    }

    private void ProcessRange(in ParsedRange range)
    {
        if (!range.HasTags) return;

        if (range.IsSelfClosing)
        {
            tagInsertions.Add((range.tagStart, range.tagEnd, range.insertString));
        }
        else
        {
            tagRemovals.Add((range.tagStart, range.tagEnd));
            if (range.closeTagStart != range.closeTagEnd)
                tagRemovals.Add((range.closeTagStart, range.closeTagEnd));
        }
    }

    private void CreateSpanForRule(IParseRule rule, in ParsedRange range)
    {
        for (int i = 0; i < ruleModPairs.Count; i++)
        {
            if (ReferenceEquals(ruleModPairs[i].rule, rule))
            {
                spans.Add(new AttributeSpan(range.start, range.end, ruleModPairs[i].modifier, range.parameter));
                return;
            }
        }
    }

    private void BuildCleanTextAndRemapIndices(string text)
    {
        tagRemovals.Sort(0, tagRemovals.Count, RemovalComparer);
        tagInsertions.Sort(0, tagInsertions.Count, InsertionComparer);

        int mapSize = text.Length + 1;

        // Use static shared buffer instead of ArrayPool to avoid allocation
        if (sharedIndexMap.Length < mapSize)
            sharedIndexMap = new int[Math.Max(mapSize, sharedIndexMap.Length * 2)];
        var indexMap = sharedIndexMap;

        // Ensure clean text buffer is large enough (worst case: same size as input)
        EnsureCleanTextCapacity(text.Length);

        int offset = 0;
        int removalIdx = 0;
        int insertionIdx = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            while (removalIdx < tagRemovals.Count && i >= tagRemovals[removalIdx].end)
                removalIdx++;

            // Check insertion at this position (self-closing tag replaces tag with insertString)
            if (insertionIdx < tagInsertions.Count && i == tagInsertions[insertionIdx].start)
            {
                var ins = tagInsertions[insertionIdx];
                indexMap[i] = cleanTextLength;
                AppendToCleanText(ins.insert);
                offset += ins.end - ins.start - ins.insert.Length;
                i = ins.end - 1; // loop will increment
                insertionIdx++;
                continue;
            }

            // Check removal
            if (removalIdx < tagRemovals.Count && i >= tagRemovals[removalIdx].start && i < tagRemovals[removalIdx].end)
            {
                offset++;
                indexMap[i] = -1;
            }
            else
            {
                indexMap[i] = i - offset;
                if (i < text.Length)
                    sharedCleanTextBuffer[cleanTextLength++] = text[i];
            }
        }

        RemapSpanIndices(spans, indexMap, mapSize, cleanTextLength);
    }

    private static void EnsureCleanTextCapacity(int required)
    {
        if (sharedCleanTextBuffer.Length >= required)
            return;

        // Grow static buffer (no ArrayPool - just allocate once and keep)
        sharedCleanTextBuffer = new char[Math.Max(required, sharedCleanTextBuffer.Length * 2)];
    }

    private void AppendToCleanText(string str)
    {
        if (string.IsNullOrEmpty(str)) return;

        EnsureCleanTextCapacity(cleanTextLength + str.Length);
        str.AsSpan().CopyTo(sharedCleanTextBuffer.AsSpan(cleanTextLength));
        cleanTextLength += str.Length;
    }

    private static void RemapSpanIndices(LSList<AttributeSpan> spans, int[] indexMap, int mapLength, int cleanTextLength)
    {
        for (int i = 0; i < spans.Count; i++)
        {
            // Use ref to modify struct in-place (no copying)
            ref var span = ref spans[i];

            // Clamp indices to valid range
            int startIdx = span.start < 0 ? 0 : (span.start >= mapLength ? mapLength - 1 : span.start);
            int endIdx = span.end < 0 ? 0 : (span.end >= mapLength ? mapLength - 1 : span.end);

            int newStart = indexMap[startIdx];
            int newEnd = indexMap[endIdx];

            // Если индекс попал на удалённый символ, ищем ближайший валидный
            if (newStart < 0)
            {
                for (int j = startIdx; j < mapLength; j++)
                {
                    if (indexMap[j] >= 0) { newStart = indexMap[j]; break; }
                }
                // Если не нашли вперёд, ищем назад
                if (newStart < 0)
                {
                    for (int j = startIdx - 1; j >= 0; j--)
                    {
                        if (indexMap[j] >= 0) { newStart = indexMap[j]; break; }
                    }
                }
            }

            if (newEnd < 0)
            {
                for (int j = endIdx; j < mapLength; j++)
                {
                    if (indexMap[j] >= 0) { newEnd = indexMap[j]; break; }
                }
                // Если не нашли вперёд, ищем назад
                if (newEnd < 0)
                {
                    for (int j = endIdx - 1; j >= 0; j--)
                    {
                        if (indexMap[j] >= 0) { newEnd = indexMap[j]; break; }
                    }
                }
            }

            // Final safety clamp to clean text bounds
            if (newStart < 0) newStart = 0;
            if (newEnd < 0) newEnd = cleanTextLength;
            if (newEnd > cleanTextLength) newEnd = cleanTextLength;
            if (newStart > newEnd) newStart = newEnd;

            // Modify in-place via ref (no new struct allocation, no copy back)
            span.start = newStart;
            span.end = newEnd;
        }
    }
}