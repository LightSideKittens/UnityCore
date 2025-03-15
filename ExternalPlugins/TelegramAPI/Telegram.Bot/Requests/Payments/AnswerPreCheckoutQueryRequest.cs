// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Once the user has confirmed their payment and shipping details, the Bot API sends the final confirmation in the form of an <see cref="Update"/> with the field <em>PreCheckoutQuery</em>. Use this method to respond to such pre-checkout queries <b>Note:</b> The Bot API must receive an answer within 10 seconds after the pre-checkout query was sent.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class AnswerPreCheckoutQueryRequest : RequestBase<bool>
    {
        /// <summary>Once the user has confirmed their payment and shipping details, the Bot API sends the final confirmation in the form of an <see cref="Update"/> with the field <em>PreCheckoutQuery</em>. Use this method to respond to such pre-checkout queries <b>Note:</b> The Bot API must receive an answer within 10 seconds after the pre-checkout query was sent.<para>Returns: </para></summary>
        public AnswerPreCheckoutQueryRequest() : base("answerPreCheckoutQuery")
        {
        }

        /// <summary>Unique identifier for the query to be answered</summary>
        [Newtonsoft.Json.JsonProperty("pre_checkout_query_id")]
        
        public string PreCheckoutQueryId { get; set; }

        /// <summary>Specify <see langword="true"/> if everything is alright (goods are available, etc.) and the bot is ready to proceed with the order. Use <see langword="false"/> if there are any problems.</summary>
        
        public bool Ok { get; set; }

        /// <summary>Error message in human readable form that explains the reason for failure to proceed with the checkout (e.g. "Sorry, somebody just bought the last of our amazing black T-shirts while you were busy filling out your payment details. Please choose a different color or garment!"). Telegram will display this message to the user.<para/>Leave <see langword="null"/> if everything is alright (goods are available, etc.) and the bot is ready to proceed with the order</summary>
        [Newtonsoft.Json.JsonProperty("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
