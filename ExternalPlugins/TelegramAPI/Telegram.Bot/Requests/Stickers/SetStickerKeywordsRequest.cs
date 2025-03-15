// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to change search keywords assigned to a regular or custom emoji sticker. The sticker must belong to a sticker set created by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetStickerKeywordsRequest : RequestBase<bool>
    {
        /// <summary>Use this method to change search keywords assigned to a regular or custom emoji sticker. The sticker must belong to a sticker set created by the bot.<para>Returns: </para></summary>
        public SetStickerKeywordsRequest() : base("setStickerKeywords")
        {
        }

        /// <summary>File identifier of the sticker</summary>
        
        public InputFileId Sticker { get; set; }

        /// <summary>A list of 0-20 search keywords for the sticker with total length of up to 64 characters</summary>
        public IEnumerable<string>? Keywords { get; set; }
    }
}
