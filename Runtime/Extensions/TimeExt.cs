using System;

namespace LSCore.Extensions
{
    public static class TimeExt
    {
        public static DateTime ToDateTime(this long tick) => new (tick);
        public static TimeSpan ToTimeSpan(this long tick) => new (tick);
    }
}