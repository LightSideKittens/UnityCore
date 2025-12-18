using System;
using System.Collections.Generic;
using LSCore.Extensions;

[Serializable]
public class RangeRule : IParseRule
{
    [Serializable]
    public struct Data
    {
        public string range;
        public string parameter;
    }
    
    public List<Data> data;
    private Range currentRange;
    
    public int TryMatch(string text, int index, IList<ParsedRange> results)
    {
        return index;
    }

    public void Finalize(int textLength, IList<ParsedRange> results)
    {
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            if (!RangeEx.TryParse(d.range, out currentRange))
            {
                RangeEx.TryParse("..", out currentRange);
            }

            var r = currentRange.GetOffsetAndLength(textLength);
            var start = r.Offset;
            var end = r.Offset + r.Length;
            results.Add(new ParsedRange(start, end, d.parameter));
        }
    }

    public void Reset() { }
}