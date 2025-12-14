using System.Buffers;
using System.Collections.Generic;
using System.Text;
using LSCore;

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

    // Временный буфер для ParsedRange от каждого правила
    private readonly List<ParsedRange> tempRanges = new();

    // Reusable buffer for tag removals (avoids allocation per Parse call)
    private readonly LSList<(int start, int end)> tagRemovals = new();
    // Buffer for self-closing tag insertions: (tagStart, tagEnd, insertString)
    private readonly LSList<(int start, int end, string insert)> tagInsertions = new();

    // Clean text после удаления тегов
    private readonly StringBuilder cleanTextBuilder = new();

    /// <summary>
    /// Clean text после парсинга (без тегов)
    /// </summary>
    public string CleanText { get; private set; }

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
        spans.Clear();
        tagRemovals.FakeClear();
        tagInsertions.FakeClear();
        cleanTextBuilder.Clear();

        foreach (var rule in allRules)
            rule.Reset();

        if (string.IsNullOrEmpty(text))
        {
            CleanText = string.Empty;
            return CleanText;
        }

        int index = 0;
        while (index < text.Length)
        {
            int newIndex = index;

            foreach (var rule in allRules)
            {
                tempRanges.Clear();
                int result = rule.TryMatch(text, index, tempRanges);

                if (result > index)
                {
                    newIndex = result;
                    foreach (var range in tempRanges)
                    {
                        ProcessRange(range);
                        CreateSpanForRule(rule, range);
                    }
                    break;
                }
            }

            if (newIndex > index)
                index = newIndex;
            else
                index++;
        }

        foreach (var rule in allRules)
        {
            tempRanges.Clear();
            rule.Finalize(text.Length, tempRanges);

            foreach (var range in tempRanges)
            {
                ProcessRange(range);
                CreateSpanForRule(rule, range);
            }
        }

        BuildCleanTextAndRemapIndices(text);
        return CleanText;
    }

    private void ProcessRange(ParsedRange range)
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

    private void CreateSpanForRule(IParseRule rule, ParsedRange range)
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
        tagRemovals.Sort((a, b) => a.start.CompareTo(b.start));
        tagInsertions.Sort((a, b) => a.start.CompareTo(b.start));

        int mapSize = text.Length + 1;
        var indexMap = ArrayPool<int>.Shared.Rent(mapSize);

        try
        {
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
                    indexMap[i] = cleanTextBuilder.Length;
                    cleanTextBuilder.Append(ins.insert);
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
                        cleanTextBuilder.Append(text[i]);
                }
            }

            CleanText = cleanTextBuilder.ToString();
            RemapSpanIndices(spans, indexMap, mapSize, CleanText.Length);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(indexMap);
        }
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