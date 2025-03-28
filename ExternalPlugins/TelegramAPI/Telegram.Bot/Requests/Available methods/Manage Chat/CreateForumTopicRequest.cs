// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to create a topic in a forum supergroup chat. The bot must be an administrator in the chat for this to work and must have the <em>CanManageTopics</em> administrator rights.<para>Returns: Information about the created topic as a <see cref="ForumTopic"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class CreateForumTopicRequest : RequestBase<ForumTopic>, IChatTargetable
    {
        /// <summary>Use this method to create a topic in a forum supergroup chat. The bot must be an administrator in the chat for this to work and must have the <em>CanManageTopics</em> administrator rights.<para>Returns: Information about the created topic as a <see cref="ForumTopic"/> object.</para></summary>
        public CreateForumTopicRequest() : base("createForumTopic")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup (in the format <c>@supergroupusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Topic name, 1-128 characters</summary>
        
        public string Name { get; set; }

        /// <summary>Color of the topic icon in RGB format. Currently, must be one of 7322096 (0x6FB9F0), 16766590 (0xFFD67E), 13338331 (0xCB86DB), 9367192 (0x8EEE98), 16749490 (0xFF93B2), or 16478047 (0xFB6F5F)</summary>
        [Newtonsoft.Json.JsonProperty("icon_color")]
        public int? IconColor { get; set; }

        /// <summary>Unique identifier of the custom emoji shown as the topic icon. Use <see cref="TelegramBotClientExtensions.GetForumTopicIconStickers">GetForumTopicIconStickers</see> to get all allowed custom emoji identifiers.</summary>
        [Newtonsoft.Json.JsonProperty("icon_custom_emoji_id")]
        public string? IconCustomEmojiId { get; set; }
    }
}
