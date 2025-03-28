// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types
{
    /// <summary>This object represents a point on the map.</summary>
    public partial class Location
    {
        /// <summary>Latitude as defined by the sender</summary>
        
        public double Latitude { get; set; }

        /// <summary>Longitude as defined by the sender</summary>
        
        public double Longitude { get; set; }

        /// <summary><em>Optional</em>. The radius of uncertainty for the location, measured in meters; 0-1500</summary>
        [Newtonsoft.Json.JsonProperty("horizontal_accuracy")]
        public double? HorizontalAccuracy { get; set; }

        /// <summary><em>Optional</em>. Time relative to the message sending date, during which the location can be updated; in seconds. For active live locations only.</summary>
        [Newtonsoft.Json.JsonProperty("live_period")]
        public int? LivePeriod { get; set; }

        /// <summary><em>Optional</em>. The direction in which user is moving, in degrees; 1-360. For active live locations only.</summary>
        public int? Heading { get; set; }

        /// <summary><em>Optional</em>. The maximum distance for proximity alerts about approaching another chat member, in meters. For sent live locations only.</summary>
        [Newtonsoft.Json.JsonProperty("proximity_alert_radius")]
        public int? ProximityAlertRadius { get; set; }
    }
}
