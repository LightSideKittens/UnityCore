// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using LabeledPrice = Telegram.Bot.Types.Payments.LabeledPrice;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to send invoices.<para>Returns: The sent <see cref="Message"/> is returned.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SendInvoiceRequest : RequestBase<Message>, IChatTargetable
    {
        /// <summary>Use this method to send invoices.<para>Returns: The sent <see cref="Message"/> is returned.</para></summary>
        public SendInvoiceRequest() : base("sendInvoice")
        {
        }

        /// <summary>Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</summary>
        [Newtonsoft.Json.JsonProperty("chat_id")]
        
        public ChatId ChatId { get; set; }

        /// <summary>Product name, 1-32 characters</summary>
        
        public string Title { get; set; }

        /// <summary>Product description, 1-255 characters</summary>
        
        public string Description { get; set; }

        /// <summary>Bot-defined invoice payload, 1-128 bytes. This will not be displayed to the user, use it for your internal processes.</summary>
        
        public string Payload { get; set; }

        /// <summary>Three-letter ISO 4217 currency code, see <a href="https://core.telegram.org/bots/payments#supported-currencies">more on currencies</a>. Pass “XTR” for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        
        public string Currency { get; set; }

        /// <summary>Price breakdown, a list of components (e.g. product price, tax, discount, delivery cost, delivery tax, bonus, etc.). Must contain exactly one item for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        
        public IEnumerable<LabeledPrice> Prices { get; set; }

        /// <summary>Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        public int? MessageThreadId { get; set; }

        /// <summary>Payment provider token, obtained via <a href="https://t.me/botfather">@BotFather</a>. Pass an empty string for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("provider_token")]
        public string? ProviderToken { get; set; }

        /// <summary>The maximum accepted amount for tips in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). For example, for a maximum tip of <c>US$ 1.45</c> pass <c><see cref="MaxTipAmount">MaxTipAmount</see> = 145</c>. See the <em>exp</em> parameter in <a href="https://core.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies). Defaults to 0. Not supported for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("max_tip_amount")]
        public int? MaxTipAmount { get; set; }

        /// <summary>A array of suggested amounts of tips in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). At most 4 suggested tip amounts can be specified. The suggested tip amounts must be positive, passed in a strictly increased order and must not exceed <see cref="MaxTipAmount">MaxTipAmount</see>.</summary>
        [Newtonsoft.Json.JsonProperty("suggested_tip_amounts")]
        public IEnumerable<int>? SuggestedTipAmounts { get; set; }

        /// <summary>Unique deep-linking parameter. If left empty, <b>forwarded copies</b> of the sent message will have a <em>Pay</em> button, allowing multiple users to pay directly from the forwarded message, using the same invoice. If non-empty, forwarded copies of the sent message will have a <em>URL</em> button with a deep link to the bot (instead of a <em>Pay</em> button), with the value used as the start parameter</summary>
        [Newtonsoft.Json.JsonProperty("start_parameter")]
        public string? StartParameter { get; set; }

        /// <summary>JSON-serialized data about the invoice, which will be shared with the payment provider. A detailed description of required fields should be provided by the payment provider.</summary>
        [Newtonsoft.Json.JsonProperty("provider_data")]
        public string? ProviderData { get; set; }

        /// <summary>URL of the product photo for the invoice. Can be a photo of the goods or a marketing image for a service. People like it better when they see what they are paying for.</summary>
        [Newtonsoft.Json.JsonProperty("photo_url")]
        public string? PhotoUrl { get; set; }

        /// <summary>Photo size in bytes</summary>
        [Newtonsoft.Json.JsonProperty("photo_size")]
        public int? PhotoSize { get; set; }

        /// <summary>Photo width</summary>
        [Newtonsoft.Json.JsonProperty("photo_width")]
        public int? PhotoWidth { get; set; }

        /// <summary>Photo height</summary>
        [Newtonsoft.Json.JsonProperty("photo_height")]
        public int? PhotoHeight { get; set; }

        /// <summary>Pass <see langword="true"/> if you require the user's full name to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_name")]
        public bool NeedName { get; set; }

        /// <summary>Pass <see langword="true"/> if you require the user's phone number to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_phone_number")]
        public bool NeedPhoneNumber { get; set; }

        /// <summary>Pass <see langword="true"/> if you require the user's email address to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_email")]
        public bool NeedEmail { get; set; }

        /// <summary>Pass <see langword="true"/> if you require the user's shipping address to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_shipping_address")]
        public bool NeedShippingAddress { get; set; }

        /// <summary>Pass <see langword="true"/> if the user's phone number should be sent to the provider. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("send_phone_number_to_provider")]
        public bool SendPhoneNumberToProvider { get; set; }

        /// <summary>Pass <see langword="true"/> if the user's email address should be sent to the provider. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("send_email_to_provider")]
        public bool SendEmailToProvider { get; set; }

        /// <summary>Pass <see langword="true"/> if the final price depends on the shipping method. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("is_flexible")]
        public bool IsFlexible { get; set; }

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

        /// <summary>An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>. If empty, one 'Pay <c>total price</c>' button will be shown. If not empty, the first button must be a Pay button.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }
    }
}
