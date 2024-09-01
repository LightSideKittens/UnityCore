using System;

namespace LSCore.Extensions
{
    public static class LSIndex
    {
        public static bool TryParseIndex(this string text, out Index index)
        {
            index = default;

            if (string.IsNullOrEmpty(text))
                return false;

            var isFromEnd = text[0] == '^';
            var valueStart = isFromEnd ? 1 : 0;

            if (int.TryParse(text[valueStart..], out var value))
            {
                index = new Index(value, isFromEnd);
                return true;
            }

            return false;
        }

        internal static Index ParseIndex(string text)
        {
            var isFromEnd = text[0] == '^';
            var valueStart = isFromEnd ? 1 : 0;
            var value = int.Parse(text[valueStart..]);
            return new Index(value, isFromEnd);
        }
    }
}