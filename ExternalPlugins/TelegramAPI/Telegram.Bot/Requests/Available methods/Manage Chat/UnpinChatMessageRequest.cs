// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to remove a message from the list of pinned messages in a chat. If the chat is not a private chat, the bot must be an administrator in the chat for this to work and must have the 'CanPinMessages' administrator right in a supergroup or 'CanEditMessages' administrator right in a channel.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class UnpinChatMessageRequest : RequestBase<bool>, IChatTargetable, IBusinessConnectable
    {
        /// <summary>Use this method to remove a message from the list of pinned messages in a chat. If the chat is not a private chat, the bot must be an administrator in the chat for this to work and must have the 'CanPinMessages' administrator right in a supergroup or 'CanEditMessages' administrator right in a channel.<para>Returns: </para></summary>
        public UnpinChatMessageRequest() : base("unpinChatMessage")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Identifier of the message to unpin. Required if <see cref="BusinessConnectionId">BusinessConnectionId</see> is specified. If not specified, the most recent pinned message (by sending date) will be unpinned.</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        public int? MessageId { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message will be unpinned</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
