// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Types.InlineQueryResults;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to send answers to an inline query<br/>No more than <b>50</b> results per query are allowed.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class AnswerInlineQueryRequest : RequestBase<bool>
    {
        /// <summary>Use this method to send answers to an inline query<br/>No more than <b>50</b> results per query are allowed.<para>Returns: </para></summary>
        public AnswerInlineQueryRequest() : base("answerInlineQuery")
        {
        }

        /// <summary>Unique identifier for the answered query</summary>
        [Newtonsoft.Json.JsonProperty("inline_query_id")]
        
        public string InlineQueryId { get; set; }

        /// <summary>A array of results for the inline query</summary>
        
        public IEnumerable<InlineQueryResult> Results { get; set; }

        /// <summary>The maximum amount of time in seconds that the result of the inline query may be cached on the server. Defaults to 300.</summary>
        [Newtonsoft.Json.JsonProperty("cache_time")]
        public int? CacheTime { get; set; }

        /// <summary>Pass <see langword="true"/> if results may be cached on the server side only for the user that sent the query. By default, results may be returned to any user who sends the same query.</summary>
        [Newtonsoft.Json.JsonProperty("is_personal")]
        public bool IsPersonal { get; set; }

        /// <summary>Pass the offset that a client should send in the next query with the same text to receive more results. Pass an empty string if there are no more results or if you don't support pagination. Offset length can't exceed 64 bytes.</summary>
        [Newtonsoft.Json.JsonProperty("next_offset")]
        public string? NextOffset { get; set; }

        /// <summary>An object describing a button to be shown above inline query results</summary>
        public InlineQueryResultsButton? Button { get; set; }
    }
}
