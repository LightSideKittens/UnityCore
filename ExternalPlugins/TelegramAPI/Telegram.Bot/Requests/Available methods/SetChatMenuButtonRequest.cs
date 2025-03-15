// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to change the bot's menu button in a private chat, or the default menu button.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetChatMenuButtonRequest : RequestBase<bool>
    {
        /// <summary>Use this method to change the bot's menu button in a private chat, or the default menu button.<para>Returns: </para></summary>
        public SetChatMenuButtonRequest() : base("setChatMenuButton")
        {
        }

        /// <summary>Unique identifier for the target private chat. If not specified, default bot's menu button will be changed</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        public long? ChatId { get; set; }

        /// <summary>An object for the bot's new menu button. Defaults to <see cref="MenuButtonDefault"/></summary>
        [Newtonsoft.Json.JsonProperty("menu_button")]
        public MenuButton? MenuButton { get; set; }
    }
}
