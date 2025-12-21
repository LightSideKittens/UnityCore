using System;
using System.Collections.Generic;


public sealed class AttributeParser
{
    public readonly List<(IParseRule rule, BaseModifier modifier)> ruleModPairs = new();
    internal readonly PooledList<AttributeSpan> spans = new();

    private readonly List<IParseRule> allRules = new();

    private readonly PooledList<ParsedRange> tempRanges = new(32);

    private readonly PooledList<(int start, int end)> tagRemovals = new();
    private readonly PooledList<(int start, int end, string insert)> tagInsertions = new();

    private static readonly RemovalComparerImpl RemovalComparer = new();
    private static readonly InsertionComparerImpl InsertionComparer = new();

    private sealed class RemovalComparerImpl : IComparer<(int start, int end)>
    {
        public int Compare((int start, int end) a, (int start, int end) b)
        {
            return a.start.CompareTo(b.start);
        }
    }

    private sealed class InsertionComparerImpl : IComparer<(int start, int end, string insert)>
    {
        public int Compare((int start, int end, string insert) a, (int start, int end, string insert) b)
        {
            return a.start.CompareTo(b.start);
        }
    }

    private static int[] sharedIndexMap = new int[1024];
    private static char[] sharedCleanTextBuffer = new char[1024];

    private int cleanTextLength;


    public ReadOnlySpan<char> CleanTextSpan => sharedCleanTextBuffer.AsSpan(0, cleanTextLength);

    private string cachedCleanTextString;

    public string CleanText
    {
        get
        {
            if (cachedCleanTextString == null && cleanTextLength > 0)
                cachedCleanTextString = new string(sharedCleanTextBuffer, 0, cleanTextLength);
            return cachedCleanTextString ?? string.Empty;
        }
    }


    public void Register(IParseRule rule, BaseModifier modifier)
    {
        ruleModPairs.Add((rule, modifier));
        allRules.Add(rule);
    }


    public void Unregister(BaseModifier modifier)
    {
        for (var i = ruleModPairs.Count - 1; i >= 0; i--)
            if (ruleModPairs[i].modifier == modifier)
            {
                allRules.Remove(ruleModPairs[i].rule);
                ruleModPairs.RemoveAt(i);
                break;
            }
    }
    
    public void InitializeModifiers(UniText uniText)
    {
        for (var i = 0; i < ruleModPairs.Count; i++) ruleModPairs[i].modifier.Initialize(uniText);
    }

    public void DeinitializeModifiers()
    {
        for (var i = 0; i < ruleModPairs.Count; i++) ruleModPairs[i].modifier.Deinitialize();
    }

    public void Release()
    {
        spans.Return();
        tempRanges.Return();
        tagRemovals.Return();
        tagInsertions.Return();
    }

    public void ResetModifiers()
    {
        for (var i = 0; i < ruleModPairs.Count; i++) ruleModPairs[i].modifier.Reset();
    }

    public void Apply()
    {
        for (var i = spans.Count - 1; i >= 0; i--)
        {
            ref readonly var span = ref spans[i];
            span.modifier.Apply(span.start, span.end, span.parameter);
        }
    }


    /// <param name="text">Исходный текст с разметкой</param>
    /// <returns>Clean text без тегов</returns>
    public void Parse(string text)
    {
        spans.FakeClear();
        tagRemovals.FakeClear();
        tagInsertions.FakeClear();
        cleanTextLength = 0;
        cachedCleanTextString = null;

        for (var r = 0; r < allRules.Count; r++)
            allRules[r].Reset();

        if (string.IsNullOrEmpty(text))
        {
            cleanTextLength = 0;
            return;
        }

        var index = 0;
        while (index < text.Length)
        {
            var newIndex = index;

            for (var r = 0; r < allRules.Count; r++)
            {
                var rule = allRules[r];
                tempRanges.FakeClear();
                var result = rule.TryMatch(text, index, tempRanges);

                if (result > index)
                {
                    newIndex = result;
                    for (var j = 0; j < tempRanges.Count; j++)
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

        for (var r = 0; r < allRules.Count; r++)
        {
            var rule = allRules[r];
            tempRanges.FakeClear();
            rule.Finalize(text.Length, tempRanges);

            for (var j = 0; j < tempRanges.Count; j++)
            {
                ref readonly var range = ref tempRanges[j];
                ProcessRange(in range);
                CreateSpanForRule(rule, in range);
            }
        }

        BuildCleanTextAndRemapIndices(text);
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
        for (var i = 0; i < ruleModPairs.Count; i++)
            if (ReferenceEquals(ruleModPairs[i].rule, rule))
            {
                spans.Add(new AttributeSpan(range.start, range.end, ruleModPairs[i].modifier, range.parameter));
                return;
            }
    }

    private void BuildCleanTextAndRemapIndices(string text)
    {
        tagRemovals.Sort(0, tagRemovals.Count, RemovalComparer);
        tagInsertions.Sort(0, tagInsertions.Count, InsertionComparer);

        var mapSize = text.Length + 1;

        if (sharedIndexMap.Length < mapSize)
            sharedIndexMap = new int[Math.Max(mapSize, sharedIndexMap.Length * 2)];
        var indexMap = sharedIndexMap;

        EnsureCleanTextCapacity(text.Length);

        var offset = 0;
        var removalIdx = 0;
        var insertionIdx = 0;

        for (var i = 0; i <= text.Length; i++)
        {
            while (removalIdx < tagRemovals.Count && i >= tagRemovals[removalIdx].end)
                removalIdx++;

            if (insertionIdx < tagInsertions.Count && i == tagInsertions[insertionIdx].start)
            {
                var ins = tagInsertions[insertionIdx];
                indexMap[i] = cleanTextLength;
                AppendToCleanText(ins.insert);
                offset += ins.end - ins.start - ins.insert.Length;
                i = ins.end - 1;
                insertionIdx++;
                continue;
            }

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

        sharedCleanTextBuffer = new char[Math.Max(required, sharedCleanTextBuffer.Length * 2)];
    }

    private void AppendToCleanText(string str)
    {
        if (string.IsNullOrEmpty(str)) return;

        EnsureCleanTextCapacity(cleanTextLength + str.Length);
        str.AsSpan().CopyTo(sharedCleanTextBuffer.AsSpan(cleanTextLength));
        cleanTextLength += str.Length;
    }

    private static void RemapSpanIndices(PooledList<AttributeSpan> spans, int[] indexMap, int mapLength,
        int cleanTextLength)
    {
        for (var i = 0; i < spans.Count; i++)
        {
            ref var span = ref spans[i];

            var startIdx = span.start < 0 ? 0 : span.start >= mapLength ? mapLength - 1 : span.start;
            var endIdx = span.end < 0 ? 0 : span.end >= mapLength ? mapLength - 1 : span.end;

            var newStart = indexMap[startIdx];
            var newEnd = indexMap[endIdx];

            if (newStart < 0)
            {
                for (var j = startIdx; j < mapLength; j++)
                    if (indexMap[j] >= 0)
                    {
                        newStart = indexMap[j];
                        break;
                    }

                if (newStart < 0)
                    for (var j = startIdx - 1; j >= 0; j--)
                        if (indexMap[j] >= 0)
                        {
                            newStart = indexMap[j];
                            break;
                        }
            }

            if (newEnd < 0)
            {
                for (var j = endIdx; j < mapLength; j++)
                    if (indexMap[j] >= 0)
                    {
                        newEnd = indexMap[j];
                        break;
                    }

                if (newEnd < 0)
                    for (var j = endIdx - 1; j >= 0; j--)
                        if (indexMap[j] >= 0)
                        {
                            newEnd = indexMap[j];
                            break;
                        }
            }

            if (newStart < 0) newStart = 0;
            if (newEnd < 0) newEnd = cleanTextLength;
            if (newEnd > cleanTextLength) newEnd = cleanTextLength;
            if (newStart > newEnd) newStart = newEnd;

            span.start = newStart;
            span.end = newEnd;
        }
    }
}