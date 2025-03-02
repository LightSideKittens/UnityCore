using System;

namespace TL
{
#pragma warning disable CS1574
	/// <summary>Source of an incoming <a href="https://corefork.telegram.org/api/stars">Telegram Star transaction</a>, or its recipient for outgoing <a href="https://corefork.telegram.org/api/stars">Telegram Star transactions</a>.		<para>See <a href="https://corefork.telegram.org/type/StarsTransactionPeer"/></para>		<para>Derived classes: <see cref="StarsTransactionPeerUnsupported"/>, <see cref="StarsTransactionPeerAppStore"/>, <see cref="StarsTransactionPeerPlayMarket"/>, <see cref="StarsTransactionPeerPremiumBot"/>, <see cref="StarsTransactionPeerFragment"/>, <see cref="StarsTransactionPeer"/>, <see cref="StarsTransactionPeerAds"/>, <see cref="StarsTransactionPeerAPI"/></para></summary>
	public abstract partial class StarsTransactionPeerBase : IObject
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction that cannot be described using the current layer.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerUnsupported"/></para></summary>
	[TLDef(0x95F2BFE4)]
	public sealed partial class StarsTransactionPeerUnsupported : StarsTransactionPeerBase
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction with the App Store, used when purchasing Telegram Stars through the App Store.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerAppStore"/></para></summary>
	[TLDef(0xB457B375)]
	public sealed partial class StarsTransactionPeerAppStore : StarsTransactionPeerBase
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction with the Play Store, used when purchasing Telegram Stars through the Play Store.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerPlayMarket"/></para></summary>
	[TLDef(0x7B560A0B)]
	public sealed partial class StarsTransactionPeerPlayMarket : StarsTransactionPeerBase
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction made using <a href="https://t.me/premiumbot">@PremiumBot</a> (i.e. using the <see cref="InputInvoiceStars"/> flow described <a href="https://corefork.telegram.org/api/stars#buying-or-gifting-stars">here »</a>).		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerPremiumBot"/></para></summary>
	[TLDef(0x250DBAF8)]
	public sealed partial class StarsTransactionPeerPremiumBot : StarsTransactionPeerBase
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction with <a href="https://fragment.com">Fragment</a>, used when purchasing Telegram Stars through <a href="https://fragment.com">Fragment</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerFragment"/></para></summary>
	[TLDef(0xE92FD902)]
	public sealed partial class StarsTransactionPeerFragment : StarsTransactionPeerBase
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction with another peer.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeer"/></para></summary>
	[TLDef(0xD80DA15D)]
	public sealed partial class StarsTransactionPeer : StarsTransactionPeerBase
	{
		/// <summary>The peer.</summary>
		public Peer peer;
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction used to pay for <a href="https://corefork.telegram.org/api/stars#paying-for-ads">Telegram ads as specified here »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerAds"/></para></summary>
	[TLDef(0x60682812)]
	public sealed partial class StarsTransactionPeerAds : StarsTransactionPeerBase
	{
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/stars">Telegram Star</a> transaction used to pay for paid API usage, such as <a href="https://corefork.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">paid bot broadcasts</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransactionPeerAPI"/></para></summary>
	[TLDef(0xF9677AAD)]
	public sealed partial class StarsTransactionPeerAPI : StarsTransactionPeerBase
	{
	}

	/// <summary><a href="https://corefork.telegram.org/api/stars">Telegram Stars topup option</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsTopupOption"/></para></summary>
	[TLDef(0x0BD915C0)]
	public sealed partial class StarsTopupOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Amount of Telegram stars.</summary>
		public long stars;

		/// <summary>Identifier of the store product associated with the option, official apps only.</summary>
		[IfFlag(0)] public string store_product;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="store_product"/> has a value</summary>
			has_store_product = 0x1,

			/// <summary>If set, the option must only be shown in the full list of topup options.</summary>
			extended = 0x2,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stars">Telegram Stars transaction »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsTransaction"/></para></summary>
	[TLDef(0x64DFC926)]
	public sealed partial class StarsTransaction : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Transaction ID.</summary>
		public string id;

		/// <summary>Amount of Stars (negative for outgoing transactions).</summary>
		public StarsAmount stars;

		/// <summary>Date of the transaction (unixtime).</summary>
		public DateTime date;

		/// <summary>Source of the incoming transaction, or its recipient for outgoing transactions.</summary>
		public StarsTransactionPeerBase peer;

		/// <summary>For transactions with bots, title of the bought product.</summary>
		[IfFlag(0)] public string title;

		/// <summary>For transactions with bots, description of the bought product.</summary>
		[IfFlag(1)] public string description;

		/// <summary>For transactions with bots, photo of the bought product.</summary>
		[IfFlag(2)] public WebDocumentBase photo;

		/// <summary>If neither <c>pending</c> nor <c>failed</c> are set, the transaction was completed successfully, and this field will contain the point in time (Unix timestamp) when the withdrawal was completed successfully.</summary>
		[IfFlag(5)] public DateTime transaction_date;

		/// <summary>If neither <c>pending</c> nor <c>failed</c> are set, the transaction was completed successfully, and this field will contain a URL where the withdrawal transaction can be viewed.</summary>
		[IfFlag(5)] public string transaction_url;

		/// <summary>Bot specified invoice payload (i.e. the <c>payload</c> passed to <see cref="InputMediaInvoice"/> when <a href="https://corefork.telegram.org/api/payments">creating the invoice</a>).</summary>
		[IfFlag(7)] public byte[] bot_payload;

		/// <summary>For <a href="https://corefork.telegram.org/api/paid-media">paid media transactions »</a>, message ID of the paid media posted to <c>peer.peer</c> (can point to a deleted message; either way, <c>extended_media</c> will always contain the bought media).</summary>
		[IfFlag(8)] public int msg_id;

		/// <summary>The purchased <a href="https://corefork.telegram.org/api/paid-media">paid media »</a>.</summary>
		[IfFlag(9)] public MessageMedia[] extended_media;

		/// <summary>The number of seconds between consecutive Telegram Star debiting for <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscriptions »</a>.</summary>
		[IfFlag(12)] public int subscription_period;

		/// <summary>ID of the message containing the <see cref="MessageMediaGiveaway"/>, for incoming <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">star giveaway prizes</a>.</summary>
		[IfFlag(13)] public int giveaway_post_id;

		/// <summary>This transaction indicates a purchase or a sale (conversion back to Stars) of a <a href="https://corefork.telegram.org/api/stars">gift »</a>.</summary>
		[IfFlag(14)] public StarGiftBase stargift;

		/// <summary>This transaction is payment for <a href="https://corefork.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">paid bot broadcasts</a>.  <br/>Paid broadcasts are only allowed if the <c>allow_paid_floodskip</c> parameter of <see cref="SchemaExtensions.Messages_SendMessage">Messages_SendMessage</see> and other message sending methods is set while trying to broadcast more than 30 messages per second to bot users. <br/>The integer value returned by this flag indicates the number of billed API calls.</summary>
		[IfFlag(15)] public int floodskip_number;

		/// <summary>This transaction is the receival (or refund) of an <a href="https://corefork.telegram.org/api/bots/referrals">affiliate commission</a> (i.e. this is the transaction received by the peer that created the <a href="https://corefork.telegram.org/api/links#referral-links">referral link</a>, flag 17 is for transactions made by users that imported the referral link).</summary>
		[IfFlag(16)] public int starref_commission_permille;

		/// <summary>For transactions made by <a href="https://corefork.telegram.org/api/bots/referrals">referred users</a>, the peer that received the affiliate commission.</summary>
		[IfFlag(17)] public Peer starref_peer;

		/// <summary>For transactions made by <a href="https://corefork.telegram.org/api/bots/referrals">referred users</a>, the amount of Telegram Stars received by the affiliate, can be negative for refunds.</summary>
		[IfFlag(17)] public StarsAmount starref_amount;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,

			/// <summary>Field <see cref="description"/> has a value</summary>
			has_description = 0x2,

			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x4,

			/// <summary>Whether this transaction is a refund.</summary>
			refund = 0x8,

			/// <summary>The transaction is currently pending.</summary>
			pending = 0x10,

			/// <summary>Fields <see cref="transaction_date"/> and <see cref="transaction_url"/> have a value</summary>
			has_transaction_date = 0x20,

			/// <summary>This transaction has failed.</summary>
			failed = 0x40,

			/// <summary>Field <see cref="bot_payload"/> has a value</summary>
			has_bot_payload = 0x80,

			/// <summary>Field <see cref="msg_id"/> has a value</summary>
			has_msg_id = 0x100,

			/// <summary>Field <see cref="extended_media"/> has a value</summary>
			has_extended_media = 0x200,

			/// <summary>This transaction was a gift from the user in <c>peer.peer</c>.</summary>
			gift = 0x400,

			/// <summary>This transaction is a <a href="https://corefork.telegram.org/api/reactions#paid-reactions">paid reaction »</a>.</summary>
			reaction = 0x800,

			/// <summary>Field <see cref="subscription_period"/> has a value</summary>
			has_subscription_period = 0x1000,

			/// <summary>Field <see cref="giveaway_post_id"/> has a value</summary>
			has_giveaway_post_id = 0x2000,

			/// <summary>Field <see cref="stargift"/> has a value</summary>
			has_stargift = 0x4000,

			/// <summary>Field <see cref="floodskip_number"/> has a value</summary>
			has_floodskip_number = 0x8000,

			/// <summary>Field <see cref="starref_commission_permille"/> has a value</summary>
			has_starref_commission_permille = 0x10000,

			/// <summary>Fields <see cref="starref_peer"/> and <see cref="starref_amount"/> have a value</summary>
			has_starref_peer = 0x20000,
			stargift_upgrade = 0x40000,
		}
	}

	/// <summary>Describes <a href="https://corefork.telegram.org/api/stars">Telegram Star revenue balances »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsRevenueStatus"/></para></summary>
	[TLDef(0xFEBE5491)]
	public sealed partial class StarsRevenueStatus : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Amount of not-yet-withdrawn Telegram Stars.</summary>
		public StarsAmount current_balance;

		/// <summary>Amount of withdrawable Telegram Stars.</summary>
		public StarsAmount available_balance;

		/// <summary>Total amount of earned Telegram Stars.</summary>
		public StarsAmount overall_revenue;

		/// <summary>Unixtime indicating when will withdrawal be available to the user. If not set, withdrawal can be started now.</summary>
		[IfFlag(1)] public int next_withdrawal_at;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, the user may <a href="https://corefork.telegram.org/api/stars#withdrawing-stars">withdraw</a> up to <c>available_balance</c> stars.</summary>
			withdrawal_enabled = 0x1,

			/// <summary>Field <see cref="next_withdrawal_at"/> has a value</summary>
			has_next_withdrawal_at = 0x2,
		}
	}


	/// <summary><a href="https://corefork.telegram.org/api/stars#buying-or-gifting-stars">Telegram Stars gift option</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsGiftOption"/></para></summary>
	[TLDef(0x5E0589F1)]
	public sealed partial class StarsGiftOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Amount of Telegram stars.</summary>
		public long stars;

		/// <summary>Identifier of the store product associated with the option, official apps only.</summary>
		[IfFlag(0)] public string store_product;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="store_product"/> has a value</summary>
			has_store_product = 0x1,

			/// <summary>If set, the option must only be shown in the full list of topup options.</summary>
			extended = 0x2,
		}
	}

	/// <summary>Pricing of a <a href="https://corefork.telegram.org/api/invites#paid-invite-links">Telegram Star subscription »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsSubscriptionPricing"/></para></summary>
	[TLDef(0x05416D58)]
	public sealed partial class StarsSubscriptionPricing : IObject
	{
		/// <summary>The user should pay <c>amount</c> stars every <c>period</c> seconds to gain and maintain access to the channel. <br/>Currently the only allowed subscription period is <c>30*24*60*60</c>, i.e. the user will be debited amount stars every month.</summary>
		public int period;

		/// <summary>Price of the subscription in Telegram Stars.</summary>
		public long amount;
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/invites#paid-invite-links">Telegram Star subscription »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsSubscription"/></para></summary>
	[TLDef(0x2E6EAB1A)]
	public sealed partial class StarsSubscription : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Subscription ID.</summary>
		public string id;

		/// <summary>Identifier of the associated private chat.</summary>
		public Peer peer;

		/// <summary>Expiration date of the current subscription period.</summary>
		public DateTime until_date;

		/// <summary>Pricing of the subscription in Telegram Stars.</summary>
		public StarsSubscriptionPricing pricing;

		/// <summary>Invitation link, used to renew the subscription after cancellation or expiration.</summary>
		[IfFlag(3)] public string chat_invite_hash;

		/// <summary>For bot subscriptions, the title of the subscription invoice</summary>
		[IfFlag(4)] public string title;

		/// <summary>For bot subscriptions, the photo from the subscription invoice</summary>
		[IfFlag(5)] public WebDocumentBase photo;

		/// <summary>For bot subscriptions, the <a href="https://corefork.telegram.org/api/links#invoice-links">identifier</a> of the subscription invoice</summary>
		[IfFlag(6)] public string invoice_slug;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether this subscription was cancelled.</summary>
			canceled = 0x1,

			/// <summary>Whether we left the associated private channel, but we can still rejoin it using <see cref="SchemaExtensions.Payments_FulfillStarsSubscription">Payments_FulfillStarsSubscription</see> because the current subscription period hasn't expired yet.</summary>
			can_refulfill = 0x2,

			/// <summary>Whether this subscription has expired because there are not enough stars on the user's balance to extend it.</summary>
			missing_balance = 0x4,

			/// <summary>Field <see cref="chat_invite_hash"/> has a value</summary>
			has_chat_invite_hash = 0x8,

			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x10,

			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x20,

			/// <summary>Field <see cref="invoice_slug"/> has a value</summary>
			has_invoice_slug = 0x40,

			/// <summary>Set if this <a href="https://corefork.telegram.org/api/subscriptions#bot-subscriptions">bot subscription</a> was cancelled by the bot</summary>
			bot_canceled = 0x80,
		}
	}

	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">Telegram Star giveaway</a> option.		<para>See <a href="https://corefork.telegram.org/constructor/starsGiveawayOption"/></para></summary>
	[TLDef(0x94CE852A)]
	public sealed partial class StarsGiveawayOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The number of Telegram Stars that will be distributed among winners</summary>
		public long stars;

		/// <summary>Number of times the chat will be boosted for one year if the <see cref="InputStorePaymentStarsGiveaway"/>.<c>boost_peer</c> flag is populated</summary>
		public int yearly_boosts;

		/// <summary>Identifier of the store product associated with the option, official apps only.</summary>
		[IfFlag(2)] public string store_product;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		/// <summary>Allowed options for the number of giveaway winners.</summary>
		public StarsGiveawayWinnersOption[] winners;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, this option must only be shown in the full list of giveaway options (i.e. they must be added to the list only when the user clicks on the expand button).</summary>
			extended = 0x1,

			/// <summary>If set, this option must be pre-selected by default in the option list.</summary>
			default_ = 0x2,

			/// <summary>Field <see cref="store_product"/> has a value</summary>
			has_store_product = 0x4,
		}
	}

	/// <summary>Allowed options for the number of giveaway winners.		<para>See <a href="https://corefork.telegram.org/constructor/starsGiveawayWinnersOption"/></para></summary>
	[TLDef(0x54236209)]
	public sealed partial class StarsGiveawayWinnersOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The number of users that will be randomly chosen as winners.</summary>
		public int users;

		/// <summary>The number of <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> each winner will receive.</summary>
		public long per_user_stars;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, this option must be pre-selected by default in the option list.</summary>
			default_ = 0x1,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/gifts">star gift, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/type/StarGift"/></para>		<para>Derived classes: <see cref="StarGift"/></para></summary>
	public abstract partial class StarGiftBase : IObject
	{
		/// <summary>Identifier of the gift</summary>
		public virtual long ID => default;

		/// <summary>For limited-supply gifts: the total number of gifts that was available in the initial supply.</summary>
		public virtual int AvailabilityTotal => default;
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/gifts">star gift, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/starGift"/></para></summary>
	[TLDef(0x02CC73C8)]
	public sealed partial class StarGift : StarGiftBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Identifier of the gift</summary>
		public long id;

		/// <summary><a href="https://corefork.telegram.org/api/stickers">Sticker</a> that represents the gift.</summary>
		public DocumentBase sticker;

		/// <summary>Price of the gift in <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public long stars;

		/// <summary>For limited-supply gifts: the remaining number of gifts that may be bought.</summary>
		[IfFlag(0)] public int availability_remains;

		/// <summary>For limited-supply gifts: the total number of gifts that was available in the initial supply.</summary>
		[IfFlag(0)] public int availability_total;

		/// <summary>The receiver of this gift may convert it to this many Telegram Stars, instead of displaying it on their profile page.<br/><c>convert_stars</c> will be equal to <c>stars</c> only if the gift was bought using recently bought Telegram Stars, otherwise it will be less than <c>stars</c>.</summary>
		public long convert_stars;

		/// <summary>For sold out gifts only: when was the gift first bought.</summary>
		[IfFlag(1)] public DateTime first_sale_date;

		/// <summary>For sold out gifts only: when was the gift last bought.</summary>
		[IfFlag(1)] public DateTime last_sale_date;

		[IfFlag(3)] public long upgrade_stars;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether this is a limited-supply gift.</summary>
			limited = 0x1,

			/// <summary>Whether this gift sold out and cannot be bought anymore.</summary>
			sold_out = 0x2,

			/// <summary>Whether this is a birthday-themed gift</summary>
			birthday = 0x4,

			/// <summary>Field <see cref="upgrade_stars"/> has a value</summary>
			has_upgrade_stars = 0x8,
		}

		/// <summary>Identifier of the gift</summary>
		public override long ID => id;

		/// <summary>For limited-supply gifts: the total number of gifts that was available in the initial supply.</summary>
		public override int AvailabilityTotal => availability_total;
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/starGiftUnique"/></para></summary>
	[TLDef(0x5C62D151)]
	public sealed partial class StarGiftUnique : StarGiftBase
	{
		public Flags flags;
		public long id;
		public string title;
		public string slug;
		public int num;
		[IfFlag(0)] public Peer owner_id;
		[IfFlag(1)] public string owner_name;
		[IfFlag(2)] public string owner_address;
		public StarGiftAttribute[] attributes;
		public int availability_issued;
		public int availability_total;
		[IfFlag(3)] public string gift_address;

		[Flags]
		public enum Flags : uint
		{
			has_owner_id = 0x1,
			has_owner_name = 0x2,
			has_owner_address = 0x4,
			has_gift_address = 0x8,
		}

		public override long ID => id;
		public override int AvailabilityTotal => availability_total;
	}

	/// <summary>Indo about an <a href="https://corefork.telegram.org/api/bots/referrals">affiliate program offered by a bot</a>		<para>See <a href="https://corefork.telegram.org/constructor/starRefProgram"/></para></summary>
	[TLDef(0xDD0C66F2)]
	public sealed partial class StarRefProgram : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>ID of the bot that offers the program</summary>
		public long bot_id;

		/// <summary>An affiliate gets a commission of <see cref="StarRefProgram"/>.<c>commission_permille</c>‰ <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> for every mini app transaction made by users they refer</summary>
		public int commission_permille;

		/// <summary>An affiliate gets a commission for every mini app transaction made by users they refer, for <c>duration_months</c> months after a referral link is imported, starting the bot for the first time</summary>
		[IfFlag(0)] public int duration_months;

		/// <summary>Point in time (Unix timestamp) when the affiliate program will be closed (optional, if not set the affiliate program isn't scheduled to be closed)</summary>
		[IfFlag(1)] public DateTime end_date;

		/// <summary>The amount of daily revenue per user in Telegram Stars of the bot that created the affiliate program. <br/>To obtain the approximated revenue per referred user, multiply this value by <c>commission_permille</c> and divide by <c>1000</c>.</summary>
		[IfFlag(2)] public StarsAmount daily_revenue_per_user;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="duration_months"/> has a value</summary>
			has_duration_months = 0x1,

			/// <summary>Field <see cref="end_date"/> has a value</summary>
			has_end_date = 0x2,

			/// <summary>Field <see cref="daily_revenue_per_user"/> has a value</summary>
			has_daily_revenue_per_user = 0x4,
		}
	}

	/// <summary>Describes a real (i.e. possibly decimal) amount of <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.		<para>See <a href="https://corefork.telegram.org/constructor/starsAmount"/></para></summary>
	[TLDef(0xBBB6B4A3)]
	public sealed partial class StarsAmount : IObject
	{
		/// <summary>The integer amount of Telegram Stars.</summary>
		public long amount;

		/// <summary>The decimal amount of Telegram Stars, expressed as nanostars (i.e. 1 nanostar is equal to <c>1/1'000'000'000</c>th of a Telegram Star). <br/>This field may also be negative (the allowed range is -999999999 to 999999999).</summary>
		public int nanos;
	}


	/// <summary><para>See <a href="https://corefork.telegram.org/type/StarGiftAttribute"/></para></summary>
	public abstract partial class StarGiftAttribute : IObject
	{
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/starGiftAttributeModel"/></para></summary>
	[TLDef(0x39D99013)]
	public sealed partial class StarGiftAttributeModel : StarGiftAttribute
	{
		public string name;
		public DocumentBase document;
		public int rarity_permille;
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/starGiftAttributePattern"/></para></summary>
	[TLDef(0x13ACFF19)]
	public sealed partial class StarGiftAttributePattern : StarGiftAttribute
	{
		public string name;
		public DocumentBase document;
		public int rarity_permille;
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/starGiftAttributeBackdrop"/></para></summary>
	[TLDef(0x94271762)]
	public sealed partial class StarGiftAttributeBackdrop : StarGiftAttribute
	{
		public string name;
		public int center_color;
		public int edge_color;
		public int pattern_color;
		public int text_color;
		public int rarity_permille;
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/starGiftAttributeOriginalDetails"/></para></summary>
	[TLDef(0xE0BFF26C)]
	public sealed partial class StarGiftAttributeOriginalDetails : StarGiftAttribute
	{
		public Flags flags;
		[IfFlag(0)] public Peer sender_id;
		public Peer recipient_id;
		public DateTime date;
		[IfFlag(1)] public TextWithEntities message;

		[Flags]
		public enum Flags : uint
		{
			has_sender_id = 0x1,
			has_message = 0x2,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/savedStarGift"/></para></summary>
	[TLDef(0x6056DBA5)]
	public sealed partial class SavedStarGift : IObject
	{
		public Flags flags;
		[IfFlag(1)] public Peer from_id;
		public DateTime date;
		public StarGiftBase gift;
		[IfFlag(2)] public TextWithEntities message;
		[IfFlag(3)] public int msg_id;
		[IfFlag(11)] public long saved_id;
		[IfFlag(4)] public long convert_stars;
		[IfFlag(6)] public long upgrade_stars;
		[IfFlag(7)] public int can_export_at;
		[IfFlag(8)] public long transfer_stars;

		[Flags]
		public enum Flags : uint
		{
			name_hidden = 0x1,
			has_from_id = 0x2,
			has_message = 0x4,
			has_msg_id = 0x8,
			has_convert_stars = 0x10,
			unsaved = 0x20,
			has_upgrade_stars = 0x40,
			has_can_export_at = 0x80,
			has_transfer_stars = 0x100,
			refunded = 0x200,
			can_upgrade = 0x400,
			has_saved_id = 0x800,
		}
	}
}