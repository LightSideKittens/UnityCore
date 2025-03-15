// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to change the list of emoji assigned to a regular or custom emoji sticker. The sticker must belong to a sticker set created by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetStickerEmojiListRequest : RequestBase<bool>
    {
        /// <summary>Use this method to change the list of emoji assigned to a regular or custom emoji sticker. The sticker must belong to a sticker set created by the bot.<para>Returns: </para></summary>
        public SetStickerEmojiListRequest() : base("setStickerEmojiList")
        {
        }

        /// <summary>File identifier of the sticker</summary>
        
        public InputFileId Sticker { get; set; }

        /// <summary>A list of 1-20 emoji associated with the sticker</summary>
        [Newtonsoft.Json.JsonProperty("emoji_list")]
        
        public IEnumerable<string> EmojiList { get; set; }
    }
}
