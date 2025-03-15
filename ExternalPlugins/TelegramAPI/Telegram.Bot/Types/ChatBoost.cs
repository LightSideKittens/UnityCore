// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object contains information about a chat boost.</summary>
    public partial class ChatBoost
    {
        /// <summary>Unique identifier of the boost</summary>
        [Newtonsoft.Json.JsonProperty("boost_id")]
        
        public string BoostId { get; set; } = default!;

        /// <summary>Point in time when the chat was boosted</summary>
        [Newtonsoft.Json.JsonProperty("add_date")]
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime AddDate { get; set; }

        /// <summary>Point in time when the boost will automatically expire, unless the booster's Telegram Premium subscription is prolonged</summary>
        [Newtonsoft.Json.JsonProperty("expiration_date")]
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime ExpirationDate { get; set; }

        /// <summary>Source of the added boost</summary>
        
        public ChatBoostSource Source { get; set; } = default!;
    }
}
