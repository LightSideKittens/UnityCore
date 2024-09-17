using System;

namespace LSCore.Extensions
{
    public static class IndexEx
    {
        public static bool TryParse(string text, out Index index)
        {
            index = default;

            if (string.IsNullOrEmpty(text))
                return false;
            
            var isFromEnd = text[0] == '^';
            var valueStart = isFromEnd ? 1 : 0;

            if (int.TryParse(text[valueStart..], out var value) && ((isFromEnd && value > 0) || (!isFromEnd && value > -1)))
            {
                index = new Index(value, isFromEnd);
                return true;
            }
            
            return false;
        }
    }
}