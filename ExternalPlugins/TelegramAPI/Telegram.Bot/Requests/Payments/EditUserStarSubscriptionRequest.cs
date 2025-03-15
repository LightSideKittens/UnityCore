// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Requests
{
    /// <summary>Allows the bot to cancel or re-enable extension of a subscription paid in Telegram Stars.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditUserStarSubscriptionRequest : RequestBase<bool>, IUserTargetable
    {
        /// <summary>Allows the bot to cancel or re-enable extension of a subscription paid in Telegram Stars.<para>Returns: </para></summary>
        public EditUserStarSubscriptionRequest() : base("editUserStarSubscription")
        {
        }

        /// <summary>Identifier of the user whose subscription will be edited</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>Telegram payment identifier for the subscription</summary>
        [Newtonsoft.Json.JsonProperty("telegram_payment_charge_id")]
        
        public string TelegramPaymentChargeId { get; set; }

        /// <summary>Pass <see langword="true"/> to cancel extension of the user subscription; the subscription must be active up to the end of the current subscription period. Pass <see langword="false"/> to allow the user to re-enable a subscription that was previously canceled by the bot.</summary>
        [Newtonsoft.Json.JsonProperty("is_canceled")]
        public bool IsCanceled { get; set; }
    }
}
