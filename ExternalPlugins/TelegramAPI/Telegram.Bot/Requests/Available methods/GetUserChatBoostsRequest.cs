// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get the list of boosts added to a chat by a user. Requires administrator rights in the chat.<para>Returns: A <see cref="UserChatBoosts"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetUserChatBoostsRequest : RequestBase<UserChatBoosts>, IChatTargetable, IUserTargetable
    {
        /// <summary>Use this method to get the list of boosts added to a chat by a user. Requires administrator rights in the chat.<para>Returns: A <see cref="UserChatBoosts"/> object.</para></summary>
        public GetUserChatBoostsRequest() : base("getUserChatBoosts")
        {
        }

        /// <summary>Unique identifier for the chat or username of the channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }
    }
}
