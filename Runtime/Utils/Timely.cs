using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// Public API for game-ready duration and relative time formatting (Game-UX style).
/// Abbreviations are short, dotless, and unambiguous (e.g., y/mo/w/d/h/min/s).
/// Includes smart coarsening so tiny lower units are hidden for large spans.
/// </summary>
public static class Timely
{
    public enum Preset
    {
        /// <summary>Smart timer: &lt;1h → mm:ss, &lt;1d → hh:mm:ss, ≥1d → d HH:mm</summary>
        Timer,
        /// <summary>Compact with up to 2 units (uses smart coarsening).</summary>
        Compact2,
        /// <summary>Compact with up to 3 units (uses smart coarsening).</summary>
        Compact3,
        /// <summary>Single dominant unit, abbreviated.</summary>
        SingleUnit,
        /// <summary>Digital fixed hh:mm:ss (always shows hours).</summary>
        DigitalHMS,
        /// <summary>Digital mm:ss (for short cooldowns).</summary>
        DigitalMS
    }

    /// <summary>
    /// Minimum fraction of the leading unit that a subordinate unit must represent to be shown (0..1).
    /// Example: if leading is 11 months, a remainder day is ~0.3% → hidden when 0.1f (10%).
    /// </summary>
    public static double SecondaryUnitMinFraction { get; set; } = 0.10;

    private const int SecondsPerMinute = 60;
    private const int SecondsPerHour   = 60 * 60;
    private const int SecondsPerDay    = 24 * 60 * 60;
    private const int SecondsPerWeek   = 7 * SecondsPerDay;
    private const int SecondsPerMonth  = 30 * SecondsPerDay;
    private const int SecondsPerYear   = 365 * SecondsPerDay;

    public readonly struct Units
    {
        public readonly long Years, Months, Weeks, Days, Hours, Minutes, Seconds;
        public readonly long TotalDays;

        public Units(long years, long months, long weeks, long days, long hours, long minutes, long seconds, long totalDays)
        {
            Years = years; Months = months; Weeks = weeks; Days = days;
            Hours = hours; Minutes = minutes; Seconds = seconds; TotalDays = totalDays;
        }

        public bool IsZero =>
            Years == 0 && Months == 0 && Weeks == 0 && Days == 0 && Hours == 0 && Minutes == 0 && Seconds == 0;
    }

    private struct UnitSymbols
    {
        public UnitSymbols(string Year, string Month, string Week, string Day, string Hour, string Minute, string Second)
        {
            this.year = Year;
            this.month = Month;
            this.week = Week;
            this.day = Day;
            this.hour = Hour;
            this.minute = Minute;
            this.second = Second;
        }

        public string year;
        public string month;
        public string week;
        public string day;
        public string hour;
        public string minute;
        public string second;
    }

    private enum UnitKind { Year, Month, Week, Day, Hour, Minute, Second }

    private static readonly Dictionary<UnitKind, long> UnitSeconds = new()
    {
        [UnitKind.Year]   = SecondsPerYear,
        [UnitKind.Month]  = SecondsPerMonth,
        [UnitKind.Week]   = SecondsPerWeek,
        [UnitKind.Day]    = SecondsPerDay,
        [UnitKind.Hour]   = SecondsPerHour,
        [UnitKind.Minute] = SecondsPerMinute,
        [UnitKind.Second] = 1
    };

    private static readonly Dictionary<UnitKind, UnitKind[]> AllowedChain = new()
    {
        [UnitKind.Year]   = new[] { UnitKind.Month },
        [UnitKind.Month]  = new[] { UnitKind.Week, UnitKind.Day },
        [UnitKind.Week]   = new[] { UnitKind.Day, UnitKind.Hour },
        [UnitKind.Day]    = new[] { UnitKind.Hour, UnitKind.Minute },
        [UnitKind.Hour]   = new[] { UnitKind.Minute, UnitKind.Second },
        [UnitKind.Minute] = new[] { UnitKind.Second },
        [UnitKind.Second] = Array.Empty<UnitKind>()
    };

    private static readonly UnitSymbols Intl = new("y", "mo", "w", "d", "h", "min", "s");

    private static readonly Dictionary<string, UnitSymbols> Abbrev = new()
    {
        ["en"] = new("y", "mo", "w", "d", "h", "min", "s"),
        ["nl"] = new("j", "mnd", "wk", "d", "u", "min", "s"),
        ["fi"] = new("v", "kk", "vk", "pv", "h", "min", "s"),
        ["fr"] = new("an", "mois", "sem", "j", "h", "min", "s"),
        ["de"] = new("J", "Mon", "Wo", "T", "Std", "Min", "Sek"),
        ["id"] = new("th", "bln", "mgg", "hr", "j", "mnt", "dtk"),
        ["it"] = new("a", "mes", "sett", "g", "h", "min", "s"),
        ["ms"] = new("thn", "bln", "mgg", "hr", "j", "min", "s"),
        ["pl"] = new("r", "mies", "tyg", "d", "godz", "min", "sek"),
        ["pt"] = new("a", "mes", "sem", "d", "h", "min", "s"),
        ["ru"] = new("г", "мес", "нед", "д", "ч", "мин", "с"),
        ["es"] = new("a", "mes", "sem", "d", "h", "min", "s"),
        ["tr"] = new("y", "ay", "hf", "g", "sa", "dk", "sn"),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UnitSymbols GetSymbols(CultureInfo culture)
    {
        if (culture == null || culture == CultureInfo.InvariantCulture)
            return Intl;

        return Abbrev.TryGetValue(culture.TwoLetterISOLanguageName, out var s) ? s : Intl;
    }

    /// <summary>Override or add abbreviations at runtime (Game-UX style).</summary>
    public static void RegisterAbbreviations(
        string twoLetterIso,
        string year, string month, string week, string day,
        string hour, string minute, string second)
    {
        if (string.IsNullOrWhiteSpace(twoLetterIso)) return;
        Abbrev[twoLetterIso] = new UnitSymbols(year, month, week, day, hour, minute, second);
    }
    
    public static string Format(TimeSpan span,
        Preset preset,
        CultureInfo culture = null)
    {
        if (span < TimeSpan.Zero) span = TimeSpan.Zero;
        culture ??= CultureInfo.CurrentCulture;

        return preset switch
        {
            Preset.Timer       => FormatTimer(span, culture),
            Preset.Compact2    => FormatCompact(span, culture, maxUnits: 2),
            Preset.Compact3    => FormatCompact(span, culture, maxUnits: 3),
            Preset.SingleUnit  => FormatSingleUnit(span, culture),
            Preset.DigitalHMS  => FormatDigitalHMS(span),
            Preset.DigitalMS   => FormatDigitalMS(span),
            _                  => FormatTimer(span, culture)
        };
    }
    
    public static string FormatTimer(TimeSpan span, CultureInfo culture)
    {
        if (span.TotalHours < 1) return FormatDigitalMS(span);
        if (span.TotalDays  < 1) return FormatDigitalHMS(span);

        var sym       = GetSymbols(culture);
        long totalDay = (long)Math.Floor(span.TotalDays);
        return $"{totalDay}{sym.day} {span.Hours:00}:{span.Minutes:00}";
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatDigitalHMS(TimeSpan span)
    {
        int hours   = (int)Math.Floor(span.TotalHours);
        int minutes = span.Minutes;
        int seconds = span.Seconds;
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatDigitalMS(TimeSpan span)
    {
        int minutes = (int)Math.Floor(span.TotalMinutes);
        int seconds = span.Seconds;
        return $"{minutes:00}:{seconds:00}";
    }
    
    public static string FormatCompact(TimeSpan span,
        CultureInfo culture,
        int maxUnits = 2)
    {
        var u   = Breakdown(span);
        var sym = GetSymbols(culture);

        long[] values  = { u.Years, u.Months, u.Weeks, u.Days, u.Hours, u.Minutes, u.Seconds };
        string[] units = { sym.year, sym.month, sym.week, sym.day, sym.hour, sym.minute, sym.second };

        int lead = -1;
        for (int i = 0; i < values.Length; i++)
            if (values[i] > 0) { lead = i; break; }

        if (lead == -1)
            return $"0{sym.second}";

        var sb = new StringBuilder(24);
        void Append(int idx) => sb.Append(values[idx]).Append(units[idx]);

        static int NextLowerIndex(int idx) => idx switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            3 => 4,
            4 => 5,
            5 => 6,
            _ => -1
        };

        Append(lead);
        int printed = 1;

        if (printed < maxUnits)
        {
            int second = NextLowerIndex(lead);
            if (second != -1 && values[second] > 0)
            {
                sb.Append(' ');
                Append(second);
                printed++;

                if (printed < maxUnits)
                {
                    int third = NextLowerIndex(second);
                    if (third != -1 && values[third] > 0)
                    {
                        sb.Append(' ');
                        Append(third);
                    }
                }
            }
        }

        return sb.ToString();
    }


    public static string FormatSingleUnit(TimeSpan span, CultureInfo culture)
    {
        var u   = Breakdown(span);
        var sym = GetSymbols(culture);

        if (u.Years   > 0) return $"{u.Years}{sym.year}";
        if (u.Months  > 0) return $"{u.Months}{sym.month}";
        if (u.Weeks   > 0) return $"{u.Weeks}{sym.week}";
        if (u.Days    > 0) return $"{u.Days}{sym.day}";
        if (u.Hours   > 0) return $"{u.Hours}{sym.hour}";
        if (u.Minutes > 0) return $"{u.Minutes}{sym.minute}";
        return $"{u.Seconds}{sym.second}";
    }

    /// <summary>
    /// Breakdown into units with optional week/month/year approximations (game-friendly),
    /// using Math.DivRem to reduce ops/allocs.
    /// </summary>
    public static Units Breakdown(TimeSpan span)
    {
        long totalSeconds = (long)Math.Floor(span.TotalSeconds);
        long remaining    = totalSeconds;

        long years  = Math.DivRem(remaining, SecondsPerYear,  out remaining);
        long months = Math.DivRem(remaining, SecondsPerMonth, out remaining);
        long weeks  = Math.DivRem(remaining, SecondsPerWeek,  out remaining);
        long days   = Math.DivRem(remaining, SecondsPerDay,   out remaining);
        long hours  = Math.DivRem(remaining, SecondsPerHour,  out remaining);
        long minutes= Math.DivRem(remaining, SecondsPerMinute,out long seconds);

        return new Units(
            years, months, weeks, days, hours, minutes, seconds,
            totalDays: (long)Math.Floor(span.TotalDays)
        );
    }
}
