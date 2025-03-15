// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to delete a sticker set that was created by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DeleteStickerSetRequest : RequestBase<bool>
    {
        /// <summary>Use this method to delete a sticker set that was created by the bot.<para>Returns: </para></summary>
        public DeleteStickerSetRequest() : base("deleteStickerSet")
        {
        }

        /// <summary>Sticker set name</summary>
        
        public string Name { get; set; }
    }
}
