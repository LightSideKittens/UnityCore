// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types.Payments
{
    /// <summary>This object contains information about a paid media purchase.</summary>
    public partial class PaidMediaPurchased
    {
        /// <summary>User who purchased the media</summary>
        
        public User From { get; set; } = default!;

        /// <summary>Bot-specified paid media payload</summary>
        [Newtonsoft.Json.JsonProperty("paid_media_payload")]
        
        public string PaidMediaPayload { get; set; } = default!;
    }
}
