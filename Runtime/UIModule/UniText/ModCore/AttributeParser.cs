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
    public readonly List<(IParseRule rule, IModifier modifier)> ruleModPairs = new();
    // Using LSList for ref access to AttributeSpan (avoids struct copying in RemapSpanIndices)
    internal readonly LSList<AttributeSpan> spans = new();

    // Все правила для итерации (кешируем для производительности)
    private readonly List<IParseRule> allRules = new();

    // Временный буфер для ParsedRange от каждого правила
    private readonly List<ParsedRange> tempRanges = new();

    // Reusable buffer for tag removals (avoids allocation per Parse call)
    // Using LSList for FakeClear optimization - (int, int) is pure value type
    private readonly LSList<(int start, int end)> tagRemovals = new();

    // Clean text после удаления тегов
    private readonly StringBuilder cleanTextBuilder = new();

    /// <summary>
    /// Clean text после парсинга (без тегов)
    /// </summary>
    public string CleanText { get; private set; }

    /// <summary>
    /// Зарегистрировать правило с модификатором уровня Itemization
    /// </summary>
    public void Register(IParseRule rule, IModifier modifier)
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
    public void DeinitializeModifiers(UniText uniText)
    {
        for (int i = 0; i < ruleModPairs.Count; i++)
        {
            ruleModPairs[i].modifier.Deinitialize(uniText);
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
        // Сброс состояния
        // FakeClear is safe for (int, int) tuple - pure value type
        spans.Clear();
        tagRemovals.FakeClear();
        cleanTextBuilder.Clear();

        foreach (var rule in allRules)
        {
            rule.Reset();
        }

        if (string.IsNullOrEmpty(text))
        {
            CleanText = string.Empty;
            return CleanText;
        }

        // Single-pass парсинг
        int index = 0;
        while (index < text.Length)
        {
            int newIndex = index;

            // Пробуем каждое правило
            foreach (var rule in allRules)
            {
                tempRanges.Clear();
                int result = rule.TryMatch(text, index, tempRanges);

                if (result > index)
                {
                    // Правило нашло что-то
                    newIndex = result;

                    // Обрабатываем найденные диапазоны
                    foreach (var range in tempRanges)
                    {
                        // Запоминаем теги для удаления
                        if (range.HasTags)
                        {
                            tagRemovals.Add((range.tagStart, range.tagEnd));
                            tagRemovals.Add((range.closeTagStart, range.closeTagEnd));
                        }

                        // Находим модификатор для этого правила и создаём span
                        CreateSpanForRule(rule, range);
                    }

                    break; // Правило сматчило — переходим к новой позиции
                }
            }

            if (newIndex > index)
            {
                index = newIndex;
            }
            else
            {
                index++;
            }
        }

        // Финализируем все правила — закрываем незакрытые теги
        foreach (var rule in allRules)
        {
            tempRanges.Clear();
            rule.Finalize(text.Length, tempRanges);

            foreach (var range in tempRanges)
            {
                if (range.HasTags)
                {
                    tagRemovals.Add((range.tagStart, range.tagEnd));
                    // closeTagStart == closeTagEnd для незакрытых тегов — пустой диапазон
                    if (range.closeTagStart != range.closeTagEnd)
                        tagRemovals.Add((range.closeTagStart, range.closeTagEnd));
                }

                CreateSpanForRule(rule, range);
            }
        }

        // Строим clean text и пересчитываем индексы
        BuildCleanTextAndRemapIndices(text, tagRemovals);
        
        return CleanText;
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

    private void BuildCleanTextAndRemapIndices(string text, LSList<(int start, int end)> tagRemovals)
    {
        // Сортируем удаления по позиции
        tagRemovals.Sort((a, b) => a.start.CompareTo(b.start));

        // Строим clean text и карту индексов (используем ArrayPool)
        int mapSize = text.Length + 1;
        var indexMap = ArrayPool<int>.Shared.Rent(mapSize);

        try
        {
            int offset = 0;
            int removalIndex = 0;

            for (int i = 0; i <= text.Length; i++)
            {
                // Проверяем, попадаем ли в удаляемый диапазон
                while (removalIndex < tagRemovals.Count && i >= tagRemovals[removalIndex].end)
                {
                    removalIndex++;
                }

                if (removalIndex < tagRemovals.Count && i >= tagRemovals[removalIndex].start && i < tagRemovals[removalIndex].end)
                {
                    // Внутри тега — увеличиваем offset
                    offset++;
                    indexMap[i] = -1; // Помечаем как удалённый
                }
                else
                {
                    indexMap[i] = i - offset;
                    if (i < text.Length)
                        cleanTextBuilder.Append(text[i]);
                }
            }

            CleanText = cleanTextBuilder.ToString();

            // Пересчитываем индексы во всех spans
            int cleanLen = CleanText.Length;
            RemapSpanIndices(spans, indexMap, mapSize, cleanLen);
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