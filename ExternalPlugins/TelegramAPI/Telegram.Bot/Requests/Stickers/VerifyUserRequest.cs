// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Requests
{
    /// <summary>Verifies a user <a href="https://telegram.org/verify#third-party-verification">on behalf of the organization</a> which is represented by the bot.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class VerifyUserRequest : RequestBase<bool>, IUserTargetable
    {
        /// <summary>Verifies a user <a href="https://telegram.org/verify#third-party-verification">on behalf of the organization</a> which is represented by the bot.<para>Returns: </para></summary>
        public VerifyUserRequest() : base("verifyUser")
        {
        }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Custom description for the verification; 0-70 characters. Must be empty if the organization isn't allowed to provide a custom verification description.</summary>
        [Newtonsoft.Json.JsonProperty("custom_description")]
        public string? CustomDescription { get; set; }
    }
}
