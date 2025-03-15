// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to forward multiple messages of any kind. If some of the specified messages can't be found or forwarded, they are skipped. Service messages and messages with protected content can't be forwarded. Album grouping is kept for forwarded messages.<para>Returns: An array of <see cref="MessageId"/> of the sent messages is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ForwardMessagesRequest : RequestBase<MessageId[]>, IChatTargetable
    {
        /// <summary>Use this method to forward multiple messages of any kind. If some of the specified messages can't be found or forwarded, they are skipped. Service messages and messages with protected content can't be forwarded. Album grouping is kept for forwarded messages.<para>Returns: An array of <see cref="MessageId"/> of the sent messages is returned.</para></summary>
        public ForwardMessagesRequest() : base("forwardMessages")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier for the chat where the original messages were sent (or channel username in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("from_chat_id")]
        
        public ChatId FromChatId { get; set; }

        /// <summary>A list of 1-100 identifiers of messages in the chat <see cref="FromChatId">FromChatId</see> to forward. The identifiers must be specified in a strictly increasing order.</summary>
        [Newtonsoft.Json.JsonProperty("message_ids")]
        
        public IEnumerable<int> MessageIds { get; set; }

        /// <summary>Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        public int? MessageThreadId { get; set; }

        /// <summary>Sends the messages <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</summary>
        [Newtonsoft.Json.JsonProperty("disable_notification")]
        public bool DisableNotification { get; set; }

        /// <summary>Protects the contents of the forwarded messages from forwarding and saving</summary>
        [Newtonsoft.Json.JsonProperty("protect_content")]
        public bool ProtectContent { get; set; }
    }
}
