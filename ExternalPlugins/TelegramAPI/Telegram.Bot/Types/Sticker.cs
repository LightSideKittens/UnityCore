// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a sticker.</summary>
    public partial class Sticker : FileBase
    {
        /// <summary>Type of the sticker, currently one of <see cref="StickerType.Regular">Regular</see>, <see cref="StickerType.Mask">Mask</see>, <see cref="StickerType.CustomEmoji">CustomEmoji</see>. The type of the sticker is independent from its format, which is determined by the fields <see cref="IsAnimated">IsAnimated</see> and <see cref="IsVideo">IsVideo</see>.</summary>
        
        public StickerType Type { get; set; }

        /// <summary>Sticker width</summary>
        
        public int Width { get; set; }

        /// <summary>Sticker height</summary>
        
        public int Height { get; set; }

        /// <summary><see langword="true"/>, if the sticker is <a href="https://telegram.org/blog/animated-stickers">animated</a></summary>
        [Newtonsoft.Json.JsonProperty("is_animated")]
        public bool IsAnimated { get; set; }

        /// <summary><see langword="true"/>, if the sticker is a <a href="https://telegram.org/blog/video-stickers-better-reactions">video sticker</a></summary>
        [Newtonsoft.Json.JsonProperty("is_video")]
        public bool IsVideo { get; set; }

        /// <summary><em>Optional</em>. Sticker thumbnail in the .WEBP or .JPG format</summary>
        public PhotoSize? Thumbnail { get; set; }

        /// <summary><em>Optional</em>. Emoji associated with the sticker</summary>
        public string? Emoji { get; set; }

        /// <summary><em>Optional</em>. Name of the sticker set to which the sticker belongs</summary>
        [Newtonsoft.Json.JsonProperty("set_name")]
        public string? SetName { get; set; }

        /// <summary><em>Optional</em>. For premium regular stickers, premium animation for the sticker</summary>
        [Newtonsoft.Json.JsonProperty("premium_animation")]
        public TGFile? PremiumAnimation { get; set; }

        /// <summary><em>Optional</em>. For mask stickers, the position where the mask should be placed</summary>
        [Newtonsoft.Json.JsonProperty("mask_position")]
        public MaskPosition? MaskPosition { get; set; }

        /// <summary><em>Optional</em>. For custom emoji stickers, unique identifier of the custom emoji</summary>
        [Newtonsoft.Json.JsonProperty("custom_emoji_id")]
        public string? CustomEmojiId { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the sticker must be repainted to a text color in messages, the color of the Telegram Premium badge in emoji status, white color on chat photos, or another appropriate color in other places</summary>
        [Newtonsoft.Json.JsonProperty("needs_repainting")]
        public bool NeedsRepainting { get; set; }
    }
}
