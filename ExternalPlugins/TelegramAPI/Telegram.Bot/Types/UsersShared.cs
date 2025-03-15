// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Types
{
    /// <summary>This object contains information about the users whose identifiers were shared with the bot using a <see cref="KeyboardButtonRequestUsers"/> button.</summary>
    public partial class UsersShared
    {
        /// <summary>Identifier of the request</summary>
        [Newtonsoft.Json.JsonProperty("request_id")]
        
        public int RequestId { get; set; }

        /// <summary>Information about users shared with the bot.</summary>
        
        public SharedUser[] Users { get; set; } = default!;
    }
}
