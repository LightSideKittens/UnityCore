using System;

namespace LSCore.Extensions
{
    public static class LSRange
    {
        public static bool TryParseRange(this string text, out Range range)
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

            var start = string.IsNullOrEmpty(startText) ? Index.Start : LSIndex.ParseIndex(startText);
            var end = string.IsNullOrEmpty(endText) ? Index.End : LSIndex.ParseIndex(endText);

            range = new Range(start, end);
            return true;
        }
    }
}