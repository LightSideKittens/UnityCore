// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get the number of members in a chat.<para>Returns: <em>Int</em> on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetChatMemberCountRequest : RequestBase<int>, IChatTargetable
    {
        /// <summary>Use this method to get the number of members in a chat.<para>Returns: <em>Int</em> on success.</para></summary>
        public GetChatMemberCountRequest() : base("getChatMemberCount")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup or channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }
    }
}
