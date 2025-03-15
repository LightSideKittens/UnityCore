// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to clear the list of pinned messages in a forum topic. The bot must be an administrator in the chat for this to work and must have the <em>CanPinMessages</em> administrator right in the supergroup.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class UnpinAllForumTopicMessagesRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Use this method to clear the list of pinned messages in a forum topic. The bot must be an administrator in the chat for this to work and must have the <em>CanPinMessages</em> administrator right in the supergroup.<para>Returns: </para></summary>
        public UnpinAllForumTopicMessagesRequest() : base("unpinAllForumTopicMessages")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup (in the format <c>@supergroupusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier for the target message thread of the forum topic</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        
        public int MessageThreadId { get; set; }
    }
}
