// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a sticker set.</summary>
    public partial class StickerSet
    {
        /// <summary>Sticker set name</summary>
        
        public string Name { get; set; } = default!;

        /// <summary>Sticker set title</summary>
        
        public string Title { get; set; } = default!;

        /// <summary>Type of stickers in the set, currently one of <see cref="StickerType.Regular">Regular</see>, <see cref="StickerType.Mask">Mask</see>, <see cref="StickerType.CustomEmoji">CustomEmoji</see></summary>
        [Newtonsoft.Json.JsonProperty("sticker_type")]
        
        public StickerType StickerType { get; set; }

        /// <summary>List of all set stickers</summary>
        
        public Sticker[] Stickers { get; set; } = default!;

        /// <summary><em>Optional</em>. Sticker set thumbnail in the .WEBP, .TGS, or .WEBM format</summary>
        public PhotoSize? Thumbnail { get; set; }
    }
}
