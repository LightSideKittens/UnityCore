// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get the current default administrator rights of the bot.<para>Returns: <see cref="ChatAdministratorRights"/> on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetMyDefaultAdministratorRightsRequest : RequestBase<ChatAdministratorRights>
    {
        /// <summary>Use this method to get the current default administrator rights of the bot.<para>Returns: <see cref="ChatAdministratorRights"/> on success.</para></summary>
        public GetMyDefaultAdministratorRightsRequest() : base("getMyDefaultAdministratorRights")
        {
        }

        /// <summary>Pass <see langword="true"/> to get default administrator rights of the bot in channels. Otherwise, default administrator rights of the bot for groups and supergroups will be returned.</summary>
        [Newtonsoft.Json.JsonProperty("for_channels")]
        public bool ForChannels { get; set; }
    }
}
