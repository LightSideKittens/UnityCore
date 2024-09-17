using System;

namespace LSCore.Extensions
{
    public static class RangeEx
    {
        public static bool TryParse(string text, out Range range)
        {
            range = default;

            if (string.IsNullOrEmpty(text))
                return false;

            var rangeSeparatorIndex = text.IndexOf("..");

            if (rangeSeparatorIndex == -1)
            {
                return false;
            }
            
            var startText = text[..rangeSeparatorIndex];
            var endText = text[(rangeSeparatorIndex + 2)..];

            Index start;
            
            if (string.IsNullOrEmpty(startText))
            {
                start = Index.Start;
            }
            else if(!IndexEx.TryParse(startText, out start))
            {
                return false;
            }
            
            Index end;
            
            if (string.IsNullOrEmpty(endText))
            {
                end = Index.End;
            }
            else if(!IndexEx.TryParse(endText, out end))
            {
                return false;
            }

            range = new Range(start, end);
            return true;
        }
        
        public static (int start, int end) Range(int count, Range range)
        {
            var start = range.Start.GetOffset(count);
            var end = range.End.GetOffset(count);

            return (start, end);
        }
    }
}