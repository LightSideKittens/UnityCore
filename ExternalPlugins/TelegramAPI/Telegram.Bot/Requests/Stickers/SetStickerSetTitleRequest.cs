// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to set the title of a created sticker set.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetStickerSetTitleRequest : RequestBase<bool>
    {
        /// <summary>Use this method to set the title of a created sticker set.<para>Returns: </para></summary>
        public SetStickerSetTitleRequest() : base("setStickerSetTitle")
        {
        }

        /// <summary>Sticker set name</summary>
        
        public string Name { get; set; }

        /// <summary>Sticker set title, 1-64 characters</summary>
        
        public string Title { get; set; }
    }
}
