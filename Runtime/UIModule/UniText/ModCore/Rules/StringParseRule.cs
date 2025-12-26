using System;

[Serializable]
public sealed class StringParseRule : IParseRule
{
    [EscapeTextArea(1, 3)]
    public string[] patterns;
    public bool hasReplacement;
    [EscapeTextArea(1, 3)]
    public string replacement;

    public int TryMatch(string text, int index, PooledList<ParsedRange> results)
    {
        if (patterns == null || patterns.Length == 0)
            return index;

        for (var p = 0; p < patterns.Length; p++)
        {
            var pattern = patterns[p];
            if (string.IsNullOrEmpty(pattern))
                continue;

            var patternLen = pattern.Length;
            if (index + patternLen > text.Length)
                continue;

            var matched = true;
            for (var i = 0; i < patternLen; i++)
            {
                if (text[index + i] != pattern[i])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                var matchEnd = index + patternLen;
                results.Add(ParsedRange.SelfClosing(index, matchEnd, hasReplacement ? (replacement ?? string.Empty) : string.Empty));
                return matchEnd;
            }
        }

        return index;
    }
}