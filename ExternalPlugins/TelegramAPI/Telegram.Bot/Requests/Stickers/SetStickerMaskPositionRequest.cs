// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to change the <see cref="MaskPosition">mask position</see> of a mask sticker. The sticker must belong to a sticker set that was created by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetStickerMaskPositionRequest : RequestBase<bool>
    {
        /// <summary>Use this method to change the <see cref="MaskPosition">mask position</see> of a mask sticker. The sticker must belong to a sticker set that was created by the bot.<para>Returns: </para></summary>
        public SetStickerMaskPositionRequest() : base("setStickerMaskPosition")
        {
        }

        /// <summary>File identifier of the sticker</summary>
        
        public InputFileId Sticker { get; set; }

        /// <summary>An object with the position where the mask should be placed on faces. Omit the parameter to remove the mask position.</summary>
        [Newtonsoft.Json.JsonProperty("mask_position")]
        public MaskPosition? MaskPosition { get; set; }
    }
}
