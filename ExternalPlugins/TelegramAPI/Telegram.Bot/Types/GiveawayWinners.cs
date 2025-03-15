// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a message about the completion of a giveaway with public winners.</summary>
    public partial class GiveawayWinners
    {
        /// <summary>The chat that created the giveaway</summary>
        
        public Chat Chat { get; set; } = default!;

        /// <summary>Identifier of the message with the giveaway in the chat</summary>
        [Newtonsoft.Json.JsonProperty("giveaway_message_id")]
        
        public int GiveawayMessageId { get; set; }

        /// <summary>Point in time when winners of the giveaway were selected</summary>
        [Newtonsoft.Json.JsonProperty("winners_selection_date")]
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime WinnersSelectionDate { get; set; }

        /// <summary>Total number of winners in the giveaway</summary>
        [Newtonsoft.Json.JsonProperty("winner_count")]
        
        public int WinnerCount { get; set; }

        /// <summary>List of up to 100 winners of the giveaway</summary>
        
        public User[] Winners { get; set; } = default!;

        /// <summary><em>Optional</em>. The number of other chats the user had to join in order to be eligible for the giveaway</summary>
        [Newtonsoft.Json.JsonProperty("additional_chat_count")]
        public int? AdditionalChatCount { get; set; }

        /// <summary><em>Optional</em>. The number of Telegram Stars that were split between giveaway winners; for Telegram Star giveaways only</summary>
        [Newtonsoft.Json.JsonProperty("prize_star_count")]
        public int? PrizeStarCount { get; set; }

        /// <summary><em>Optional</em>. The number of months the Telegram Premium subscription won from the giveaway will be active for; for Telegram Premium giveaways only</summary>
        [Newtonsoft.Json.JsonProperty("premium_subscription_month_count")]
        public int? PremiumSubscriptionMonthCount { get; set; }

        /// <summary><em>Optional</em>. Number of undistributed prizes</summary>
        [Newtonsoft.Json.JsonProperty("unclaimed_prize_count")]
        public int? UnclaimedPrizeCount { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if only users who had joined the chats after the giveaway started were eligible to win</summary>
        [Newtonsoft.Json.JsonProperty("only_new_members")]
        public bool OnlyNewMembers { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the giveaway was canceled because the payment for it was refunded</summary>
        [Newtonsoft.Json.JsonProperty("was_refunded")]
        public bool WasRefunded { get; set; }

        /// <summary><em>Optional</em>. Description of additional giveaway prize</summary>
        [Newtonsoft.Json.JsonProperty("prize_description")]
        public string? PrizeDescription { get; set; }
    }
}
