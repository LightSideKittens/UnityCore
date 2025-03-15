// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Requests
{
    /// <summary>Refunds a successful payment in <a href="https://t.me/BotNews/90">Telegram Stars</a>.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class RefundStarPaymentRequest : RequestBase<bool>, IUserTargetable
    {
        /// <summary>Refunds a successful payment in <a href="https://t.me/BotNews/90">Telegram Stars</a>.<para>Returns: </para></summary>
        public RefundStarPaymentRequest() : base("refundStarPayment")
        {
        }

        /// <summary>Identifier of the user whose payment will be refunded</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Telegram payment identifier</summary>
        [Newtonsoft.Json.JsonProperty("telegram_payment_charge_id")]
        
        public string TelegramPaymentChargeId { get; set; }
    }
}
