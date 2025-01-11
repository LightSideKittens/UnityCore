namespace LSCore.Extensions
{
    public static class PrimitiveExtensions
    {
        public static int ToPosNeg(this bool b) => b ? 1 : -1;
        public static int ToInt(this bool b) => b ? 1 : 0;
        public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
    }
}