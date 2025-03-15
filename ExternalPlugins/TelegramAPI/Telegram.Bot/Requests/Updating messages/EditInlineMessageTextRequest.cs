// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit text and <a href="https://core.telegram.org/bots/api#games">game</a> messages.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditInlineMessageTextRequest : RequestBase<bool>, IBusinessConnectable
    {
        /// <summary>Use this method to edit text and <a href="https://core.telegram.org/bots/api#games">game</a> messages.<para>Returns: </para></summary>
        public EditInlineMessageTextRequest() : base("editMessageText")
        {
        }

        /// <summary>Identifier of the inline message</summary>
        [Newtonsoft.Json.JsonProperty("inline_message_id")]
        
        public string InlineMessageId { get; set; }

        /// <summary>New text of the message, 1-4096 characters after entities parsing</summary>
        
        public string Text { get; set; }

        /// <summary>Mode for parsing entities in the message text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</summary>
        [Newtonsoft.Json.JsonProperty("parse_mode")]
        public ParseMode ParseMode { get; set; }

        /// <summary>A list of special entities that appear in message text, which can be specified instead of <see cref="ParseMode">ParseMode</see></summary>
        public IEnumerable<MessageEntity>? Entities { get; set; }

        /// <summary>Link preview generation options for the message</summary>
        [Newtonsoft.Json.JsonProperty("link_preview_options")]
        public LinkPreviewOptions? LinkPreviewOptions { get; set; }

        /// <summary>An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message to be edited was sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
