// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to send phone contacts.<para>Returns: The sent <see cref="Message"/> is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SendContactRequest : RequestBase<Message>, IChatTargetable, IBusinessConnectable
    {
        /// <summary>Use this method to send phone contacts.<para>Returns: The sent <see cref="Message"/> is returned.</para></summary>
        public SendContactRequest() : base("sendContact")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Contact's phone number</summary>
        [Newtonsoft.Json.JsonProperty("phone_number")]
        
        public string PhoneNumber { get; set; }

        /// <summary>Contact's first name</summary>
        [Newtonsoft.Json.JsonProperty("first_name")]
        
        public string FirstName { get; set; }

        /// <summary>Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        public int? MessageThreadId { get; set; }

        /// <summary>Contact's last name</summary>
        [Newtonsoft.Json.JsonProperty("last_name")]
        public string? LastName { get; set; }

        /// <summary>Additional data about the contact in the form of a <a href="https://en.wikipedia.org/wiki/VCard">vCard</a>, 0-2048 bytes</summary>
        public string? Vcard { get; set; }

        /// <summary>Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</summary>
        [Newtonsoft.Json.JsonProperty("disable_notification")]
        public bool DisableNotification { get; set; }

        /// <summary>Protects the contents of the sent message from forwarding and saving</summary>
        [Newtonsoft.Json.JsonProperty("protect_content")]
        public bool ProtectContent { get; set; }

        /// <summary>Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</summary>
        [Newtonsoft.Json.JsonProperty("allow_paid_broadcast")]
        public bool AllowPaidBroadcast { get; set; }

        /// <summary>Unique identifier of the message effect to be added to the message; for private chats only</summary>
        [Newtonsoft.Json.JsonProperty("message_effect_id")]
        public string? MessageEffectId { get; set; }

        /// <summary>Description of the message to reply to</summary>
        [Newtonsoft.Json.JsonProperty("reply_parameters")]
        public ReplyParameters? ReplyParameters { get; set; }

        /// <summary>Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public ReplyMarkup? ReplyMarkup { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message will be sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
