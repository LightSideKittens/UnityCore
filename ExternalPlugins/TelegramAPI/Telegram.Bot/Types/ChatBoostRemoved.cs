// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a boost removed from a chat.</summary>
    public partial class ChatBoostRemoved
    {
        /// <summary>Chat which was boosted</summary>
        
        public Chat Chat { get; set; } = default!;

        /// <summary>Unique identifier of the boost</summary>
        [Newtonsoft.Json.JsonProperty("boost_id")]
        
        public string BoostId { get; set; } = default!;

        /// <summary>Point in time when the boost was removed</summary>
        [Newtonsoft.Json.JsonProperty("remove_date")]
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime RemoveDate { get; set; }

        /// <summary>Source of the removed boost</summary>
        
        public ChatBoostSource Source { get; set; } = default!;
    }
}
