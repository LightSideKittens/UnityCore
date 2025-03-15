// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to set the result of an interaction with a <a href="https://core.telegram.org/bots/webapps">Web App</a> and send a corresponding message on behalf of the user to the chat from which the query originated.<para>Returns: A <see cref="SentWebAppMessage"/> object is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class AnswerWebAppQueryRequest : RequestBase<SentWebAppMessage>
    {
        /// <summary>Use this method to set the result of an interaction with a <a href="https://core.telegram.org/bots/webapps">Web App</a> and send a corresponding message on behalf of the user to the chat from which the query originated.<para>Returns: A <see cref="SentWebAppMessage"/> object is returned.</para></summary>
        public AnswerWebAppQueryRequest() : base("answerWebAppQuery")
        {
        }

        /// <summary>Unique identifier for the query to be answered</summary>
        [Newtonsoft.Json.JsonProperty("web_app_query_id")]
        
        public string WebAppQueryId { get; set; }

        /// <summary>An object describing the message to be sent</summary>
        
        public InlineQueryResult Result { get; set; }
    }
}
