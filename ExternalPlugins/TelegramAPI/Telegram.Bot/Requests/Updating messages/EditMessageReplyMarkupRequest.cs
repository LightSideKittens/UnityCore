// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit only the reply markup of messages.<para>Returns: The edited <see cref="Message"/> is returned</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditMessageReplyMarkupRequest : RequestBase<Message>, IChatTargetable, IBusinessConnectable
    {
        /// <summary>Use this method to edit only the reply markup of messages.<para>Returns: The edited <see cref="Message"/> is returned</para></summary>
        public EditMessageReplyMarkupRequest() : base("editMessageReplyMarkup")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Identifier of the message to edit</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

        /// <summary>An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message to be edited was sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
