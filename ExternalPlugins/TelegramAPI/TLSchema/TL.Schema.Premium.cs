using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Describes a Telegram Premium subscription option		<para>See <a href="https://corefork.telegram.org/constructor/premiumSubscriptionOption"/></para></summary>
    [TLDef(0x5F2D1DF2)]
    public sealed partial class PremiumSubscriptionOption : IObject
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>Identifier of the last in-store transaction for the currently used subscription on the current account.</summary>
        [IfFlag(3)] public string transaction;
        /// <summary>Duration of subscription in months</summary>
        public int months;
        /// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
        public string currency;
        /// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
        public long amount;
        /// <summary><a href="https://corefork.telegram.org/api/links">Deep link</a> used to initiate payment</summary>
        public string bot_url;
        /// <summary>Store product ID, only for official apps</summary>
        [IfFlag(0)] public string store_product;

        [Flags] public enum Flags : uint
        {
            /// <summary>Field <see cref="store_product"/> has a value</summary>
            has_store_product = 0x1,
            /// <summary>Whether this subscription option is currently in use.</summary>
            current = 0x2,
            /// <summary>Whether this subscription option can be used to upgrade the existing Telegram Premium subscription. When upgrading Telegram Premium subscriptions bought through stores, make sure that the store transaction ID is equal to <c>transaction</c>, to avoid upgrading someone else's account, if the client is currently logged into multiple accounts.</summary>
            can_purchase_upgrade = 0x4,
            /// <summary>Field <see cref="transaction"/> has a value</summary>
            has_transaction = 0x8,
        }
    }
    
	/// <summary>Telegram Premium gift option		<para>See <a href="https://corefork.telegram.org/constructor/premiumGiftOption"/></para></summary>
	[TLDef(0x74C34319)]
	public sealed partial class PremiumGiftOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Duration of gifted Telegram Premium subscription</summary>
		public int months;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;
		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;
		/// <summary>An <a href="https://corefork.telegram.org/api/links#invoice-links">invoice deep link »</a> to an invoice for in-app payment, using the official Premium bot; may be empty if direct payment isn't available.</summary>
		public string bot_url;
		/// <summary>An identifier for the App Store/Play Store product associated with the Premium gift.</summary>
		[IfFlag(0)] public string store_product;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="store_product"/> has a value</summary>
			has_store_product = 0x1,
		}
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/giveaways">giveaway/gift</a> option.		<para>See <a href="https://corefork.telegram.org/constructor/premiumGiftCodeOption"/></para></summary>
	[TLDef(0x257E962B)]
	public sealed partial class PremiumGiftCodeOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Number of users which will be able to activate the gift codes.</summary>
		public int users;
		/// <summary>Duration in months of each gifted <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription.</summary>
		public int months;
		/// <summary>Identifier of the store product associated with the option, official apps only.</summary>
		[IfFlag(0)] public string store_product;
		/// <summary>Number of times the store product must be paid</summary>
		[IfFlag(1)] public int store_quantity;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;
		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="store_product"/> has a value</summary>
			has_store_product = 0x1,
			/// <summary>Field <see cref="store_quantity"/> has a value</summary>
			has_store_quantity = 0x2,
		}
	}

	/// <summary>List of <a href="https://corefork.telegram.org/api/boost">boosts</a> that were applied to a peer by multiple users.		<para>See <a href="https://corefork.telegram.org/constructor/premium.boostsList"/></para></summary>
	[TLDef(0x86F8613C)]
	public sealed partial class Premium_BoostsList : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of results</summary>
		public int count;
		/// <summary><a href="https://corefork.telegram.org/api/boost">Boosts</a></summary>
		public Boost[] boosts;
		/// <summary>Offset that can be used for <a href="https://corefork.telegram.org/api/offsets">pagination</a>.</summary>
		[IfFlag(0)] public string next_offset;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
	}
	/// <summary>A list of peers we are currently <a href="https://corefork.telegram.org/api/boost">boosting</a>, and how many <a href="https://corefork.telegram.org/api/boost">boost slots</a> we have left.		<para>See <a href="https://corefork.telegram.org/constructor/premium.myBoosts"/></para></summary>
	[TLDef(0x9AE228E2)]
	public sealed partial class Premium_MyBoosts : IObject, IPeerResolver
	{
		/// <summary>Info about boosted peers and remaining boost slots.</summary>
		public MyBoost[] my_boosts;
		/// <summary>Referenced chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Referenced users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Contains info about the current <a href="https://corefork.telegram.org/api/boost">boost status</a> of a peer.		<para>See <a href="https://corefork.telegram.org/constructor/premium.boostsStatus"/></para></summary>
	[TLDef(0x4959427A)]
	public sealed partial class Premium_BoostsStatus : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The current boost level of the channel/supergroup.</summary>
		public int level;
		/// <summary>The number of boosts acquired so far in the current level.</summary>
		public int current_level_boosts;
		/// <summary>Total number of boosts acquired so far.</summary>
		public int boosts;
		/// <summary>The number of boosts acquired from created Telegram Premium <a href="https://corefork.telegram.org/api/giveaways">gift codes</a> and <a href="https://corefork.telegram.org/api/giveaways">giveaways</a>; only returned to channel/supergroup admins.</summary>
		[IfFlag(4)] public int gift_boosts;
		/// <summary>Total number of boosts needed to reach the next level; if absent, the next level isn't available.</summary>
		[IfFlag(0)] public int next_level_boosts;
		/// <summary>Only returned to channel/supergroup admins: contains the approximated number of Premium users subscribed to the channel/supergroup, related to the total number of subscribers.</summary>
		[IfFlag(1)] public StatsPercentValue premium_audience;
		/// <summary><a href="https://corefork.telegram.org/api/links#boost-links">Boost deep link »</a> that can be used to boost the chat.</summary>
		public string boost_url;
		/// <summary>A list of prepaid <a href="https://corefork.telegram.org/api/giveaways">giveaways</a> available for the chat; only returned to channel/supergroup admins.</summary>
		[IfFlag(3)] public PrepaidGiveawayBase[] prepaid_giveaways;
		/// <summary>Indicates which of our <a href="https://corefork.telegram.org/api/boost">boost slots</a> we've assigned to this peer (populated if <c>my_boost</c> is set).</summary>
		[IfFlag(2)] public int[] my_boost_slots;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_level_boosts"/> has a value</summary>
			has_next_level_boosts = 0x1,
			/// <summary>Field <see cref="premium_audience"/> has a value</summary>
			has_premium_audience = 0x2,
			/// <summary>Whether we're currently boosting this channel/supergroup, <c>my_boost_slots</c> will also be set.</summary>
			my_boost = 0x4,
			/// <summary>Field <see cref="prepaid_giveaways"/> has a value</summary>
			has_prepaid_giveaways = 0x8,
			/// <summary>Field <see cref="gift_boosts"/> has a value</summary>
			has_gift_boosts = 0x10,
		}
	}
    
}