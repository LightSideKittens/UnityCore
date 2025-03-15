// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents changes in the status of a chat member.</summary>
    public partial class ChatMemberUpdated
    {
        /// <summary>Chat the user belongs to</summary>
        
        public Chat Chat { get; set; } = default!;

        /// <summary>Performer of the action, which resulted in the change</summary>
        
        public User From { get; set; } = default!;

        /// <summary>Date the change was done</summary>
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }

        /// <summary>Previous information about the chat member</summary>
        [Newtonsoft.Json.JsonProperty("old_chat_member")]
        
        public ChatMember OldChatMember { get; set; } = default!;

        /// <summary>New information about the chat member</summary>
        [Newtonsoft.Json.JsonProperty("new_chat_member")]
        
        public ChatMember NewChatMember { get; set; } = default!;

        /// <summary><em>Optional</em>. Chat invite link, which was used by the user to join the chat; for joining by invite link events only.</summary>
        [Newtonsoft.Json.JsonProperty("invite_link")]
        public ChatInviteLink? InviteLink { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the user joined the chat after sending a direct join request without using an invite link and being approved by an administrator</summary>
        [Newtonsoft.Json.JsonProperty("via_join_request")]
        public bool ViaJoinRequest { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the user joined the chat via a chat folder invite link</summary>
        [Newtonsoft.Json.JsonProperty("via_chat_folder_invite_link")]
        public bool ViaChatFolderInviteLink { get; set; }
    }
}
