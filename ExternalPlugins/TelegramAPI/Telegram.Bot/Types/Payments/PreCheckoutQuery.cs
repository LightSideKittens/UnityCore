// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types.Payments
{
    /// <summary>This object contains information about an incoming pre-checkout query.</summary>
    public partial class PreCheckoutQuery
    {
        /// <summary>Unique query identifier</summary>
        
        public string Id { get; set; } = default!;

        /// <summary>User who sent the query</summary>
        
        public User From { get; set; } = default!;

        /// <summary>Three-letter ISO 4217 <a href="https://core.telegram.org/bots/payments#supported-currencies">currency</a> code, or “XTR” for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a></summary>
        
        public string Currency { get; set; } = default!;

        /// <summary>Total price in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the <em>exp</em> parameter in <a href="https://core.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
        [Newtonsoft.Json.JsonProperty("total_amount")]
        
        public int TotalAmount { get; set; }

        /// <summary>Bot-specified invoice payload</summary>
        [Newtonsoft.Json.JsonProperty("invoice_payload")]
        
        public string InvoicePayload { get; set; } = default!;

        /// <summary><em>Optional</em>. Identifier of the shipping option chosen by the user</summary>
        [Newtonsoft.Json.JsonProperty("shipping_option_id")]
        public string? ShippingOptionId { get; set; }

        /// <summary><em>Optional</em>. Order information provided by the user</summary>
        [Newtonsoft.Json.JsonProperty("order_info")]
        public OrderInfo? OrderInfo { get; set; }
    }
}
