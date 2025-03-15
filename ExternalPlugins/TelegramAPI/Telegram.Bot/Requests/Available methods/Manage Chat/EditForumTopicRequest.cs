// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit name and icon of a topic in a forum supergroup chat. The bot must be an administrator in the chat for this to work and must have the <em>CanManageTopics</em> administrator rights, unless it is the creator of the topic.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditForumTopicRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Use this method to edit name and icon of a topic in a forum supergroup chat. The bot must be an administrator in the chat for this to work and must have the <em>CanManageTopics</em> administrator rights, unless it is the creator of the topic.<para>Returns: </para></summary>
        public EditForumTopicRequest() : base("editForumTopic")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup (in the format <c>@supergroupusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier for the target message thread of the forum topic</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        
        public int MessageThreadId { get; set; }

        /// <summary>New topic name, 0-128 characters. If not specified or empty, the current name of the topic will be kept</summary>
        public string? Name { get; set; }

        /// <summary>New unique identifier of the custom emoji shown as the topic icon. Use <see cref="TelegramBotClientExtensions.GetForumTopicIconStickers">GetForumTopicIconStickers</see> to get all allowed custom emoji identifiers. Pass an empty string to remove the icon. If not specified, the current icon will be kept</summary>
        [Newtonsoft.Json.JsonProperty("icon_custom_emoji_id")]
        public string? IconCustomEmojiId { get; set; }
    }
}
