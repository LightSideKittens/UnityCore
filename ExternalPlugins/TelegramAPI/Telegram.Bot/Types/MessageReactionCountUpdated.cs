// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents reaction changes on a message with anonymous reactions.</summary>
    public partial class MessageReactionCountUpdated
    {
        /// <summary>The chat containing the message</summary>
        
        public Chat Chat { get; set; } = default!;

        /// <summary>Unique message identifier inside the chat</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

        /// <summary>Date of the change</summary>
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }

        /// <summary>List of reactions that are present on the message</summary>
        
        public ReactionCount[] Reactions { get; set; } = default!;
    }
}
