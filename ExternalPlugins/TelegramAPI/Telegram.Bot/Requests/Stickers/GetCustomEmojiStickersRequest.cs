// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get information about custom emoji stickers by their identifiers.<para>Returns: An Array of <see cref="Sticker"/> objects.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetCustomEmojiStickersRequest : RequestBase<Sticker[]>
    {
        /// <summary>Use this method to get information about custom emoji stickers by their identifiers.<para>Returns: An Array of <see cref="Sticker"/> objects.</para></summary>
        public GetCustomEmojiStickersRequest() : base("getCustomEmojiStickers")
        {
        }

        /// <summary>A list of custom emoji identifiers. At most 200 custom emoji identifiers can be specified.</summary>
        [Newtonsoft.Json.JsonProperty("custom_emoji_ids")]
        
        public IEnumerable<string> CustomEmojiIds { get; set; }
    }
}
