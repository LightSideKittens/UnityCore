// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>Represents an invite link for a chat.</summary>
    public partial class ChatInviteLink
    {
        /// <summary>The invite link. If the link was created by another chat administrator, then the second part of the link will be replaced with “…”.</summary>
        [Newtonsoft.Json.JsonProperty("invite_link")]
        
        public string InviteLink { get; set; } = default!;

        /// <summary>Creator of the link</summary>
        
        public User Creator { get; set; } = default!;

        /// <summary><see langword="true"/>, if users joining the chat via the link need to be approved by chat administrators</summary>
        [Newtonsoft.Json.JsonProperty("creates_join_request")]
        public bool CreatesJoinRequest { get; set; }

        /// <summary><see langword="true"/>, if the link is primary</summary>
        [Newtonsoft.Json.JsonProperty("is_primary")]
        public bool IsPrimary { get; set; }

        /// <summary><see langword="true"/>, if the link is revoked</summary>
        [Newtonsoft.Json.JsonProperty("is_revoked")]
        public bool IsRevoked { get; set; }

        /// <summary><em>Optional</em>. Invite link name</summary>
        public string? Name { get; set; }

        /// <summary><em>Optional</em>. Point in time when the link will expire or has been expired</summary>
        [Newtonsoft.Json.JsonProperty("expire_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? ExpireDate { get; set; }

        /// <summary><em>Optional</em>. The maximum number of users that can be members of the chat simultaneously after joining the chat via this invite link; 1-99999</summary>
        [Newtonsoft.Json.JsonProperty("member_limit")]
        public int? MemberLimit { get; set; }

        /// <summary><em>Optional</em>. Number of pending join requests created using this link</summary>
        [Newtonsoft.Json.JsonProperty("pending_join_request_count")]
        public int? PendingJoinRequestCount { get; set; }

        /// <summary><em>Optional</em>. The number of seconds the subscription will be active for before the next payment</summary>
        [Newtonsoft.Json.JsonProperty("subscription_period")]
        public int? SubscriptionPeriod { get; set; }

        /// <summary><em>Optional</em>. The amount of Telegram Stars a user must pay initially and after each subsequent subscription period to be a member of the chat using the link</summary>
        [Newtonsoft.Json.JsonProperty("subscription_price")]
        public int? SubscriptionPrice { get; set; }
    }
}
