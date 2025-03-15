// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Verifies a chat <a href="https://telegram.org/verify#third-party-verification">on behalf of the organization</a> which is represented by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class VerifyChatRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Verifies a chat <a href="https://telegram.org/verify#third-party-verification">on behalf of the organization</a> which is represented by the bot.<para>Returns: </para></summary>
        public VerifyChatRequest() : base("verifyChat")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Custom description for the verification; 0-70 characters. Must be empty if the organization isn't allowed to provide a custom verification description.</summary>
        [Newtonsoft.Json.JsonProperty("custom_description")]
        public string? CustomDescription { get; set; }
    }
}
