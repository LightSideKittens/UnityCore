// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to get information about the connection of the bot with a business account.<para>Returns: A <see cref="BusinessConnection"/> object on success.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetBusinessConnectionRequest : RequestBase<BusinessConnection>, IBusinessConnectable
    {
        /// <summary>Use this method to get information about the connection of the bot with a business account.<para>Returns: A <see cref="BusinessConnection"/> object on success.</para></summary>
        public GetBusinessConnectionRequest() : base("getBusinessConnection")
        {
        }

        /// <summary>Unique identifier of the business connection</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        
        public string BusinessConnectionId { get; set; }
    }
}
