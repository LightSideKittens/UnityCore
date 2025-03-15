using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>An update is available for the application.		<para>See <a href="https://corefork.telegram.org/constructor/help.appUpdate"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.noAppUpdate">help.noAppUpdate</a></remarks>
    [TLDef(0xCCBBCE30)]
    public sealed partial class Help_AppUpdate : IObject
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>Update ID</summary>
        public int id;
        /// <summary>New version name</summary>
        public string version;
        /// <summary>Text description of the update</summary>
        public string text;
        /// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
        public MessageEntity[] entities;
        /// <summary>Application binary</summary>
        [IfFlag(1)] public DocumentBase document;
        /// <summary>Application download URL</summary>
        [IfFlag(2)] public string url;
        /// <summary>Associated sticker</summary>
        [IfFlag(3)] public DocumentBase sticker;

        [Flags] public enum Flags : uint
        {
            /// <summary>Unskippable, the new info must be shown to the user (with a popup or something else)</summary>
            can_not_skip = 0x1,
            /// <summary>Field <see cref="document"/> has a value</summary>
            has_document = 0x2,
            /// <summary>Field <see cref="url"/> has a value</summary>
            has_url = 0x4,
            /// <summary>Field <see cref="sticker"/> has a value</summary>
            has_sticker = 0x8,
        }
    }
    
    /// <summary>Text of a text message with an invitation to install Telegram.		<para>See <a href="https://corefork.telegram.org/constructor/help.inviteText"/></para></summary>
    [TLDef(0x18CB9F78)]
    public sealed partial class Help_InviteText : IObject
    {
        /// <summary>Text of the message</summary>
        public string message;
    }
	
    /// <summary>Info on support user.		<para>See <a href="https://corefork.telegram.org/constructor/help.support"/></para></summary>
    [TLDef(0x17C6B5F6)]
    public sealed partial class Help_Support : IObject
    {
        /// <summary>Phone number</summary>
        public string phone_number;
        /// <summary>User</summary>
        public UserBase user;
    }
	/// <summary>Info about the latest telegram Terms Of Service		<para>See <a href="https://corefork.telegram.org/constructor/help.termsOfService"/></para></summary>
	[TLDef(0x780A0310)]
	public sealed partial class Help_TermsOfService : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the new terms</summary>
		public DataJSON id;
		/// <summary>Text of the new terms</summary>
		public string text;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		public MessageEntity[] entities;
		/// <summary>Minimum age required to sign up to telegram, the user must confirm that they is older than the minimum age.</summary>
		[IfFlag(1)] public int min_age_confirm;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether a prompt must be showed to the user, in order to accept the new terms.</summary>
			popup = 0x1,
			/// <summary>Field <see cref="min_age_confirm"/> has a value</summary>
			has_min_age_confirm = 0x2,
		}
	}
	/// <summary>Recent t.me URLs		<para>See <a href="https://corefork.telegram.org/constructor/help.recentMeUrls"/></para></summary>
	[TLDef(0x0E0310D7)]
	public sealed partial class Help_RecentMeUrls : IObject, IPeerResolver
	{
		/// <summary>URLs</summary>
		public RecentMeUrl[] urls;
		/// <summary>Chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Update of Telegram's terms of service		<para>See <a href="https://corefork.telegram.org/type/help.TermsOfServiceUpdate"/></para>		<para>Derived classes: <see cref="Help_TermsOfServiceUpdateEmpty"/>, <see cref="Help_TermsOfServiceUpdate"/></para></summary>
	public abstract partial class Help_TermsOfServiceUpdateBase : IObject { }
	/// <summary>No changes were made to telegram's terms of service		<para>See <a href="https://corefork.telegram.org/constructor/help.termsOfServiceUpdateEmpty"/></para></summary>
	[TLDef(0xE3309F7F)]
	public sealed partial class Help_TermsOfServiceUpdateEmpty : Help_TermsOfServiceUpdateBase
	{
		/// <summary>New TOS updates will have to be queried using <see cref="SchemaExtensions.Help_GetTermsOfServiceUpdate">Help_GetTermsOfServiceUpdate</see> in <c>expires</c> seconds</summary>
		public DateTime expires;
	}
	/// <summary>Info about an update of telegram's terms of service. If the terms of service are declined, then the <see cref="SchemaExtensions.Account_DeleteAccount">Account_DeleteAccount</see> method should be called with the reason "Decline ToS update"		<para>See <a href="https://corefork.telegram.org/constructor/help.termsOfServiceUpdate"/></para></summary>
	[TLDef(0x28ECF961)]
	public sealed partial class Help_TermsOfServiceUpdate : Help_TermsOfServiceUpdateBase
	{
		/// <summary>New TOS updates will have to be queried using <see cref="SchemaExtensions.Help_GetTermsOfServiceUpdate">Help_GetTermsOfServiceUpdate</see> in <c>expires</c> seconds</summary>
		public DateTime expires;
		/// <summary>New terms of service</summary>
		public Help_TermsOfService terms_of_service;
	}
	
	/// <summary>Deep link info, see <a href="https://corefork.telegram.org/api/links#unsupported-links">the here for more details</a>		<para>See <a href="https://corefork.telegram.org/constructor/help.deepLinkInfo"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.deepLinkInfoEmpty">help.deepLinkInfoEmpty</a></remarks>
	[TLDef(0x6A4EE832)]
	public sealed partial class Help_DeepLinkInfo : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Message to show to the user</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;

		[Flags] public enum Flags : uint
		{
			/// <summary>An update of the app is required to parse this link</summary>
			update_app = 0x1,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,
		}
	}
	/// <summary>Telegram <a href="https://corefork.telegram.org/passport">passport</a> configuration		<para>See <a href="https://corefork.telegram.org/constructor/help.passportConfig"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.passportConfigNotModified">help.passportConfigNotModified</a></remarks>
	[TLDef(0xA098D6AF)]
	public sealed partial class Help_PassportConfig : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary>Localization</summary>
		public DataJSON countries_langs;
	}
	/// <summary>Localized name for telegram support		<para>See <a href="https://corefork.telegram.org/constructor/help.supportName"/></para></summary>
	[TLDef(0x8C05F1C9)]
	public sealed partial class Help_SupportName : IObject
	{
		/// <summary>Localized name</summary>
		public string name;
	}

	/// <summary>Internal use		<para>See <a href="https://corefork.telegram.org/constructor/help.userInfo"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.userInfoEmpty">help.userInfoEmpty</a></remarks>
	[TLDef(0x01EB3758)]
	public sealed partial class Help_UserInfo : IObject
	{
		/// <summary>Info</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		public MessageEntity[] entities;
		/// <summary>Author</summary>
		public string author;
		/// <summary>Date</summary>
		public DateTime date;
	}

	/// <summary>Info about pinned MTProxy or Public Service Announcement peers.		<para>See <a href="https://corefork.telegram.org/type/help.PromoData"/></para>		<para>Derived classes: <see cref="Help_PromoDataEmpty"/>, <see cref="Help_PromoData"/></para></summary>
	public abstract partial class Help_PromoDataBase : IObject { }
	/// <summary>No PSA/MTProxy info is available		<para>See <a href="https://corefork.telegram.org/constructor/help.promoDataEmpty"/></para></summary>
	[TLDef(0x98F6AC75)]
	public sealed partial class Help_PromoDataEmpty : Help_PromoDataBase
	{
		/// <summary>Re-fetch PSA/MTProxy info after the specified number of seconds</summary>
		public DateTime expires;
	}
	/// <summary>MTProxy/Public Service Announcement information		<para>See <a href="https://corefork.telegram.org/constructor/help.promoData"/></para></summary>
	[TLDef(0x8C39793F)]
	public sealed partial class Help_PromoData : Help_PromoDataBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Expiry of PSA/MTProxy info</summary>
		public DateTime expires;
		/// <summary>MTProxy/PSA peer</summary>
		public Peer peer;
		/// <summary>Chat info</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>User info</summary>
		public Dictionary<long, User> users;
		/// <summary>PSA type</summary>
		[IfFlag(1)] public string psa_type;
		/// <summary>PSA message</summary>
		[IfFlag(2)] public string psa_message;

		[Flags] public enum Flags : uint
		{
			/// <summary>MTProxy-related channel</summary>
			proxy = 0x1,
			/// <summary>Field <see cref="psa_type"/> has a value</summary>
			has_psa_type = 0x2,
			/// <summary>Field <see cref="psa_message"/> has a value</summary>
			has_psa_message = 0x4,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the result</summary>
		public IPeerInfo UserOrChat => peer?.UserOrChat(users, chats);
	}
	/// <summary>Country code and phone number pattern of a specific country		<para>See <a href="https://corefork.telegram.org/constructor/help.countryCode"/></para></summary>
	[TLDef(0x4203C5EF)]
	public sealed partial class Help_CountryCode : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ISO country code</summary>
		public string country_code;
		/// <summary>Possible phone prefixes</summary>
		[IfFlag(0)] public string[] prefixes;
		/// <summary>Phone patterns: for example, <c>XXX XXX XXX</c></summary>
		[IfFlag(1)] public string[] patterns;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="prefixes"/> has a value</summary>
			has_prefixes = 0x1,
			/// <summary>Field <see cref="patterns"/> has a value</summary>
			has_patterns = 0x2,
		}
	}

	/// <summary>Name, ISO code, localized name and phone codes/patterns of a specific country		<para>See <a href="https://corefork.telegram.org/constructor/help.country"/></para></summary>
	[TLDef(0xC3878E23)]
	public sealed partial class Help_Country : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ISO code of country</summary>
		public string iso2;
		/// <summary>Name of the country in the country's language</summary>
		public string default_name;
		/// <summary>Name of the country in the user's language, if different from the original name</summary>
		[IfFlag(1)] public string name;
		/// <summary>Phone codes/patterns</summary>
		public Help_CountryCode[] country_codes;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this country should not be shown in the list</summary>
			hidden = 0x1,
			/// <summary>Field <see cref="name"/> has a value</summary>
			has_name = 0x2,
		}
	}

	/// <summary>Name, ISO code, localized name and phone codes/patterns of all available countries		<para>See <a href="https://corefork.telegram.org/constructor/help.countriesList"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.countriesListNotModified">help.countriesListNotModified</a></remarks>
	[TLDef(0x87D0759E)]
	public sealed partial class Help_CountriesList : IObject
	{
		/// <summary>Name, ISO code, localized name and phone codes/patterns of all available countries</summary>
		public Help_Country[] countries;
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
	}
	/// <summary>Telegram Premium promotion information		<para>See <a href="https://corefork.telegram.org/constructor/help.premiumPromo"/></para></summary>
	[TLDef(0x5334759C)]
	public sealed partial class Help_PremiumPromo : IObject
	{
		/// <summary>Description of the current state of the user's Telegram Premium subscription</summary>
		public string status_text;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		public MessageEntity[] status_entities;
		/// <summary>A list of <a href="https://corefork.telegram.org/api/premium">premium feature identifiers »</a>, associated to each video</summary>
		public string[] video_sections;
		/// <summary>A list of videos</summary>
		public DocumentBase[] videos;
		/// <summary>Telegram Premium subscription options</summary>
		public PremiumSubscriptionOption[] period_options;
		/// <summary>Related user information</summary>
		public Dictionary<long, User> users;
	}
	/// <summary>Contains various <a href="https://corefork.telegram.org/api/config#client-configuration">client configuration parameters</a>		<para>See <a href="https://corefork.telegram.org/constructor/help.appConfig"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.appConfigNotModified">help.appConfigNotModified</a></remarks>
	[TLDef(0xDD18782E)]
	public sealed partial class Help_AppConfig : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary><a href="https://corefork.telegram.org/api/config#client-configuration">Client configuration parameters</a></summary>
		public JsonObject config;
	}

	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/colors">color palette »</a>.		<para>See <a href="https://corefork.telegram.org/type/help.PeerColorSet"/></para>		<para>Derived classes: <see cref="Help_PeerColorSet"/>, <see cref="Help_PeerColorProfileSet"/></para></summary>
	public abstract partial class Help_PeerColorSetBase : IObject { }
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/colors">color palette that can be used in message accents »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/help.peerColorSet"/></para></summary>
	[TLDef(0x26219A58)]
	public sealed partial class Help_PeerColorSet : Help_PeerColorSetBase
	{
		/// <summary>A list of 1-3 colors in RGB format, describing the accent color.</summary>
		public int[] colors;
	}
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/colors">color palette that can be used in profile pages »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/help.peerColorProfileSet"/></para></summary>
	[TLDef(0x767D61EB)]
	public sealed partial class Help_PeerColorProfileSet : Help_PeerColorSetBase
	{
		/// <summary>A list of 1-2 colors in RGB format, shown in the color palette settings to describe the current palette.</summary>
		public int[] palette_colors;
		/// <summary>A list of 1-2 colors in RGB format describing the colors used to generate the actual background used in the profile page.</summary>
		public int[] bg_colors;
		/// <summary>A list of 2 colors in RGB format describing the colors of the gradient used for the unread active story indicator around the profile photo.</summary>
		public int[] story_colors;
	}

	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/colors">color palette »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/help.peerColorOption"/></para></summary>
	[TLDef(0xADEC6EBE)]
	public sealed partial class Help_PeerColorOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Palette ID.</summary>
		public int color_id;
		/// <summary>Light mode palette. <br/>Will be empty for IDs <c>0</c> to <c>6</c> inclusive, in which case a palette containing a single color from the following colors should be used: red, orange, violet, green, cyan, blue, pink for indexes 0 to 6 (i.e. the same colors used for randomized fallback <a href="https://corefork.telegram.org/api/colors">message accent colors</a>).</summary>
		[IfFlag(1)] public Help_PeerColorSetBase colors;
		/// <summary>Dark mode palette. Optional, defaults to the palette in <c>colors</c> (or the autogenerated palette for IDs <c>0</c> to <c>6</c>) if absent.</summary>
		[IfFlag(2)] public Help_PeerColorSetBase dark_colors;
		/// <summary>Channels can use this palette only after reaching at least the <a href="https://corefork.telegram.org/api/boost">boost level</a> specified in this field.</summary>
		[IfFlag(3)] public int channel_min_level;
		/// <summary>Supergroups can use this palette only after reaching at least the <a href="https://corefork.telegram.org/api/boost">boost level</a> specified in this field.</summary>
		[IfFlag(4)] public int group_min_level;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this palette should not be displayed as an option to the user when choosing a palette to apply to profile pages or message accents.</summary>
			hidden = 0x1,
			/// <summary>Field <see cref="colors"/> has a value</summary>
			has_colors = 0x2,
			/// <summary>Field <see cref="dark_colors"/> has a value</summary>
			has_dark_colors = 0x4,
			/// <summary>Field <see cref="channel_min_level"/> has a value</summary>
			has_channel_min_level = 0x8,
			/// <summary>Field <see cref="group_min_level"/> has a value</summary>
			has_group_min_level = 0x10,
		}
	}

	/// <summary>Contains info about multiple <a href="https://corefork.telegram.org/api/colors">color palettes »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/help.peerColors"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.peerColorsNotModified">help.peerColorsNotModified</a></remarks>
	[TLDef(0x00F8ED08)]
	public sealed partial class Help_PeerColors : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary>Usable <a href="https://corefork.telegram.org/api/colors">color palettes</a>.</summary>
		public Help_PeerColorOption[] colors;
	}
	/// <summary>Timezone information that may be used elsewhere in the API, such as to set <a href="https://corefork.telegram.org/api/business#opening-hours">Telegram Business opening hours »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/help.timezonesList"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/help.timezonesListNotModified">help.timezonesListNotModified</a></remarks>
	[TLDef(0x7B74ED71)]
	public sealed partial class Help_TimezonesList : IObject
	{
		/// <summary>Timezones</summary>
		public Timezone[] timezones;
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
	}

}