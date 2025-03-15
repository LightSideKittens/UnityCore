// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types
{
    /// <summary>Describes the opening hours of a business.</summary>
    public partial class BusinessOpeningHours
    {
        /// <summary>Unique name of the time zone for which the opening hours are defined</summary>
        [Newtonsoft.Json.JsonProperty("time_zone_name")]
        
        public string TimeZoneName { get; set; } = default!;

        /// <summary>List of time intervals describing business opening hours</summary>
        [Newtonsoft.Json.JsonProperty("opening_hours")]
        
        public BusinessOpeningHoursInterval[] OpeningHours { get; set; } = default!;
    }
}
