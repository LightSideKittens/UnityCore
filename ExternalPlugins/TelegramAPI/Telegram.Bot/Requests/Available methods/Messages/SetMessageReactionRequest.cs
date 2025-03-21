// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to change the chosen reactions on a message. Service messages of some types can't be reacted to. Automatically forwarded messages from a channel to its discussion group have the same available reactions as messages in the channel. Bots can't use paid reactions.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetMessageReactionRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Use this method to change the chosen reactions on a message. Service messages of some types can't be reacted to. Automatically forwarded messages from a channel to its discussion group have the same available reactions as messages in the channel. Bots can't use paid reactions.<para>Returns: </para></summary>
        public SetMessageReactionRequest() : base("setMessageReaction")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Identifier of the target message. If the message belongs to a media group, the reaction is set to the first non-deleted message in the group instead.</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

        /// <summary>A list of reaction types to set on the message. Currently, as non-premium users, bots can set up to one reaction per message. A custom emoji reaction can be used if it is either already present on the message or explicitly allowed by chat administrators. Paid reactions can't be used by bots.</summary>
        public IEnumerable<ReactionType>? Reaction { get; set; }

        /// <summary>Pass <see langword="true"/> to set the reaction with a big animation</summary>
        [Newtonsoft.Json.JsonProperty("is_big")]
        public bool IsBig { get; set; }
    }
}
