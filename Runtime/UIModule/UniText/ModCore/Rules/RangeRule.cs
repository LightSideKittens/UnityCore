using System;
using System.Collections.Generic;
using LSCore.Extensions;

[Serializable]
public class RangeRule : IParseRule
{
    public string range;
    public string parameter;
    private Range currentRange;
    
    public int TryMatch(string text, int index, List<ParsedRange> results)
    {
        return index;
    }

    public void Finalize(int textLength, List<ParsedRange> results)
    {
        if (!RangeEx.TryParse(range, out currentRange))
        {
            RangeEx.TryParse("..", out currentRange);
        }

        var r = currentRange.GetOffsetAndLength(textLength);
        var start = r.Offset;
        var end = r.Offset + r.Length;
        results.Add(new ParsedRange(start, end, parameter));
    }

    public void Reset() { }
}