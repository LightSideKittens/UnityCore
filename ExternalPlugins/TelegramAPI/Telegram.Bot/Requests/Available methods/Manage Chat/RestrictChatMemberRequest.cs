// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Serialization;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to restrict a user in a supergroup. The bot must be an administrator in the supergroup for this to work and must have the appropriate administrator rights. Pass <em>True</em> for all permissions to lift restrictions from a user.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class RestrictChatMemberRequest : RequestBase<bool>, IChatTargetable, IUserTargetable
    {
        /// <summary>Use this method to restrict a user in a supergroup. The bot must be an administrator in the supergroup for this to work and must have the appropriate administrator rights. Pass <em>True</em> for all permissions to lift restrictions from a user.<para>Returns: </para></summary>
        public RestrictChatMemberRequest() : base("restrictChatMember")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup (in the format <c>@supergroupusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Unique identifier of the target user</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>An object for new user permissions</summary>
        
        public ChatPermissions Permissions { get; set; }

        /// <summary>Pass <see langword="true"/> if chat permissions are set independently. Otherwise, the <em>CanSendOtherMessages</em> and <em>CanAddWebPagePreviews</em> permissions will imply the <em>CanSendMessages</em>, <em>CanSendAudios</em>, <em>CanSendDocuments</em>, <em>CanSendPhotos</em>, <em>CanSendVideos</em>, <em>CanSendVideoNotes</em>, and <em>CanSendVoiceNotes</em> permissions; the <em>CanSendPolls</em> permission will imply the <em>CanSendMessages</em> permission.</summary>
        [Newtonsoft.Json.JsonProperty("use_independent_chat_permissions")]
        public bool UseIndependentChatPermissions { get; set; }

        /// <summary>Date when restrictions will be lifted for the user, in UTC. If user is restricted for more than 366 days or less than 30 seconds from the current time, they are considered to be restricted forever</summary>
        [Newtonsoft.Json.JsonProperty("until_date")]
        [Newtonsoft.Json.JsonConverter(typeof(BanTimeConverter))]
        public DateTime? UntilDate { get; set; }
    }
}
