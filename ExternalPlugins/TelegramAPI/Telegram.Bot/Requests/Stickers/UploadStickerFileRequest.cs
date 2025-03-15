// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to upload a file with a sticker for later use in the <see cref="TelegramBotClientExtensions.CreateNewStickerSet">CreateNewStickerSet</see>, <see cref="TelegramBotClientExtensions.AddStickerToSet">AddStickerToSet</see>, or <see cref="TelegramBotClientExtensions.ReplaceStickerInSet">ReplaceStickerInSet</see> methods (the file can be used multiple times).<para>Returns: The uploaded <see cref="TGFile"/> on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class UploadStickerFileRequest : FileRequestBase<TGFile>, IUserTargetable
    {
        /// <summary>Use this method to upload a file with a sticker for later use in the <see cref="TelegramBotClientExtensions.CreateNewStickerSet">CreateNewStickerSet</see>, <see cref="TelegramBotClientExtensions.AddStickerToSet">AddStickerToSet</see>, or <see cref="TelegramBotClientExtensions.ReplaceStickerInSet">ReplaceStickerInSet</see> methods (the file can be used multiple times).<para>Returns: The uploaded <see cref="TGFile"/> on success.</para></summary>
        public UploadStickerFileRequest() : base("uploadStickerFile")
        {
        }

        /// <summary>User identifier of sticker file owner</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>A file with the sticker in .WEBP, .PNG, .TGS, or .WEBM format. See <a href="https://core.telegram.org/stickers">https://core.telegram.org/stickers</a> for technical requirements. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></summary>
        
        public InputFileStream Sticker { get; set; }

        /// <summary>Format of the sticker, must be one of <see cref="StickerFormat.Static">Static</see>, <see cref="StickerFormat.Animated">Animated</see>, <see cref="StickerFormat.Video">Video</see></summary>
        [Newtonsoft.Json.JsonProperty("sticker_format")]
        
        public StickerFormat StickerFormat { get; set; }
    }
}
