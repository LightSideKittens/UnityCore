// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Requests
{
    /// <summary>Sends a gift to the given user or channel chat. The gift can't be converted to Telegram Stars by the receiver.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SendGiftRequest : RequestBase<bool>
    {
        /// <summary>Sends a gift to the given user or channel chat. The gift can't be converted to Telegram Stars by the receiver.<para>Returns: </para></summary>
        public SendGiftRequest() : base("sendGift")
        {
        }

        /// <summary>Identifier of the gift</summary>
        [Newtonsoft.Json.JsonProperty("gift_id")]
        
        public string GiftId { get; set; }

        /// <summary>Required if <see cref="ChatId">ChatId</see> is not specified. Unique identifier of the target user who will receive the gift.</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        public long? UserId { get; set; }

        /// <summary>Required if <see cref="UserId">UserId</see> is not specified. Unique identifier for the chat or username of the channel (in the format <c>@channelusername</c>) that will receive the gift.</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        public ChatId? ChatId { get; set; }

        /// <summary>Pass <see langword="true"/> to pay for the gift upgrade from the bot's balance, thereby making the upgrade free for the receiver</summary>
        [Newtonsoft.Json.JsonProperty("pay_for_upgrade")]
        public bool PayForUpgrade { get; set; }

        /// <summary>Text that will be shown along with the gift; 0-128 characters</summary>
        public string? Text { get; set; }

        /// <summary>Mode for parsing entities in the text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details. Entities other than <see cref="MessageEntityType.Bold">Bold</see>, <see cref="MessageEntityType.Italic">Italic</see>, <see cref="MessageEntityType.Underline">Underline</see>, <see cref="MessageEntityType.Strikethrough">Strikethrough</see>, <see cref="MessageEntityType.Spoiler">Spoiler</see>, and <see cref="MessageEntityType.CustomEmoji">CustomEmoji</see> are ignored.</summary>
        [Newtonsoft.Json.JsonProperty("text_parse_mode")]
        public ParseMode TextParseMode { get; set; }

        /// <summary>A list of special entities that appear in the gift text. It can be specified instead of <see cref="TextParseMode">TextParseMode</see>. Entities other than <see cref="MessageEntityType.Bold">Bold</see>, <see cref="MessageEntityType.Italic">Italic</see>, <see cref="MessageEntityType.Underline">Underline</see>, <see cref="MessageEntityType.Strikethrough">Strikethrough</see>, <see cref="MessageEntityType.Spoiler">Spoiler</see>, and <see cref="MessageEntityType.CustomEmoji">CustomEmoji</see> are ignored.</summary>
        [Newtonsoft.Json.JsonProperty("text_entities")]
        public IEnumerable<MessageEntity>? TextEntities { get; set; }
    }
}
