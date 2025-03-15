// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types
{
    /// <summary>This object contains information about one answer option in a poll.</summary>
    public partial class PollOption
    {
        /// <summary>Option text, 1-100 characters</summary>
        
        public string Text { get; set; } = default!;

        /// <summary><em>Optional</em>. Special entities that appear in the option <see cref="Text">Text</see>. Currently, only custom emoji entities are allowed in poll option texts</summary>
        [Newtonsoft.Json.JsonProperty("text_entities")]
        public MessageEntity[]? TextEntities { get; set; }

        /// <summary>Number of users that voted for this option</summary>
        [Newtonsoft.Json.JsonProperty("voter_count")]
        
        public int VoterCount { get; set; }
    }
}
