// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>Describes an inline message to be sent by a user of a Mini App.</summary>
    public partial class PreparedInlineMessage
    {
        /// <summary>Unique identifier of the prepared message</summary>
        
        public string Id { get; set; } = default!;

        /// <summary>Expiration date of the prepared message,. Expired prepared messages can no longer be used</summary>
        [Newtonsoft.Json.JsonProperty("expiration_date")]
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime ExpirationDate { get; set; }
    }
}
