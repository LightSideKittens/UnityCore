// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method for your bot to leave a group, supergroup or channel.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class LeaveChatRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Use this method for your bot to leave a group, supergroup or channel.<para>Returns: </para></summary>
        public LeaveChatRequest() : base("leaveChat")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup or channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }
    }
}
