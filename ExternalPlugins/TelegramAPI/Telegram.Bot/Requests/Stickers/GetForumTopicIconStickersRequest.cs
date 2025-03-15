// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get custom emoji stickers, which can be used as a forum topic icon by any user.<para>Returns: An Array of <see cref="Sticker"/> objects.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetForumTopicIconStickersRequest : ParameterlessRequest<Sticker[]>
    {
        /// <summary>Use this method to get custom emoji stickers, which can be used as a forum topic icon by any user.<para>Returns: An Array of <see cref="Sticker"/> objects.</para></summary>
        public GetForumTopicIconStickersRequest() : base("getForumTopicIconStickers")
        {
        }
    }
}
