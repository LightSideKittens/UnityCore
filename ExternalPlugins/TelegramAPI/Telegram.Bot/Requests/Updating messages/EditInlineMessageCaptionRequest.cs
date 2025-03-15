// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit captions of messages.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditInlineMessageCaptionRequest : RequestBase<bool>, IBusinessConnectable
    {
        /// <summary>Use this method to edit captions of messages.<para>Returns: </para></summary>
        public EditInlineMessageCaptionRequest() : base("editMessageCaption")
        {
        }

        /// <summary>Identifier of the inline message</summary>
        [Newtonsoft.Json.JsonProperty("inline_message_id")]
        
        public string InlineMessageId { get; set; }

        /// <summary>New caption of the message, 0-1024 characters after entities parsing</summary>
        public string? Caption { get; set; }

        /// <summary>Mode for parsing entities in the message caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</summary>
        [Newtonsoft.Json.JsonProperty("parse_mode")]
        public ParseMode ParseMode { get; set; }

        /// <summary>A list of special entities that appear in the caption, which can be specified instead of <see cref="ParseMode">ParseMode</see></summary>
        [Newtonsoft.Json.JsonProperty("caption_entities")]
        public IEnumerable<MessageEntity>? CaptionEntities { get; set; }

        /// <summary>Pass <see langword="true"/>, if the caption must be shown above the message media. Supported only for animation, photo and video messages.</summary>
        [Newtonsoft.Json.JsonProperty("show_caption_above_media")]
        public bool ShowCaptionAboveMedia { get; set; }

        /// <summary>An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message to be edited was sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
