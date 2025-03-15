// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get data for high score tables. Will return the score of the specified user and several of their neighbors in a game.<para>Returns: An Array of <see cref="GameHighScore"/> objects.</para></summary>
    /// <remarks>This method will currently return scores for the target user, plus two of their closest neighbors on each side. Will also return the top three users if the user and their neighbors are not among them. Please note that this behavior is subject to change.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetGameHighScoresRequest : RequestBase<GameHighScore[]>, IChatTargetable, IUserTargetable
    {
        /// <summary>Use this method to get data for high score tables. Will return the score of the specified user and several of their neighbors in a game.<para>Returns: An Array of <see cref="GameHighScore"/> objects.</para></summary>
        /// <remarks>This method will currently return scores for the target user, plus two of their closest neighbors on each side. Will also return the top three users if the user and their neighbors are not among them. Please note that this behavior is subject to change.</remarks>
        public GetGameHighScoresRequest() : base("getGameHighScores")
        {
        }

        /// <summary>Target user id</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Unique identifier for the target chat</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public long ChatId { get; set; }

        /// <summary>Identifier of the sent message</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int MessageId { get; set; }

        /// <inheritdoc/>
        ChatId IChatTargetable.ChatId => ChatId;
    }
}
