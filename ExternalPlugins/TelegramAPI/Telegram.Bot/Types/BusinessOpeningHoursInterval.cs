// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types
{
    /// <summary>Describes an interval of time during which a business is open.</summary>
    public partial class BusinessOpeningHoursInterval
    {
        /// <summary>The minute's sequence number in a week, starting on Monday, marking the start of the time interval during which the business is open; 0 - 7 * 24 * 60</summary>
        [Newtonsoft.Json.JsonProperty("opening_minute")]
        
        public int OpeningMinute { get; set; }

        /// <summary>The minute's sequence number in a week, starting on Monday, marking the end of the time interval during which the business is open; 0 - 8 * 24 * 60</summary>
        [Newtonsoft.Json.JsonProperty("closing_minute")]
        
        public int ClosingMinute { get; set; }
    }
}
