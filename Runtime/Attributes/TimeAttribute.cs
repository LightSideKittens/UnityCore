using System;

namespace LSCore.Attributes
{
    public abstract class TimeAttribute : Attribute
    {
        public Options options;

        [Flags]
        public enum Options
        {
            Year = 1,
            Month = 1 << 1,
            Day = 1 << 2,
            Hour = 1 << 3,
            Minute = 1 << 4,
            Second = 1 << 5,
            Millisecond = 1 << 6,
            
            Default = FullDate | Timer,
            FullDateTime = FullDate | FullTime,
            FullDate = Year | Date,
            FullTime = Timer | Millisecond,
            
            Date = Month | Day,
            Timer = WatchTime | Second,
            
            WatchTime = Hour | Minute,
            WatchDateTime = Date | WatchTime
        }

        public TimeAttribute(Options options = Options.Default)
        {
            this.options = options;
        }
    }

    public class DateTimeAttribute : TimeAttribute
    {
        public DateTime defaultValue;

        public DateTimeAttribute(int addHours, int addMinutes, int addSeconds)
        {
            defaultValue = DateTime.Now;
            defaultValue = defaultValue.AddHours(addHours);
            defaultValue = defaultValue.AddMinutes(addMinutes);
            defaultValue = defaultValue.AddSeconds(addSeconds);
        }

        public DateTimeAttribute(int addDays, int addHours, int addMinutes, int addSeconds) : this(addHours, addMinutes, addSeconds)
        {
            defaultValue = defaultValue.AddDays(addDays);
        }

        public DateTimeAttribute(int addDays, int addHours, int addMinutes, int addSeconds, int addMilliseconds) : this(addDays, addHours,
            addMinutes, addSeconds)
        {
            defaultValue = defaultValue.AddMilliseconds(addMilliseconds);
        }
        
        public DateTimeAttribute() : this(4, 0, 0) { }
    }

    public class MinTimeSpanAttribute : Attribute
    {
        public TimeSpan value;
        
        public MinTimeSpanAttribute(long ticks) => value = new TimeSpan(ticks);
        public MinTimeSpanAttribute(int hours, int minutes, int seconds) => value = new TimeSpan(hours, minutes, seconds);
        public MinTimeSpanAttribute(int days, int hours, int minutes, int seconds) : this(days, hours, minutes, seconds, 0) { }
        public MinTimeSpanAttribute(int days, int hours, int minutes, int seconds, int milliseconds) => value = new TimeSpan(days, hours, minutes, seconds, milliseconds);
    }

    public class MaxTimeSpanAttribute : MinTimeSpanAttribute
    {
        public MaxTimeSpanAttribute(long ticks) : base(ticks) { }
        public MaxTimeSpanAttribute(int hours, int minutes, int seconds) : base(hours, minutes, seconds) { }
        public MaxTimeSpanAttribute(int days, int hours, int minutes, int seconds) : base(days, hours, minutes, seconds) { }
        public MaxTimeSpanAttribute(int days, int hours, int minutes, int seconds, int milliseconds) : base(days, hours, minutes, seconds, milliseconds) { }
    }

    public class TimeSpanAttribute : TimeAttribute
    {
        public static TimeSpan DefaultValue { get; } = new(1, 0, 0);
        
        public TimeSpan defaultValue;
        
        public TimeSpanAttribute()
        {
            options = Options.Day | Options.FullTime;
            defaultValue = DefaultValue;
        }
        
        public TimeSpanAttribute(long ticks)
        {
            options = Options.Day | Options.FullTime;
            defaultValue = new TimeSpan(ticks);
        }

        public TimeSpanAttribute(int hours, int minutes, int seconds)
        {
            options = Options.Day | Options.FullTime;
            defaultValue = new TimeSpan(hours, minutes, seconds);
        }

        public TimeSpanAttribute(int days, int hours, int minutes, int seconds)
            : this(days, hours, minutes, seconds, 0)
        {
            
        }
        
        public TimeSpanAttribute(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            options = Options.Day | Options.FullTime;
            defaultValue = new TimeSpan(days, hours, minutes, seconds, milliseconds);
        }
    }
}