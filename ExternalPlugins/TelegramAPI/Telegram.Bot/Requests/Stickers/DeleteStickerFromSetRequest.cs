// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to delete a sticker from a set created by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DeleteStickerFromSetRequest : RequestBase<bool>
    {
        /// <summary>Use this method to delete a sticker from a set created by the bot.<para>Returns: </para></summary>
        public DeleteStickerFromSetRequest() : base("deleteStickerFromSet")
        {
        }

        /// <summary>File identifier of the sticker</summary>
        
        public InputFileId Sticker { get; set; }
    }
}
