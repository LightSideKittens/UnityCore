// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to unban a previously banned channel chat in a supergroup or channel. The bot must be an administrator for this to work and must have the appropriate administrator rights.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class UnbanChatSenderChatRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Use this method to unban a previously banned channel chat in a supergroup or channel. The bot must be an administrator for this to work and must have the appropriate administrator rights.<para>Returns: </para></summary>
        public UnbanChatSenderChatRequest() : base("unbanChatSenderChat")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier of the target sender chat</summary>
        [Newtonsoft.Json.JsonProperty("sender_chat_id")]
        
        public long SenderChatId { get; set; }
    }
}
