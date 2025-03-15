// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to replace an existing sticker in a sticker set with a new one. The method is equivalent to calling <see cref="TelegramBotClientExtensions.DeleteStickerFromSet">DeleteStickerFromSet</see>, then <see cref="TelegramBotClientExtensions.AddStickerToSet">AddStickerToSet</see>, then <see cref="TelegramBotClientExtensions.SetStickerPositionInSet">SetStickerPositionInSet</see>.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ReplaceStickerInSetRequest : FileRequestBase<bool>, IUserTargetable
    {
        /// <summary>Use this method to replace an existing sticker in a sticker set with a new one. The method is equivalent to calling <see cref="TelegramBotClientExtensions.DeleteStickerFromSet">DeleteStickerFromSet</see>, then <see cref="TelegramBotClientExtensions.AddStickerToSet">AddStickerToSet</see>, then <see cref="TelegramBotClientExtensions.SetStickerPositionInSet">SetStickerPositionInSet</see>.<para>Returns: </para></summary>
        public ReplaceStickerInSetRequest() : base("replaceStickerInSet")
        {
        }

        /// <summary>User identifier of the sticker set owner</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Sticker set name</summary>
        
        public string Name { get; set; }

        /// <summary>File identifier of the replaced sticker</summary>
        [Newtonsoft.Json.JsonProperty("old_sticker")]
        
        public string OldSticker { get; set; }

        /// <summary>An object with information about the added sticker. If exactly the same sticker had already been added to the set, then the set remains unchanged.</summary>
        
        public InputSticker Sticker { get; set; }
    }
}
