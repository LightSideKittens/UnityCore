// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to send a group of photos, videos, documents or audios as an album. Documents and audio files can be only grouped in an album with messages of the same type.<para>Returns: An array of <see cref="Message">Messages</see> that were sent is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SendMediaGroupRequest : FileRequestBase<Message[]>, IChatTargetable, IBusinessConnectable
    {
        /// <summary>Use this method to send a group of photos, videos, documents or audios as an album. Documents and audio files can be only grouped in an album with messages of the same type.<para>Returns: An array of <see cref="Message">Messages</see> that were sent is returned.</para></summary>
        public SendMediaGroupRequest() : base("sendMediaGroup")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>A array describing messages to be sent, must include 2-10 items</summary>
        
        public IEnumerable<IAlbumInputMedia> Media { get; set; }

        /// <summary>Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        public int? MessageThreadId { get; set; }

        /// <summary>Sends messages <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</summary>
        [Newtonsoft.Json.JsonProperty("disable_notification")]
        public bool DisableNotification { get; set; }

        /// <summary>Protects the contents of the sent messages from forwarding and saving</summary>
        [Newtonsoft.Json.JsonProperty("protect_content")]
        public bool ProtectContent { get; set; }

        /// <summary>Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</summary>
        [Newtonsoft.Json.JsonProperty("allow_paid_broadcast")]
        public bool AllowPaidBroadcast { get; set; }

        /// <summary>Unique identifier of the message effect to be added to the message; for private chats only</summary>
        [Newtonsoft.Json.JsonProperty("message_effect_id")]
        public string? MessageEffectId { get; set; }

        /// <summary>Description of the message to reply to</summary>
        [Newtonsoft.Json.JsonProperty("reply_parameters")]
        public ReplyParameters? ReplyParameters { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message will be sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
