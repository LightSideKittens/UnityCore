// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a change of a reaction on a message performed by a user.</summary>
    public partial class MessageReactionUpdated
    {
        /// <summary>The chat containing the message the user reacted to</summary>
        
        public Chat Chat { get; set; } = default!;

        /// <summary>Unique identifier of the message inside the chat</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

        /// <summary><em>Optional</em>. The user that changed the reaction, if the user isn't anonymous</summary>
        public User? User { get; set; }

        /// <summary><em>Optional</em>. The chat on behalf of which the reaction was changed, if the user is anonymous</summary>
        [Newtonsoft.Json.JsonProperty("actor_chat")]
        public Chat? ActorChat { get; set; }

        /// <summary>Date of the change</summary>
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }

        /// <summary>Previous list of reaction types that were set by the user</summary>
        [Newtonsoft.Json.JsonProperty("old_reaction")]
        
        public ReactionType[] OldReaction { get; set; } = default!;

        /// <summary>New list of reaction types that have been set by the user</summary>
        [Newtonsoft.Json.JsonProperty("new_reaction")]
        
        public ReactionType[] NewReaction { get; set; } = default!;
    }
}
