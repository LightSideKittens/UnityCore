using System;

namespace TL
{
#pragma warning disable CS1574
	/// <summary>Object describing a message.		<para>See <a href="https://corefork.telegram.org/type/Message"/></para>		<para>Derived classes: <see cref="MessageEmpty"/>, <see cref="Message"/>, <see cref="MessageService"/></para></summary>
	public abstract partial class MessageBase : IObject
	{
		/// <summary>ID of the message</summary>
		public virtual int ID => default;

		/// <summary>ID of the sender of the message</summary>
		public virtual Peer From => default;

		/// <summary>Peer ID, the chat where this message was sent</summary>
		public virtual Peer Peer => default;

		/// <summary>Reply information</summary>
		public virtual MessageReplyHeaderBase ReplyTo => default;

		/// <summary>Date of the message</summary>
		public virtual DateTime Date => default;

		/// <summary>Reactions to this message</summary>
		public virtual MessageReactions Reactions => default;

		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		public virtual int TtlPeriod => default;
	}

	/// <summary>Empty constructor, non-existent message.		<para>See <a href="https://corefork.telegram.org/constructor/messageEmpty"/></para></summary>
	[TLDef(0x90A6CA84)]
	public sealed partial class MessageEmpty : MessageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Message identifier</summary>
		public int id;

		/// <summary>Peer ID, the chat where this message was sent</summary>
		[IfFlag(0)] public Peer peer_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="peer_id"/> has a value</summary>
			has_peer_id = 0x1,
		}

		/// <summary>Message identifier</summary>
		public override int ID => id;

		/// <summary>Peer ID, the chat where this message was sent</summary>
		public override Peer Peer => peer_id;
	}

	/// <summary>A message		<para>See <a href="https://corefork.telegram.org/constructor/message"/></para></summary>
	[TLDef(0x96FDBBE9)]
	public sealed partial class Message : MessageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Extra bits of information, use <c>flags2.HasFlag(...)</c> to test for those</summary>
		public Flags2 flags2;

		/// <summary>ID of the message</summary>
		public int id;

		/// <summary>ID of the sender of the message</summary>
		[IfFlag(8)] public Peer from_id;

		/// <summary>Supergroups only, contains the number of <a href="https://corefork.telegram.org/api/boost">boosts</a> this user has given the current supergroup, and should be shown in the UI in the header of the message. <br/>Only present for incoming messages from non-anonymous supergroup members that have boosted the supergroup. <br/>Note that this counter should be locally overridden for non-anonymous <em>outgoing</em> messages, according to the current value of <see cref="ChannelFull"/>.<c>boosts_applied</c>, to ensure the value is correct even for messages sent by the current user before a supergroup was boosted (or after a boost has expired or the number of boosts has changed); do not update this value for incoming messages from other users, even if their boosts have changed.</summary>
		[IfFlag(29)] public int from_boosts_applied;

		/// <summary>Peer ID, the chat where this message was sent</summary>
		public Peer peer_id;

		/// <summary>Messages fetched from a <a href="https://corefork.telegram.org/api/saved-messages">saved messages dialog »</a> will have <c>peer</c>=<see cref="InputPeerSelf"/> and the <c>saved_peer_id</c> flag set to the ID of the saved dialog.<br/></summary>
		[IfFlag(28)] public Peer saved_peer_id;

		/// <summary>Info about forwarded messages</summary>
		[IfFlag(2)] public MessageFwdHeader fwd_from;

		/// <summary>ID of the inline bot that generated the message</summary>
		[IfFlag(11)] public long via_bot_id;

		/// <summary>Whether the message was sent by the <a href="https://corefork.telegram.org/api/business#connected-bots">business bot</a> specified in <c>via_bot_id</c> on behalf of the user.</summary>
		[IfFlag(32)] public long via_business_bot_id;

		/// <summary>Reply information</summary>
		[IfFlag(3)] public MessageReplyHeaderBase reply_to;

		/// <summary>Date of the message</summary>
		public DateTime date;

		/// <summary>The message</summary>
		public string message;

		/// <summary>Media attachment</summary>
		[IfFlag(9)] public MessageMedia media;

		/// <summary>Reply markup (bot/inline keyboards)</summary>
		[IfFlag(6)] public ReplyMarkup reply_markup;

		/// <summary>Message <a href="https://corefork.telegram.org/api/entities">entities</a> for styled text</summary>
		[IfFlag(7)] public MessageEntity[] entities;

		/// <summary>View count for channel posts</summary>
		[IfFlag(10)] public int views;

		/// <summary>Forward counter</summary>
		[IfFlag(10)] public int forwards;

		/// <summary>Info about <a href="https://corefork.telegram.org/api/threads">post comments (for channels) or message replies (for groups)</a></summary>
		[IfFlag(23)] public MessageReplies replies;

		/// <summary>Last edit date of this message</summary>
		[IfFlag(15)] public DateTime edit_date;

		/// <summary>Name of the author of this message for channel posts (with signatures enabled)</summary>
		[IfFlag(16)] public string post_author;

		/// <summary>Multiple media messages sent using <see cref="SchemaExtensions.Messages_SendMultiMedia">Messages_SendMultiMedia</see> with the same grouped ID indicate an <a href="https://corefork.telegram.org/api/files#albums-grouped-media">album or media group</a></summary>
		[IfFlag(17)] public long grouped_id;

		/// <summary>Reactions to this message</summary>
		[IfFlag(20)] public MessageReactions reactions;

		/// <summary>Contains the reason why access to this message must be restricted.</summary>
		[IfFlag(22)] public RestrictionReason[] restriction_reason;

		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		[IfFlag(25)] public int ttl_period;

		/// <summary>If set, this message is a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut message »</a> (note that quick reply shortcut messages <em>sent</em> to a private chat will <em>not</em> have this field set).</summary>
		[IfFlag(30)] public int quick_reply_shortcut_id;

		/// <summary>A <a href="https://corefork.telegram.org/api/effects">message effect that should be played as specified here »</a>.</summary>
		[IfFlag(34)] public long effect;

		/// <summary>Represents a <a href="https://corefork.telegram.org/api/factcheck">fact-check »</a>.</summary>
		[IfFlag(35)] public FactCheck factcheck;

		[IfFlag(37)] public DateTime report_delivery_until_date;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Is this an outgoing message</summary>
			out_ = 0x2,

			/// <summary>Field <see cref="fwd_from"/> has a value</summary>
			has_fwd_from = 0x4,

			/// <summary>Field <see cref="reply_to"/> has a value</summary>
			has_reply_to = 0x8,

			/// <summary>Whether we were <a href="https://corefork.telegram.org/api/mentions">mentioned</a> in this message</summary>
			mentioned = 0x10,

			/// <summary>Whether there are unread media attachments in this message</summary>
			media_unread = 0x20,

			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x40,

			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x80,

			/// <summary>Field <see cref="from_id"/> has a value</summary>
			has_from_id = 0x100,

			/// <summary>Field <see cref="media"/> has a value</summary>
			has_media = 0x200,

			/// <summary>Fields <see cref="views"/> and <see cref="forwards"/> have a value</summary>
			has_views = 0x400,

			/// <summary>Field <see cref="via_bot_id"/> has a value</summary>
			has_via_bot_id = 0x800,

			/// <summary>Whether this is a silent message (no notification triggered)</summary>
			silent = 0x2000,

			/// <summary>Whether this is a channel post</summary>
			post = 0x4000,

			/// <summary>Field <see cref="edit_date"/> has a value</summary>
			has_edit_date = 0x8000,

			/// <summary>Field <see cref="post_author"/> has a value</summary>
			has_post_author = 0x10000,

			/// <summary>Field <see cref="grouped_id"/> has a value</summary>
			has_grouped_id = 0x20000,

			/// <summary>Whether this is a <a href="https://corefork.telegram.org/api/scheduled-messages">scheduled message</a></summary>
			from_scheduled = 0x40000,

			/// <summary>This is a legacy message: it has to be refetched with the new layer</summary>
			legacy = 0x80000,

			/// <summary>Field <see cref="reactions"/> has a value</summary>
			has_reactions = 0x100000,

			/// <summary>Whether the message should be shown as not modified to the user, even if an edit date is present</summary>
			edit_hide = 0x200000,

			/// <summary>Field <see cref="restriction_reason"/> has a value</summary>
			has_restriction_reason = 0x400000,

			/// <summary>Field <see cref="replies"/> has a value</summary>
			has_replies = 0x800000,

			/// <summary>Whether this message is <a href="https://corefork.telegram.org/api/pin">pinned</a></summary>
			pinned = 0x1000000,

			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x2000000,

			/// <summary>Whether this message is <a href="https://telegram.org/blog/protected-content-delete-by-date-and-more">protected</a> and thus cannot be forwarded; clients should also prevent users from saving attached media (i.e. videos should only be streamed, photos should be kept in RAM, et cetera).</summary>
			noforwards = 0x4000000,

			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x8000000,

			/// <summary>Field <see cref="saved_peer_id"/> has a value</summary>
			has_saved_peer_id = 0x10000000,

			/// <summary>Field <see cref="from_boosts_applied"/> has a value</summary>
			has_from_boosts_applied = 0x20000000,

			/// <summary>Field <see cref="quick_reply_shortcut_id"/> has a value</summary>
			has_quick_reply_shortcut_id = 0x40000000,
		}

		[Flags]
		public enum Flags2 : uint
		{
			/// <summary>Field <see cref="via_business_bot_id"/> has a value</summary>
			has_via_business_bot_id = 0x1,

			/// <summary>If set, the message was sent because of a scheduled action by the message sender, for example, as away, or a greeting service message.</summary>
			offline = 0x2,

			/// <summary>Field <see cref="effect"/> has a value</summary>
			has_effect = 0x4,

			/// <summary>Field <see cref="factcheck"/> has a value</summary>
			has_factcheck = 0x8,

			/// <summary>The video contained in the message is currently being processed by the server (i.e. to generate alternative qualities, that will be contained in the final <see cref="MessageMediaDocument"/>.<c>alt_document</c>), and will be sent once the video is processed, which will happen approximately at the specified <c>date</c> (i.e. messages with this flag set should be treated similarly to <a href="https://corefork.telegram.org/api/scheduled-messages">scheduled messages</a>, but instead of the scheduled date, <c>date</c> contains the estimated conversion date). <br/>See <a href="https://corefork.telegram.org/api/files#video-qualities">here »</a> for more info.</summary>
			video_processing_pending = 0x10,

			/// <summary>Field <see cref="report_delivery_until_date"/> has a value</summary>
			has_report_delivery_until_date = 0x20,
		}

		/// <summary>ID of the message</summary>
		public override int ID => id;

		/// <summary>ID of the sender of the message</summary>
		public override Peer From => from_id;

		/// <summary>Peer ID, the chat where this message was sent</summary>
		public override Peer Peer => peer_id;

		/// <summary>Reply information</summary>
		public override MessageReplyHeaderBase ReplyTo => reply_to;

		/// <summary>Date of the message</summary>
		public override DateTime Date => date;

		/// <summary>Reactions to this message</summary>
		public override MessageReactions Reactions => reactions;

		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		public override int TtlPeriod => ttl_period;
	}

	/// <summary>Indicates a service message		<para>See <a href="https://corefork.telegram.org/constructor/messageService"/></para></summary>
	[TLDef(0xD3D28540)]
	public sealed partial class MessageService : MessageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Message ID</summary>
		public int id;

		/// <summary>ID of the sender of this message</summary>
		[IfFlag(8)] public Peer from_id;

		/// <summary>Sender of service message</summary>
		public Peer peer_id;

		/// <summary>Reply (thread) information</summary>
		[IfFlag(3)] public MessageReplyHeaderBase reply_to;

		/// <summary>Message date</summary>
		public DateTime date;

		/// <summary>Event connected with the service message</summary>
		public MessageAction action;

		[IfFlag(20)] public MessageReactions reactions;

		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		[IfFlag(25)] public int ttl_period;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether the message is outgoing</summary>
			out_ = 0x2,

			/// <summary>Field <see cref="reply_to"/> has a value</summary>
			has_reply_to = 0x8,

			/// <summary>Whether we were mentioned in the message</summary>
			mentioned = 0x10,

			/// <summary>Whether the message contains unread media</summary>
			media_unread = 0x20,

			/// <summary>Field <see cref="from_id"/> has a value</summary>
			has_from_id = 0x100,
			reactions_are_possible = 0x200,

			/// <summary>Whether the message is silent</summary>
			silent = 0x2000,

			/// <summary>Whether it's a channel post</summary>
			post = 0x4000,

			/// <summary>This is a legacy message: it has to be refetched with the new layer</summary>
			legacy = 0x80000,

			/// <summary>Field <see cref="reactions"/> has a value</summary>
			has_reactions = 0x100000,

			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x2000000,
		}

		/// <summary>Message ID</summary>
		public override int ID => id;

		/// <summary>ID of the sender of this message</summary>
		public override Peer From => from_id;

		/// <summary>Sender of service message</summary>
		public override Peer Peer => peer_id;

		/// <summary>Reply (thread) information</summary>
		public override MessageReplyHeaderBase ReplyTo => reply_to;

		/// <summary>Message date</summary>
		public override DateTime Date => date;

		public override MessageReactions Reactions => reactions;

		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		public override int TtlPeriod => ttl_period;
	}

	/// <summary>Media		<para>See <a href="https://corefork.telegram.org/type/MessageMedia"/></para>		<para>Derived classes: <see cref="MessageMediaPhoto"/>, <see cref="MessageMediaGeo"/>, <see cref="MessageMediaContact"/>, <see cref="MessageMediaUnsupported"/>, <see cref="MessageMediaDocument"/>, <see cref="MessageMediaWebPage"/>, <see cref="MessageMediaVenue"/>, <see cref="MessageMediaGame"/>, <see cref="MessageMediaInvoice"/>, <see cref="MessageMediaGeoLive"/>, <see cref="MessageMediaPoll"/>, <see cref="MessageMediaDice"/>, <see cref="MessageMediaStory"/>, <see cref="MessageMediaGiveaway"/>, <see cref="MessageMediaGiveawayResults"/>, <see cref="MessageMediaPaidMedia"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messageMediaEmpty">messageMediaEmpty</a></remarks>
	public abstract partial class MessageMedia : IObject
	{
	}

	/// <summary>Attached photo.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaPhoto"/></para></summary>
	[TLDef(0x695150D7)]
	public sealed partial class MessageMediaPhoto : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Photo</summary>
		[IfFlag(0)] public PhotoBase photo;

		/// <summary>Time to live in seconds of self-destructing photo</summary>
		[IfFlag(2)] public int ttl_seconds;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x1,

			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x4,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x8,
		}
	}

	/// <summary>Attached map.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaGeo"/></para></summary>
	[TLDef(0x56E0D474)]
	public sealed partial class MessageMediaGeo : MessageMedia
	{
		/// <summary>GeoPoint</summary>
		public GeoPoint geo;
	}

	/// <summary>Attached contact.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaContact"/></para></summary>
	[TLDef(0x70322949)]
	public sealed partial class MessageMediaContact : MessageMedia
	{
		/// <summary>Phone number</summary>
		public string phone_number;

		/// <summary>Contact's first name</summary>
		public string first_name;

		/// <summary>Contact's last name</summary>
		public string last_name;

		/// <summary>VCARD of contact</summary>
		public string vcard;

		/// <summary>User identifier or <c>0</c>, if the user with the given phone number is not registered</summary>
		public long user_id;
	}

	/// <summary>Current version of the client does not support this media type.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaUnsupported"/></para></summary>
	[TLDef(0x9F84F49E)]
	public sealed partial class MessageMediaUnsupported : MessageMedia
	{
	}

	/// <summary>Document (video, audio, voice, sticker, any media type except photo)		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaDocument"/></para></summary>
	[TLDef(0x52D8CCD9)]
	public sealed partial class MessageMediaDocument : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Attached document</summary>
		[IfFlag(0)] public DocumentBase document;

		/// <summary>Videos only, contains alternative qualities of the video.</summary>
		[IfFlag(5)] public DocumentBase[] alt_documents;

		[IfFlag(9)] public PhotoBase video_cover;
		[IfFlag(10)] public int video_timestamp;

		/// <summary>Time to live of self-destructing document</summary>
		[IfFlag(2)] public int ttl_seconds;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="document"/> has a value</summary>
			has_document = 0x1,

			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x4,

			/// <summary>Whether this is a normal sticker, if not set this is a premium sticker and a premium sticker animation must be played.</summary>
			nopremium = 0x8,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x10,

			/// <summary>Field <see cref="alt_documents"/> has a value</summary>
			has_alt_documents = 0x20,

			/// <summary>Whether this is a video.</summary>
			video = 0x40,

			/// <summary>Whether this is a round video.</summary>
			round = 0x80,

			/// <summary>Whether this is a voice message.</summary>
			voice = 0x100,

			/// <summary>Field <see cref="video_cover"/> has a value</summary>
			has_video_cover = 0x200,

			/// <summary>Field <see cref="video_timestamp"/> has a value</summary>
			has_video_timestamp = 0x400,
		}
	}

	/// <summary>Preview of webpage		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaWebPage"/></para></summary>
	[TLDef(0xDDF10C3B)]
	public sealed partial class MessageMediaWebPage : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Webpage preview</summary>
		public WebPageBase webpage;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, specifies that a large media preview should be used.</summary>
			force_large_media = 0x1,

			/// <summary>If set, specifies that a small media preview should be used.</summary>
			force_small_media = 0x2,

			/// <summary>If set, indicates that the URL used for the webpage preview was specified manually using <see cref="InputMediaWebPage"/>, and may not be related to any of the URLs specified in the message.</summary>
			manual = 0x8,

			/// <summary>If set, the webpage can be opened directly without user confirmation; otherwise, user confirmation is required, showing the exact URL that will be opened.</summary>
			safe = 0x10,
		}
	}

	/// <summary>Venue		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaVenue"/></para></summary>
	[TLDef(0x2EC0533F)]
	public sealed partial class MessageMediaVenue : MessageMedia
	{
		/// <summary>Geolocation of venue</summary>
		public GeoPoint geo;

		/// <summary>Venue name</summary>
		public string title;

		/// <summary>Address</summary>
		public string address;

		/// <summary>Venue provider: currently only "foursquare" and "gplaces" (Google Places) need to be supported</summary>
		public string provider;

		/// <summary>Venue ID in the provider's database</summary>
		public string venue_id;

		/// <summary>Venue type in the provider's database</summary>
		public string venue_type;
	}

	/// <summary>Telegram game		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaGame"/></para></summary>
	[TLDef(0xFDB19008)]
	public sealed partial class MessageMediaGame : MessageMedia
	{
		/// <summary>Game</summary>
		public Game game;
	}

	/// <summary>Invoice		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaInvoice"/></para></summary>
	[TLDef(0xF6A548D3)]
	public sealed partial class MessageMediaInvoice : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Product name, 1-32 characters</summary>
		public string title;

		/// <summary>Product description, 1-255 characters</summary>
		public string description;

		/// <summary>URL of the product photo for the invoice. Can be a photo of the goods or a marketing image for a service. People like it better when they see what they are paying for.</summary>
		[IfFlag(0)] public WebDocumentBase photo;

		/// <summary>Message ID of receipt: if set, clients should change the text of the first <see cref="KeyboardButtonBuy"/> button always attached to the <see cref="Message"/> to a localized version of the word <c>Receipt</c></summary>
		[IfFlag(2)] public int receipt_msg_id;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code, or <c>XTR</c> for <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;

		/// <summary>Unique bot deep-linking parameter that can be used to generate this invoice</summary>
		public string start_param;

		/// <summary>Deprecated</summary>
		[IfFlag(4)] public MessageExtendedMediaBase extended_media;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x1,

			/// <summary>Whether the shipping address was requested</summary>
			shipping_address_requested = 0x2,

			/// <summary>Field <see cref="receipt_msg_id"/> has a value</summary>
			has_receipt_msg_id = 0x4,

			/// <summary>Whether this is an example invoice</summary>
			test = 0x8,

			/// <summary>Field <see cref="extended_media"/> has a value</summary>
			has_extended_media = 0x10,
		}
	}

	/// <summary>Indicates a <a href="https://corefork.telegram.org/api/live-location">live geolocation</a>		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaGeoLive"/></para></summary>
	[TLDef(0xB940C666)]
	public sealed partial class MessageMediaGeoLive : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Geolocation</summary>
		public GeoPoint geo;

		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a direction in which the location moves, in degrees; 1-360</summary>
		[IfFlag(0)] public int heading;

		/// <summary>Validity period of provided geolocation</summary>
		public int period;

		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a maximum distance to another chat member for proximity alerts, in meters (0-100000).</summary>
		[IfFlag(1)] public int proximity_notification_radius;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="heading"/> has a value</summary>
			has_heading = 0x1,

			/// <summary>Field <see cref="proximity_notification_radius"/> has a value</summary>
			has_proximity_notification_radius = 0x2,
		}
	}

	/// <summary>Poll		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaPoll"/></para></summary>
	[TLDef(0x4BD6E798)]
	public sealed partial class MessageMediaPoll : MessageMedia
	{
		/// <summary>The poll</summary>
		public Poll poll;

		/// <summary>The results of the poll</summary>
		public PollResults results;
	}

	/// <summary><a href="https://corefork.telegram.org/api/dice">Dice-based animated sticker</a>		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaDice"/></para></summary>
	[TLDef(0x3F7EE58B)]
	public sealed partial class MessageMediaDice : MessageMedia
	{
		/// <summary><a href="https://corefork.telegram.org/api/dice">Dice value</a></summary>
		public int value;

		/// <summary>The emoji, for now 🏀, 🎲 and 🎯 are supported</summary>
		public string emoticon;
	}

	/// <summary>Represents a forwarded <a href="https://corefork.telegram.org/api/stories">story</a> or a story mention.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaStory"/></para></summary>
	[TLDef(0x68CB6283)]
	public sealed partial class MessageMediaStory : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Peer that posted the story.</summary>
		public Peer peer;

		/// <summary>Story ID</summary>
		public int id;

		/// <summary>The story itself, if absent fetch it using <see cref="SchemaExtensions.Stories_GetStoriesByID">Stories_GetStoriesByID</see> and the <c>peer</c>/<c>id</c> parameters specified above.</summary>
		[IfFlag(0)] public StoryItemBase story;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="story"/> has a value</summary>
			has_story = 0x1,

			/// <summary>If set, indicates that this someone has mentioned us in this story (i.e. by tagging us in the description) or vice versa, we have mentioned the other peer (if the message is outgoing).</summary>
			via_mention = 0x2,
		}
	}

	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/giveaways">giveaway, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaGiveaway"/></para></summary>
	[TLDef(0xAA073BEB)]
	public sealed partial class MessageMediaGiveaway : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The channels that the user must join to participate in the giveaway.</summary>
		public long[] channels;

		/// <summary>If set, only users residing in these countries can participate in the giveaway, (specified as a list of two-letter ISO 3166-1 alpha-2 country codes); otherwise there are no country-based limitations.</summary>
		[IfFlag(1)] public string[] countries_iso2;

		/// <summary>Can contain a textual description of additional giveaway prizes.</summary>
		[IfFlag(3)] public string prize_description;

		/// <summary>Number of <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscriptions given away.</summary>
		public int quantity;

		/// <summary>Duration in months of each <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription in the giveaway.</summary>
		[IfFlag(4)] public int months;

		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-giveaways">Telegram Star giveaways</a>, the total number of Telegram Stars being given away.</summary>
		[IfFlag(5)] public long stars;

		/// <summary>The end date of the giveaway.</summary>
		public DateTime until_date;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, only new subscribers starting from the giveaway creation date will be able to participate to the giveaway.</summary>
			only_new_subscribers = 0x1,

			/// <summary>Field <see cref="countries_iso2"/> has a value</summary>
			has_countries_iso2 = 0x2,

			/// <summary>If set, giveaway winners are public and will be listed in a <see cref="MessageMediaGiveawayResults"/> message that will be automatically sent to the channel once the giveaway ends.</summary>
			winners_are_visible = 0x4,

			/// <summary>Field <see cref="prize_description"/> has a value</summary>
			has_prize_description = 0x8,

			/// <summary>Field <see cref="months"/> has a value</summary>
			has_months = 0x10,

			/// <summary>Field <see cref="stars"/> has a value</summary>
			has_stars = 0x20,
		}
	}

	/// <summary>A <a href="https://corefork.telegram.org/api/giveaways">giveaway</a> with public winners has finished, this constructor contains info about the winners.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaGiveawayResults"/></para></summary>
	[TLDef(0xCEAA3EA1)]
	public sealed partial class MessageMediaGiveawayResults : MessageMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>ID of the channel/supergroup that was automatically <a href="https://corefork.telegram.org/api/boost">boosted</a> by the winners of the giveaway for duration of the Premium subscription.</summary>
		public long channel_id;

		/// <summary>Number of other channels that participated in the giveaway.</summary>
		[IfFlag(3)] public int additional_peers_count;

		/// <summary>Identifier of the message with the giveaway in <c>channel_id</c>.</summary>
		public int launch_msg_id;

		/// <summary>Total number of winners in the giveaway.</summary>
		public int winners_count;

		/// <summary>Number of not-yet-claimed prizes.</summary>
		public int unclaimed_count;

		/// <summary>Up to 100 user identifiers of the winners of the giveaway.</summary>
		public long[] winners;

		/// <summary>Duration in months of each <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription in the giveaway.</summary>
		[IfFlag(4)] public int months;

		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-giveaways">Telegram Star giveaways</a>, the total number of Telegram Stars being given away.</summary>
		[IfFlag(5)] public long stars;

		/// <summary>Can contain a textual description of additional giveaway prizes.</summary>
		[IfFlag(1)] public string prize_description;

		/// <summary>Point in time (Unix timestamp) when the winners were selected. May be bigger than winners selection date specified in initial parameters of the giveaway.</summary>
		public DateTime until_date;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, only new subscribers starting from the giveaway creation date participated in the giveaway.</summary>
			only_new_subscribers = 0x1,

			/// <summary>Field <see cref="prize_description"/> has a value</summary>
			has_prize_description = 0x2,

			/// <summary>If set, the giveaway was canceled and was fully refunded.</summary>
			refunded = 0x4,

			/// <summary>Field <see cref="additional_peers_count"/> has a value</summary>
			has_additional_peers_count = 0x8,

			/// <summary>Field <see cref="months"/> has a value</summary>
			has_months = 0x10,

			/// <summary>Field <see cref="stars"/> has a value</summary>
			has_stars = 0x20,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/paid-media">Paid media, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/messageMediaPaidMedia"/></para></summary>
	[TLDef(0xA8852491)]
	public sealed partial class MessageMediaPaidMedia : MessageMedia
	{
		/// <summary>The price of the media in <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public long stars_amount;

		/// <summary>Either the paid-for media, or super low resolution media previews if the media wasn't purchased yet, <a href="https://corefork.telegram.org/api/paid-media#viewing-paid-media">see here »</a> for more info.</summary>
		public MessageExtendedMediaBase[] extended_media;
	}

	/// <summary>Object describing actions connected to a service message.		<para>See <a href="https://corefork.telegram.org/type/MessageAction"/></para>		<para>Derived classes: <see cref="MessageActionChatCreate"/>, <see cref="MessageActionChatEditTitle"/>, <see cref="MessageActionChatEditPhoto"/>, <see cref="MessageActionChatDeletePhoto"/>, <see cref="MessageActionChatAddUser"/>, <see cref="MessageActionChatDeleteUser"/>, <see cref="MessageActionChatJoinedByLink"/>, <see cref="MessageActionChannelCreate"/>, <see cref="MessageActionChatMigrateTo"/>, <see cref="MessageActionChannelMigrateFrom"/>, <see cref="MessageActionPinMessage"/>, <see cref="MessageActionHistoryClear"/>, <see cref="MessageActionGameScore"/>, <see cref="MessageActionPaymentSentMe"/>, <see cref="MessageActionPaymentSent"/>, <see cref="MessageActionPhoneCall"/>, <see cref="MessageActionScreenshotTaken"/>, <see cref="MessageActionCustomAction"/>, <see cref="MessageActionBotAllowed"/>, <see cref="MessageActionSecureValuesSentMe"/>, <see cref="MessageActionSecureValuesSent"/>, <see cref="MessageActionContactSignUp"/>, <see cref="MessageActionGeoProximityReached"/>, <see cref="MessageActionGroupCall"/>, <see cref="MessageActionInviteToGroupCall"/>, <see cref="MessageActionSetMessagesTTL"/>, <see cref="MessageActionGroupCallScheduled"/>, <see cref="MessageActionSetChatTheme"/>, <see cref="MessageActionChatJoinedByRequest"/>, <see cref="MessageActionWebViewDataSentMe"/>, <see cref="MessageActionWebViewDataSent"/>, <see cref="MessageActionGiftPremium"/>, <see cref="MessageActionTopicCreate"/>, <see cref="MessageActionTopicEdit"/>, <see cref="MessageActionSuggestProfilePhoto"/>, <see cref="MessageActionRequestedPeer"/>, <see cref="MessageActionSetChatWallPaper"/>, <see cref="MessageActionGiftCode"/>, <see cref="MessageActionGiveawayLaunch"/>, <see cref="MessageActionGiveawayResults"/>, <see cref="MessageActionBoostApply"/>, <see cref="MessageActionRequestedPeerSentMe"/>, <see cref="MessageActionPaymentRefunded"/>, <see cref="MessageActionGiftStars"/>, <see cref="MessageActionPrizeStars"/>, <see cref="MessageActionStarGift"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messageActionEmpty">messageActionEmpty</a></remarks>
	public abstract partial class MessageAction : IObject
	{
	}

	/// <summary>Group created		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatCreate"/></para></summary>
	[TLDef(0xBD47CBAD)]
	public sealed partial class MessageActionChatCreate : MessageAction
	{
		/// <summary>Group name</summary>
		public string title;

		/// <summary>List of group members</summary>
		public long[] users;
	}

	/// <summary>Group name changed.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatEditTitle"/></para></summary>
	[TLDef(0xB5A1CE5A)]
	public sealed partial class MessageActionChatEditTitle : MessageAction
	{
		/// <summary>New group name</summary>
		public string title;
	}

	/// <summary>Group profile changed		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatEditPhoto"/></para></summary>
	[TLDef(0x7FCB13A8)]
	public sealed partial class MessageActionChatEditPhoto : MessageAction
	{
		/// <summary>New group profile photo</summary>
		public PhotoBase photo;
	}

	/// <summary>Group profile photo removed.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatDeletePhoto"/></para></summary>
	[TLDef(0x95E3FBEF)]
	public sealed partial class MessageActionChatDeletePhoto : MessageAction
	{
	}

	/// <summary>New member in the group		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatAddUser"/></para></summary>
	[TLDef(0x15CEFD00)]
	public sealed partial class MessageActionChatAddUser : MessageAction
	{
		/// <summary>Users that were invited to the chat</summary>
		public long[] users;
	}

	/// <summary>User left the group.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatDeleteUser"/></para></summary>
	[TLDef(0xA43F30CC)]
	public sealed partial class MessageActionChatDeleteUser : MessageAction
	{
		/// <summary>Leaving user ID</summary>
		public long user_id;
	}

	/// <summary>A user joined the chat via an invite link		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatJoinedByLink"/></para></summary>
	[TLDef(0x031224C3)]
	public sealed partial class MessageActionChatJoinedByLink : MessageAction
	{
		/// <summary>ID of the user that created the invite link</summary>
		public long inviter_id;
	}

	/// <summary>The channel was created		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChannelCreate"/></para></summary>
	[TLDef(0x95D2AC92)]
	public sealed partial class MessageActionChannelCreate : MessageAction
	{
		/// <summary>Original channel/supergroup title</summary>
		public string title;
	}

	/// <summary>Indicates the chat was <a href="https://corefork.telegram.org/api/channel">migrated</a> to the specified supergroup		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatMigrateTo"/></para></summary>
	[TLDef(0xE1037F92)]
	public sealed partial class MessageActionChatMigrateTo : MessageAction
	{
		/// <summary>The supergroup it was migrated to</summary>
		public long channel_id;
	}

	/// <summary>Indicates the channel was <a href="https://corefork.telegram.org/api/channel">migrated</a> from the specified chat		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChannelMigrateFrom"/></para></summary>
	[TLDef(0xEA3948E9)]
	public sealed partial class MessageActionChannelMigrateFrom : MessageAction
	{
		/// <summary>The old chat title</summary>
		public string title;

		/// <summary>The old chat ID</summary>
		public long chat_id;
	}

	/// <summary>A message was pinned		<para>See <a href="https://corefork.telegram.org/constructor/messageActionPinMessage"/></para></summary>
	[TLDef(0x94BD38ED)]
	public sealed partial class MessageActionPinMessage : MessageAction
	{
	}

	/// <summary>Chat history was cleared		<para>See <a href="https://corefork.telegram.org/constructor/messageActionHistoryClear"/></para></summary>
	[TLDef(0x9FBAB604)]
	public sealed partial class MessageActionHistoryClear : MessageAction
	{
	}

	/// <summary>Someone scored in a game		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGameScore"/></para></summary>
	[TLDef(0x92A72876)]
	public sealed partial class MessageActionGameScore : MessageAction
	{
		/// <summary>Game ID</summary>
		public long game_id;

		/// <summary>Score</summary>
		public int score;
	}

	/// <summary>A user just sent a payment to me (a bot)		<para>See <a href="https://corefork.telegram.org/constructor/messageActionPaymentSentMe"/></para></summary>
	[TLDef(0xFFA00CCC)]
	public sealed partial class MessageActionPaymentSentMe : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code, or <c>XTR</c> for <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public string currency;

		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;

		/// <summary>Bot specified invoice payload</summary>
		public byte[] payload;

		/// <summary>Order info provided by the user</summary>
		[IfFlag(0)] public PaymentRequestedInfo info;

		/// <summary>Identifier of the shipping option chosen by the user</summary>
		[IfFlag(1)] public string shipping_option_id;

		/// <summary>Provider payment identifier</summary>
		public PaymentCharge charge;

		/// <summary>Expiration date of the <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription »</a>.</summary>
		[IfFlag(4)] public DateTime subscription_until_date;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="info"/> has a value</summary>
			has_info = 0x1,

			/// <summary>Field <see cref="shipping_option_id"/> has a value</summary>
			has_shipping_option_id = 0x2,

			/// <summary>Whether this is the first payment of a recurring payment we just subscribed to</summary>
			recurring_init = 0x4,

			/// <summary>Whether this payment is part of a recurring payment</summary>
			recurring_used = 0x8,

			/// <summary>Field <see cref="subscription_until_date"/> has a value</summary>
			has_subscription_until_date = 0x10,
		}
	}

	/// <summary>A payment was sent		<para>See <a href="https://corefork.telegram.org/constructor/messageActionPaymentSent"/></para></summary>
	[TLDef(0xC624B16E)]
	public sealed partial class MessageActionPaymentSent : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code, or <c>XTR</c> for <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public string currency;

		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;

		/// <summary>An invoice slug taken from an <a href="https://corefork.telegram.org/api/links#invoice-links">invoice deep link</a> or from the <a href="https://corefork.telegram.org/api/config#premium-invoice-slug"><c>premium_invoice_slug</c> app config parameter »</a></summary>
		[IfFlag(0)] public string invoice_slug;

		/// <summary>Expiration date of the <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription »</a>.</summary>
		[IfFlag(4)] public DateTime subscription_until_date;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="invoice_slug"/> has a value</summary>
			has_invoice_slug = 0x1,

			/// <summary>Whether this is the first payment of a recurring payment we just subscribed to</summary>
			recurring_init = 0x4,

			/// <summary>Whether this payment is part of a recurring payment</summary>
			recurring_used = 0x8,

			/// <summary>Field <see cref="subscription_until_date"/> has a value</summary>
			has_subscription_until_date = 0x10,
		}
	}

	/// <summary>A phone call		<para>See <a href="https://corefork.telegram.org/constructor/messageActionPhoneCall"/></para></summary>
	[TLDef(0x80E11A7F)]
	public sealed partial class MessageActionPhoneCall : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Call ID</summary>
		public long call_id;

		/// <summary>If the call has ended, the reason why it ended</summary>
		[IfFlag(0)] public PhoneCallDiscardReason reason;

		/// <summary>Duration of the call in seconds</summary>
		[IfFlag(1)] public int duration;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="reason"/> has a value</summary>
			has_reason = 0x1,

			/// <summary>Field <see cref="duration"/> has a value</summary>
			has_duration = 0x2,

			/// <summary>Is this a video call?</summary>
			video = 0x4,
		}
	}

	/// <summary>A screenshot of the chat was taken		<para>See <a href="https://corefork.telegram.org/constructor/messageActionScreenshotTaken"/></para></summary>
	[TLDef(0x4792929B)]
	public sealed partial class MessageActionScreenshotTaken : MessageAction
	{
	}

	/// <summary>Custom action (most likely not supported by the current layer, an upgrade might be needed)		<para>See <a href="https://corefork.telegram.org/constructor/messageActionCustomAction"/></para></summary>
	[TLDef(0xFAE69F56)]
	public sealed partial class MessageActionCustomAction : MessageAction
	{
		/// <summary>Action message</summary>
		public string message;
	}

	/// <summary>We have given the bot permission to send us direct messages.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionBotAllowed"/></para></summary>
	[TLDef(0xC516D679)]
	public sealed partial class MessageActionBotAllowed : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>We have authorized the bot to send us messages by logging into a website via <a href="https://corefork.telegram.org/widgets/login">Telegram Login »</a>; this field contains the domain name of the website on which the user has logged in.</summary>
		[IfFlag(0)] public string domain;

		/// <summary>We have authorized the bot to send us messages by opening the specified <a href="https://corefork.telegram.org/api/bots/webapps">bot mini app</a>.</summary>
		[IfFlag(2)] public BotApp app;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="domain"/> has a value</summary>
			has_domain = 0x1,

			/// <summary>We have authorized the bot to send us messages by installing the bot's <a href="https://corefork.telegram.org/api/bots/attach">attachment menu</a>.</summary>
			attach_menu = 0x2,

			/// <summary>Field <see cref="app"/> has a value</summary>
			has_app = 0x4,

			/// <summary>We have allowed the bot to send us messages using <see cref="SchemaExtensions.Bots_AllowSendMessage">Bots_AllowSendMessage</see>.</summary>
			from_request = 0x8,
		}
	}

	/// <summary>Secure <a href="https://corefork.telegram.org/passport">telegram passport</a> values were received		<para>See <a href="https://corefork.telegram.org/constructor/messageActionSecureValuesSentMe"/></para></summary>
	[TLDef(0x1B287353)]
	public sealed partial class MessageActionSecureValuesSentMe : MessageAction
	{
		/// <summary>Vector with information about documents and other Telegram Passport elements that were shared with the bot</summary>
		public SecureValue[] values;

		/// <summary>Encrypted credentials required to decrypt the data</summary>
		public SecureCredentialsEncrypted credentials;
	}

	/// <summary>Request for secure <a href="https://corefork.telegram.org/passport">telegram passport</a> values was sent		<para>See <a href="https://corefork.telegram.org/constructor/messageActionSecureValuesSent"/></para></summary>
	[TLDef(0xD95C6154)]
	public sealed partial class MessageActionSecureValuesSent : MessageAction
	{
		/// <summary>Secure value types</summary>
		public SecureValueType[] types;
	}

	/// <summary>A contact just signed up to telegram		<para>See <a href="https://corefork.telegram.org/constructor/messageActionContactSignUp"/></para></summary>
	[TLDef(0xF3F25F76)]
	public sealed partial class MessageActionContactSignUp : MessageAction
	{
	}

	/// <summary>A user of the chat is now in proximity of another user		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGeoProximityReached"/></para></summary>
	[TLDef(0x98E0D697)]
	public sealed partial class MessageActionGeoProximityReached : MessageAction
	{
		/// <summary>The user or chat that is now in proximity of <c>to_id</c></summary>
		public Peer from_id;

		/// <summary>The user or chat that subscribed to <a href="https://corefork.telegram.org/api/live-location#proximity-alert">live geolocation proximity alerts</a></summary>
		public Peer to_id;

		/// <summary>Distance, in meters (0-100000)</summary>
		public int distance;
	}

	/// <summary>The group call has ended		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGroupCall"/></para></summary>
	[TLDef(0x7A0D7F42)]
	public sealed partial class MessageActionGroupCall : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Group call</summary>
		public InputGroupCall call;

		/// <summary>Group call duration</summary>
		[IfFlag(0)] public int duration;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="duration"/> has a value</summary>
			has_duration = 0x1,
		}
	}

	/// <summary>A set of users was invited to the group call		<para>See <a href="https://corefork.telegram.org/constructor/messageActionInviteToGroupCall"/></para></summary>
	[TLDef(0x502F92F7)]
	public sealed partial class MessageActionInviteToGroupCall : MessageAction
	{
		/// <summary>The group call</summary>
		public InputGroupCall call;

		/// <summary>The invited users</summary>
		public long[] users;
	}

	/// <summary>The Time-To-Live of messages in this chat was changed.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionSetMessagesTTL"/></para></summary>
	[TLDef(0x3C134D7B)]
	public sealed partial class MessageActionSetMessagesTTL : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>New Time-To-Live of all messages sent in this chat; if 0, autodeletion was disabled.</summary>
		public int period;

		/// <summary>If set, the chat TTL setting was set not due to a manual change by one of participants, but automatically because one of the participants has the <see cref="SchemaExtensions.Messages_SetDefaultHistoryTTL">Messages_SetDefaultHistoryTTL</see>. For example, when a user writes to us for the first time and we have set a default messages TTL of 1 week, this service message (with <c>auto_setting_from=our_userid</c>) will be emitted before our first message.</summary>
		[IfFlag(0)] public long auto_setting_from;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="auto_setting_from"/> has a value</summary>
			has_auto_setting_from = 0x1,
		}
	}

	/// <summary>A group call was scheduled		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGroupCallScheduled"/></para></summary>
	[TLDef(0xB3A07661)]
	public sealed partial class MessageActionGroupCallScheduled : MessageAction
	{
		/// <summary>The group call</summary>
		public InputGroupCall call;

		/// <summary>When is this group call scheduled to start</summary>
		public DateTime schedule_date;
	}

	/// <summary>The chat theme was changed		<para>See <a href="https://corefork.telegram.org/constructor/messageActionSetChatTheme"/></para></summary>
	[TLDef(0xAA786345)]
	public sealed partial class MessageActionSetChatTheme : MessageAction
	{
		/// <summary>The emoji that identifies a chat theme</summary>
		public string emoticon;
	}

	/// <summary>A user was accepted into the group by an admin		<para>See <a href="https://corefork.telegram.org/constructor/messageActionChatJoinedByRequest"/></para></summary>
	[TLDef(0xEBBCA3CB)]
	public sealed partial class MessageActionChatJoinedByRequest : MessageAction
	{
	}

	/// <summary>Data from an opened <a href="https://corefork.telegram.org/api/bots/webapps">reply keyboard bot mini app</a> was relayed to the bot that owns it (bot side service message).		<para>See <a href="https://corefork.telegram.org/constructor/messageActionWebViewDataSentMe"/></para></summary>
	[TLDef(0x47DD8079, inheritBefore = true)]
	public sealed partial class MessageActionWebViewDataSentMe : MessageActionWebViewDataSent
	{
		/// <summary>Relayed data.</summary>
		public string data;
	}

	/// <summary>Data from an opened <a href="https://corefork.telegram.org/api/bots/webapps">reply keyboard bot mini app</a> was relayed to the bot that owns it (user side service message).		<para>See <a href="https://corefork.telegram.org/constructor/messageActionWebViewDataSent"/></para></summary>
	[TLDef(0xB4C38CB5)]
	public partial class MessageActionWebViewDataSent : MessageAction
	{
		/// <summary>Text of the <see cref="KeyboardButtonSimpleWebView"/> that was pressed to open the web app.</summary>
		public string text;
	}

	/// <summary>Info about a gifted Telegram Premium subscription		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGiftPremium"/></para></summary>
	[TLDef(0x6C6274FA)]
	public sealed partial class MessageActionGiftPremium : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Price of the gift in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		/// <summary>Duration of the gifted Telegram Premium subscription</summary>
		public int months;

		/// <summary>If the gift was bought using a cryptocurrency, the cryptocurrency name.</summary>
		[IfFlag(0)] public string crypto_currency;

		/// <summary>If the gift was bought using a cryptocurrency, price of the gift in the smallest units of a cryptocurrency.</summary>
		[IfFlag(0)] public long crypto_amount;

		/// <summary>Message attached with the gift</summary>
		[IfFlag(1)] public TextWithEntities message;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Fields <see cref="crypto_currency"/> and <see cref="crypto_amount"/> have a value</summary>
			has_crypto_currency = 0x1,

			/// <summary>Field <see cref="message"/> has a value</summary>
			has_message = 0x2,
		}
	}

	/// <summary>A <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> was created.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionTopicCreate"/></para></summary>
	[TLDef(0x0D999256)]
	public sealed partial class MessageActionTopicCreate : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Topic name.</summary>
		public string title;

		/// <summary>If no custom emoji icon is specified, specifies the color of the fallback topic icon (RGB), one of <c>0x6FB9F0</c>, <c>0xFFD67E</c>, <c>0xCB86DB</c>, <c>0x8EEE98</c>, <c>0xFF93B2</c>, or <c>0xFB6F5F</c>.</summary>
		public int icon_color;

		/// <summary>ID of the <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji</a> used as topic icon.</summary>
		[IfFlag(0)] public long icon_emoji_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="icon_emoji_id"/> has a value</summary>
			has_icon_emoji_id = 0x1,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Forum topic</a> information was edited.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionTopicEdit"/></para></summary>
	[TLDef(0xC0944820)]
	public sealed partial class MessageActionTopicEdit : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>New topic title.</summary>
		[IfFlag(0)] public string title;

		/// <summary>ID of the new <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji</a> used as topic icon, or if it was removed.</summary>
		[IfFlag(1)] public long icon_emoji_id;

		/// <summary>Whether the topic was opened or closed.</summary>
		[IfFlag(2)] public bool closed;

		/// <summary>Whether the topic was hidden or unhidden (only valid for the "General" topic, <c>id=1</c>).</summary>
		[IfFlag(3)] public bool hidden;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,

			/// <summary>Field <see cref="icon_emoji_id"/> has a value</summary>
			has_icon_emoji_id = 0x2,

			/// <summary>Field <see cref="closed"/> has a value</summary>
			has_closed = 0x4,

			/// <summary>Field <see cref="hidden"/> has a value</summary>
			has_hidden = 0x8,
		}
	}

	/// <summary>A new profile picture was suggested using <see cref="SchemaExtensions.Photos_UploadContactProfilePhoto">Photos_UploadContactProfilePhoto</see>.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionSuggestProfilePhoto"/></para></summary>
	[TLDef(0x57DE635E)]
	public sealed partial class MessageActionSuggestProfilePhoto : MessageAction
	{
		/// <summary>The photo that the user suggested we set as profile picture.</summary>
		public PhotoBase photo;
	}

	/// <summary>Contains info about one or more peers that the we (the user) shared with the bot after clicking on a <see cref="KeyboardButtonRequestPeer"/> button (service message sent by the user).		<para>See <a href="https://corefork.telegram.org/constructor/messageActionRequestedPeer"/></para></summary>
	[TLDef(0x31518E9B)]
	public sealed partial class MessageActionRequestedPeer : MessageAction
	{
		/// <summary><c>button_id</c> contained in the <see cref="KeyboardButtonRequestPeer"/></summary>
		public int button_id;

		/// <summary>The shared peers</summary>
		public Peer[] peers;
	}

	/// <summary>The <a href="https://corefork.telegram.org/api/wallpapers">wallpaper »</a> of the current chat was changed.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionSetChatWallPaper"/></para></summary>
	[TLDef(0x5060A3F4)]
	public sealed partial class MessageActionSetChatWallPaper : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>New <a href="https://corefork.telegram.org/api/wallpapers">wallpaper</a></summary>
		public WallPaperBase wallpaper;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, indicates the user applied a <a href="https://corefork.telegram.org/api/wallpapers">wallpaper »</a> previously sent by the other user in a <see cref="MessageActionSetChatWallPaper"/> message.</summary>
			same = 0x1,

			/// <summary>If set, indicates the wallpaper was forcefully applied for both sides, without explicit confirmation from the other side. <br/>If the message is incoming, and we did not like the new wallpaper the other user has chosen for us, we can re-set our previous wallpaper just on our side, by invoking <see cref="SchemaExtensions.Messages_SetChatWallPaper">Messages_SetChatWallPaper</see>, providing only the <c>revert</c> flag (and obviously the <c>peer</c> parameter).</summary>
			for_both = 0x2,
		}
	}

	/// <summary>Contains a <a href="https://corefork.telegram.org/api/links#premium-giftcode-links">Telegram Premium giftcode link</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGiftCode"/></para></summary>
	[TLDef(0x56D03994)]
	public sealed partial class MessageActionGiftCode : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Identifier of the channel/supergroup that created the gift code <a href="https://corefork.telegram.org/api/giveaways">either directly or through a giveaway</a>: if we import this giftcode link, we will also automatically <a href="https://corefork.telegram.org/api/boost">boost</a> this channel/supergroup.</summary>
		[IfFlag(1)] public Peer boost_peer;

		/// <summary>Duration in months of the gifted <a href="https://corefork.telegram.org/api/premium">Telegram Premium subscription</a>.</summary>
		public int months;

		/// <summary>Slug of the <a href="https://corefork.telegram.org/api/links#premium-giftcode-links">Telegram Premium giftcode link</a></summary>
		public string slug;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		[IfFlag(2)] public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		[IfFlag(2)] public long amount;

		/// <summary>If set, the gift was made using the specified cryptocurrency.</summary>
		[IfFlag(3)] public string crypto_currency;

		/// <summary>If <c>crypto_currency</c> is set, contains the paid amount, in the smallest units of the cryptocurrency.</summary>
		[IfFlag(3)] public long crypto_amount;

		/// <summary>Message attached with the gift</summary>
		[IfFlag(4)] public TextWithEntities message;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, this gift code was received from a <a href="https://corefork.telegram.org/api/giveaways">giveaway »</a> started by a channel/supergroup we're subscribed to.</summary>
			via_giveaway = 0x1,

			/// <summary>Field <see cref="boost_peer"/> has a value</summary>
			has_boost_peer = 0x2,

			/// <summary>If set, the link was not <a href="https://corefork.telegram.org/api/links#premium-giftcode-links">redeemed</a> yet.</summary>
			unclaimed = 0x4,

			/// <summary>Fields <see cref="crypto_currency"/> and <see cref="crypto_amount"/> have a value</summary>
			has_crypto_currency = 0x8,

			/// <summary>Field <see cref="message"/> has a value</summary>
			has_message = 0x10,
		}
	}

	/// <summary>A <a href="https://corefork.telegram.org/api/giveaways">giveaway</a> was started.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGiveawayLaunch"/></para></summary>
	[TLDef(0xA80F51E4)]
	public sealed partial class MessageActionGiveawayLaunch : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-giveaways">Telegram Star giveaways</a>, the total number of Telegram Stars being given away.</summary>
		[IfFlag(0)] public long stars;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="stars"/> has a value</summary>
			has_stars = 0x1,
		}
	}

	/// <summary>A <a href="https://corefork.telegram.org/api/giveaways">giveaway</a> has ended.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGiveawayResults"/></para></summary>
	[TLDef(0x87E2F155)]
	public sealed partial class MessageActionGiveawayResults : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Number of winners in the giveaway</summary>
		public int winners_count;

		/// <summary>Number of undistributed prizes</summary>
		public int unclaimed_count;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, this is a <a href="https://corefork.telegram.org/api/stars#star-giveaways">Telegram Star giveaway</a></summary>
			stars = 0x1,
		}
	}

	/// <summary>Some <a href="https://corefork.telegram.org/api/boost">boosts »</a> were applied to the channel or supergroup.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionBoostApply"/></para></summary>
	[TLDef(0xCC02AA6D)]
	public sealed partial class MessageActionBoostApply : MessageAction
	{
		/// <summary>Number of applied <a href="https://corefork.telegram.org/api/boost">boosts</a>.</summary>
		public int boosts;
	}

	/// <summary>Contains info about one or more peers that the a user shared with the me (the bot) after clicking on a <see cref="KeyboardButtonRequestPeer"/> button (service message received by the bot).		<para>See <a href="https://corefork.telegram.org/constructor/messageActionRequestedPeerSentMe"/></para></summary>
	[TLDef(0x93B31848)]
	public sealed partial class MessageActionRequestedPeerSentMe : MessageAction
	{
		/// <summary><c>button_id</c> contained in the <see cref="KeyboardButtonRequestPeer"/></summary>
		public int button_id;

		/// <summary>Info about the shared peers.</summary>
		public RequestedPeer[] peers;
	}

	/// <summary>Describes a payment refund (service message received by both users and bots).		<para>See <a href="https://corefork.telegram.org/constructor/messageActionPaymentRefunded"/></para></summary>
	[TLDef(0x41B3E202)]
	public sealed partial class MessageActionPaymentRefunded : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Identifier of the peer that returned the funds.</summary>
		public Peer peer;

		/// <summary>Currency, <c>XTR</c> for Telegram Stars.</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;

		/// <summary>Bot specified invoice payload (only received by bots).</summary>
		[IfFlag(0)] public byte[] payload;

		/// <summary>Provider payment identifier</summary>
		public PaymentCharge charge;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="payload"/> has a value</summary>
			has_payload = 0x1,
		}
	}

	/// <summary>You gifted or were gifted some <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionGiftStars"/></para></summary>
	[TLDef(0x45D5B021)]
	public sealed partial class MessageActionGiftStars : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Price of the gift in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		/// <summary>Amount of gifted stars</summary>
		public long stars;

		/// <summary>If the gift was bought using a cryptocurrency, the cryptocurrency name.</summary>
		[IfFlag(0)] public string crypto_currency;

		/// <summary>If the gift was bought using a cryptocurrency, price of the gift in the smallest units of a cryptocurrency.</summary>
		[IfFlag(0)] public long crypto_amount;

		/// <summary>Identifier of the transaction, only visible to the receiver of the gift.</summary>
		[IfFlag(1)] public string transaction_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Fields <see cref="crypto_currency"/> and <see cref="crypto_amount"/> have a value</summary>
			has_crypto_currency = 0x1,

			/// <summary>Field <see cref="transaction_id"/> has a value</summary>
			has_transaction_id = 0x2,
		}
	}

	/// <summary>You won some <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> in a <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">Telegram Star giveaway »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionPrizeStars"/></para></summary>
	[TLDef(0xB00C47A2)]
	public sealed partial class MessageActionPrizeStars : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The number of Telegram Stars you won</summary>
		public long stars;

		/// <summary>ID of the telegram star transaction.</summary>
		public string transaction_id;

		/// <summary>Identifier of the peer that was automatically boosted by the winners of the giveaway.</summary>
		public Peer boost_peer;

		/// <summary>ID of the message containing the <see cref="MessageMediaGiveaway"/></summary>
		public int giveaway_msg_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, this indicates the reverse transaction that refunds the remaining stars to the creator of a giveaway if, when the giveaway ends, the number of members in the channel is smaller than the number of winners in the giveaway.</summary>
			unclaimed = 0x1,
		}
	}

	/// <summary>You received a <a href="https://corefork.telegram.org/api/gifts">gift, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/messageActionStarGift"/></para></summary>
	[TLDef(0x4717E8A4)]
	public sealed partial class MessageActionStarGift : MessageAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Info about the gift</summary>
		public StarGiftBase gift;

		/// <summary>Additional message from the sender of the gift</summary>
		[IfFlag(1)] public TextWithEntities message;

		/// <summary>The receiver of this gift may convert it to this many Telegram Stars, instead of displaying it on their profile page.<br/><c>convert_stars</c> will be equal to <c>stars</c> only if the gift was bought using recently bought Telegram Stars, otherwise it will be less than <c>stars</c>.</summary>
		[IfFlag(4)] public long convert_stars;

		[IfFlag(5)] public int upgrade_msg_id;
		[IfFlag(8)] public long upgrade_stars;
		[IfFlag(11)] public Peer from_id;
		[IfFlag(12)] public Peer peer;
		[IfFlag(12)] public long saved_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, the name of the sender of the gift will be hidden if the destination user decides to display the gift on their profile</summary>
			name_hidden = 0x1,

			/// <summary>Field <see cref="message"/> has a value</summary>
			has_message = 0x2,

			/// <summary>Whether this gift was added to the destination user's profile (may be toggled using <see cref="SchemaExtensions.Payments_SaveStarGift">Payments_SaveStarGift</see> and fetched using <see cref="SchemaExtensions.Payments_GetUserStarGifts">Payments_GetUserStarGifts</see>)</summary>
			saved = 0x4,

			/// <summary>Whether this gift was converted to <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> and cannot be displayed on the profile anymore.</summary>
			converted = 0x8,

			/// <summary>Field <see cref="convert_stars"/> has a value</summary>
			has_convert_stars = 0x10,
			upgraded = 0x20,

			/// <summary>Field <see cref="upgrade_stars"/> has a value</summary>
			has_upgrade_stars = 0x100,
			refunded = 0x200,
			can_upgrade = 0x400,

			/// <summary>Field <see cref="from_id"/> has a value</summary>
			has_from_id = 0x800,

			/// <summary>Fields <see cref="peer"/> and <see cref="saved_id"/> have a value</summary>
			has_peer = 0x1000,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/messageActionStarGiftUnique"/></para></summary>
	[TLDef(0xACDFCB81)]
	public sealed partial class MessageActionStarGiftUnique : MessageAction
	{
		public Flags flags;
		public StarGiftBase gift;
		[IfFlag(3)] public int can_export_at;
		[IfFlag(4)] public long transfer_stars;
		[IfFlag(6)] public Peer from_id;
		[IfFlag(7)] public Peer peer;
		[IfFlag(7)] public long saved_id;

		[Flags]
		public enum Flags : uint
		{
			upgrade = 0x1,
			transferred = 0x2,
			saved = 0x4,
			has_can_export_at = 0x8,
			has_transfer_stars = 0x10,
			refunded = 0x20,
			has_from_id = 0x40,
			has_peer = 0x80,
		}
	}

	/// <summary>Contains Diffie-Hellman key generation protocol parameters.		<para>See <a href="https://corefork.telegram.org/type/messages.DhConfig"/></para>		<para>Derived classes: <see cref="Messages_DhConfigNotModified"/>, <see cref="Messages_DhConfig"/></para></summary>
	public abstract partial class Messages_DhConfigBase : IObject
	{
	}

	/// <summary>Configuring parameters did not change.		<para>See <a href="https://corefork.telegram.org/constructor/messages.dhConfigNotModified"/></para></summary>
	[TLDef(0xC0E24635)]
	public sealed partial class Messages_DhConfigNotModified : Messages_DhConfigBase
	{
		/// <summary>Random sequence of bytes of assigned length</summary>
		public byte[] random;
	}

	/// <summary>New set of configuring parameters.		<para>See <a href="https://corefork.telegram.org/constructor/messages.dhConfig"/></para></summary>
	[TLDef(0x2C221EDD)]
	public sealed partial class Messages_DhConfig : Messages_DhConfigBase
	{
		/// <summary>New value <strong>prime</strong>, see <a href="https://en.wikipedia.org/wiki/Diffie%E2%80%93Hellman_key_exchange">Wikipedia</a></summary>
		public int g;

		/// <summary>New value <strong>primitive root</strong>, see <a href="https://en.wikipedia.org/wiki/Diffie%E2%80%93Hellman_key_exchange">Wikipedia</a></summary>
		public byte[] p;

		/// <summary>Version of set of parameters</summary>
		public int version;

		/// <summary>Random sequence of bytes of assigned length</summary>
		public byte[] random;
	}

	/// <summary>Found stickers		<para>See <a href="https://corefork.telegram.org/constructor/messages.stickers"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.stickersNotModified">messages.stickersNotModified</a></remarks>
	[TLDef(0x30A6EC7E)]
	public sealed partial class Messages_Stickers : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;

		/// <summary>Stickers</summary>
		public DocumentBase[] stickers;
	}

	/// <summary>A stickerpack is a group of stickers associated to the same emoji.<br/>It is <strong>not</strong> a sticker pack the way it is usually intended, you may be looking for a <see cref="StickerSet"/>.		<para>See <a href="https://corefork.telegram.org/constructor/stickerPack"/></para></summary>
	[TLDef(0x12B299D4)]
	public sealed partial class StickerPack : IObject
	{
		/// <summary>Emoji</summary>
		public string emoticon;

		/// <summary>Stickers</summary>
		public long[] documents;
	}

	/// <summary>Info about all installed stickers		<para>See <a href="https://corefork.telegram.org/constructor/messages.allStickers"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.allStickersNotModified">messages.allStickersNotModified</a></remarks>
	[TLDef(0xCDBBCEBB)]
	public sealed partial class Messages_AllStickers : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;

		/// <summary>All stickersets</summary>
		public StickerSet[] sets;
	}

	/// <summary>Events affected by operation		<para>See <a href="https://corefork.telegram.org/constructor/messages.affectedMessages"/></para></summary>
	[TLDef(0x84D19185)]
	public partial class Messages_AffectedMessages : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;

		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;
	}

	/// <summary>Stickerset and stickers inside it		<para>See <a href="https://corefork.telegram.org/constructor/messages.stickerSet"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.stickerSetNotModified">messages.stickerSetNotModified</a></remarks>
	[TLDef(0x6E153F16)]
	public sealed partial class Messages_StickerSet : IObject
	{
		/// <summary>The stickerset</summary>
		public StickerSet set;

		/// <summary>Emoji info for stickers</summary>
		public StickerPack[] packs;

		/// <summary>Keywords for some or every sticker in the stickerset.</summary>
		public StickerKeyword[] keywords;

		/// <summary>Stickers in stickerset</summary>
		public DocumentBase[] documents;
	}

	/// <summary>Message entities, representing styled text in a message		<para>See <a href="https://corefork.telegram.org/type/MessageEntity"/></para>		<para>Derived classes: <see cref="MessageEntityUnknown"/>, <see cref="MessageEntityMention"/>, <see cref="MessageEntityHashtag"/>, <see cref="MessageEntityBotCommand"/>, <see cref="MessageEntityUrl"/>, <see cref="MessageEntityEmail"/>, <see cref="MessageEntityBold"/>, <see cref="MessageEntityItalic"/>, <see cref="MessageEntityCode"/>, <see cref="MessageEntityPre"/>, <see cref="MessageEntityTextUrl"/>, <see cref="MessageEntityMentionName"/>, <see cref="InputMessageEntityMentionName"/>, <see cref="MessageEntityPhone"/>, <see cref="MessageEntityCashtag"/>, <see cref="MessageEntityUnderline"/>, <see cref="MessageEntityStrike"/>, <see cref="MessageEntityBankCard"/>, <see cref="MessageEntitySpoiler"/>, <see cref="MessageEntityCustomEmoji"/>, <see cref="MessageEntityBlockquote"/></para></summary>
	public abstract partial class MessageEntity : IObject
	{
		/// <summary>Offset of message entity within message (in <a href="https://corefork.telegram.org/api/entities#entity-length">UTF-16 code units</a>)</summary>
		public int offset;

		/// <summary>Length of message entity within message (in <a href="https://corefork.telegram.org/api/entities#entity-length">UTF-16 code units</a>)</summary>
		public int length;
	}

	/// <summary>Unknown message entity		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityUnknown"/></para></summary>
	[TLDef(0xBB92BA95)]
	public sealed partial class MessageEntityUnknown : MessageEntity
	{
	}

	/// <summary>Message entity <a href="https://corefork.telegram.org/api/mentions">mentioning</a> a user by <c>@username</c>; <see cref="MessageEntityMentionName"/> can also be used to mention users by their ID.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityMention"/></para></summary>
	[TLDef(0xFA04579D)]
	public sealed partial class MessageEntityMention : MessageEntity
	{
	}

	/// <summary><strong>#hashtag</strong> message entity		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityHashtag"/></para></summary>
	[TLDef(0x6F635B0D)]
	public sealed partial class MessageEntityHashtag : MessageEntity
	{
	}

	/// <summary>Message entity representing a bot /command		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityBotCommand"/></para></summary>
	[TLDef(0x6CEF8AC7)]
	public sealed partial class MessageEntityBotCommand : MessageEntity
	{
	}

	/// <summary>Message entity representing an in-text url: <a href="https://google.com">https://google.com</a>; for <a href="https://google.com">text urls</a>, use <see cref="MessageEntityTextUrl"/>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityUrl"/></para></summary>
	[TLDef(0x6ED02538)]
	public sealed partial class MessageEntityUrl : MessageEntity
	{
	}

	/// <summary>Message entity representing an <a href="mailto:email@example.com">email@example.com</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityEmail"/></para></summary>
	[TLDef(0x64E475C2)]
	public sealed partial class MessageEntityEmail : MessageEntity
	{
	}

	/// <summary>Message entity representing <strong>bold text</strong>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityBold"/></para></summary>
	[TLDef(0xBD610BC9)]
	public sealed partial class MessageEntityBold : MessageEntity
	{
	}

	/// <summary>Message entity representing <em>italic text</em>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityItalic"/></para></summary>
	[TLDef(0x826F8B60)]
	public sealed partial class MessageEntityItalic : MessageEntity
	{
	}

	/// <summary>Message entity representing a <c>codeblock</c>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityCode"/></para></summary>
	[TLDef(0x28A20571)]
	public sealed partial class MessageEntityCode : MessageEntity
	{
	}

	/// <summary>Message entity representing a preformatted <c>codeblock</c>, allowing the user to specify a programming language for the codeblock.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityPre"/></para></summary>
	[TLDef(0x73924BE0, inheritBefore = true)]
	public sealed partial class MessageEntityPre : MessageEntity
	{
		/// <summary>Programming language of the code</summary>
		public string language;
	}

	/// <summary>Message entity representing a <a href="https://google.com">text url</a>: for in-text urls like <a href="https://google.com">https://google.com</a> use <see cref="MessageEntityUrl"/>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityTextUrl"/></para></summary>
	[TLDef(0x76A6D327, inheritBefore = true)]
	public sealed partial class MessageEntityTextUrl : MessageEntity
	{
		/// <summary>The actual URL</summary>
		public string url;
	}

	/// <summary>Message entity representing a <a href="https://corefork.telegram.org/api/mentions">user mention</a>: for <em>creating</em> a mention use <see cref="InputMessageEntityMentionName"/>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityMentionName"/></para></summary>
	[TLDef(0xDC7B1140, inheritBefore = true)]
	public sealed partial class MessageEntityMentionName : MessageEntity
	{
		/// <summary>Identifier of the user that was mentioned</summary>
		public long user_id;
	}

	/// <summary>Message entity that can be used to create a user <a href="https://corefork.telegram.org/api/mentions">user mention</a>: received mentions use the <see cref="MessageEntityMentionName"/>, instead.		<para>See <a href="https://corefork.telegram.org/constructor/inputMessageEntityMentionName"/></para></summary>
	[TLDef(0x208E68C9, inheritBefore = true)]
	public sealed partial class InputMessageEntityMentionName : MessageEntity
	{
		/// <summary>Identifier of the user that was mentioned</summary>
		public InputUserBase user_id;
	}

	/// <summary>Message entity representing a phone number.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityPhone"/></para></summary>
	[TLDef(0x9B69E34B)]
	public sealed partial class MessageEntityPhone : MessageEntity
	{
	}

	/// <summary>Message entity representing a <strong>$cashtag</strong>.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityCashtag"/></para></summary>
	[TLDef(0x4C4E743F)]
	public sealed partial class MessageEntityCashtag : MessageEntity
	{
	}

	/// <summary>Message entity representing underlined text.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityUnderline"/></para></summary>
	[TLDef(0x9C4E7E8B)]
	public sealed partial class MessageEntityUnderline : MessageEntity
	{
	}

	/// <summary>Message entity representing <del>strikethrough</del> text.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityStrike"/></para></summary>
	[TLDef(0xBF0693D4)]
	public sealed partial class MessageEntityStrike : MessageEntity
	{
	}

	/// <summary>Indicates a credit card number		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityBankCard"/></para></summary>
	[TLDef(0x761E6AF4)]
	public sealed partial class MessageEntityBankCard : MessageEntity
	{
	}

	/// <summary>Message entity representing a spoiler		<para>See <a href="https://corefork.telegram.org/constructor/messageEntitySpoiler"/></para></summary>
	[TLDef(0x32CA960F)]
	public sealed partial class MessageEntitySpoiler : MessageEntity
	{
	}

	/// <summary>Represents a custom emoji.<br/>Note that this entity must wrap exactly one regular emoji (the one contained in <see cref="DocumentAttributeCustomEmoji"/>.<c>alt</c>) in the related text, otherwise the server will ignore it.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityCustomEmoji"/></para></summary>
	[TLDef(0xC8CF05F8, inheritBefore = true)]
	public sealed partial class MessageEntityCustomEmoji : MessageEntity
	{
		/// <summary>Document ID of the <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji</a>, use <see cref="SchemaExtensions.Messages_GetCustomEmojiDocuments">Messages_GetCustomEmojiDocuments</see> to fetch the emoji animation and the actual emoji it represents.</summary>
		public long document_id;
	}

	/// <summary>Message entity representing a block quote.		<para>See <a href="https://corefork.telegram.org/constructor/messageEntityBlockquote"/></para></summary>
	[TLDef(0xF1CCAAAC)]
	public sealed partial class MessageEntityBlockquote : MessageEntity
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether the quote is collapsed by default.</summary>
			collapsed = 0x1,
		}
	}
	
	/// <summary>Saved gifs		<para>See <a href="https://corefork.telegram.org/constructor/messages.savedGifs"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.savedGifsNotModified">messages.savedGifsNotModified</a></remarks>
    [TLDef(0x84A02A0D)]
    public sealed partial class Messages_SavedGifs : IObject
    {
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public long hash;
    	/// <summary>List of saved gifs</summary>
    	public DocumentBase[] gifs;
    }
	
	/// <summary>Info about a forwarded message		<para>See <a href="https://corefork.telegram.org/constructor/messageFwdHeader"/></para></summary>
    [TLDef(0x4E4DF4BB)]
    public sealed partial class MessageFwdHeader : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary>The ID of the user that originally sent the message</summary>
    	[IfFlag(0)] public Peer from_id;
    	/// <summary>The name of the user that originally sent the message</summary>
    	[IfFlag(5)] public string from_name;
    	/// <summary>When was the message originally sent</summary>
    	public DateTime date;
    	/// <summary>ID of the channel message that was forwarded</summary>
    	[IfFlag(2)] public int channel_post;
    	/// <summary>For channels and if signatures are enabled, author of the channel message</summary>
    	[IfFlag(3)] public string post_author;
    	/// <summary>Only for messages forwarded to <a href="https://corefork.telegram.org/api/saved-messages">saved messages »</a>, contains the dialog where the message was originally sent.</summary>
    	[IfFlag(4)] public Peer saved_from_peer;
    	/// <summary>Only for messages forwarded to <a href="https://corefork.telegram.org/api/saved-messages">saved messages »</a>, contains the original ID of the message in <c>saved_from_peer</c>.</summary>
    	[IfFlag(4)] public int saved_from_msg_id;
    	/// <summary>Only for forwarded messages reforwarded to <a href="https://corefork.telegram.org/api/saved-messages">saved messages »</a>, contains the sender of the original message (i.e. if user A sends a message, then user B forwards it somewhere, then user C saves it to saved messages, this field will contain the ID of user B and <c>from_id</c> will contain the ID of user A).</summary>
    	[IfFlag(8)] public Peer saved_from_id;
    	/// <summary>Only for forwarded messages from users with forward privacy enabled, sent by users with forward privacy enabled, reforwarded to <a href="https://corefork.telegram.org/api/saved-messages">saved messages »</a>, contains the sender of the original message (i.e. if user A (fwd privacy enabled) sends a message, then user B (fwd privacy enabled) forwards it somewhere, then user C saves it to saved messages, this field will contain the name of user B and <c>from_name</c> will contain the name of user A).</summary>
    	[IfFlag(9)] public string saved_from_name;
    	/// <summary>Only for forwarded messages reforwarded to <a href="https://corefork.telegram.org/api/saved-messages">saved messages »</a>, indicates when was the original message sent (i.e. if user A sends a message @ unixtime 1, then user B forwards it somewhere @ unixtime 2, then user C saves it to saved messages @ unixtime 3, this field will contain 2, <c>date</c> will contain 1 and the <c>date</c> of the containing <see cref="Message"/> will contain 3).</summary>
    	[IfFlag(10)] public DateTime saved_date;
    	/// <summary>PSA type</summary>
    	[IfFlag(6)] public string psa_type;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Field <see cref="from_id"/> has a value</summary>
    		has_from_id = 0x1,
    		/// <summary>Field <see cref="channel_post"/> has a value</summary>
    		has_channel_post = 0x4,
    		/// <summary>Field <see cref="post_author"/> has a value</summary>
    		has_post_author = 0x8,
    		/// <summary>Fields <see cref="saved_from_peer"/> and <see cref="saved_from_msg_id"/> have a value</summary>
    		has_saved_from_peer = 0x10,
    		/// <summary>Field <see cref="from_name"/> has a value</summary>
    		has_from_name = 0x20,
    		/// <summary>Field <see cref="psa_type"/> has a value</summary>
    		has_psa_type = 0x40,
    		/// <summary>Whether this message was <a href="https://corefork.telegram.org/api/import">imported from a foreign chat service, click here for more info »</a></summary>
    		imported = 0x80,
    		/// <summary>Field <see cref="saved_from_id"/> has a value</summary>
    		has_saved_from_id = 0x100,
    		/// <summary>Field <see cref="saved_from_name"/> has a value</summary>
    		has_saved_from_name = 0x200,
    		/// <summary>Field <see cref="saved_date"/> has a value</summary>
    		has_saved_date = 0x400,
    		/// <summary>Only for messages forwarded to <a href="https://corefork.telegram.org/api/saved-messages">saved messages »</a>, set if the original message was outgoing (though the message may have been originally outgoing even if this flag is not set, if <c>from_id</c> points to the current user).</summary>
    		saved_out = 0x800,
    	}
    }
	
	/// <summary><a href="https://corefork.telegram.org/api/reactions">Message reactions »</a>		<para>See <a href="https://corefork.telegram.org/constructor/messageReactions"/></para></summary>
	[TLDef(0x0A339F0B)]
	public sealed partial class MessageReactions : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Reactions</summary>
		public ReactionCount[] results;
		/// <summary>List of recent peers and their reactions</summary>
		[IfFlag(1)] public MessagePeerReaction[] recent_reactions;
		/// <summary><a href="https://corefork.telegram.org/api/reactions#paid-reactions">Paid Telegram Star reactions leaderboard »</a> for this message.</summary>
		[IfFlag(4)] public MessageReactor[] top_reactors;

		[Flags] public enum Flags : uint
		{
			/// <summary>Similar to <a href="https://corefork.telegram.org/api/min">min</a> objects, used for <a href="https://corefork.telegram.org/api/reactions">message reaction »</a> constructors that are the same for all users so they don't have the reactions sent by the current user (you can use <see cref="SchemaExtensions.Messages_GetMessagesReactions">Messages_GetMessagesReactions</see> to get the full reaction info).</summary>
			min = 0x1,
			/// <summary>Field <see cref="recent_reactions"/> has a value</summary>
			has_recent_reactions = 0x2,
			/// <summary>Whether <see cref="SchemaExtensions.Messages_GetMessageReactionsList">Messages_GetMessageReactionsList</see> can be used to see how each specific peer reacted to the message</summary>
			can_see_list = 0x4,
			/// <summary>If set or if there are no reactions, all present and future reactions should be treated as <a href="https://corefork.telegram.org/api/saved-messages#tags">message tags, see here » for more info</a>.</summary>
			reactions_as_tags = 0x8,
			/// <summary>Field <see cref="top_reactors"/> has a value</summary>
			has_top_reactors = 0x10,
		}
	}
	
	/// <summary>How a certain peer reacted to the message		<para>See <a href="https://corefork.telegram.org/constructor/messagePeerReaction"/></para></summary>
	[TLDef(0x8C79B63C)]
	public sealed partial class MessagePeerReaction : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer that reacted to the message</summary>
		public Peer peer_id;
		/// <summary>When was this reaction added</summary>
		public DateTime date;
		/// <summary>Reaction emoji</summary>
		public Reaction reaction;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the specified <a href="https://corefork.telegram.org/api/reactions">message reaction »</a> should elicit a bigger and longer reaction</summary>
			big = 0x1,
			/// <summary>Whether the reaction wasn't yet marked as read by the current user</summary>
			unread = 0x2,
			/// <summary>Starting from layer 159, <see cref="SchemaExtensions.Messages_SendReaction">Messages_SendReaction</see> will send reactions from the peer (user or channel) specified using <see cref="SchemaExtensions.Messages_SaveDefaultSendAs">Messages_SaveDefaultSendAs</see>. <br/>If set, this flag indicates that this reaction was sent by us, even if the <c>peer</c> doesn't point to the current account.</summary>
			my = 0x4,
		}
	}
	
	/// <summary><a href="https://corefork.telegram.org/api/paid-media">Paid media, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/type/MessageExtendedMedia"/></para>		<para>Derived classes: <see cref="MessageExtendedMediaPreview"/>, <see cref="MessageExtendedMedia"/></para></summary>
	public abstract partial class MessageExtendedMediaBase : IObject { }
	/// <summary>Paid media preview for not yet purchased paid media, <a href="https://corefork.telegram.org/api/paid-media">see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/messageExtendedMediaPreview"/></para></summary>
	[TLDef(0xAD628CC8)]
	public sealed partial class MessageExtendedMediaPreview : MessageExtendedMediaBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Width</summary>
		[IfFlag(0)] public int w;
		/// <summary>Height</summary>
		[IfFlag(0)] public int h;
		/// <summary><a href="https://corefork.telegram.org/api/files#stripped-thumbnails">Extremely low resolution thumbnail</a>.</summary>
		[IfFlag(1)] public PhotoSizeBase thumb;
		/// <summary>Video duration for videos.</summary>
		[IfFlag(2)] public int video_duration;

		[Flags] public enum Flags : uint
		{
			/// <summary>Fields <see cref="w"/> and <see cref="h"/> have a value</summary>
			has_w = 0x1,
			/// <summary>Field <see cref="thumb"/> has a value</summary>
			has_thumb = 0x2,
			/// <summary>Field <see cref="video_duration"/> has a value</summary>
			has_video_duration = 0x4,
		}
	}
	/// <summary>Already purchased paid media, <a href="https://corefork.telegram.org/api/paid-media">see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/messageExtendedMedia"/></para></summary>
	[TLDef(0xEE479C64)]
	public sealed partial class MessageExtendedMedia : MessageExtendedMediaBase
	{
		/// <summary>The media we purchased.</summary>
		public MessageMedia media;
	}
	
	/// <summary>How a user voted in a poll		<para>See <a href="https://corefork.telegram.org/type/MessagePeerVote"/></para>		<para>Derived classes: <see cref="MessagePeerVote"/>, <see cref="MessagePeerVoteInputOption"/>, <see cref="MessagePeerVoteMultiple"/></para></summary>
	public abstract partial class MessagePeerVoteBase : IObject
	{
		/// <summary>Peer ID</summary>
		public virtual Peer Peer => default;
		/// <summary>When did the peer cast the vote</summary>
		public virtual DateTime Date => default;
	}
	/// <summary>How a peer voted in a poll		<para>See <a href="https://corefork.telegram.org/constructor/messagePeerVote"/></para></summary>
	[TLDef(0xB6CC2D5C)]
	public sealed partial class MessagePeerVote : MessagePeerVoteBase
	{
		/// <summary>Peer ID</summary>
		public Peer peer;
		/// <summary>The option chosen by the peer</summary>
		public byte[] option;
		/// <summary>When did the peer cast the vote</summary>
		public DateTime date;

		/// <summary>Peer ID</summary>
		public override Peer Peer => peer;
		/// <summary>When did the peer cast the vote</summary>
		public override DateTime Date => date;
	}
	/// <summary>How a peer voted in a poll (reduced constructor, returned if an <c>option</c> was provided to <see cref="SchemaExtensions.Messages_GetPollVotes">Messages_GetPollVotes</see>)		<para>See <a href="https://corefork.telegram.org/constructor/messagePeerVoteInputOption"/></para></summary>
	[TLDef(0x74CDA504)]
	public sealed partial class MessagePeerVoteInputOption : MessagePeerVoteBase
	{
		/// <summary>The peer that voted for the queried <c>option</c></summary>
		public Peer peer;
		/// <summary>When did the peer cast the vote</summary>
		public DateTime date;

		/// <summary>The peer that voted for the queried <c>option</c></summary>
		public override Peer Peer => peer;
		/// <summary>When did the peer cast the vote</summary>
		public override DateTime Date => date;
	}
	/// <summary>How a peer voted in a multiple-choice poll		<para>See <a href="https://corefork.telegram.org/constructor/messagePeerVoteMultiple"/></para></summary>
	[TLDef(0x4628F6E6)]
	public sealed partial class MessagePeerVoteMultiple : MessagePeerVoteBase
	{
		/// <summary>Peer ID</summary>
		public Peer peer;
		/// <summary>Options chosen by the peer</summary>
		public byte[][] options;
		/// <summary>When did the peer cast their votes</summary>
		public DateTime date;

		/// <summary>Peer ID</summary>
		public override Peer Peer => peer;
		/// <summary>When did the peer cast their votes</summary>
		public override DateTime Date => date;
	}
	
	/// <summary>Info about a user in the <a href="https://corefork.telegram.org/api/reactions#paid-reactions">paid Star reactions leaderboard</a> for a message.		<para>See <a href="https://corefork.telegram.org/constructor/messageReactor"/></para></summary>
	[TLDef(0x4BA3A95A)]
	public sealed partial class MessageReactor : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Identifier of the peer that reacted: may be unset for anonymous reactors different from the current user (i.e. if the current user sent an anonymous reaction <c>anonymous</c> will be set but this field will also be set).</summary>
		[IfFlag(3)] public Peer peer_id;
		/// <summary>The number of sent Telegram Stars.</summary>
		public int count;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, the reactor is one of the most active reactors; may be unset if the reactor is the current user.</summary>
			top = 0x1,
			/// <summary>If set, this reactor is the current user.</summary>
			my = 0x2,
			/// <summary>If set, the reactor is anonymous.</summary>
			anonymous = 0x4,
			/// <summary>Field <see cref="peer_id"/> has a value</summary>
			has_peer_id = 0x8,
		}
	}
	
	/// <summary>Report menu option		<para>See <a href="https://corefork.telegram.org/constructor/messageReportOption"/></para></summary>
	[TLDef(0x7903E3D9)]
	public sealed partial class MessageReportOption : IObject
	{
		/// <summary>Option title</summary>
		public string text;
		/// <summary>Option identifier: if the user selects this option, re-invoke <see cref="SchemaExtensions.Messages_Report">Messages_Report</see>, passing this option to <c>option</c></summary>
		public byte[] option;
	}
	
	
}