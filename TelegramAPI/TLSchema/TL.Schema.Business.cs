using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>A time interval, indicating the opening hours of a business.		<para>See <a href="https://corefork.telegram.org/constructor/businessWeeklyOpen"/></para></summary>
    [TLDef(0x120B1AB9)]
    public sealed partial class BusinessWeeklyOpen : IObject
    {
        /// <summary>Start minute in minutes of the week, <c>0</c> to <c>7*24*60</c> inclusively.</summary>
        public int start_minute;
        /// <summary>End minute in minutes of the week, <c>1</c> to <c>8*24*60</c> inclusively (<c>8</c> and not <c>7</c> because this allows to specify intervals that, for example, start on <c>Sunday 21:00</c> and end on <c>Monday 04:00</c> (<c>6*24*60+21*60</c> to <c>7*24*60+4*60</c>) without passing an invalid <c>end_minute &lt; start_minute</c>). See <a href="https://corefork.telegram.org/api/business#opening-hours">here »</a> for more info.</summary>
        public int end_minute;
    }
    
    	/// <summary>Specifies a set of <a href="https://corefork.telegram.org/api/business#opening-hours">Telegram Business opening hours</a>.		<para>See <a href="https://corefork.telegram.org/constructor/businessWorkHours"/></para></summary>
	[TLDef(0x8C92B098)]
	public sealed partial class BusinessWorkHours : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>An ID of one of the timezones returned by <see cref="SchemaExtensions.Help_GetTimezonesList">Help_GetTimezonesList</see>.  <br/>  The timezone ID is contained <see cref="Timezone"/>.<c>id</c>, a human-readable, localized name of the timezone is available in <see cref="Timezone"/>.<c>name</c> and the <see cref="Timezone"/>.<c>utc_offset</c> field contains the UTC offset in seconds, which may be displayed in hh:mm format by the client together with the human-readable name (i.e. <c>$name UTC -01:00</c>).</summary>
		public string timezone_id;
		/// <summary>A list of time intervals (max 28) represented by <see cref="BusinessWeeklyOpen">businessWeeklyOpen »</see>, indicating the opening hours of their business.</summary>
		public BusinessWeeklyOpen[] weekly_open;

		[Flags] public enum Flags : uint
		{
			/// <summary>Ignored if set while invoking <see cref="SchemaExtensions.Account_UpdateBusinessWorkHours">Account_UpdateBusinessWorkHours</see>, only returned by the server in <see cref="UserFull"/>.<c>business_work_hours</c>, indicating whether the business is currently open according to the current time and the values in <c>weekly_open</c> and <c>timezone</c>.</summary>
			open_now = 0x1,
		}
	}

	/// <summary>Represents the location of a <a href="https://corefork.telegram.org/api/business#location">Telegram Business »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/businessLocation"/></para></summary>
	[TLDef(0xAC5C1AF7)]
	public sealed partial class BusinessLocation : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Geographical coordinates (optional).</summary>
		[IfFlag(0)] public GeoPoint geo_point;
		/// <summary>Textual description of the address (mandatory).</summary>
		public string address;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="geo_point"/> has a value</summary>
			has_geo_point = 0x1,
		}
	}


	/// <summary>Specifies the chats that <strong>can</strong> receive Telegram Business <a href="https://corefork.telegram.org/api/business#away-messages">away »</a> and <a href="https://corefork.telegram.org/api/business#greeting-messages">greeting »</a> messages.		<para>See <a href="https://corefork.telegram.org/constructor/businessRecipients"/></para></summary>
	[TLDef(0x21108FF7)]
	public sealed partial class BusinessRecipients : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Only private chats with the specified users.</summary>
		[IfFlag(4)] public long[] users;

		[Flags] public enum Flags : uint
		{
			/// <summary>All existing private chats.</summary>
			existing_chats = 0x1,
			/// <summary>All new private chats.</summary>
			new_chats = 0x2,
			/// <summary>All private chats with contacts.</summary>
			contacts = 0x4,
			/// <summary>All private chats with non-contacts.</summary>
			non_contacts = 0x8,
			/// <summary>Field <see cref="users"/> has a value</summary>
			has_users = 0x10,
			/// <summary>If set, inverts the selection.</summary>
			exclude_selected = 0x20,
		}
	}

	/// <summary>Specifies when should the <a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away messages</a> be sent.		<para>See <a href="https://corefork.telegram.org/type/BusinessAwayMessageSchedule"/></para>		<para>Derived classes: <see cref="BusinessAwayMessageScheduleAlways"/>, <see cref="BusinessAwayMessageScheduleOutsideWorkHours"/>, <see cref="BusinessAwayMessageScheduleCustom"/></para></summary>
	public abstract partial class BusinessAwayMessageSchedule : IObject { }
	/// <summary>Always send <a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away messages</a> to users writing to us in private.		<para>See <a href="https://corefork.telegram.org/constructor/businessAwayMessageScheduleAlways"/></para></summary>
	[TLDef(0xC9B9E2B9)]
	public sealed partial class BusinessAwayMessageScheduleAlways : BusinessAwayMessageSchedule { }
	/// <summary>Send <a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away messages</a> to users writing to us in private outside of the configured <a href="https://corefork.telegram.org/api/business#opening-hours">Telegram Business working hours</a>.		<para>See <a href="https://corefork.telegram.org/constructor/businessAwayMessageScheduleOutsideWorkHours"/></para></summary>
	[TLDef(0xC3F2F501)]
	public sealed partial class BusinessAwayMessageScheduleOutsideWorkHours : BusinessAwayMessageSchedule { }
	/// <summary>Send <a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away messages</a> to users writing to us in private in the specified time span.		<para>See <a href="https://corefork.telegram.org/constructor/businessAwayMessageScheduleCustom"/></para></summary>
	[TLDef(0xCC4D9ECC)]
	public sealed partial class BusinessAwayMessageScheduleCustom : BusinessAwayMessageSchedule
	{
		/// <summary>Start date (UNIX timestamp).</summary>
		public DateTime start_date;
		/// <summary>End date (UNIX timestamp).</summary>
		public DateTime end_date;
	}


	/// <summary>Describes a <a href="https://corefork.telegram.org/api/business#greeting-messages">Telegram Business greeting</a>, automatically sent to new users writing to us in private for the first time, or after a certain inactivity period.		<para>See <a href="https://corefork.telegram.org/constructor/businessGreetingMessage"/></para></summary>
	[TLDef(0xE519ABAB)]
	public sealed partial class BusinessGreetingMessage : IObject
	{
		/// <summary>ID of a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shorcut, containing the greeting messages to send, see here » for more info</a>.</summary>
		public int shortcut_id;
		/// <summary>Allowed recipients for the greeting messages.</summary>
		public BusinessRecipients recipients;
		/// <summary>The number of days after which a private chat will be considered as inactive; currently, must be one of 7, 14, 21, or 28.</summary>
		public int no_activity_days;
	}


	/// <summary>Describes a <a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away message</a>, automatically sent to users writing to us when we're offline, during closing hours, while we're on vacation, or in some other custom time period when we cannot immediately answer to the user.		<para>See <a href="https://corefork.telegram.org/constructor/businessAwayMessage"/></para></summary>
	[TLDef(0xEF156A5C)]
	public sealed partial class BusinessAwayMessage : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shorcut, containing the away messages to send, see here » for more info</a>.</summary>
		public int shortcut_id;
		/// <summary>Specifies when should the away messages be sent.</summary>
		public BusinessAwayMessageSchedule schedule;
		/// <summary>Allowed recipients for the away messages.</summary>
		public BusinessRecipients recipients;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, the messages will not be sent if the account was online in the last 10 minutes.</summary>
			offline_only = 0x1,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/business#business-introduction">Telegram Business introduction »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/businessIntro"/></para></summary>
	[TLDef(0x5A0A066D)]
	public sealed partial class BusinessIntro : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Title of the introduction message (max <a href="https://corefork.telegram.org/api/config#intro-title-length-limit">intro_title_length_limit »</a> UTF-8 characters).</summary>
		public string title;
		/// <summary>Profile introduction (max <a href="https://corefork.telegram.org/api/config#intro-description-length-limit">intro_description_length_limit »</a> UTF-8 characters).</summary>
		public string description;
		/// <summary>Optional introduction <a href="https://corefork.telegram.org/api/stickers">sticker</a>.</summary>
		[IfFlag(0)] public DocumentBase sticker;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="sticker"/> has a value</summary>
			has_sticker = 0x1,
		}
	}
	/// <summary>Specifies the private chats that a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business bot »</a> may receive messages and interact with.		<para>See <a href="https://corefork.telegram.org/constructor/businessBotRecipients"/></para></summary>
	[TLDef(0xB88CF373)]
	public sealed partial class BusinessBotRecipients : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Explicitly selected private chats.</summary>
		[IfFlag(4)] public long[] users;
		/// <summary>Identifiers of private chats that are always excluded.</summary>
		[IfFlag(6)] public long[] exclude_users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Selects all existing private chats.</summary>
			existing_chats = 0x1,
			/// <summary>Selects all new private chats.</summary>
			new_chats = 0x2,
			/// <summary>Selects all private chats with contacts.</summary>
			contacts = 0x4,
			/// <summary>Selects all private chats with non-contacts.</summary>
			non_contacts = 0x8,
			/// <summary>Field <see cref="users"/> has a value</summary>
			has_users = 0x10,
			/// <summary>If set, then all private chats <em>except</em> the ones selected by <c>existing_chats</c>, <c>new_chats</c>, <c>contacts</c>, <c>non_contacts</c> and <c>users</c> are chosen. <br/>Note that if this flag is set, any values passed in <c>exclude_users</c> will be merged and moved into <c>users</c> by the server, thus <c>exclude_users</c> will always be empty.</summary>
			exclude_selected = 0x20,
			/// <summary>Field <see cref="exclude_users"/> has a value</summary>
			has_exclude_users = 0x40,
		}
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/business#business-chat-links">business chat deep link »</a> created by the current account.		<para>See <a href="https://corefork.telegram.org/constructor/businessChatLink"/></para></summary>
	[TLDef(0xB4AE666F)]
	public sealed partial class BusinessChatLink : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/links#business-chat-links">Business chat deep link</a>.</summary>
		public string link;
		/// <summary>Message to pre-fill in the message input field.</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(0)] public MessageEntity[] entities;
		/// <summary>Human-readable name of the link, to simplify management in the UI (only visible to the creator of the link).</summary>
		[IfFlag(1)] public string title;
		/// <summary>Number of times the link was resolved (clicked/scanned/etc...).</summary>
		public int views;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x1,
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x2,
		}
	}
		
}