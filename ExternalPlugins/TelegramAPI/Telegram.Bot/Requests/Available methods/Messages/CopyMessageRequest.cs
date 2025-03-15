// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to copy messages of any kind. Service messages, paid media messages, giveaway messages, giveaway winners messages, and invoice messages can't be copied. A quiz <see cref="Poll"/> can be copied only if the value of the field <em>CorrectOptionId</em> is known to the bot. The method is analogous to the method <see cref="TelegramBotClientExtensions.ForwardMessage">ForwardMessage</see>, but the copied message doesn't have a link to the original message.<para>Returns: The <see cref="MessageId"/> of the sent message on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class CopyMessageRequest : RequestBase<MessageId>, IChatTargetable
    {
        /// <summary>Use this method to copy messages of any kind. Service messages, paid media messages, giveaway messages, giveaway winners messages, and invoice messages can't be copied. A quiz <see cref="Poll"/> can be copied only if the value of the field <em>CorrectOptionId</em> is known to the bot. The method is analogous to the method <see cref="TelegramBotClientExtensions.ForwardMessage">ForwardMessage</see>, but the copied message doesn't have a link to the original message.<para>Returns: The <see cref="MessageId"/> of the sent message on success.</para></summary>
        public CopyMessageRequest() : base("copyMessage")
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

        /// <summary>New start timestamp for the copied video in the message</summary>
        [Newtonsoft.Json.JsonProperty("video_start_timestamp")]
        public int? VideoStartTimestamp { get; set; }

        /// <summary>New caption for media, 0-1024 characters after entities parsing. If not specified, the original caption is kept</summary>
        public string? Caption { get; set; }

        /// <summary>Mode for parsing entities in the new caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</summary>
        [Newtonsoft.Json.JsonProperty("parse_mode")]
        public ParseMode ParseMode { get; set; }

        /// <summary>A list of special entities that appear in the new caption, which can be specified instead of <see cref="ParseMode">ParseMode</see></summary>
        [Newtonsoft.Json.JsonProperty("caption_entities")]
        public IEnumerable<MessageEntity>? CaptionEntities { get; set; }

        /// <summary>Pass <see langword="true"/>, if the caption must be shown above the message media. Ignored if a new caption isn't specified.</summary>
        [Newtonsoft.Json.JsonProperty("show_caption_above_media")]
        public bool ShowCaptionAboveMedia { get; set; }

        /// <summary>Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</summary>
        [Newtonsoft.Json.JsonProperty("disable_notification")]
        public bool DisableNotification { get; set; }

        /// <summary>Protects the contents of the sent message from forwarding and saving</summary>
        [Newtonsoft.Json.JsonProperty("protect_content")]
        public bool ProtectContent { get; set; }

        /// <summary>Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</summary>
        [Newtonsoft.Json.JsonProperty("allow_paid_broadcast")]
        public bool AllowPaidBroadcast { get; set; }

        /// <summary>Description of the message to reply to</summary>
        [Newtonsoft.Json.JsonProperty("reply_parameters")]
        public ReplyParameters? ReplyParameters { get; set; }

        /// <summary>Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public ReplyMarkup? ReplyMarkup { get; set; }
    }
}
