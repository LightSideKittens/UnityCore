// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get a sticker set.<para>Returns: A <see cref="StickerSet"/> object is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetStickerSetRequest : RequestBase<StickerSet>
    {
        /// <summary>Use this method to get a sticker set.<para>Returns: A <see cref="StickerSet"/> object is returned.</para></summary>
        public GetStickerSetRequest() : base("getStickerSet")
        {
        }

        /// <summary>Name of the sticker set</summary>
        
        public string Name { get; set; }
    }
}
