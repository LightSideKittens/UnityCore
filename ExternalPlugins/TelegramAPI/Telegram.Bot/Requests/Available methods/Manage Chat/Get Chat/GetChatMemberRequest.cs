// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get information about a member of a chat. The method is only guaranteed to work for other users if the bot is an administrator in the chat.<para>Returns: A <see cref="ChatMember"/> object on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetChatMemberRequest : RequestBase<ChatMember>, IChatTargetable, IUserTargetable
    {
        /// <summary>Use this method to get information about a member of a chat. The method is only guaranteed to work for other users if the bot is an administrator in the chat.<para>Returns: A <see cref="ChatMember"/> object on success.</para></summary>
        public GetChatMemberRequest() : base("getChatMember")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup or channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }
    }
}
