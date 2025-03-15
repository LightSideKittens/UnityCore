// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to move a sticker in a set created by the bot to a specific position.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetStickerPositionInSetRequest : RequestBase<bool>
    {
        /// <summary>Use this method to move a sticker in a set created by the bot to a specific position.<para>Returns: </para></summary>
        public SetStickerPositionInSetRequest() : base("setStickerPositionInSet")
        {
        }

        /// <summary>File identifier of the sticker</summary>
        
        public InputFileId Sticker { get; set; }

        /// <summary>New sticker position in the set, zero-based</summary>
        
        public int Position { get; set; }
    }
}
