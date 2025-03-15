// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types
{
    /// <summary>This object represents a service message about a new forum topic created in the chat.</summary>
    public partial class ForumTopicCreated
    {
        /// <summary>Name of the topic</summary>
        
        public string Name { get; set; } = default!;

        /// <summary>Color of the topic icon in RGB format</summary>
        [Newtonsoft.Json.JsonProperty("icon_color")]
        
        public int IconColor { get; set; }

        /// <summary><em>Optional</em>. Unique identifier of the custom emoji shown as the topic icon</summary>
        [Newtonsoft.Json.JsonProperty("icon_custom_emoji_id")]
        public string? IconCustomEmojiId { get; set; }
    }
}
