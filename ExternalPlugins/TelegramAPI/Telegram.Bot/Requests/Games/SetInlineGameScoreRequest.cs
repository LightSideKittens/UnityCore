// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to set the score of the specified user in a game message.<para>Returns: </para></summary>
    /// <remarks>Returns an error, if the new score is not greater than the user's current score in the chat and <see cref="Force">Force</see> is <em>False</em>.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetInlineGameScoreRequest : RequestBase<bool>, IUserTargetable
    {
        /// <summary>Use this method to set the score of the specified user in a game message.<para>Returns: </para></summary>
        /// <remarks>Returns an error, if the new score is not greater than the user's current score in the chat and <see cref="Force">Force</see> is <em>False</em>.</remarks>
        public SetInlineGameScoreRequest() : base("setGameScore")
        {
        }

        /// <summary>User identifier</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>New score, must be non-negative</summary>
        
        public int Score { get; set; }

        /// <summary>Identifier of the inline message</summary>
        [Newtonsoft.Json.JsonProperty("inline_message_id")]
        
        public string InlineMessageId { get; set; }

        /// <summary>Pass <see langword="true"/> if the high score is allowed to decrease. This can be useful when fixing mistakes or banning cheaters</summary>
        public bool Force { get; set; }

        /// <summary>Pass <see langword="true"/> if the game message should not be automatically edited to include the current scoreboard</summary>
        [Newtonsoft.Json.JsonProperty("disable_edit_message")]
        public bool DisableEditMessage { get; set; }
    }
}
