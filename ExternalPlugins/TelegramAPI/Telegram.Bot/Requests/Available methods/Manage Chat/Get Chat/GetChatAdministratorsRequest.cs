// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get a list of administrators in a chat, which aren't bots.<para>Returns: An Array of <see cref="ChatMember"/> objects.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetChatAdministratorsRequest : RequestBase<ChatMember[]>, IChatTargetable
    {
        /// <summary>Use this method to get a list of administrators in a chat, which aren't bots.<para>Returns: An Array of <see cref="ChatMember"/> objects.</para></summary>
        public GetChatAdministratorsRequest() : base("getChatAdministrators")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup or channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }
    }
}
