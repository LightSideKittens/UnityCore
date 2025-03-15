// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit only the reply markup of messages.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditInlineMessageReplyMarkupRequest : RequestBase<bool>, IBusinessConnectable
    {
        /// <summary>Use this method to edit only the reply markup of messages.<para>Returns: </para></summary>
        public EditInlineMessageReplyMarkupRequest() : base("editMessageReplyMarkup")
        {
        }

        /// <summary>Identifier of the inline message</summary>
        [Newtonsoft.Json.JsonProperty("inline_message_id")]
        
        public string InlineMessageId { get; set; }

        /// <summary>An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message to be edited was sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
