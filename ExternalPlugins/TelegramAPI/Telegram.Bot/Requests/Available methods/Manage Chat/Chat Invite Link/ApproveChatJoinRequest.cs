// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to approve a chat join request. The bot must be an administrator in the chat for this to work and must have the <em>CanInviteUsers</em> administrator right.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ApproveChatJoinRequest : RequestBase<bool>, IChatTargetable, IUserTargetable
    {
        /// <summary>Use this method to approve a chat join request. The bot must be an administrator in the chat for this to work and must have the <em>CanInviteUsers</em> administrator right.<para>Returns: </para></summary>
        public ApproveChatJoinRequest() : base("approveChatJoinRequest")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }
    }
}
