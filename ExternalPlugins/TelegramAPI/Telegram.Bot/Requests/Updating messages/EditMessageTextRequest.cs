// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit text and <a href="https://core.telegram.org/bots/api#games">game</a> messages.<para>Returns: The edited <see cref="Message"/> is returned</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditMessageTextRequest : RequestBase<Message>, IChatTargetable, IBusinessConnectable
    {
        /// <summary>Use this method to edit text and <a href="https://core.telegram.org/bots/api#games">game</a> messages.<para>Returns: The edited <see cref="Message"/> is returned</para></summary>
        public EditMessageTextRequest() : base("editMessageText")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Identifier of the message to edit</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

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
