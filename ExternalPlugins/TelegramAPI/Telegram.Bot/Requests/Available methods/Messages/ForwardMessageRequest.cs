// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to forward messages of any kind. Service messages and messages with protected content can't be forwarded.<para>Returns: The sent <see cref="Message"/> is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ForwardMessageRequest : RequestBase<Message>, IChatTargetable
    {
        /// <summary>Use this method to forward messages of any kind. Service messages and messages with protected content can't be forwarded.<para>Returns: The sent <see cref="Message"/> is returned.</para></summary>
        public ForwardMessageRequest() : base("forwardMessage")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier for the chat where the original message was sent (or channel username in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("from_chat_id")]
        
        public ChatId FromChatId { get; set; }

        /// <summary>Message identifier in the chat specified in <see cref="FromChatId">FromChatId</see></summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

        /// <summary>Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        public int? MessageThreadId { get; set; }

        /// <summary>New start timestamp for the forwarded video in the message</summary>
        [Newtonsoft.Json.JsonProperty("video_start_timestamp")]
        public int? VideoStartTimestamp { get; set; }

        /// <summary>Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</summary>
        [Newtonsoft.Json.JsonProperty("disable_notification")]
        public bool DisableNotification { get; set; }

        /// <summary>Protects the contents of the forwarded message from forwarding and saving</summary>
        [Newtonsoft.Json.JsonProperty("protect_content")]
        public bool ProtectContent { get; set; }
    }
}
