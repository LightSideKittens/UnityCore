// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>Describes the current status of a webhook.</summary>
    public partial class WebhookInfo
    {
        /// <summary>Webhook URL, may be empty if webhook is not set up</summary>
        
        public string Url { get; set; } = default!;

        /// <summary><see langword="true"/>, if a custom certificate was provided for webhook certificate checks</summary>
        [Newtonsoft.Json.JsonProperty("has_custom_certificate")]
        public bool HasCustomCertificate { get; set; }

        /// <summary>Number of updates awaiting delivery</summary>
        [Newtonsoft.Json.JsonProperty("pending_update_count")]
        
        public int PendingUpdateCount { get; set; }

        /// <summary><em>Optional</em>. Currently used webhook IP address</summary>
        [Newtonsoft.Json.JsonProperty("ip_address")]
        public string? IpAddress { get; set; }

        /// <summary><em>Optional</em>. DateTime for the most recent error that happened when trying to deliver an update via webhook</summary>
        [Newtonsoft.Json.JsonProperty("last_error_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? LastErrorDate { get; set; }

        /// <summary><em>Optional</em>. Error message in human-readable format for the most recent error that happened when trying to deliver an update via webhook</summary>
        [Newtonsoft.Json.JsonProperty("last_error_message")]
        public string? LastErrorMessage { get; set; }

        /// <summary><em>Optional</em>. DateTime of the most recent error that happened when trying to synchronize available updates with Telegram datacenters</summary>
        [Newtonsoft.Json.JsonProperty("last_synchronization_error_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? LastSynchronizationErrorDate { get; set; }

        /// <summary><em>Optional</em>. The maximum allowed number of simultaneous HTTPS connections to the webhook for update delivery</summary>
        [Newtonsoft.Json.JsonProperty("max_connections")]
        public int? MaxConnections { get; set; }

        /// <summary><em>Optional</em>. A list of update types the bot is subscribed to. Defaults to all update types except <em>ChatMember</em></summary>
        [Newtonsoft.Json.JsonProperty("allowed_updates")]
        public UpdateType[]? AllowedUpdates { get; set; }
    }
}
