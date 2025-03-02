using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Payment form		<para>See <a href="https://corefork.telegram.org/type/payments.PaymentForm"/></para>		<para>Derived classes: <see cref="Payments_PaymentForm"/>, <see cref="Payments_PaymentFormStars"/>, <see cref="Payments_PaymentFormStarGift"/></para></summary>
    public abstract partial class Payments_PaymentFormBase : IObject
    {
        /// <summary>Form ID</summary>
        public virtual long FormId => default;
        /// <summary>Invoice</summary>
        public virtual Invoice Invoice => default;
    }
    
    	/// <summary>Payment form		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentForm"/></para></summary>
	[TLDef(0xA0058751)]
	public sealed partial class Payments_PaymentForm : Payments_PaymentFormBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Form ID</summary>
		public long form_id;
		/// <summary>Bot ID</summary>
		public long bot_id;
		/// <summary>Form title</summary>
		public string title;
		/// <summary>Description</summary>
		public string description;
		/// <summary>Product photo</summary>
		[IfFlag(5)] public WebDocumentBase photo;
		/// <summary>Invoice</summary>
		public Invoice invoice;
		/// <summary>Payment provider ID.</summary>
		public long provider_id;
		/// <summary>Payment form URL</summary>
		public string url;
		/// <summary>Payment provider name.<br/>One of the following:<br/>- <c>stripe</c></summary>
		[IfFlag(4)] public string native_provider;
		/// <summary>Contains information about the payment provider, if available, to support it natively without the need for opening the URL.<br/>A JSON object that can contain the following fields:<br/><br/>- <c>apple_pay_merchant_id</c>: Apple Pay merchant ID<br/>- <c>google_pay_public_key</c>: Google Pay public key<br/>- <c>need_country</c>: True, if the user country must be provided,<br/>- <c>need_zip</c>: True, if the user ZIP/postal code must be provided,<br/>- <c>need_cardholder_name</c>: True, if the cardholder name must be provided<br/></summary>
		[IfFlag(4)] public DataJSON native_params;
		/// <summary>Additional payment methods</summary>
		[IfFlag(6)] public PaymentFormMethod[] additional_methods;
		/// <summary>Saved server-side order information</summary>
		[IfFlag(0)] public PaymentRequestedInfo saved_info;
		/// <summary>Contains information about saved card credentials</summary>
		[IfFlag(1)] public PaymentSavedCredentials[] saved_credentials;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="saved_info"/> has a value</summary>
			has_saved_info = 0x1,
			/// <summary>Field <see cref="saved_credentials"/> has a value</summary>
			has_saved_credentials = 0x2,
			/// <summary>Whether the user can choose to save credentials.</summary>
			can_save_credentials = 0x4,
			/// <summary>Indicates that the user can save payment credentials, but only after setting up a <a href="https://corefork.telegram.org/api/srp">2FA password</a> (currently the account doesn't have a <a href="https://corefork.telegram.org/api/srp">2FA password</a>)</summary>
			password_missing = 0x8,
			/// <summary>Fields <see cref="native_provider"/> and <see cref="native_params"/> have a value</summary>
			has_native_provider = 0x10,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x20,
			/// <summary>Field <see cref="additional_methods"/> has a value</summary>
			has_additional_methods = 0x40,
		}

		/// <summary>Form ID</summary>
		public override long FormId => form_id;
		/// <summary>Invoice</summary>
		public override Invoice Invoice => invoice;
	}
	/// <summary>Represents a payment form, for payments to be using <a href="https://corefork.telegram.org/api/stars">Telegram Stars, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentFormStars"/></para></summary>
	[TLDef(0x7BF6B15C)]
	public sealed partial class Payments_PaymentFormStars : Payments_PaymentFormBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Form ID.</summary>
		public long form_id;
		/// <summary>Bot ID.</summary>
		public long bot_id;
		/// <summary>Form title</summary>
		public string title;
		/// <summary>Description</summary>
		public string description;
		/// <summary>Product photo</summary>
		[IfFlag(5)] public WebDocumentBase photo;
		/// <summary>Invoice</summary>
		public Invoice invoice;
		/// <summary>Info about users mentioned in the other fields.</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x20,
		}

		/// <summary>Form ID.</summary>
		public override long FormId => form_id;
		/// <summary>Invoice</summary>
		public override Invoice Invoice => invoice;
	}
	/// <summary>Represents a payment form for a <a href="https://corefork.telegram.org/api/gifts">gift, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentFormStarGift"/></para></summary>
	[TLDef(0xB425CFE1)]
	public sealed partial class Payments_PaymentFormStarGift : Payments_PaymentFormBase
	{
		/// <summary>Form ID.</summary>
		public long form_id;
		/// <summary>Invoice</summary>
		public Invoice invoice;

		/// <summary>Form ID.</summary>
		public override long FormId => form_id;
		/// <summary>Invoice</summary>
		public override Invoice Invoice => invoice;
	}

	/// <summary>Validated user-provided info		<para>See <a href="https://corefork.telegram.org/constructor/payments.validatedRequestedInfo"/></para></summary>
	[TLDef(0xD1451883)]
	public sealed partial class Payments_ValidatedRequestedInfo : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID</summary>
		[IfFlag(0)] public string id;
		/// <summary>Shipping options</summary>
		[IfFlag(1)] public ShippingOption[] shipping_options;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="id"/> has a value</summary>
			has_id = 0x1,
			/// <summary>Field <see cref="shipping_options"/> has a value</summary>
			has_shipping_options = 0x2,
		}
	}

	/// <summary>Payment result		<para>See <a href="https://corefork.telegram.org/type/payments.PaymentResult"/></para>		<para>Derived classes: <see cref="Payments_PaymentResult"/>, <see cref="Payments_PaymentVerificationNeeded"/></para></summary>
	public abstract partial class Payments_PaymentResultBase : IObject { }
	/// <summary>Payment result		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentResult"/></para></summary>
	[TLDef(0x4E5F810D)]
	public sealed partial class Payments_PaymentResult : Payments_PaymentResultBase
	{
		/// <summary>Info about the payment</summary>
		public UpdatesBase updates;
	}
	/// <summary>Payment was not successful, additional verification is needed		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentVerificationNeeded"/></para></summary>
	[TLDef(0xD8411139)]
	public sealed partial class Payments_PaymentVerificationNeeded : Payments_PaymentResultBase
	{
		/// <summary>URL for additional payment credentials verification</summary>
		public string url;
	}

	/// <summary>Payment receipt		<para>See <a href="https://corefork.telegram.org/type/payments.PaymentReceipt"/></para>		<para>Derived classes: <see cref="Payments_PaymentReceipt"/>, <see cref="Payments_PaymentReceiptStars"/></para></summary>
	public abstract partial class Payments_PaymentReceiptBase : IObject
	{
		/// <summary>Date of generation</summary>
		public virtual DateTime Date => default;
		/// <summary>Bot ID</summary>
		public virtual long BotId => default;
		/// <summary>Title</summary>
		public virtual string Title => default;
		/// <summary>Description</summary>
		public virtual string Description => default;
		/// <summary>Photo</summary>
		public virtual WebDocumentBase Photo => default;
		/// <summary>Invoice</summary>
		public virtual Invoice Invoice => default;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public virtual string Currency => default;
		/// <summary>Total amount in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public virtual long TotalAmount => default;
		/// <summary>Users</summary>
		public virtual Dictionary<long, User> Users => default;
	}
	/// <summary>Receipt		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentReceipt"/></para></summary>
	[TLDef(0x70C4FE03)]
	public sealed partial class Payments_PaymentReceipt : Payments_PaymentReceiptBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Date of generation</summary>
		public DateTime date;
		/// <summary>Bot ID</summary>
		public long bot_id;
		/// <summary>Provider ID</summary>
		public long provider_id;
		/// <summary>Title</summary>
		public string title;
		/// <summary>Description</summary>
		public string description;
		/// <summary>Photo</summary>
		[IfFlag(2)] public WebDocumentBase photo;
		/// <summary>Invoice</summary>
		public Invoice invoice;
		/// <summary>Info</summary>
		[IfFlag(0)] public PaymentRequestedInfo info;
		/// <summary>Selected shipping option</summary>
		[IfFlag(1)] public ShippingOption shipping;
		/// <summary>Tipped amount</summary>
		[IfFlag(3)] public long tip_amount;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;
		/// <summary>Total amount in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;
		/// <summary>Payment credential name</summary>
		public string credentials_title;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="info"/> has a value</summary>
			has_info = 0x1,
			/// <summary>Field <see cref="shipping"/> has a value</summary>
			has_shipping = 0x2,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x4,
			/// <summary>Field <see cref="tip_amount"/> has a value</summary>
			has_tip_amount = 0x8,
		}

		/// <summary>Date of generation</summary>
		public override DateTime Date => date;
		/// <summary>Bot ID</summary>
		public override long BotId => bot_id;
		/// <summary>Title</summary>
		public override string Title => title;
		/// <summary>Description</summary>
		public override string Description => description;
		/// <summary>Photo</summary>
		public override WebDocumentBase Photo => photo;
		/// <summary>Invoice</summary>
		public override Invoice Invoice => invoice;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public override string Currency => currency;
		/// <summary>Total amount in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public override long TotalAmount => total_amount;
		/// <summary>Users</summary>
		public override Dictionary<long, User> Users => users;
	}
	/// <summary>Receipt for <a href="https://corefork.telegram.org/api/stars">payment made using Telegram Stars</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.paymentReceiptStars"/></para></summary>
	[TLDef(0xDABBF83A)]
	public sealed partial class Payments_PaymentReceiptStars : Payments_PaymentReceiptBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Date of generation</summary>
		public DateTime date;
		/// <summary>Bot ID</summary>
		public long bot_id;
		/// <summary>Title</summary>
		public string title;
		/// <summary>Description</summary>
		public string description;
		/// <summary>Product photo</summary>
		[IfFlag(2)] public WebDocumentBase photo;
		/// <summary>Invoice</summary>
		public Invoice invoice;
		/// <summary>Currency, always <c>XTR</c>.</summary>
		public string currency;
		/// <summary>Amount of <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public long total_amount;
		/// <summary>Transaction ID</summary>
		public string transaction_id;
		/// <summary>Info about users mentioned in the other fields.</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x4,
		}

		/// <summary>Date of generation</summary>
		public override DateTime Date => date;
		/// <summary>Bot ID</summary>
		public override long BotId => bot_id;
		/// <summary>Title</summary>
		public override string Title => title;
		/// <summary>Description</summary>
		public override string Description => description;
		/// <summary>Product photo</summary>
		public override WebDocumentBase Photo => photo;
		/// <summary>Invoice</summary>
		public override Invoice Invoice => invoice;
		/// <summary>Currency, always <c>XTR</c>.</summary>
		public override string Currency => currency;
		/// <summary>Amount of <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public override long TotalAmount => total_amount;
		/// <summary>Info about users mentioned in the other fields.</summary>
		public override Dictionary<long, User> Users => users;
	}

	/// <summary>Saved server-side order information		<para>See <a href="https://corefork.telegram.org/constructor/payments.savedInfo"/></para></summary>
	[TLDef(0xFB8FE43C)]
	public sealed partial class Payments_SavedInfo : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Saved server-side order information</summary>
		[IfFlag(0)] public PaymentRequestedInfo saved_info;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="saved_info"/> has a value</summary>
			has_saved_info = 0x1,
			/// <summary>Whether the user has some saved payment credentials</summary>
			has_saved_credentials = 0x2,
		}
	}
	
	/// <summary>Payment identifier		<para>See <a href="https://corefork.telegram.org/constructor/paymentCharge"/></para></summary>
	[TLDef(0xEA02C27E)]
	public sealed partial class PaymentCharge : IObject
	{
		/// <summary>Telegram payment identifier</summary>
		public string id;
		/// <summary>Provider payment identifier</summary>
		public string provider_charge_id;
	}
	
	/// <summary>Order info provided by the user		<para>See <a href="https://corefork.telegram.org/constructor/paymentRequestedInfo"/></para></summary>
	[TLDef(0x909C3F94)]
	public sealed partial class PaymentRequestedInfo : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>User's full name</summary>
		[IfFlag(0)] public string name;
		/// <summary>User's phone number</summary>
		[IfFlag(1)] public string phone;
		/// <summary>User's email address</summary>
		[IfFlag(2)] public string email;
		/// <summary>User's shipping address</summary>
		[IfFlag(3)] public PostAddress shipping_address;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="name"/> has a value</summary>
			has_name = 0x1,
			/// <summary>Field <see cref="phone"/> has a value</summary>
			has_phone = 0x2,
			/// <summary>Field <see cref="email"/> has a value</summary>
			has_email = 0x4,
			/// <summary>Field <see cref="shipping_address"/> has a value</summary>
			has_shipping_address = 0x8,
		}
	}

	/// <summary>Saved payment credentials		<para>See <a href="https://corefork.telegram.org/type/PaymentSavedCredentials"/></para>		<para>Derived classes: <see cref="PaymentSavedCredentialsCard"/></para></summary>
	public abstract partial class PaymentSavedCredentials : IObject { }
	/// <summary>Saved credit card		<para>See <a href="https://corefork.telegram.org/constructor/paymentSavedCredentialsCard"/></para></summary>
	[TLDef(0xCDC27A1F)]
	public sealed partial class PaymentSavedCredentialsCard : PaymentSavedCredentials
	{
		/// <summary>Card ID</summary>
		public string id;
		/// <summary>Title</summary>
		public string title;
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/payments.starGiftUpgradePreview"/></para></summary>
	[TLDef(0x167BD90B)]
	public sealed partial class Payments_StarGiftUpgradePreview : IObject
	{
		public StarGiftAttribute[] sample_attributes;
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/payments.uniqueStarGift"/></para></summary>
	[TLDef(0xCAA2F60B)]
	public sealed partial class Payments_UniqueStarGift : IObject
	{
		public StarGiftBase gift;
		public Dictionary<long, User> users;
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/payments.savedStarGifts"/></para></summary>
	[TLDef(0x95F389B1)]
	public sealed partial class Payments_SavedStarGifts : IObject, IPeerResolver
	{
		public Flags flags;
		public int count;
		[IfFlag(1)] public bool chat_notifications_enabled;
		public SavedStarGift[] gifts;
		[IfFlag(0)] public string next_offset;
		public Dictionary<long, ChatBase> chats;
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			has_next_offset = 0x1,
			has_chat_notifications_enabled = 0x2,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}


	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/payments.starGiftWithdrawalUrl"/></para></summary>
	[TLDef(0x84AA3A9C)]
	public sealed partial class Payments_StarGiftWithdrawalUrl : IObject
	{
		public string url;
	}
	/// <summary>Credit card info, provided by the card's bank(s)		<para>See <a href="https://corefork.telegram.org/constructor/payments.bankCardData"/></para></summary>
	[TLDef(0x3E24E573)]
	public sealed partial class Payments_BankCardData : IObject
	{
		/// <summary>Credit card title</summary>
		public string title;
		/// <summary>Info URL(s) provided by the card's bank(s)</summary>
		public BankCardOpenUrl[] open_urls;
	}
	/// <summary>Exported <a href="https://corefork.telegram.org/api/links#invoice-links">invoice deep link</a>		<para>See <a href="https://corefork.telegram.org/constructor/payments.exportedInvoice"/></para></summary>
	[TLDef(0xAED0CBD9)]
	public sealed partial class Payments_ExportedInvoice : IObject
	{
		/// <summary>Exported <a href="https://corefork.telegram.org/api/links#invoice-links">invoice deep link</a></summary>
		public string url;
	}
	/// <summary>Represents an additional payment method		<para>See <a href="https://corefork.telegram.org/constructor/paymentFormMethod"/></para></summary>
	[TLDef(0x88F8F21B)]
	public sealed partial class PaymentFormMethod : IObject
	{
		/// <summary>URL to open in a webview to process the payment</summary>
		public string url;
		/// <summary>Payment method description</summary>
		public string title;
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/links#premium-giftcode-links">Telegram Premium giftcode link</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.checkedGiftCode"/></para></summary>
	[TLDef(0x284A1096)]
	public sealed partial class Payments_CheckedGiftCode : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The peer that created the gift code.</summary>
		[IfFlag(4)] public Peer from_id;
		/// <summary>Message ID of the giveaway in the channel specified in <c>from_id</c>.</summary>
		[IfFlag(3)] public int giveaway_msg_id;
		/// <summary>The destination user of the gift.</summary>
		[IfFlag(0)] public long to_id;
		/// <summary>Creation date of the gift code.</summary>
		public DateTime date;
		/// <summary>Duration in months of the gifted <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription.</summary>
		public int months;
		/// <summary>When was the giftcode imported, if it was imported.</summary>
		[IfFlag(1)] public DateTime used_date;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="to_id"/> has a value</summary>
			has_to_id = 0x1,
			/// <summary>Field <see cref="used_date"/> has a value</summary>
			has_used_date = 0x2,
			/// <summary>Whether this giftcode was created by a <a href="https://corefork.telegram.org/api/giveaways">giveaway</a>.</summary>
			via_giveaway = 0x4,
			/// <summary>Field <see cref="giveaway_msg_id"/> has a value</summary>
			has_giveaway_msg_id = 0x8,
			/// <summary>Field <see cref="from_id"/> has a value</summary>
			has_from_id = 0x10,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Info about a <a href="https://corefork.telegram.org/api/giveaways">Telegram Premium Giveaway</a>.		<para>See <a href="https://corefork.telegram.org/type/payments.GiveawayInfo"/></para>		<para>Derived classes: <see cref="Payments_GiveawayInfo"/>, <see cref="Payments_GiveawayInfoResults"/></para></summary>
	public abstract partial class Payments_GiveawayInfoBase : IObject
	{
		/// <summary>When was the giveaway started</summary>
		public virtual DateTime StartDate => default;
	}
	/// <summary>Contains info about an ongoing <a href="https://corefork.telegram.org/api/giveaways">giveaway</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.giveawayInfo"/></para></summary>
	[TLDef(0x4367DAA0)]
	public sealed partial class Payments_GiveawayInfo : Payments_GiveawayInfoBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>When was the giveaway started</summary>
		public DateTime start_date;
		/// <summary>The current user can't participate in the giveaway, because they were already a member of the channel when the giveaway started, and the <c>only_new_subscribers</c> was set when starting the giveaway.</summary>
		[IfFlag(1)] public DateTime joined_too_early_date;
		/// <summary>If set, the current user can't participate in the giveaway, because they are an administrator in one of the channels (ID specified in this flag) that created the giveaway.</summary>
		[IfFlag(2)] public long admin_disallowed_chat_id;
		/// <summary>If set, the current user can't participate in this giveaway, because their phone number is from the specified disallowed country (specified as a two-letter ISO 3166-1 alpha-2 country code).</summary>
		[IfFlag(4)] public string disallowed_country;

		[Flags] public enum Flags : uint
		{
			/// <summary>The current user is participating in the giveaway.</summary>
			participating = 0x1,
			/// <summary>Field <see cref="joined_too_early_date"/> has a value</summary>
			has_joined_too_early_date = 0x2,
			/// <summary>Field <see cref="admin_disallowed_chat_id"/> has a value</summary>
			has_admin_disallowed_chat_id = 0x4,
			/// <summary>If set, the giveaway has ended and the results are being prepared.</summary>
			preparing_results = 0x8,
			/// <summary>Field <see cref="disallowed_country"/> has a value</summary>
			has_disallowed_country = 0x10,
		}

		/// <summary>When was the giveaway started</summary>
		public override DateTime StartDate => start_date;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/giveaways">giveaway</a> has ended.		<para>See <a href="https://corefork.telegram.org/constructor/payments.giveawayInfoResults"/></para></summary>
	[TLDef(0xE175E66F)]
	public sealed partial class Payments_GiveawayInfoResults : Payments_GiveawayInfoBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Start date of the giveaway</summary>
		public DateTime start_date;
		/// <summary>If we're one of the winners of this giveaway, contains the <a href="https://corefork.telegram.org/api/links#premium-giftcode-links">Premium gift code</a>, see <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on the full giveaway flow.</summary>
		[IfFlag(3)] public string gift_code_slug;
		/// <summary>If we're one of the winners of this <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">Telegram Star giveaway</a>, the number <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> we won.</summary>
		[IfFlag(4)] public long stars_prize;
		/// <summary>End date of the giveaway. May be bigger than the end date specified in parameters of the giveaway.</summary>
		public DateTime finish_date;
		/// <summary>Number of winners in the giveaway</summary>
		public int winners_count;
		/// <summary>Number of winners, which activated their <a href="https://corefork.telegram.org/api/links#premium-giftcode-links">gift codes</a>.</summary>
		[IfFlag(2)] public int activated_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether we're one of the winners of this giveaway.</summary>
			winner = 0x1,
			/// <summary>Whether the giveaway was canceled and was fully refunded.</summary>
			refunded = 0x2,
			/// <summary>Field <see cref="activated_count"/> has a value</summary>
			has_activated_count = 0x4,
			/// <summary>Field <see cref="gift_code_slug"/> has a value</summary>
			has_gift_code_slug = 0x8,
			/// <summary>Field <see cref="stars_prize"/> has a value</summary>
			has_stars_prize = 0x10,
		}

		/// <summary>Start date of the giveaway</summary>
		public override DateTime StartDate => start_date;
	}
	/// <summary>Info about the current <a href="https://corefork.telegram.org/api/stars#balance-and-transaction-history">Telegram Star subscriptions, balance and transaction history »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.starsStatus"/></para></summary>
	[TLDef(0x6C9CE8ED)]
	public sealed partial class Payments_StarsStatus : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Current Telegram Star balance.</summary>
		public StarsAmount balance;
		/// <summary>Info about current Telegram Star subscriptions, only returned when invoking <see cref="SchemaExtensions.Payments_GetStarsTransactions">Payments_GetStarsTransactions</see> and <see cref="SchemaExtensions.Payments_GetStarsSubscriptions">Payments_GetStarsSubscriptions</see>.</summary>
		[IfFlag(1)] public StarsSubscription[] subscriptions;
		/// <summary>Offset for pagination of subscriptions: only usable and returned when invoking <see cref="SchemaExtensions.Payments_GetStarsSubscriptions">Payments_GetStarsSubscriptions</see>.</summary>
		[IfFlag(2)] public string subscriptions_next_offset;
		/// <summary>The number of Telegram Stars the user should buy to be able to extend expired subscriptions soon (i.e. the current balance is not enough to extend all expired subscriptions).</summary>
		[IfFlag(4)] public long subscriptions_missing_balance;
		/// <summary>List of Telegram Star transactions (partial if <c>next_offset</c> is set).</summary>
		[IfFlag(3)] public StarsTransaction[] history;
		/// <summary>Offset to use to fetch more transactions from the transaction history using <see cref="SchemaExtensions.Payments_GetStarsTransactions">Payments_GetStarsTransactions</see>.</summary>
		[IfFlag(0)] public string next_offset;
		/// <summary>Chats mentioned in <c>history</c>.</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in <c>history</c>.</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
			/// <summary>Field <see cref="subscriptions"/> has a value</summary>
			has_subscriptions = 0x2,
			/// <summary>Field <see cref="subscriptions_next_offset"/> has a value</summary>
			has_subscriptions_next_offset = 0x4,
			/// <summary>Field <see cref="history"/> has a value</summary>
			has_history = 0x8,
			/// <summary>Field <see cref="subscriptions_missing_balance"/> has a value</summary>
			has_subscriptions_missing_balance = 0x10,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary><a href="https://corefork.telegram.org/api/stars">Star revenue statistics, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/payments.starsRevenueStats"/></para></summary>
	[TLDef(0xC92BB73B)]
	public sealed partial class Payments_StarsRevenueStats : IObject
	{
		/// <summary>Star revenue graph (number of earned stars)</summary>
		public StatsGraphBase revenue_graph;
		/// <summary>Current balance, current withdrawable balance and overall earned Telegram Stars</summary>
		public StarsRevenueStatus status;
		/// <summary>Current conversion rate of Telegram Stars to USD</summary>
		public double usd_rate;
	}

	/// <summary>Contains the URL to use to <a href="https://corefork.telegram.org/api/stars#withdrawing-stars">withdraw Telegram Star revenue</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.starsRevenueWithdrawalUrl"/></para></summary>
	[TLDef(0x1DAB80B7)]
	public sealed partial class Payments_StarsRevenueWithdrawalUrl : IObject
	{
		/// <summary>Contains the URL to use to <a href="https://corefork.telegram.org/api/stars#withdrawing-stars">withdraw Telegram Star revenue</a>.</summary>
		public string url;
	}

	/// <summary>Contains a URL leading to a page where the user will be able to place ads for the channel/bot, paying using <a href="https://corefork.telegram.org/api/stars#paying-for-ads">Telegram Stars</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.starsRevenueAdsAccountUrl"/></para></summary>
	[TLDef(0x394E7F21)]
	public sealed partial class Payments_StarsRevenueAdsAccountUrl : IObject
	{
		/// <summary>URL to open.</summary>
		public string url;
	}

	/// <summary>Available <a href="https://corefork.telegram.org/api/gifts">gifts »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/payments.starGifts"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/payments.starGiftsNotModified">payments.starGiftsNotModified</a></remarks>
	[TLDef(0x901689EA)]
	public sealed partial class Payments_StarGifts : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary>List of available gifts.</summary>
		public StarGiftBase[] gifts;
	}
	/// <summary>Active <a href="https://corefork.telegram.org/api/bots/referrals#becoming-an-affiliate">affiliations</a>		<para>See <a href="https://corefork.telegram.org/constructor/payments.connectedStarRefBots"/></para></summary>
	[TLDef(0x98D5EA1D)]
	public sealed partial class Payments_ConnectedStarRefBots : IObject
	{
		/// <summary>Total number of active affiliations</summary>
		public int count;
		/// <summary>The affiliations</summary>
		public ConnectedBotStarRef[] connected_bots;
		/// <summary>Peers mentioned in <c>connected_bots</c></summary>
		public Dictionary<long, User> users;
	}

	/// <summary>A list of suggested <a href="https://corefork.telegram.org/api/bots/webapps">mini apps</a> with available <a href="https://corefork.telegram.org/api/bots/referrals">affiliate programs</a>		<para>See <a href="https://corefork.telegram.org/constructor/payments.suggestedStarRefBots"/></para></summary>
	[TLDef(0xB4D5D859)]
	public sealed partial class Payments_SuggestedStarRefBots : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of results (for pagination)</summary>
		public int count;
		/// <summary>Suggested affiliate programs (full or partial list to be fetched using pagination)</summary>
		public StarRefProgram[] suggested_bots;
		/// <summary>Peers mentioned in <c>suggested_bots</c></summary>
		public Dictionary<long, User> users;
		/// <summary>Next offset for <a href="https://corefork.telegram.org/api/offsets">pagination</a></summary>
		[IfFlag(0)] public string next_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
	}

}