// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Telegram.Bot.Requests
{
    /// <summary>Stores a message that can be sent by a user of a Mini App.<para>Returns: A <see cref="PreparedInlineMessage"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SavePreparedInlineMessageRequest : RequestBase<PreparedInlineMessage>, IUserTargetable
    {
        /// <summary>Stores a message that can be sent by a user of a Mini App.<para>Returns: A <see cref="PreparedInlineMessage"/> object.</para></summary>
        public SavePreparedInlineMessageRequest() : base("savePreparedInlineMessage")
        {
        }

        /// <summary>Unique identifier of the target user that can use the prepared message</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>An object describing the message to be sent</summary>
        
        public InlineQueryResult Result { get; set; }

        /// <summary>Pass <see langword="true"/> if the message can be sent to private chats with users</summary>
        [Newtonsoft.Json.JsonProperty("allow_user_chats")]
        public bool AllowUserChats { get; set; }

        /// <summary>Pass <see langword="true"/> if the message can be sent to private chats with bots</summary>
        [Newtonsoft.Json.JsonProperty("allow_bot_chats")]
        public bool AllowBotChats { get; set; }

        /// <summary>Pass <see langword="true"/> if the message can be sent to group and supergroup chats</summary>
        [Newtonsoft.Json.JsonProperty("allow_group_chats")]
        public bool AllowGroupChats { get; set; }

        /// <summary>Pass <see langword="true"/> if the message can be sent to channel chats</summary>
        [Newtonsoft.Json.JsonProperty("allow_channel_chats")]
        public bool AllowChannelChats { get; set; }
    }
}
