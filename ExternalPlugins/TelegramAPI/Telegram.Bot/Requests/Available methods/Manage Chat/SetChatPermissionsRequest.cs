// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to set default chat permissions for all members. The bot must be an administrator in the group or a supergroup for this to work and must have the <em>CanRestrictMembers</em> administrator rights.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetChatPermissionsRequest : RequestBase<bool>, IChatTargetable
    {
        /// <summary>Use this method to set default chat permissions for all members. The bot must be an administrator in the group or a supergroup for this to work and must have the <em>CanRestrictMembers</em> administrator rights.<para>Returns: </para></summary>
        public SetChatPermissionsRequest() : base("setChatPermissions")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target supergroup (in the format <c>@supergroupusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>An object for new default chat permissions</summary>
        
        public ChatPermissions Permissions { get; set; }

        /// <summary>Pass <see langword="true"/> if chat permissions are set independently. Otherwise, the <em>CanSendOtherMessages</em> and <em>CanAddWebPagePreviews</em> permissions will imply the <em>CanSendMessages</em>, <em>CanSendAudios</em>, <em>CanSendDocuments</em>, <em>CanSendPhotos</em>, <em>CanSendVideos</em>, <em>CanSendVideoNotes</em>, and <em>CanSendVoiceNotes</em> permissions; the <em>CanSendPolls</em> permission will imply the <em>CanSendMessages</em> permission.</summary>
        [Newtonsoft.Json.JsonProperty("use_independent_chat_permissions")]
        public bool UseIndependentChatPermissions { get; set; }
    }
}
