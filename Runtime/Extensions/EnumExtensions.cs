using System;

namespace LSCore.Extensions
{
    public static class EnumExtensions<T> where T : Enum
    {
        public static T[] Values { get; }
        
        static EnumExtensions()
        {
            Values = (T[])Enum.GetValues(typeof(T));
        }
    }
}