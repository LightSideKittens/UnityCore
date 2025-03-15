// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get up-to-date information about the chat.<para>Returns: A <see cref="ChatFullInfo"/> object on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetChatRequest : RequestBase<ChatFullInfo>, IChatTargetable
    {
        /// <summary>Use this method to get up-to-date information about the chat.<para>Returns: A <see cref="ChatFullInfo"/> object on success.</para></summary>
        public GetChatRequest() : base("getChat")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup or channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }
    }
}
