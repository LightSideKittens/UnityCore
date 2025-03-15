// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object contains information about a poll.</summary>
    public partial class Poll
    {
        /// <summary>Unique poll identifier</summary>
        
        public string Id { get; set; } = default!;

        /// <summary>Poll question, 1-300 characters</summary>
        
        public string Question { get; set; } = default!;

        /// <summary><em>Optional</em>. Special entities that appear in the <see cref="Question">Question</see>. Currently, only custom emoji entities are allowed in poll questions</summary>
        [Newtonsoft.Json.JsonProperty("question_entities")]
        public MessageEntity[]? QuestionEntities { get; set; }

        /// <summary>List of poll options</summary>
        
        public PollOption[] Options { get; set; } = default!;

        /// <summary>Total number of users that voted in the poll</summary>
        [Newtonsoft.Json.JsonProperty("total_voter_count")]
        
        public int TotalVoterCount { get; set; }

        /// <summary><see langword="true"/>, if the poll is closed</summary>
        [Newtonsoft.Json.JsonProperty("is_closed")]
        public bool IsClosed { get; set; }

        /// <summary><see langword="true"/>, if the poll is anonymous</summary>
        [Newtonsoft.Json.JsonProperty("is_anonymous")]
        public bool IsAnonymous { get; set; }

        /// <summary>Poll type, currently can be “regular” or “quiz”</summary>
        
        public string Type { get; set; } = default!;

        /// <summary><see langword="true"/>, if the poll allows multiple answers</summary>
        [Newtonsoft.Json.JsonProperty("allows_multiple_answers")]
        public bool AllowsMultipleAnswers { get; set; }

        /// <summary><em>Optional</em>. 0-based identifier of the correct answer option. Available only for polls in the quiz mode, which are closed, or was sent (not forwarded) by the bot or to the private chat with the bot.</summary>
        [Newtonsoft.Json.JsonProperty("correct_option_id")]
        public int? CorrectOptionId { get; set; }

        /// <summary><em>Optional</em>. Text that is shown when a user chooses an incorrect answer or taps on the lamp icon in a quiz-style poll, 0-200 characters</summary>
        public string? Explanation { get; set; }

        /// <summary><em>Optional</em>. Special entities like usernames, URLs, bot commands, etc. that appear in the <see cref="Explanation">Explanation</see></summary>
        [Newtonsoft.Json.JsonProperty("explanation_entities")]
        public MessageEntity[]? ExplanationEntities { get; set; }

        /// <summary><em>Optional</em>. Amount of time in seconds the poll will be active after creation</summary>
        [Newtonsoft.Json.JsonProperty("open_period")]
        public int? OpenPeriod { get; set; }

        /// <summary><em>Optional</em>. Point in time when the poll will be automatically closed</summary>
        [Newtonsoft.Json.JsonProperty("close_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? CloseDate { get; set; }
    }
}
