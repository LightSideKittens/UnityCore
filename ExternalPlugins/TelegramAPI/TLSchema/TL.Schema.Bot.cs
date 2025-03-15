using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    	/// <summary>Inline message		<para>See <a href="https://corefork.telegram.org/type/BotInlineMessage"/></para>		<para>Derived classes: <see cref="BotInlineMessageMediaAuto"/>, <see cref="BotInlineMessageText"/>, <see cref="BotInlineMessageMediaGeo"/>, <see cref="BotInlineMessageMediaVenue"/>, <see cref="BotInlineMessageMediaContact"/>, <see cref="BotInlineMessageMediaInvoice"/>, <see cref="BotInlineMessageMediaWebPage"/></para></summary>
	public abstract partial class BotInlineMessage : IObject { }
	/// <summary>Send whatever media is attached to the <see cref="BotInlineMediaResult"/>		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageMediaAuto"/></para></summary>
	[TLDef(0x764CF810)]
	public sealed partial class BotInlineMessageMediaAuto : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Caption</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;
		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x8,
		}
	}
	/// <summary>Send a simple text message		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageText"/></para></summary>
	[TLDef(0x8C7F65E2)]
	public sealed partial class BotInlineMessageText : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The message</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;
		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Disable webpage preview</summary>
			no_webpage = 0x1,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x8,
		}
	}
	/// <summary>Send a geolocation		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageMediaGeo"/></para></summary>
	[TLDef(0x051846FD)]
	public sealed partial class BotInlineMessageMediaGeo : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Geolocation</summary>
		public GeoPoint geo;
		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a direction in which the location moves, in degrees; 1-360.</summary>
		[IfFlag(0)] public int heading;
		/// <summary>Validity period</summary>
		[IfFlag(1)] public int period;
		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a maximum distance to another chat member for proximity alerts, in meters (0-100000).</summary>
		[IfFlag(3)] public int proximity_notification_radius;
		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="heading"/> has a value</summary>
			has_heading = 0x1,
			/// <summary>Field <see cref="period"/> has a value</summary>
			has_period = 0x2,
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
			/// <summary>Field <see cref="proximity_notification_radius"/> has a value</summary>
			has_proximity_notification_radius = 0x8,
		}
	}
	/// <summary>Send a venue		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageMediaVenue"/></para></summary>
	[TLDef(0x8A86659C)]
	public sealed partial class BotInlineMessageMediaVenue : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
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
		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
		}
	}
	/// <summary>Send a contact		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageMediaContact"/></para></summary>
	[TLDef(0x18D1CDC2)]
	public sealed partial class BotInlineMessageMediaContact : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Phone number</summary>
		public string phone_number;
		/// <summary>First name</summary>
		public string first_name;
		/// <summary>Last name</summary>
		public string last_name;
		/// <summary>VCard info</summary>
		public string vcard;
		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
		}
	}
	/// <summary>Send an invoice		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageMediaInvoice"/></para></summary>
	[TLDef(0x354A9B09)]
	public sealed partial class BotInlineMessageMediaInvoice : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Product name, 1-32 characters</summary>
		public string title;
		/// <summary>Product description, 1-255 characters</summary>
		public string description;
		/// <summary>Product photo</summary>
		[IfFlag(0)] public WebDocumentBase photo;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code, or <c>XTR</c> for <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public string currency;
		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;
		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x1,
			/// <summary>Set this flag if you require the user's shipping address to complete the order</summary>
			shipping_address_requested = 0x2,
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
			/// <summary>Test invoice</summary>
			test = 0x8,
		}
	}
	/// <summary>Specifies options that must be used to generate the link preview for the message, or even a standalone link preview without an attached message.		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMessageMediaWebPage"/></para></summary>
	[TLDef(0x809AD9A6)]
	public sealed partial class BotInlineMessageMediaWebPage : BotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The message, can be empty.</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;
		/// <summary>The URL to use for the link preview.</summary>
		public string url;
		/// <summary>Reply markup for sending bot buttons</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x8,
			/// <summary>If set, specifies that a large media preview should be used.</summary>
			force_large_media = 0x10,
			/// <summary>If set, specifies that a small media preview should be used.</summary>
			force_small_media = 0x20,
			/// <summary>If set, indicates that the URL used for the webpage preview was specified manually using <see cref="InputMediaWebPage"/>, and may not be related to any of the URLs specified in the message.</summary>
			manual = 0x80,
			/// <summary>If set, the link can be opened directly without user confirmation.</summary>
			safe = 0x100,
		}
	}

	/// <summary>Results of an inline query		<para>See <a href="https://corefork.telegram.org/type/BotInlineResult"/></para>		<para>Derived classes: <see cref="BotInlineResult"/>, <see cref="BotInlineMediaResult"/></para></summary>
	public abstract partial class BotInlineResultBase : IObject
	{
		/// <summary>Result ID</summary>
		public virtual string ID => default;
		/// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
		public virtual string Type => default;
		/// <summary>Result title</summary>
		public virtual string Title => default;
		/// <summary>Result description</summary>
		public virtual string Description => default;
		/// <summary>Message to send</summary>
		public virtual BotInlineMessage SendMessage => default;
	}
	/// <summary>Generic result		<para>See <a href="https://corefork.telegram.org/constructor/botInlineResult"/></para></summary>
	[TLDef(0x11965F3A)]
	public sealed partial class BotInlineResult : BotInlineResultBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Result ID</summary>
		public string id;
		/// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
		public string type;
		/// <summary>Result title</summary>
		[IfFlag(1)] public string title;
		/// <summary>Result description</summary>
		[IfFlag(2)] public string description;
		/// <summary>URL of article or webpage</summary>
		[IfFlag(3)] public string url;
		/// <summary>Thumbnail for the result</summary>
		[IfFlag(4)] public WebDocumentBase thumb;
		/// <summary>Content of the result</summary>
		[IfFlag(5)] public WebDocumentBase content;
		/// <summary>Message to send</summary>
		public BotInlineMessage send_message;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x2,
			/// <summary>Field <see cref="description"/> has a value</summary>
			has_description = 0x4,
			/// <summary>Field <see cref="url"/> has a value</summary>
			has_url = 0x8,
			/// <summary>Field <see cref="thumb"/> has a value</summary>
			has_thumb = 0x10,
			/// <summary>Field <see cref="content"/> has a value</summary>
			has_content = 0x20,
		}

		/// <summary>Result ID</summary>
		public override string ID => id;
		/// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
		public override string Type => type;
		/// <summary>Result title</summary>
		public override string Title => title;
		/// <summary>Result description</summary>
		public override string Description => description;
		/// <summary>Message to send</summary>
		public override BotInlineMessage SendMessage => send_message;
	}

    /// <summary>Media result		<para>See <a href="https://corefork.telegram.org/constructor/botInlineMediaResult"/></para></summary>
    [TLDef(0x17DB940B)]
    public sealed partial class BotInlineMediaResult : BotInlineResultBase
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>Result ID</summary>
        public string id;
        /// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
        public string type;
        /// <summary>If type is <c>photo</c>, the photo to send</summary>
        [IfFlag(0)] public PhotoBase photo;
        /// <summary>If type is <c>document</c>, the document to send</summary>
        [IfFlag(1)] public DocumentBase document;
        /// <summary>Result title</summary>
        [IfFlag(2)] public string title;
        /// <summary>Description</summary>
        [IfFlag(3)] public string description;
        /// <summary>Depending on the <c>type</c> and on the <see cref="BotInlineMessage"/>, contains the caption of the media or the content of the message to be sent <strong>instead</strong> of the media</summary>
        public BotInlineMessage send_message;

        [Flags] public enum Flags : uint
        {
            /// <summary>Field <see cref="photo"/> has a value</summary>
            has_photo = 0x1,
            /// <summary>Field <see cref="document"/> has a value</summary>
            has_document = 0x2,
            /// <summary>Field <see cref="title"/> has a value</summary>
            has_title = 0x4,
            /// <summary>Field <see cref="description"/> has a value</summary>
            has_description = 0x8,
        }

        /// <summary>Result ID</summary>
        public override string ID => id;
        /// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
        public override string Type => type;
        /// <summary>Result title</summary>
        public override string Title => title;
        /// <summary>Description</summary>
        public override string Description => description;
        /// <summary>Depending on the <c>type</c> and on the <see cref="BotInlineMessage"/>, contains the caption of the media or the content of the message to be sent <strong>instead</strong> of the media</summary>
        public override BotInlineMessage SendMessage => send_message;
    }
    
    	/// <summary>Describes a bot command that can be used in a chat		<para>See <a href="https://corefork.telegram.org/constructor/botCommand"/></para></summary>
	[TLDef(0xC27AC8C7)]
	public sealed partial class BotCommand : IObject
	{
		/// <summary><c>/command</c> name</summary>
		public string command;
		/// <summary>Description of the command</summary>
		public string description;
	}

	/// <summary>Info about bots (available bot commands, etc)		<para>See <a href="https://corefork.telegram.org/constructor/botInfo"/></para></summary>
	[TLDef(0x4D8A0299)]
	public sealed partial class BotInfo : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the bot</summary>
		[IfFlag(0)] public long user_id;
		/// <summary>Description of the bot</summary>
		[IfFlag(1)] public string description;
		/// <summary>Description photo</summary>
		[IfFlag(4)] public PhotoBase description_photo;
		/// <summary>Description animation in MPEG4 format</summary>
		[IfFlag(5)] public DocumentBase description_document;
		/// <summary>Bot commands that can be used in the chat</summary>
		[IfFlag(2)] public BotCommand[] commands;
		/// <summary>Indicates the action to execute when pressing the in-UI menu button for bots</summary>
		[IfFlag(3)] public BotMenuButtonBase menu_button;
		/// <summary>The HTTP link to the privacy policy of the bot. If not set, then the <c>/privacy</c> command must be used, if supported by the bot (i.e. if it's present in the <c>commands</c> vector). If it isn't supported, then <a href="https://telegram.org/privacy-tpa">https://telegram.org/privacy-tpa</a> must be opened, instead.</summary>
		[IfFlag(7)] public string privacy_policy_url;
		/// <summary><a href="https://corefork.telegram.org/api/bots/webapps">Mini app »</a> settings<br/></summary>
		[IfFlag(8)] public BotAppSettings app_settings;
		[IfFlag(9)] public BotVerifierSettings verifier_settings;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="user_id"/> has a value</summary>
			has_user_id = 0x1,
			/// <summary>Field <see cref="description"/> has a value</summary>
			has_description = 0x2,
			/// <summary>Field <see cref="commands"/> has a value</summary>
			has_commands = 0x4,
			/// <summary>Field <see cref="menu_button"/> has a value</summary>
			has_menu_button = 0x8,
			/// <summary>Field <see cref="description_photo"/> has a value</summary>
			has_description_photo = 0x10,
			/// <summary>Field <see cref="description_document"/> has a value</summary>
			has_description_document = 0x20,
			/// <summary>If set, the bot has some <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-app-previews">preview medias for the configured Main Mini App, see here »</a> for more info on Main Mini App preview medias.</summary>
			has_preview_medias = 0x40,
			/// <summary>Field <see cref="privacy_policy_url"/> has a value</summary>
			has_privacy_policy_url = 0x80,
			/// <summary>Field <see cref="app_settings"/> has a value</summary>
			has_app_settings = 0x100,
			/// <summary>Field <see cref="verifier_settings"/> has a value</summary>
			has_verifier_settings = 0x200,
		}
	}
	
	/// <summary>Represents a scope where the bot commands, specified using <see cref="SchemaExtensions.Bots_SetBotCommands">Bots_SetBotCommands</see> will be valid.		<para>See <a href="https://corefork.telegram.org/type/BotCommandScope"/></para>		<para>Derived classes: <see cref="BotCommandScopeUsers"/>, <see cref="BotCommandScopeChats"/>, <see cref="BotCommandScopeChatAdmins"/>, <see cref="BotCommandScopePeer"/>, <see cref="BotCommandScopePeerAdmins"/>, <see cref="BotCommandScopePeerUser"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/botCommandScopeDefault">botCommandScopeDefault</a></remarks>
	public abstract partial class BotCommandScope : IObject { }
	/// <summary>The specified bot commands will only be valid in all private chats with users.		<para>See <a href="https://corefork.telegram.org/constructor/botCommandScopeUsers"/></para></summary>
	[TLDef(0x3C4F04D8)]
	public sealed partial class BotCommandScopeUsers : BotCommandScope { }
	/// <summary>The specified bot commands will be valid in all <a href="https://corefork.telegram.org/api/channel">groups and supergroups</a>.		<para>See <a href="https://corefork.telegram.org/constructor/botCommandScopeChats"/></para></summary>
	[TLDef(0x6FE1A881)]
	public sealed partial class BotCommandScopeChats : BotCommandScope { }
	/// <summary>The specified bot commands will be valid only for chat administrators, in all <a href="https://corefork.telegram.org/api/channel">groups and supergroups</a>.		<para>See <a href="https://corefork.telegram.org/constructor/botCommandScopeChatAdmins"/></para></summary>
	[TLDef(0xB9AA606A)]
	public sealed partial class BotCommandScopeChatAdmins : BotCommandScope { }
	/// <summary>The specified bot commands will be valid only in a specific dialog.		<para>See <a href="https://corefork.telegram.org/constructor/botCommandScopePeer"/></para></summary>
	[TLDef(0xDB9D897D)]
	public partial class BotCommandScopePeer : BotCommandScope
	{
		/// <summary>The dialog</summary>
		public InputPeer peer;
	}
	/// <summary>The specified bot commands will be valid for all admins of the specified <a href="https://corefork.telegram.org/api/channel">group or supergroup</a>.		<para>See <a href="https://corefork.telegram.org/constructor/botCommandScopePeerAdmins"/></para></summary>
	[TLDef(0x3FD863D1)]
	public sealed partial class BotCommandScopePeerAdmins : BotCommandScopePeer { }
	/// <summary>The specified bot commands will be valid only for a specific user in the specified <a href="https://corefork.telegram.org/api/channel">group or supergroup</a>.		<para>See <a href="https://corefork.telegram.org/constructor/botCommandScopePeerUser"/></para></summary>
	[TLDef(0x0A1321F3, inheritBefore = true)]
	public sealed partial class BotCommandScopePeerUser : BotCommandScopePeer
	{
		/// <summary>The user</summary>
		public InputUserBase user_id;
	}
	/// <summary>Indicates the action to execute when pressing the in-UI menu button for bots		<para>See <a href="https://corefork.telegram.org/type/BotMenuButton"/></para>		<para>Derived classes: <see cref="BotMenuButtonCommands"/>, <see cref="BotMenuButton"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/botMenuButtonDefault">botMenuButtonDefault</a></remarks>
	public abstract partial class BotMenuButtonBase : IObject { }
	/// <summary><a href="https://corefork.telegram.org/api/bots/menu">Bot menu button</a> that opens the bot command list when clicked.		<para>See <a href="https://corefork.telegram.org/constructor/botMenuButtonCommands"/></para></summary>
	[TLDef(0x4258C205)]
	public sealed partial class BotMenuButtonCommands : BotMenuButtonBase { }
	/// <summary><a href="https://corefork.telegram.org/api/bots/menu">Bot menu button</a> that opens a <a href="https://corefork.telegram.org/api/bots/webapps">web app</a> when clicked.		<para>See <a href="https://corefork.telegram.org/constructor/botMenuButton"/></para></summary>
	[TLDef(0xC7B57CE6)]
	public sealed partial class BotMenuButton : BotMenuButtonBase
	{
		/// <summary>Title to be displayed on the menu button instead of 'Menu'</summary>
		public string text;
		/// <summary>URL of a <a href="https://corefork.telegram.org/api/bots/webapps">web app</a> to open when the user clicks on the button</summary>
		public string url;
	}
	/// <summary>Contains information about a <a href="https://corefork.telegram.org/api/bots/webapps#direct-link-mini-apps">direct link Mini App</a>.		<para>See <a href="https://corefork.telegram.org/constructor/botApp"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/botAppNotModified">botAppNotModified</a></remarks>
	[TLDef(0x95FCD1D6)]
	public sealed partial class BotApp : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>bot mini app ID</summary>
		public long id;
		/// <summary>bot mini app access hash</summary>
		public long access_hash;
		/// <summary>bot mini app short name, used to generate <a href="https://corefork.telegram.org/api/links#direct-mini-app-links">Direct Mini App deep links</a>.</summary>
		public string short_name;
		/// <summary>bot mini app title.</summary>
		public string title;
		/// <summary>bot mini app description.</summary>
		public string description;
		/// <summary>bot mini app photo.</summary>
		public PhotoBase photo;
		/// <summary>bot mini app animation.</summary>
		[IfFlag(0)] public DocumentBase document;
		/// <summary>Hash to pass to <see cref="SchemaExtensions.Messages_GetBotApp">Messages_GetBotApp</see>, to avoid refetching bot app info if it hasn't changed.</summary>
		public long hash;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="document"/> has a value</summary>
			has_document = 0x1,
		}
	}
	/// <summary>Localized information about a bot.		<para>See <a href="https://corefork.telegram.org/constructor/bots.botInfo"/></para></summary>
	[TLDef(0xE8A775B0)]
	public sealed partial class Bots_BotInfo : IObject
	{
		/// <summary>Bot name</summary>
		public string name;
		/// <summary>Bot about text</summary>
		public string about;
		/// <summary>Bot description</summary>
		public string description;
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/business#connected-bots">bot business connection</a>.		<para>See <a href="https://corefork.telegram.org/constructor/botBusinessConnection"/></para></summary>
	[TLDef(0x896433B4)]
	public sealed partial class BotBusinessConnection : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Business connection ID, used to identify messages coming from the connection and to reply to them as specified <a href="https://corefork.telegram.org/api/business#connected-bots">here »</a>.</summary>
		public string connection_id;
		/// <summary>ID of the user that the bot is connected to via this connection.</summary>
		public long user_id;
		/// <summary>ID of the datacenter where to send queries wrapped in a <see cref="SchemaExtensions.InvokeWithBusinessConnection">InvokeWithBusinessConnection</see> as specified <a href="https://corefork.telegram.org/api/business#connected-bots">here »</a>.</summary>
		public int dc_id;
		/// <summary>When was the connection created.</summary>
		public DateTime date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the bot can reply on behalf of the user to messages it receives through the business connection</summary>
			can_reply = 0x1,
			/// <summary>Whether this business connection is currently disabled</summary>
			disabled = 0x2,
		}
	}
	/// <summary>Popular <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-apps">Main Mini Apps</a>, to be used in the <a href="https://corefork.telegram.org/api/search#apps-tab">apps tab of global search »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/bots.popularAppBots"/></para></summary>
	[TLDef(0x1991B13B)]
	public sealed partial class Bots_PopularAppBots : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Offset for <a href="https://corefork.telegram.org/api/offsets">pagination</a>.</summary>
		[IfFlag(0)] public string next_offset;
		/// <summary>The bots associated to each <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-apps">Main Mini App, see here »</a> for more info.</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-app-previews">Main Mini App preview media, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/botPreviewMedia"/></para></summary>
	[TLDef(0x23E91BA3)]
	public sealed partial class BotPreviewMedia : IObject
	{
		/// <summary>When was this media last updated.</summary>
		public DateTime date;
		/// <summary>The actual photo/video.</summary>
		public MessageMedia media;
	}

	/// <summary>Contains info about <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-app-previews">Main Mini App previews, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/bots.previewInfo"/></para></summary>
	[TLDef(0x0CA71D64)]
	public sealed partial class Bots_PreviewInfo : IObject
	{
		/// <summary>All preview medias for the language code passed to <see cref="SchemaExtensions.Bots_GetPreviewInfo">Bots_GetPreviewInfo</see>.</summary>
		public BotPreviewMedia[] media;
		/// <summary>All available language codes for which preview medias were uploaded (regardless of the language code passed to <see cref="SchemaExtensions.Bots_GetPreviewInfo">Bots_GetPreviewInfo</see>).</summary>
		public string[] lang_codes;
	}

	/// <summary><a href="https://corefork.telegram.org/api/bots/webapps">Mini app »</a> settings		<para>See <a href="https://corefork.telegram.org/constructor/botAppSettings"/></para></summary>
	[TLDef(0xC99B1950)]
	public sealed partial class BotAppSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>SVG placeholder logo, compressed using the same format used for <a href="https://corefork.telegram.org/api/files#vector-thumbnails">vector thumbnails »</a>.</summary>
		[IfFlag(0)] public byte[] placeholder_path;
		/// <summary>Default light mode background color</summary>
		[IfFlag(1)] public int background_color;
		/// <summary>Default dark mode background color</summary>
		[IfFlag(2)] public int background_dark_color;
		/// <summary>Default light mode header color</summary>
		[IfFlag(3)] public int header_color;
		/// <summary>Default dark mode header color</summary>
		[IfFlag(4)] public int header_dark_color;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="placeholder_path"/> has a value</summary>
			has_placeholder_path = 0x1,
			/// <summary>Field <see cref="background_color"/> has a value</summary>
			has_background_color = 0x2,
			/// <summary>Field <see cref="background_dark_color"/> has a value</summary>
			has_background_dark_color = 0x4,
			/// <summary>Field <see cref="header_color"/> has a value</summary>
			has_header_color = 0x8,
			/// <summary>Field <see cref="header_dark_color"/> has a value</summary>
			has_header_dark_color = 0x10,
		}
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/botVerifierSettings"/></para></summary>
	[TLDef(0xB0CD6617)]
	public sealed partial class BotVerifierSettings : IObject
	{
		public Flags flags;
		public long icon;
		public string company;
		[IfFlag(0)] public string custom_description;

		[Flags] public enum Flags : uint
		{
			has_custom_description = 0x1,
			can_modify_custom_description = 0x2,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/botVerification"/></para></summary>
	[TLDef(0xF93CD45C)]
	public sealed partial class BotVerification : IObject
	{
		public long bot_id;
		public long icon;
		public string description;
	}
			
}