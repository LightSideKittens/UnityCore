// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Returns the list of gifts that can be sent by the bot to users and channel chats.<para>Returns: A <see cref="GiftList"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetAvailableGiftsRequest : ParameterlessRequest<GiftList>
    {
        /// <summary>Returns the list of gifts that can be sent by the bot to users and channel chats.<para>Returns: A <see cref="GiftList"/> object.</para></summary>
        public GetAvailableGiftsRequest() : base("getAvailableGifts")
        {
        }
    }
}
