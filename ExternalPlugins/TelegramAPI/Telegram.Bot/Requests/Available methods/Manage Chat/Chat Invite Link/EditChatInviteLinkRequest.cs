// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Serialization;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit a non-primary invite link created by the bot. The bot must be an administrator in the chat for this to work and must have the appropriate administrator rights.<para>Returns: The edited invite link as a <see cref="ChatInviteLink"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditChatInviteLinkRequest : RequestBase<ChatInviteLink>, IChatTargetable
    {
        /// <summary>Use this method to edit a non-primary invite link created by the bot. The bot must be an administrator in the chat for this to work and must have the appropriate administrator rights.<para>Returns: The edited invite link as a <see cref="ChatInviteLink"/> object.</para></summary>
        public EditChatInviteLinkRequest() : base("editChatInviteLink")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>The invite link to edit</summary>
        [Newtonsoft.Json.JsonProperty("invite_link")]
        
        public string InviteLink { get; set; }

        /// <summary>Invite link name; 0-32 characters</summary>
        public string? Name { get; set; }

        /// <summary>Point in time when the link will expire</summary>
        [Newtonsoft.Json.JsonProperty("expire_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? ExpireDate { get; set; }

        /// <summary>The maximum number of users that can be members of the chat simultaneously after joining the chat via this invite link; 1-99999</summary>
        [Newtonsoft.Json.JsonProperty("member_limit")]
        public int? MemberLimit { get; set; }

        /// <summary><see langword="true"/>, if users joining the chat via the link need to be approved by chat administrators. If <see langword="true"/>, <see cref="MemberLimit">MemberLimit</see> can't be specified</summary>
        [Newtonsoft.Json.JsonProperty("creates_join_request")]
        public bool CreatesJoinRequest { get; set; }
    }
}
