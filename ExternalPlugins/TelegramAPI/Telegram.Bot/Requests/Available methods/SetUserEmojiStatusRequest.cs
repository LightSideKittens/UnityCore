// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Requests
{
    /// <summary>Changes the emoji status for a given user that previously allowed the bot to manage their emoji status via the Mini App method <a href="https://core.telegram.org/bots/webapps#initializing-mini-apps">requestEmojiStatusAccess</a>.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetUserEmojiStatusRequest : RequestBase<bool>, IUserTargetable
    {
        /// <summary>Changes the emoji status for a given user that previously allowed the bot to manage their emoji status via the Mini App method <a href="https://core.telegram.org/bots/webapps#initializing-mini-apps">requestEmojiStatusAccess</a>.<para>Returns: </para></summary>
        public SetUserEmojiStatusRequest() : base("setUserEmojiStatus")
        {
        }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Custom emoji identifier of the emoji status to set. Pass an empty string to remove the status.</summary>
        [Newtonsoft.Json.JsonProperty("emoji_status_custom_emoji_id")]
        public string? EmojiStatusCustomEmojiId { get; set; }

        /// <summary>Expiration date of the emoji status, if any</summary>
        [Newtonsoft.Json.JsonProperty("emoji_status_expiration_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? EmojiStatusExpirationDate { get; set; }
    }
}
