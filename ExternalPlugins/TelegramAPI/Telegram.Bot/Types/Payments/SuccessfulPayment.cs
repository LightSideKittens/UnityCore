// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types.Payments
{
    /// <summary>This object contains basic information about a successful payment. Note that if the buyer initiates a chargeback with the relevant payment provider following this transaction, the funds may be debited from your balance. This is outside of Telegram's control.</summary>
    public partial class SuccessfulPayment
    {
        /// <summary>Three-letter ISO 4217 <a href="https://core.telegram.org/bots/payments#supported-currencies">currency</a> code, or “XTR” for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a></summary>
        
        public string Currency { get; set; } = default!;

        /// <summary>Total price in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the <em>exp</em> parameter in <a href="https://core.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
        [Newtonsoft.Json.JsonProperty("total_amount")]
        
        public int TotalAmount { get; set; }

        /// <summary>Bot-specified invoice payload</summary>
        [Newtonsoft.Json.JsonProperty("invoice_payload")]
        
        public string InvoicePayload { get; set; } = default!;

        /// <summary><em>Optional</em>. Expiration date of the subscription,; for recurring payments only</summary>
        [Newtonsoft.Json.JsonProperty("subscription_expiration_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? SubscriptionExpirationDate { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the payment is a recurring payment for a subscription</summary>
        [Newtonsoft.Json.JsonProperty("is_recurring")]
        public bool IsRecurring { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the payment is the first payment for a subscription</summary>
        [Newtonsoft.Json.JsonProperty("is_first_recurring")]
        public bool IsFirstRecurring { get; set; }

        /// <summary><em>Optional</em>. Identifier of the shipping option chosen by the user</summary>
        [Newtonsoft.Json.JsonProperty("shipping_option_id")]
        public string? ShippingOptionId { get; set; }

        /// <summary><em>Optional</em>. Order information provided by the user</summary>
        [Newtonsoft.Json.JsonProperty("order_info")]
        public OrderInfo? OrderInfo { get; set; }

        /// <summary>Telegram payment identifier</summary>
        [Newtonsoft.Json.JsonProperty("telegram_payment_charge_id")]
        
        public string TelegramPaymentChargeId { get; set; } = default!;

        /// <summary>Provider payment identifier</summary>
        [Newtonsoft.Json.JsonProperty("provider_payment_charge_id")]
        
        public string ProviderPaymentChargeId { get; set; } = default!;
    }
}
