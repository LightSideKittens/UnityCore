// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to set a custom title for an administrator in a supergroup promoted by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetChatAdministratorCustomTitleRequest : RequestBase<bool>, IChatTargetable, IUserTargetable
    {
        /// <summary>Use this method to set a custom title for an administrator in a supergroup promoted by the bot.<para>Returns: </para></summary>
        public SetChatAdministratorCustomTitleRequest() : base("setChatAdministratorCustomTitle")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup (in the format <c>@supergroupusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>New custom title for the administrator; 0-16 characters, emoji are not allowed</summary>
        [Newtonsoft.Json.JsonProperty("custom_title")]
        
        public string CustomTitle { get; set; }
    }
}
