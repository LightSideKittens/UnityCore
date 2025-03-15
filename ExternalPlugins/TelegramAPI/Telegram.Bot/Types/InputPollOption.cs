// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>This object contains information about one answer option in a poll to be sent.</summary>
    public partial class InputPollOption
    {
        /// <summary>Option text, 1-100 characters</summary>
        
        public string Text { get; set; }

        /// <summary><em>Optional</em>. Mode for parsing entities in the text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details. Currently, only custom emoji entities are allowed</summary>
        [Newtonsoft.Json.JsonProperty("text_parse_mode")]
        public ParseMode TextParseMode { get; set; }

        /// <summary><em>Optional</em>. A list of special entities that appear in the poll option text. It can be specified instead of <see cref="TextParseMode">TextParseMode</see></summary>
        [Newtonsoft.Json.JsonProperty("text_entities")]
        public MessageEntity[]? TextEntities { get; set; }

        /// <summary>Initializes an instance of <see cref="InputPollOption"/></summary>
        /// <param name="text">Option text, 1-100 characters</param>
        
        public InputPollOption(string text) => Text = text;

        /// <summary>Instantiates a new <see cref="InputPollOption"/></summary>
        public InputPollOption() { }
    }
}
