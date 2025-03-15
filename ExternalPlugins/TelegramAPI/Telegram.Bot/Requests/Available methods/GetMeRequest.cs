// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>A simple method for testing your bot's authentication token.<para>Returns: Basic information about the bot in form of a <see cref="User"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetMeRequest : ParameterlessRequest<User>
    {
        /// <summary>A simple method for testing your bot's authentication token.<para>Returns: Basic information about the bot in form of a <see cref="User"/> object.</para></summary>
        public GetMeRequest() : base("getMe")
        {
        }
    }
}
