using System;

namespace LSCore.Extensions.Time
{
    public static class Ext
    {
        public static DateTime ToDateTime(this long tick) => new (tick);
        public static TimeSpan ToTimeSpan(this long tick)
        {
            return tick is 1 or 0 ? TimeSpan.Zero : new TimeSpan(tick);
        }
    }
}