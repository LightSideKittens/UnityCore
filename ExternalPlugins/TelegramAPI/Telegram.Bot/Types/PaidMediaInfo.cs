// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types
{
    /// <summary>Describes the paid media added to a message.</summary>
    public partial class PaidMediaInfo
    {
        /// <summary>The number of Telegram Stars that must be paid to buy access to the media</summary>
        [Newtonsoft.Json.JsonProperty("star_count")]
        
        public int StarCount { get; set; }

        /// <summary>Information about the paid media</summary>
        [Newtonsoft.Json.JsonProperty("paid_media")]
        
        public PaidMedia[] PaidMedia { get; set; } = default!;
    }
}
