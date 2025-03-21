// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to set the thumbnail of a custom emoji sticker set.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetCustomEmojiStickerSetThumbnailRequest : RequestBase<bool>
    {
        /// <summary>Use this method to set the thumbnail of a custom emoji sticker set.<para>Returns: </para></summary>
        public SetCustomEmojiStickerSetThumbnailRequest() : base("setCustomEmojiStickerSetThumbnail")
        {
        }

        /// <summary>Sticker set name</summary>
        
        public string Name { get; set; }

        /// <summary>Custom emoji identifier of a sticker from the sticker set; pass an empty string to drop the thumbnail and use the first sticker as the thumbnail.</summary>
        [Newtonsoft.Json.JsonProperty("custom_emoji_id")]
        public string? CustomEmojiId { get; set; }
    }
}
