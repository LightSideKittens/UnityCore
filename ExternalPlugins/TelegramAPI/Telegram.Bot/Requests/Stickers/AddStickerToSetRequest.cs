// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to add a new sticker to a set created by the bot. Emoji sticker sets can have up to 200 stickers. Other sticker sets can have up to 120 stickers.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class AddStickerToSetRequest : FileRequestBase<bool>, IUserTargetable
    {
        /// <summary>Use this method to add a new sticker to a set created by the bot. Emoji sticker sets can have up to 200 stickers. Other sticker sets can have up to 120 stickers.<para>Returns: </para></summary>
        public AddStickerToSetRequest() : base("addStickerToSet")
        {
        }

        /// <summary>User identifier of sticker set owner</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Sticker set name</summary>
        
        public string Name { get; set; }

        /// <summary>An object with information about the added sticker. If exactly the same sticker had already been added to the set, then the set isn't changed.</summary>
        
        public InputSticker Sticker { get; set; }
    }
}
