using System;
using System.Collections.Generic;

namespace TL
{
	#pragma warning disable IDE1006, CS1574
	/// <summary>Boolean type.		<para>See <a href="https://corefork.telegram.org/type/Bool"/></para></summary>
	public enum Bool : uint
	{
		///<summary>Constructor may be interpreted as a <strong>boolean</strong><c>false</c> value.</summary>
		False = 0xBC799737,
		///<summary>The constructor can be interpreted as a <strong>boolean</strong><c>true</c> value.</summary>
		True = 0x997275B5,
	}

	/// <summary>See <a href="https://corefork.telegram.org/mtproto/TL-formal#predefined-identifiers">predefined identifiers</a>.		<para>See <a href="https://corefork.telegram.org/constructor/true"/></para></summary>
	[TLDef(0x3FEDD339)]
	public sealed partial class True : IObject { }

	/// <summary>Error.		<para>See <a href="https://corefork.telegram.org/constructor/error"/></para></summary>
	[TLDef(0xC4B9F9BB)]
	public sealed partial class Error : IObject
	{
		/// <summary>Error code</summary>
		public int code;
		/// <summary>Message</summary>
		public string text;
	}

	/// <summary>Corresponds to an arbitrary empty object.		<para>See <a href="https://corefork.telegram.org/constructor/null"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/null">null</a></remarks>
	[TLDef(0x56730BCC)]
	public sealed partial class Null : IObject { }
	
	/// <summary>Object describes the file type.		<para>See <a href="https://corefork.telegram.org/type/storage.FileType"/></para></summary>
	public enum Storage_FileType : uint
	{
		///<summary>Unknown type.</summary>
		unknown = 0xAA963B05,
		///<summary>Part of a bigger file.</summary>
		partial = 0x40BC6F52,
		///<summary>JPEG image. MIME type: <c>image/jpeg</c>.</summary>
		jpeg = 0x007EFE0E,
		///<summary>GIF image. MIME type: <c>image/gif</c>.</summary>
		gif = 0xCAE1AADF,
		///<summary>PNG image. MIME type: <c>image/png</c>.</summary>
		png = 0x0A4F63C0,
		///<summary>PDF document image. MIME type: <c>application/pdf</c>.</summary>
		pdf = 0xAE1E508D,
		///<summary>Mp3 audio. MIME type: <c>audio/mpeg</c>.</summary>
		mp3 = 0x528A0677,
		///<summary>Quicktime video. MIME type: <c>video/quicktime</c>.</summary>
		mov = 0x4B09EBBC,
		///<summary>MPEG-4 video. MIME type: <c>video/mp4</c>.</summary>
		mp4 = 0xB3CEA0E4,
		///<summary>WEBP image. MIME type: <c>image/webp</c>.</summary>
		webp = 0x1081464C,
	}

	/// <summary>GeoPoint.		<para>See <a href="https://corefork.telegram.org/constructor/geoPoint"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/geoPointEmpty">geoPointEmpty</a></remarks>
	[TLDef(0xB2A2F663)]
	public sealed partial class GeoPoint : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Longitude</summary>
		public double lon;
		/// <summary>Latitude</summary>
		public double lat;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>The estimated horizontal accuracy of the location, in meters; as defined by the sender.</summary>
		[IfFlag(0)] public int accuracy_radius;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="accuracy_radius"/> has a value</summary>
			has_accuracy_radius = 0x1,
		}
	}

	/// <summary>Object contains info on a <a href="https://corefork.telegram.org/api/wallpapers">wallpaper</a>.		<para>See <a href="https://corefork.telegram.org/type/WallPaper"/></para>		<para>Derived classes: <see cref="WallPaper"/>, <see cref="WallPaperNoFile"/></para></summary>
	public abstract partial class WallPaperBase : IObject
	{
		/// <summary>Identifier</summary>
		public virtual long ID => default;
		/// <summary>Info on how to generate the wallpaper, according to <a href="https://corefork.telegram.org/api/wallpapers">these instructions »</a>.</summary>
		public virtual WallPaperSettings Settings => default;
	}
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/wallpapers">wallpaper</a> based on an image.		<para>See <a href="https://corefork.telegram.org/constructor/wallPaper"/></para></summary>
	[TLDef(0xA437C3ED)]
	public sealed partial class WallPaper : WallPaperBase
	{
		/// <summary>Identifier</summary>
		public long id;
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>Unique wallpaper ID, used when generating <a href="https://corefork.telegram.org/api/links#wallpaper-links">wallpaper links</a> or <a href="https://corefork.telegram.org/api/wallpapers">importing wallpaper links</a>.</summary>
		public string slug;
		/// <summary>The actual wallpaper</summary>
		public DocumentBase document;
		/// <summary>Info on how to generate the wallpaper, according to <a href="https://corefork.telegram.org/api/wallpapers">these instructions »</a>.</summary>
		[IfFlag(2)] public WallPaperSettings settings;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether we created this wallpaper</summary>
			creator = 0x1,
			/// <summary>Whether this is the default wallpaper</summary>
			default_ = 0x2,
			/// <summary>Field <see cref="settings"/> has a value</summary>
			has_settings = 0x4,
			/// <summary>Whether this is a <a href="https://corefork.telegram.org/api/wallpapers#pattern-wallpapers">pattern wallpaper »</a></summary>
			pattern = 0x8,
			/// <summary>Whether this wallpaper should be used in dark mode.</summary>
			dark = 0x10,
		}

		/// <summary>Identifier</summary>
		public override long ID => id;
		/// <summary>Info on how to generate the wallpaper, according to <a href="https://corefork.telegram.org/api/wallpapers">these instructions »</a>.</summary>
		public override WallPaperSettings Settings => settings;
	}
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/wallpapers">wallpaper</a> only based on colors/gradients.		<para>See <a href="https://corefork.telegram.org/constructor/wallPaperNoFile"/></para></summary>
	[TLDef(0xE0804116)]
	public sealed partial class WallPaperNoFile : WallPaperBase
	{
		/// <summary>Wallpaper ID</summary>
		public long id;
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Info on how to generate the wallpaper.</summary>
		[IfFlag(2)] public WallPaperSettings settings;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is the default wallpaper</summary>
			default_ = 0x2,
			/// <summary>Field <see cref="settings"/> has a value</summary>
			has_settings = 0x4,
			/// <summary>Whether this wallpaper should be used in dark mode.</summary>
			dark = 0x10,
		}

		/// <summary>Wallpaper ID</summary>
		public override long ID => id;
		/// <summary>Info on how to generate the wallpaper.</summary>
		public override WallPaperSettings Settings => settings;
	}

	/// <summary>Report reason		<para>See <a href="https://corefork.telegram.org/type/ReportReason"/></para></summary>
	public enum ReportReason : uint
	{
		///<summary>Report for spam</summary>
		Spam = 0x58DBCAB8,
		///<summary>Report for violence</summary>
		Violence = 0x1E22C78D,
		///<summary>Report for pornography</summary>
		Pornography = 0x2E59D922,
		///<summary>Report for child abuse</summary>
		ChildAbuse = 0xADF44EE3,
		///<summary>Other</summary>
		Other = 0xC1E4A2B1,
		///<summary>Report for copyrighted content</summary>
		Copyright = 0x9B89F93A,
		///<summary>Report an irrelevant geogroup</summary>
		GeoIrrelevant = 0xDBD4FEED,
		///<summary>Report for impersonation</summary>
		Fake = 0xF5DDD6E7,
		///<summary>Report for illegal drugs</summary>
		IllegalDrugs = 0x0A8EB2BE,
		///<summary>Report for divulgation of personal details</summary>
		PersonalDetails = 0x9EC7863D,
	}


	/// <summary>Data center		<para>See <a href="https://corefork.telegram.org/constructor/dcOption"/></para></summary>
	[TLDef(0x18B7A10D)]
	public sealed partial class DcOption : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>DC ID</summary>
		public int id;
		/// <summary>IP address of DC</summary>
		public string ip_address;
		/// <summary>Port</summary>
		public int port;
		/// <summary>If the <c>tcpo_only</c> flag is set, specifies the secret to use when connecting using <a href="https://corefork.telegram.org/mtproto/mtproto-transports#transport-obfuscation">transport obfuscation</a></summary>
		[IfFlag(10)] public byte[] secret;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the specified IP is an IPv6 address</summary>
			ipv6 = 0x1,
			/// <summary>Whether this DC should only be used to <a href="https://corefork.telegram.org/api/files">download or upload files</a></summary>
			media_only = 0x2,
			/// <summary>Whether this DC only supports connection with <a href="https://corefork.telegram.org/mtproto/mtproto-transports#transport-obfuscation">transport obfuscation</a></summary>
			tcpo_only = 0x4,
			/// <summary>Whether this is a <a href="https://corefork.telegram.org/cdn">CDN DC</a>.</summary>
			cdn = 0x8,
			/// <summary>If set, this IP should be used when connecting through a proxy</summary>
			static_ = 0x10,
			/// <summary>If set, clients must connect using only the specified port, without trying any other port.</summary>
			this_port_only = 0x20,
			/// <summary>Field <see cref="secret"/> has a value</summary>
			has_secret = 0x400,
		}
	}

	/// <summary>Current configuration		<para>See <a href="https://corefork.telegram.org/constructor/config"/></para></summary>
	[TLDef(0xCC1A241E)]
	public sealed partial class Config : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Current date at the server</summary>
		public DateTime date;
		/// <summary>Expiration date of this config: when it expires it'll have to be refetched using <see cref="SchemaExtensions.Help_GetConfig">Help_GetConfig</see></summary>
		public DateTime expires;
		/// <summary>Whether we're connected to the test DCs</summary>
		public bool test_mode;
		/// <summary>ID of the DC that returned the reply</summary>
		public int this_dc;
		/// <summary>DC IP list</summary>
		public DcOption[] dc_options;
		/// <summary>Domain name for fetching encrypted DC list from DNS TXT record</summary>
		public string dc_txt_domain_name;
		/// <summary>Maximum member count for normal <a href="https://corefork.telegram.org/api/channel">groups</a></summary>
		public int chat_size_max;
		/// <summary>Maximum member count for <a href="https://corefork.telegram.org/api/channel">supergroups</a></summary>
		public int megagroup_size_max;
		/// <summary>Maximum number of messages that can be forwarded at once using <see cref="SchemaExtensions.Messages_ForwardMessages">Messages_ForwardMessages</see>.</summary>
		public int forwarded_count_max;
		/// <summary>The client should <see cref="SchemaExtensions.Account_UpdateStatus">Account_UpdateStatus</see> every N milliseconds</summary>
		public int online_update_period_ms;
		/// <summary>Delay before offline status needs to be sent to the server</summary>
		public int offline_blur_timeout_ms;
		/// <summary>Time without any user activity after which it should be treated offline</summary>
		public int offline_idle_timeout_ms;
		/// <summary>If we are offline, but were online from some other client in last <c>online_cloud_timeout_ms</c> milliseconds after we had gone offline, then delay offline notification for <c>notify_cloud_delay_ms</c> milliseconds.</summary>
		public int online_cloud_timeout_ms;
		/// <summary>If we are offline, but online from some other client then delay sending the offline notification for <c>notify_cloud_delay_ms</c> milliseconds.</summary>
		public int notify_cloud_delay_ms;
		/// <summary>If some other client is online, then delay notification for <c>notification_default_delay_ms</c> milliseconds</summary>
		public int notify_default_delay_ms;
		/// <summary>Not for client use</summary>
		public int push_chat_period_ms;
		/// <summary>Not for client use</summary>
		public int push_chat_limit;
		/// <summary>Only messages with age smaller than the one specified can be edited</summary>
		public int edit_time_limit;
		/// <summary>Only channel/supergroup messages with age smaller than the specified can be deleted</summary>
		public int revoke_time_limit;
		/// <summary>Only private messages with age smaller than the specified can be deleted</summary>
		public int revoke_pm_time_limit;
		/// <summary>Exponential decay rate for computing <a href="https://corefork.telegram.org/api/top-rating">top peer rating</a></summary>
		public int rating_e_decay;
		/// <summary>Maximum number of recent stickers</summary>
		public int stickers_recent_limit;
		/// <summary>Indicates that round videos (video notes) and voice messages sent in channels and older than the specified period must be marked as read</summary>
		public int channels_read_media_period;
		/// <summary>Temporary <a href="https://corefork.telegram.org/passport">passport</a> sessions</summary>
		[IfFlag(0)] public int tmp_sessions;
		/// <summary>Maximum allowed outgoing ring time in VoIP calls: if the user we're calling doesn't reply within the specified time (in milliseconds), we should hang up the call</summary>
		public int call_receive_timeout_ms;
		/// <summary>Maximum allowed incoming ring time in VoIP calls: if the current user doesn't reply within the specified time (in milliseconds), the call will be automatically refused</summary>
		public int call_ring_timeout_ms;
		/// <summary>VoIP connection timeout: if the instance of libtgvoip on the other side of the call doesn't connect to our instance of libtgvoip within the specified time (in milliseconds), the call must be aborted</summary>
		public int call_connect_timeout_ms;
		/// <summary>If during a VoIP call a packet isn't received for the specified period of time, the call must be aborted</summary>
		public int call_packet_timeout_ms;
		/// <summary>The domain to use to parse <a href="https://corefork.telegram.org/api/links">deep links »</a>.</summary>
		public string me_url_prefix;
		/// <summary>URL to use to auto-update the current app</summary>
		[IfFlag(7)] public string autoupdate_url_prefix;
		/// <summary>Username of the bot to use to search for GIFs</summary>
		[IfFlag(9)] public string gif_search_username;
		/// <summary>Username of the bot to use to search for venues</summary>
		[IfFlag(10)] public string venue_search_username;
		/// <summary>Username of the bot to use for image search</summary>
		[IfFlag(11)] public string img_search_username;
		/// <summary>ID of the map provider to use for venues</summary>
		[IfFlag(12)] public string static_maps_provider;
		/// <summary>Maximum length of caption (length in utf8 codepoints)</summary>
		public int caption_length_max;
		/// <summary>Maximum length of messages (length in utf8 codepoints)</summary>
		public int message_length_max;
		/// <summary>DC ID to use to download <a href="https://corefork.telegram.org/api/files#downloading-webfiles">webfiles</a></summary>
		public int webfile_dc_id;
		/// <summary>Suggested language code</summary>
		[IfFlag(2)] public string suggested_lang_code;
		/// <summary>Language pack version</summary>
		[IfFlag(2)] public int lang_pack_version;
		/// <summary>Basic language pack version</summary>
		[IfFlag(2)] public int base_lang_pack_version;
		/// <summary>Default <a href="https://corefork.telegram.org/api/reactions">message reaction</a></summary>
		[IfFlag(15)] public Reaction reactions_default;
		/// <summary>Autologin token, <a href="https://corefork.telegram.org/api/url-authorization#link-url-authorization">click here for more info on URL authorization »</a>.</summary>
		[IfFlag(16)] public string autologin_token;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="tmp_sessions"/> has a value</summary>
			has_tmp_sessions = 0x1,
			/// <summary>Fields <see cref="suggested_lang_code"/>, <see cref="lang_pack_version"/> and <see cref="base_lang_pack_version"/> have a value</summary>
			has_suggested_lang_code = 0x4,
			/// <summary>Whether the client should use P2P by default for phone calls with contacts</summary>
			default_p2p_contacts = 0x8,
			/// <summary>Whether the client should preload featured stickers</summary>
			preload_featured_stickers = 0x10,
			/// <summary>Whether incoming private messages can be deleted for both participants</summary>
			revoke_pm_inbox = 0x40,
			/// <summary>Field <see cref="autoupdate_url_prefix"/> has a value</summary>
			has_autoupdate_url_prefix = 0x80,
			/// <summary>Indicates that telegram is <em>probably</em> censored by governments/ISPs in the current region</summary>
			blocked_mode = 0x100,
			/// <summary>Field <see cref="gif_search_username"/> has a value</summary>
			has_gif_search_username = 0x200,
			/// <summary>Field <see cref="venue_search_username"/> has a value</summary>
			has_venue_search_username = 0x400,
			/// <summary>Field <see cref="img_search_username"/> has a value</summary>
			has_img_search_username = 0x800,
			/// <summary>Field <see cref="static_maps_provider"/> has a value</summary>
			has_static_maps_provider = 0x1000,
			/// <summary>Whether to forcefully connect using IPv6 <see cref="DcOption"/>, even if the client knows that IPv4 is available.</summary>
			force_try_ipv6 = 0x4000,
			/// <summary>Field <see cref="reactions_default"/> has a value</summary>
			has_reactions_default = 0x8000,
			/// <summary>Field <see cref="autologin_token"/> has a value</summary>
			has_autologin_token = 0x10000,
		}
	}

	/// <summary>Nearest data center, according to geo-ip.		<para>See <a href="https://corefork.telegram.org/constructor/nearestDc"/></para></summary>
	[TLDef(0x8E1A1775)]
	public sealed partial class NearestDc : IObject
	{
		/// <summary>Country code determined by geo-ip</summary>
		public string country;
		/// <summary>Number of current data center</summary>
		public int this_dc;
		/// <summary>Number of nearest data center</summary>
		public int nearest_dc;
	}


	/// <summary>Object defines the set of users and/or groups that generate notifications.		<para>See <a href="https://corefork.telegram.org/type/NotifyPeer"/></para>		<para>Derived classes: <see cref="NotifyPeer"/>, <see cref="NotifyUsers"/>, <see cref="NotifyChats"/>, <see cref="NotifyBroadcasts"/>, <see cref="NotifyForumTopic"/></para></summary>
	public abstract partial class NotifyPeerBase : IObject { }
	/// <summary>Notifications generated by a certain user or group.		<para>See <a href="https://corefork.telegram.org/constructor/notifyPeer"/></para></summary>
	[TLDef(0x9FD40BD8)]
	public sealed partial class NotifyPeer : NotifyPeerBase
	{
		/// <summary>user or group</summary>
		public Peer peer;
	}
	/// <summary>Notifications generated by all users.		<para>See <a href="https://corefork.telegram.org/constructor/notifyUsers"/></para></summary>
	[TLDef(0xB4C83B4C)]
	public sealed partial class NotifyUsers : NotifyPeerBase { }
	/// <summary>Notifications generated by all groups.		<para>See <a href="https://corefork.telegram.org/constructor/notifyChats"/></para></summary>
	[TLDef(0xC007CEC3)]
	public sealed partial class NotifyChats : NotifyPeerBase { }
	/// <summary>Channel notification settings		<para>See <a href="https://corefork.telegram.org/constructor/notifyBroadcasts"/></para></summary>
	[TLDef(0xD612E8EF)]
	public sealed partial class NotifyBroadcasts : NotifyPeerBase { }
	/// <summary>Notifications generated by a <a href="https://corefork.telegram.org/api/forum#forum-topics">topic</a> in a <a href="https://corefork.telegram.org/api/forum">forum</a>.		<para>See <a href="https://corefork.telegram.org/constructor/notifyForumTopic"/></para></summary>
	[TLDef(0x226E6308)]
	public sealed partial class NotifyForumTopic : NotifyPeerBase
	{
		/// <summary>Forum ID</summary>
		public Peer peer;
		/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Topic ID</a></summary>
		public int top_msg_id;
	}

	/// <summary>User actions. Use this to provide users with detailed info about their chat partner's actions: typing or sending attachments of all kinds.		<para>See <a href="https://corefork.telegram.org/type/SendMessageAction"/></para>		<para>Derived classes: <see cref="SendMessageTypingAction"/>, <see cref="SendMessageCancelAction"/>, <see cref="SendMessageRecordVideoAction"/>, <see cref="SendMessageUploadVideoAction"/>, <see cref="SendMessageRecordAudioAction"/>, <see cref="SendMessageUploadAudioAction"/>, <see cref="SendMessageUploadPhotoAction"/>, <see cref="SendMessageUploadDocumentAction"/>, <see cref="SendMessageGeoLocationAction"/>, <see cref="SendMessageChooseContactAction"/>, <see cref="SendMessageGamePlayAction"/>, <see cref="SendMessageRecordRoundAction"/>, <see cref="SendMessageUploadRoundAction"/>, <see cref="SpeakingInGroupCallAction"/>, <see cref="SendMessageHistoryImportAction"/>, <see cref="SendMessageChooseStickerAction"/>, <see cref="SendMessageEmojiInteraction"/>, <see cref="SendMessageEmojiInteractionSeen"/></para></summary>
	public abstract partial class SendMessageAction : IObject { }
	/// <summary>User is typing.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageTypingAction"/></para></summary>
	[TLDef(0x16BF744E)]
	public sealed partial class SendMessageTypingAction : SendMessageAction { }
	/// <summary>Invalidate all previous action updates. E.g. when user deletes entered text or aborts a video upload.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageCancelAction"/></para></summary>
	[TLDef(0xFD5EC8F5)]
	public sealed partial class SendMessageCancelAction : SendMessageAction { }
	/// <summary>User is recording a video.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageRecordVideoAction"/></para></summary>
	[TLDef(0xA187D66F)]
	public sealed partial class SendMessageRecordVideoAction : SendMessageAction { }
	/// <summary>User is uploading a video.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageUploadVideoAction"/></para></summary>
	[TLDef(0xE9763AEC)]
	public sealed partial class SendMessageUploadVideoAction : SendMessageAction
	{
		/// <summary>Progress percentage</summary>
		public int progress;
	}
	/// <summary>User is recording a voice message.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageRecordAudioAction"/></para></summary>
	[TLDef(0xD52F73F7)]
	public sealed partial class SendMessageRecordAudioAction : SendMessageAction { }
	/// <summary>User is uploading a voice message.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageUploadAudioAction"/></para></summary>
	[TLDef(0xF351D7AB)]
	public sealed partial class SendMessageUploadAudioAction : SendMessageAction
	{
		/// <summary>Progress percentage</summary>
		public int progress;
	}
	/// <summary>User is uploading a photo.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageUploadPhotoAction"/></para></summary>
	[TLDef(0xD1D34A26)]
	public sealed partial class SendMessageUploadPhotoAction : SendMessageAction
	{
		/// <summary>Progress percentage</summary>
		public int progress;
	}
	/// <summary>User is uploading a file.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageUploadDocumentAction"/></para></summary>
	[TLDef(0xAA0CD9E4)]
	public sealed partial class SendMessageUploadDocumentAction : SendMessageAction
	{
		/// <summary>Progress percentage</summary>
		public int progress;
	}
	/// <summary>User is selecting a location to share.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageGeoLocationAction"/></para></summary>
	[TLDef(0x176F8BA1)]
	public sealed partial class SendMessageGeoLocationAction : SendMessageAction { }
	/// <summary>User is selecting a contact to share.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageChooseContactAction"/></para></summary>
	[TLDef(0x628CBC6F)]
	public sealed partial class SendMessageChooseContactAction : SendMessageAction { }
	/// <summary>User is playing a game		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageGamePlayAction"/></para></summary>
	[TLDef(0xDD6A8F48)]
	public sealed partial class SendMessageGamePlayAction : SendMessageAction { }
	/// <summary>User is recording a round video to share		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageRecordRoundAction"/></para></summary>
	[TLDef(0x88F27FBC)]
	public sealed partial class SendMessageRecordRoundAction : SendMessageAction { }
	/// <summary>User is uploading a round video		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageUploadRoundAction"/></para></summary>
	[TLDef(0x243E1C66)]
	public sealed partial class SendMessageUploadRoundAction : SendMessageAction
	{
		/// <summary>Progress percentage</summary>
		public int progress;
	}
	/// <summary>User is currently speaking in the group call		<para>See <a href="https://corefork.telegram.org/constructor/speakingInGroupCallAction"/></para></summary>
	[TLDef(0xD92C2285)]
	public sealed partial class SpeakingInGroupCallAction : SendMessageAction { }
	/// <summary>Chat history is being imported		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageHistoryImportAction"/></para></summary>
	[TLDef(0xDBDA9246)]
	public sealed partial class SendMessageHistoryImportAction : SendMessageAction
	{
		/// <summary>Progress percentage</summary>
		public int progress;
	}
	/// <summary>User is choosing a sticker		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageChooseStickerAction"/></para></summary>
	[TLDef(0xB05AC6B1)]
	public sealed partial class SendMessageChooseStickerAction : SendMessageAction { }
	/// <summary>User has clicked on an animated emoji triggering a <a href="https://corefork.telegram.org/api/animated-emojis#emoji-reactions">reaction, click here for more info »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageEmojiInteraction"/></para></summary>
	[TLDef(0x25972BCB)]
	public sealed partial class SendMessageEmojiInteraction : SendMessageAction
	{
		/// <summary>Emoji</summary>
		public string emoticon;
		/// <summary>Message ID of the animated emoji that was clicked</summary>
		public int msg_id;
		/// <summary>A JSON object with interaction info, <a href="https://corefork.telegram.org/api/animated-emojis#emoji-reactions">click here for more info »</a></summary>
		public DataJSON interaction;
	}
	/// <summary>User is watching an animated emoji reaction triggered by another user, <a href="https://corefork.telegram.org/api/animated-emojis#emoji-reactions">click here for more info »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/sendMessageEmojiInteractionSeen"/></para></summary>
	[TLDef(0xB665902E)]
	public sealed partial class SendMessageEmojiInteractionSeen : SendMessageAction
	{
		/// <summary>Emoji</summary>
		public string emoticon;
	}


	/// <summary>Message ID, for which PUSH-notifications were cancelled.		<para>See <a href="https://corefork.telegram.org/constructor/receivedNotifyMessage"/></para></summary>
	[TLDef(0xA384B779)]
	public sealed partial class ReceivedNotifyMessage : IObject
	{
		/// <summary>Message ID, for which PUSH-notifications were canceled</summary>
		public int id;
		/// <summary>Reserved for future use</summary>
		public int flags;
	}

	/// <summary>Exported chat invite		<para>See <a href="https://corefork.telegram.org/type/ExportedChatInvite"/></para>		<para>Derived classes: <see cref="ChatInviteExported"/>, <see cref="ChatInvitePublicJoinRequests"/></para></summary>
	public abstract partial class ExportedChatInvite : IObject { }

	/// <summary>Reply markup for bot and inline keyboards		<para>See <a href="https://corefork.telegram.org/type/ReplyMarkup"/></para>		<para>Derived classes: <see cref="ReplyKeyboardHide"/>, <see cref="ReplyKeyboardForceReply"/>, <see cref="ReplyKeyboardMarkup"/>, <see cref="ReplyInlineMarkup"/></para></summary>
	public abstract partial class ReplyMarkup : IObject { }
	/// <summary>Hide sent bot keyboard		<para>See <a href="https://corefork.telegram.org/constructor/replyKeyboardHide"/></para></summary>
	[TLDef(0xA03E5B85)]
	public sealed partial class ReplyKeyboardHide : ReplyMarkup
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Use this flag if you want to remove the keyboard for specific users only. Targets: 1) users that are @mentioned in the text of the Message object; 2) if the bot's message is a reply (has reply_to_message_id), sender of the original message.<br/><br/>Example: A user votes in a poll, bot returns confirmation message in reply to the vote and removes the keyboard for that user, while still showing the keyboard with poll options to users who haven't voted yet</summary>
			selective = 0x4,
		}
	}
	/// <summary>Force the user to send a reply		<para>See <a href="https://corefork.telegram.org/constructor/replyKeyboardForceReply"/></para></summary>
	[TLDef(0x86B40B08)]
	public sealed partial class ReplyKeyboardForceReply : ReplyMarkup
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The placeholder to be shown in the input field when the keyboard is active; 1-64 characters.</summary>
		[IfFlag(3)] public string placeholder;

		[Flags] public enum Flags : uint
		{
			/// <summary>Requests clients to hide the keyboard as soon as it's been used. The keyboard will still be available, but clients will automatically display the usual letter-keyboard in the chat – the user can press a special button in the input field to see the custom keyboard again.</summary>
			single_use = 0x2,
			/// <summary>Use this parameter if you want to show the keyboard to specific users only. Targets: 1) users that are @mentioned in the text of the Message object; 2) if the bot's message is a reply (has reply_to_message_id), sender of the original message. <br/>Example: A user requests to change the bot's language, bot replies to the request with a keyboard to select the new language. Other users in the group don't see the keyboard.</summary>
			selective = 0x4,
			/// <summary>Field <see cref="placeholder"/> has a value</summary>
			has_placeholder = 0x8,
		}
	}
	/// <summary>Bot keyboard		<para>See <a href="https://corefork.telegram.org/constructor/replyKeyboardMarkup"/></para></summary>
	[TLDef(0x85DD99D1)]
	public sealed partial class ReplyKeyboardMarkup : ReplyMarkup
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Button row</summary>
		public KeyboardButtonRow[] rows;
		/// <summary>The placeholder to be shown in the input field when the keyboard is active; 1-64 characters.</summary>
		[IfFlag(3)] public string placeholder;

		[Flags] public enum Flags : uint
		{
			/// <summary>Requests clients to resize the keyboard vertically for optimal fit (e.g., make the keyboard smaller if there are just two rows of buttons). If not set, the custom keyboard is always of the same height as the app's standard keyboard.</summary>
			resize = 0x1,
			/// <summary>Requests clients to hide the keyboard as soon as it's been used. The keyboard will still be available, but clients will automatically display the usual letter-keyboard in the chat – the user can press a special button in the input field to see the custom keyboard again.</summary>
			single_use = 0x2,
			/// <summary>Use this parameter if you want to show the keyboard to specific users only. Targets: 1) users that are @mentioned in the text of the Message object; 2) if the bot's message is a reply (has reply_to_message_id), sender of the original message.<br/><br/>Example: A user requests to change the bot's language, bot replies to the request with a keyboard to select the new language. Other users in the group don't see the keyboard.</summary>
			selective = 0x4,
			/// <summary>Field <see cref="placeholder"/> has a value</summary>
			has_placeholder = 0x8,
			/// <summary>Requests clients to always show the keyboard when the regular keyboard is hidden.</summary>
			persistent = 0x10,
		}
	}
	/// <summary>Bot or inline keyboard		<para>See <a href="https://corefork.telegram.org/constructor/replyInlineMarkup"/></para></summary>
	[TLDef(0x48A30254)]
	public sealed partial class ReplyInlineMarkup : ReplyMarkup
	{
		/// <summary>Bot or inline keyboard rows</summary>
		public KeyboardButtonRow[] rows;
	}
	


	/// <summary>Link to a message in a supergroup/channel		<para>See <a href="https://corefork.telegram.org/constructor/exportedMessageLink"/></para></summary>
	[TLDef(0x5DAB1AF4)]
	public sealed partial class ExportedMessageLink : IObject
	{
		/// <summary>URL</summary>
		public string link;
		/// <summary>Embed code</summary>
		public string html;
	}



	/// <summary>The bot requested the user to message them in private		<para>See <a href="https://corefork.telegram.org/constructor/inlineBotSwitchPM"/></para></summary>
	[TLDef(0x3C20629F)]
	public sealed partial class InlineBotSwitchPM : IObject
	{
		/// <summary>Text for the button that switches the user to a private chat with the bot and sends the bot a start message with the parameter <c>start_parameter</c> (can be empty)</summary>
		public string text;
		/// <summary>The parameter for the <c>/start parameter</c></summary>
		public string start_param;
	}

	/// <summary>Top peer		<para>See <a href="https://corefork.telegram.org/constructor/topPeer"/></para></summary>
	[TLDef(0xEDCDC05B)]
	public sealed partial class TopPeer : IObject
	{
		/// <summary>Peer</summary>
		public Peer peer;
		/// <summary>Rating as computed in <a href="https://corefork.telegram.org/api/top-rating">top peer rating »</a></summary>
		public double rating;
	}

	/// <summary>Top peer category		<para>See <a href="https://corefork.telegram.org/type/TopPeerCategory"/></para></summary>
	public enum TopPeerCategory : uint
	{
		///<summary>Most used bots</summary>
		BotsPM = 0xAB661B5B,
		///<summary>Most used inline bots</summary>
		BotsInline = 0x148677E2,
		///<summary>Users we've chatted most frequently with</summary>
		Correspondents = 0x0637B7ED,
		///<summary>Often-opened groups and supergroups</summary>
		Groups = 0xBD17A14A,
		///<summary>Most frequently visited channels</summary>
		Channels = 0x161D9628,
		///<summary>Most frequently called users</summary>
		PhoneCalls = 0x1E76A78C,
		///<summary>Users to which the users often forwards messages to</summary>
		ForwardUsers = 0xA8406CA9,
		///<summary>Chats to which the users often forwards messages to</summary>
		ForwardChats = 0xFBEEC0F0,
		///<summary>Most frequently used <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-apps">Main Mini Bot Apps</a>.</summary>
		BotsApp = 0xFD9E7BEC,
	}

	/// <summary>Top peer category		<para>See <a href="https://corefork.telegram.org/constructor/topPeerCategoryPeers"/></para></summary>
	[TLDef(0xFB834291)]
	public sealed partial class TopPeerCategoryPeers : IObject
	{
		/// <summary>Top peer category of peers</summary>
		public TopPeerCategory category;
		/// <summary>Count of peers</summary>
		public int count;
		/// <summary>Peers</summary>
		public TopPeer[] peers;
	}


	/// <summary>Represents a message <a href="https://corefork.telegram.org/api/drafts">draft</a>.		<para>See <a href="https://corefork.telegram.org/type/DraftMessage"/></para>		<para>Derived classes: <see cref="DraftMessageEmpty"/>, <see cref="DraftMessage"/></para></summary>
	public abstract partial class DraftMessageBase : IObject { }
	/// <summary>Empty draft		<para>See <a href="https://corefork.telegram.org/constructor/draftMessageEmpty"/></para></summary>
	[TLDef(0x1B0C841A)]
	public sealed partial class DraftMessageEmpty : DraftMessageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>When was the draft last updated</summary>
		[IfFlag(0)] public DateTime date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="date"/> has a value</summary>
			has_date = 0x1,
		}
	}
	/// <summary>Represents a message <a href="https://corefork.telegram.org/api/drafts">draft</a>.		<para>See <a href="https://corefork.telegram.org/constructor/draftMessage"/></para></summary>
	[TLDef(0x2D65321F)]
	public sealed partial class DraftMessage : DraftMessageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, indicates that the message should be sent in reply to the specified message or story.</summary>
		[IfFlag(4)] public InputReplyTo reply_to;
		/// <summary>The draft</summary>
		public string message;
		/// <summary>Message <a href="https://corefork.telegram.org/api/entities">entities</a> for styled text.</summary>
		[IfFlag(3)] public MessageEntity[] entities;
		/// <summary>Media.</summary>
		[IfFlag(5)] public InputMedia media;
		/// <summary>Date of last update of the draft.</summary>
		public DateTime date;
		/// <summary>A <a href="https://corefork.telegram.org/api/effects">message effect that should be played as specified here »</a>.</summary>
		[IfFlag(7)] public long effect;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether no webpage preview will be generated</summary>
			no_webpage = 0x2,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x8,
			/// <summary>Field <see cref="reply_to"/> has a value</summary>
			has_reply_to = 0x10,
			/// <summary>Field <see cref="media"/> has a value</summary>
			has_media = 0x20,
			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x40,
			/// <summary>Field <see cref="effect"/> has a value</summary>
			has_effect = 0x80,
		}
	}
	

	/// <summary>Position on a photo where a mask should be placed when <a href="https://corefork.telegram.org/api/stickers#attached-stickers">attaching stickers to media »</a>		<para>See <a href="https://corefork.telegram.org/constructor/maskCoords"/></para></summary>
	[TLDef(0xAED6DBB2)]
	public sealed partial class MaskCoords : IObject
	{
		/// <summary>Part of the face, relative to which the mask should be placed</summary>
		public int n;
		/// <summary>Shift by X-axis measured in widths of the mask scaled to the face size, from left to right. (For example, -1.0 will place the mask just to the left of the default mask position)</summary>
		public double x;
		/// <summary>Shift by Y-axis measured in widths of the mask scaled to the face size, from left to right. (For example, -1.0 will place the mask just below the default mask position)</summary>
		public double y;
		/// <summary>Mask scaling coefficient. (For example, 2.0 means a doubled size)</summary>
		public double zoom;
	}


	/// <summary>Indicates an already sent game		<para>See <a href="https://corefork.telegram.org/constructor/game"/></para></summary>
	[TLDef(0xBDF9653B)]
	public sealed partial class Game : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the game</summary>
		public long id;
		/// <summary>Access hash of the game</summary>
		public long access_hash;
		/// <summary>Short name for the game</summary>
		public string short_name;
		/// <summary>Title of the game</summary>
		public string title;
		/// <summary>Game description</summary>
		public string description;
		/// <summary>Game preview</summary>
		public PhotoBase photo;
		/// <summary>Optional attached document</summary>
		[IfFlag(0)] public DocumentBase document;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="document"/> has a value</summary>
			has_document = 0x1,
		}
	}


	/// <summary>Game highscore		<para>See <a href="https://corefork.telegram.org/constructor/highScore"/></para></summary>
	[TLDef(0x73A379EB)]
	public sealed partial class HighScore : IObject
	{
		/// <summary>Position in highscore list</summary>
		public int pos;
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>Score</summary>
		public int score;
	}



	/// <summary>Represents a json-encoded object		<para>See <a href="https://corefork.telegram.org/constructor/dataJSON"/></para></summary>
	[TLDef(0x7D748D04)]
	public sealed partial class DataJSON : IObject
	{
		/// <summary>JSON-encoded object</summary>
		public string data;
	}

	/// <summary>This object represents a portion of the price for goods or services.		<para>See <a href="https://corefork.telegram.org/constructor/labeledPrice"/></para></summary>
	[TLDef(0xCB296BF8)]
	public sealed partial class LabeledPrice : IObject
	{
		/// <summary>Portion label</summary>
		public string label;
		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;
	}

	/// <summary>Invoice		<para>See <a href="https://corefork.telegram.org/constructor/invoice"/></para></summary>
	[TLDef(0x049EE584)]
	public sealed partial class Invoice : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code, or <c>XTR</c> for <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public string currency;
		/// <summary>Price breakdown, a list of components (e.g. product price, tax, discount, delivery cost, delivery tax, bonus, etc.)</summary>
		public LabeledPrice[] prices;
		/// <summary>The maximum accepted amount for tips in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		[IfFlag(8)] public long max_tip_amount;
		/// <summary>A vector of suggested amounts of tips in the <em>smallest units</em> of the currency (integer, not float/double). At most 4 suggested tip amounts can be specified. The suggested tip amounts must be positive, passed in a strictly increased order and must not exceed <c>max_tip_amount</c>.</summary>
		[IfFlag(8)] public long[] suggested_tip_amounts;
		/// <summary>Terms of service URL</summary>
		[IfFlag(10)] public string terms_url;
		/// <summary>The number of seconds between consecutive Telegram Star debiting for <a href="https://corefork.telegram.org/api/subscriptions#bot-subscriptions">bot subscription</a> invoices</summary>
		[IfFlag(11)] public int subscription_period;

		[Flags] public enum Flags : uint
		{
			/// <summary>Test invoice</summary>
			test = 0x1,
			/// <summary>Set this flag if you require the user's full name to complete the order</summary>
			name_requested = 0x2,
			/// <summary>Set this flag if you require the user's phone number to complete the order</summary>
			phone_requested = 0x4,
			/// <summary>Set this flag if you require the user's email address to complete the order</summary>
			email_requested = 0x8,
			/// <summary>Set this flag if you require the user's shipping address to complete the order</summary>
			shipping_address_requested = 0x10,
			/// <summary>Set this flag if the final price depends on the shipping method</summary>
			flexible = 0x20,
			/// <summary>Set this flag if user's phone number should be sent to provider</summary>
			phone_to_provider = 0x40,
			/// <summary>Set this flag if user's email address should be sent to provider</summary>
			email_to_provider = 0x80,
			/// <summary>Fields <see cref="max_tip_amount"/> and <see cref="suggested_tip_amounts"/> have a value</summary>
			has_max_tip_amount = 0x100,
			/// <summary>Whether this is a recurring payment</summary>
			recurring = 0x200,
			/// <summary>Field <see cref="terms_url"/> has a value</summary>
			has_terms_url = 0x400,
			/// <summary>Field <see cref="subscription_period"/> has a value</summary>
			has_subscription_period = 0x800,
		}
	}

	/// <summary>Shipping address		<para>See <a href="https://corefork.telegram.org/constructor/postAddress"/></para></summary>
	[TLDef(0x1E8CAAEB)]
	public sealed partial class PostAddress : IObject
	{
		/// <summary>First line for the address</summary>
		public string street_line1;
		/// <summary>Second line for the address</summary>
		public string street_line2;
		/// <summary>City</summary>
		public string city;
		/// <summary>State, if applicable (empty otherwise)</summary>
		public string state;
		/// <summary>ISO 3166-1 alpha-2 country code</summary>
		public string country_iso2;
		/// <summary>Address post code</summary>
		public string post_code;
	}
	
	/// <summary>Remote document		<para>See <a href="https://corefork.telegram.org/type/WebDocument"/></para>		<para>Derived classes: <see cref="WebDocument"/>, <see cref="WebDocumentNoProxy"/></para></summary>
	public abstract partial class WebDocumentBase : IObject
	{
		/// <summary>Document URL</summary>
		public virtual string Url => default;
		/// <summary>File size</summary>
		public virtual int Size => default;
		/// <summary>MIME type</summary>
		public virtual string MimeType => default;
		/// <summary>Attributes for media types</summary>
		public virtual DocumentAttribute[] Attributes => default;
	}
	/// <summary>Remote document		<para>See <a href="https://corefork.telegram.org/constructor/webDocument"/></para></summary>
	[TLDef(0x1C570ED1)]
	public sealed partial class WebDocument : WebDocumentBase
	{
		/// <summary>Document URL</summary>
		public string url;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>File size</summary>
		public int size;
		/// <summary>MIME type</summary>
		public string mime_type;
		/// <summary>Attributes for media types</summary>
		public DocumentAttribute[] attributes;

		/// <summary>Document URL</summary>
		public override string Url => url;
		/// <summary>File size</summary>
		public override int Size => size;
		/// <summary>MIME type</summary>
		public override string MimeType => mime_type;
		/// <summary>Attributes for media types</summary>
		public override DocumentAttribute[] Attributes => attributes;
	}
	/// <summary>Remote document that can be downloaded without <a href="https://corefork.telegram.org/api/files">proxying through telegram</a>		<para>See <a href="https://corefork.telegram.org/constructor/webDocumentNoProxy"/></para></summary>
	[TLDef(0xF9C8BCC6)]
	public sealed partial class WebDocumentNoProxy : WebDocumentBase
	{
		/// <summary>Document URL</summary>
		public string url;
		/// <summary>File size</summary>
		public int size;
		/// <summary>MIME type</summary>
		public string mime_type;
		/// <summary>Attributes for media types</summary>
		public DocumentAttribute[] attributes;

		/// <summary>Document URL</summary>
		public override string Url => url;
		/// <summary>File size</summary>
		public override int Size => size;
		/// <summary>MIME type</summary>
		public override string MimeType => mime_type;
		/// <summary>Attributes for media types</summary>
		public override DocumentAttribute[] Attributes => attributes;
	}
	


	/// <summary>Shipping option		<para>See <a href="https://corefork.telegram.org/constructor/shippingOption"/></para></summary>
	[TLDef(0xB6213CDF)]
	public sealed partial class ShippingOption : IObject
	{
		/// <summary>Option ID</summary>
		public string id;
		/// <summary>Title</summary>
		public string title;
		/// <summary>List of price portions</summary>
		public LabeledPrice[] prices;
	}

	/// <summary>Public key to use <strong>only</strong> during handshakes to <a href="https://corefork.telegram.org/cdn">CDN</a> DCs.		<para>See <a href="https://corefork.telegram.org/constructor/cdnPublicKey"/></para></summary>
	[TLDef(0xC982EABA)]
	public sealed partial class CdnPublicKey : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/cdn">CDN DC</a> ID</summary>
		public int dc_id;
		/// <summary>RSA public key</summary>
		public string public_key;
	}

	/// <summary>Configuration for <a href="https://corefork.telegram.org/cdn">CDN</a> file downloads.		<para>See <a href="https://corefork.telegram.org/constructor/cdnConfig"/></para></summary>
	[TLDef(0x5725E40A)]
	public sealed partial class CdnConfig : IObject
	{
		/// <summary>Vector of public keys to use <strong>only</strong> during handshakes to <a href="https://corefork.telegram.org/cdn">CDN</a> DCs.</summary>
		public CdnPublicKey[] public_keys;
	}

	/// <summary>Language pack string		<para>See <a href="https://corefork.telegram.org/type/LangPackString"/></para>		<para>Derived classes: <see cref="LangPackString"/>, <see cref="LangPackStringPluralized"/>, <see cref="LangPackStringDeleted"/></para></summary>
	public abstract partial class LangPackStringBase : IObject
	{
		/// <summary>Language key</summary>
		public virtual string Key => default;
	}
	/// <summary>Translated localization string		<para>See <a href="https://corefork.telegram.org/constructor/langPackString"/></para></summary>
	[TLDef(0xCAD181F6)]
	public sealed partial class LangPackString : LangPackStringBase
	{
		/// <summary>Language key</summary>
		public string key;
		/// <summary>Value</summary>
		public string value;

		/// <summary>Language key</summary>
		public override string Key => key;
	}
	/// <summary>A language pack string which has different forms based on the number of some object it mentions. See <a href="https://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html">https://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html</a> for more info		<para>See <a href="https://corefork.telegram.org/constructor/langPackStringPluralized"/></para></summary>
	[TLDef(0x6C47AC9F)]
	public sealed partial class LangPackStringPluralized : LangPackStringBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Localization key</summary>
		public string key;
		/// <summary>Value for zero objects</summary>
		[IfFlag(0)] public string zero_value;
		/// <summary>Value for one object</summary>
		[IfFlag(1)] public string one_value;
		/// <summary>Value for two objects</summary>
		[IfFlag(2)] public string two_value;
		/// <summary>Value for a few objects</summary>
		[IfFlag(3)] public string few_value;
		/// <summary>Value for many objects</summary>
		[IfFlag(4)] public string many_value;
		/// <summary>Default value</summary>
		public string other_value;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="zero_value"/> has a value</summary>
			has_zero_value = 0x1,
			/// <summary>Field <see cref="one_value"/> has a value</summary>
			has_one_value = 0x2,
			/// <summary>Field <see cref="two_value"/> has a value</summary>
			has_two_value = 0x4,
			/// <summary>Field <see cref="few_value"/> has a value</summary>
			has_few_value = 0x8,
			/// <summary>Field <see cref="many_value"/> has a value</summary>
			has_many_value = 0x10,
		}

		/// <summary>Localization key</summary>
		public override string Key => key;
	}
	/// <summary>Deleted localization string		<para>See <a href="https://corefork.telegram.org/constructor/langPackStringDeleted"/></para></summary>
	[TLDef(0x2979EEB2)]
	public sealed partial class LangPackStringDeleted : LangPackStringBase
	{
		/// <summary>Localization key</summary>
		public string key;

		/// <summary>Localization key</summary>
		public override string Key => key;
	}

	/// <summary>Changes to the app's localization pack		<para>See <a href="https://corefork.telegram.org/constructor/langPackDifference"/></para></summary>
	[TLDef(0xF385C1F6)]
	public sealed partial class LangPackDifference : IObject
	{
		/// <summary>Language code</summary>
		public string lang_code;
		/// <summary>Previous version number</summary>
		public int from_version;
		/// <summary>New version number</summary>
		public int version;
		/// <summary>Localized strings</summary>
		public LangPackStringBase[] strings;
	}

	/// <summary>Identifies a localization pack		<para>See <a href="https://corefork.telegram.org/constructor/langPackLanguage"/></para></summary>
	[TLDef(0xEECA5CE3)]
	public sealed partial class LangPackLanguage : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Language name</summary>
		public string name;
		/// <summary>Language name in the language itself</summary>
		public string native_name;
		/// <summary>Language code (pack identifier)</summary>
		public string lang_code;
		/// <summary>Identifier of a base language pack; may be empty. If a string is missed in the language pack, then it should be fetched from base language pack. Unsupported in custom language packs</summary>
		[IfFlag(1)] public string base_lang_code;
		/// <summary>A language code to be used to apply plural forms. See <a href="https://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html">https://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html</a> for more info</summary>
		public string plural_code;
		/// <summary>Total number of non-deleted strings from the language pack</summary>
		public int strings_count;
		/// <summary>Total number of translated strings from the language pack</summary>
		public int translated_count;
		/// <summary>Link to language translation interface; empty for custom local language packs</summary>
		public string translations_url;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the language pack is official</summary>
			official = 0x1,
			/// <summary>Field <see cref="base_lang_code"/> has a value</summary>
			has_base_lang_code = 0x2,
			/// <summary>Is this a localization pack for an RTL language</summary>
			rtl = 0x4,
			/// <summary>Is this a beta localization pack?</summary>
			beta = 0x8,
		}
	}
	
	/// <summary>Popular contact		<para>See <a href="https://corefork.telegram.org/constructor/popularContact"/></para></summary>
	[TLDef(0x5CE14175)]
	public sealed partial class PopularContact : IObject
	{
		/// <summary>Contact identifier</summary>
		public long client_id;
		/// <summary>How many people imported this contact</summary>
		public int importers;
	}
	
	/// <summary>Recent t.me urls		<para>See <a href="https://corefork.telegram.org/type/RecentMeUrl"/></para>		<para>Derived classes: <see cref="RecentMeUrlUnknown"/>, <see cref="RecentMeUrlUser"/>, <see cref="RecentMeUrlChat"/>, <see cref="RecentMeUrlChatInvite"/>, <see cref="RecentMeUrlStickerSet"/></para></summary>
	public abstract partial class RecentMeUrl : IObject
	{
		/// <summary>URL</summary>
		public string url;
	}
	/// <summary>Unknown t.me url		<para>See <a href="https://corefork.telegram.org/constructor/recentMeUrlUnknown"/></para></summary>
	[TLDef(0x46E1D13D)]
	public sealed partial class RecentMeUrlUnknown : RecentMeUrl { }
	/// <summary>Recent t.me link to a user		<para>See <a href="https://corefork.telegram.org/constructor/recentMeUrlUser"/></para></summary>
	[TLDef(0xB92C09E2, inheritBefore = true)]
	public sealed partial class RecentMeUrlUser : RecentMeUrl
	{
		/// <summary>User ID</summary>
		public long user_id;
	}
	/// <summary>Recent t.me link to a chat		<para>See <a href="https://corefork.telegram.org/constructor/recentMeUrlChat"/></para></summary>
	[TLDef(0xB2DA71D2, inheritBefore = true)]
	public sealed partial class RecentMeUrlChat : RecentMeUrl
	{
		/// <summary>Chat ID</summary>
		public long chat_id;
	}
	/// <summary>Recent t.me invite link to a chat		<para>See <a href="https://corefork.telegram.org/constructor/recentMeUrlChatInvite"/></para></summary>
	[TLDef(0xEB49081D, inheritBefore = true)]
	public sealed partial class RecentMeUrlChatInvite : RecentMeUrl
	{
		/// <summary>Chat invitation</summary>
		public ChatInviteBase chat_invite;
	}
	/// <summary>Recent t.me stickerset installation URL		<para>See <a href="https://corefork.telegram.org/constructor/recentMeUrlStickerSet"/></para></summary>
	[TLDef(0xBC0A57DC, inheritBefore = true)]
	public sealed partial class RecentMeUrlStickerSet : RecentMeUrl
	{
		/// <summary>Stickerset</summary>
		public StickerSetCoveredBase set;
	}


	/// <summary>Represents a bot logged in using the <a href="https://corefork.telegram.org/widgets/login">Telegram login widget</a>		<para>See <a href="https://corefork.telegram.org/constructor/webAuthorization"/></para></summary>
	[TLDef(0xA6F8F452)]
	public sealed partial class WebAuthorization : IObject
	{
		/// <summary>Authorization hash</summary>
		public long hash;
		/// <summary>Bot ID</summary>
		public long bot_id;
		/// <summary>The domain name of the website on which the user has logged in.</summary>
		public string domain;
		/// <summary>Browser user-agent</summary>
		public string browser;
		/// <summary>Platform</summary>
		public string platform;
		/// <summary>When was the web session created</summary>
		public DateTime date_created;
		/// <summary>When was the web session last active</summary>
		public DateTime date_active;
		/// <summary>IP address</summary>
		public string ip;
		/// <summary>Region, determined from IP address</summary>
		public string region;
	}




	/// <summary>SHA256 Hash of an uploaded file, to be checked for validity after download		<para>See <a href="https://corefork.telegram.org/constructor/fileHash"/></para></summary>
	[TLDef(0xF39B035C)]
	public sealed partial class FileHash : IObject
	{
		/// <summary>Offset from where to start computing SHA-256 hash</summary>
		public long offset;
		/// <summary>Length</summary>
		public int limit;
		/// <summary>SHA-256 Hash of file chunk, to be checked for validity after download</summary>
		public byte[] hash;
	}



	/// <summary>Saved contact		<para>See <a href="https://corefork.telegram.org/type/SavedContact"/></para>		<para>Derived classes: <see cref="SavedPhoneContact"/></para></summary>
	public abstract partial class SavedContact : IObject { }
	/// <summary>Saved contact		<para>See <a href="https://corefork.telegram.org/constructor/savedPhoneContact"/></para></summary>
	[TLDef(0x1142BD56)]
	public sealed partial class SavedPhoneContact : SavedContact
	{
		/// <summary>Phone number</summary>
		public string phone;
		/// <summary>First name</summary>
		public string first_name;
		/// <summary>Last name</summary>
		public string last_name;
		/// <summary>Date added</summary>
		public DateTime date;
	}


	/// <summary>Key derivation function to use when generating the <a href="https://corefork.telegram.org/api/srp">password hash for SRP two-factor authorization</a>		<para>See <a href="https://corefork.telegram.org/type/PasswordKdfAlgo"/></para>		<para>Derived classes: <see cref="PasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/passwordKdfAlgoUnknown">passwordKdfAlgoUnknown</a></remarks>
	public abstract partial class PasswordKdfAlgo : IObject { }
	/// <summary>This key derivation algorithm defines that <a href="https://corefork.telegram.org/api/srp">SRP 2FA login</a> must be used		<para>See <a href="https://corefork.telegram.org/constructor/passwordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow"/></para></summary>
	[TLDef(0x3A912D4A)]
	public sealed partial class PasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow : PasswordKdfAlgo
	{
		/// <summary>One of two salts used by the derivation function (see <a href="https://corefork.telegram.org/api/srp">SRP 2FA login</a>)</summary>
		public byte[] salt1;
		/// <summary>One of two salts used by the derivation function (see <a href="https://corefork.telegram.org/api/srp">SRP 2FA login</a>)</summary>
		public byte[] salt2;
		/// <summary>Base (see <a href="https://corefork.telegram.org/api/srp">SRP 2FA login</a>)</summary>
		public int g;
		/// <summary>2048-bit modulus (see <a href="https://corefork.telegram.org/api/srp">SRP 2FA login</a>)</summary>
		public byte[] p;
	}
	

	/// <summary>JSON key: value pair		<para>See <a href="https://corefork.telegram.org/constructor/jsonObjectValue"/></para></summary>
	[TLDef(0xC0DE1BD9)]
	public sealed partial class JsonObjectValue : IObject
	{
		/// <summary>Key</summary>
		public string key;
		/// <summary>Value</summary>
		public JSONValue value;
	}

	/// <summary>JSON value		<para>See <a href="https://corefork.telegram.org/type/JSONValue"/></para>		<para>Derived classes: <see cref="JsonNull"/>, <see cref="JsonBool"/>, <see cref="JsonNumber"/>, <see cref="JsonString"/>, <see cref="JsonArray"/>, <see cref="JsonObject"/></para></summary>
	public abstract partial class JSONValue : IObject { }
	/// <summary>null JSON value		<para>See <a href="https://corefork.telegram.org/constructor/jsonNull"/></para></summary>
	[TLDef(0x3F6D7B68)]
	public sealed partial class JsonNull : JSONValue { }
	/// <summary>JSON boolean value		<para>See <a href="https://corefork.telegram.org/constructor/jsonBool"/></para></summary>
	[TLDef(0xC7345E6A)]
	public sealed partial class JsonBool : JSONValue
	{
		/// <summary>Value</summary>
		public bool value;
	}
	/// <summary>JSON numeric value		<para>See <a href="https://corefork.telegram.org/constructor/jsonNumber"/></para></summary>
	[TLDef(0x2BE0DFA4)]
	public sealed partial class JsonNumber : JSONValue
	{
		/// <summary>Value</summary>
		public double value;
	}
	/// <summary>JSON string		<para>See <a href="https://corefork.telegram.org/constructor/jsonString"/></para></summary>
	[TLDef(0xB71E767A)]
	public sealed partial class JsonString : JSONValue
	{
		/// <summary>Value</summary>
		public string value;
	}
	/// <summary>JSON array		<para>See <a href="https://corefork.telegram.org/constructor/jsonArray"/></para></summary>
	[TLDef(0xF7444763)]
	public sealed partial class JsonArray : JSONValue
	{
		/// <summary>JSON values</summary>
		public JSONValue[] value;
	}
	/// <summary>JSON object value		<para>See <a href="https://corefork.telegram.org/constructor/jsonObject"/></para></summary>
	[TLDef(0x99C1D49D)]
	public sealed partial class JsonObject : JSONValue
	{
		/// <summary>Values</summary>
		public JsonObjectValue[] value;
	}


	/// <summary>A possible answer of a poll		<para>See <a href="https://corefork.telegram.org/constructor/pollAnswer"/></para></summary>
	[TLDef(0xFF16E2CA)]
	public sealed partial class PollAnswer : IObject
	{
		/// <summary>Textual representation of the answer (only <a href="https://corefork.telegram.org/api/premium">Premium</a> users can use <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji entities</a> here).</summary>
		public TextWithEntities text;
		/// <summary>The param that has to be passed to <see cref="SchemaExtensions.Messages_SendVote">Messages_SendVote</see>.</summary>
		public byte[] option;
	}

	/// <summary>Poll		<para>See <a href="https://corefork.telegram.org/constructor/poll"/></para></summary>
	[TLDef(0x58747131)]
	public sealed partial class Poll : IObject
	{
		/// <summary>ID of the poll</summary>
		public long id;
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The question of the poll (only <a href="https://corefork.telegram.org/api/premium">Premium</a> users can use <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji entities</a> here).</summary>
		public TextWithEntities question;
		/// <summary>The possible answers, vote using <see cref="SchemaExtensions.Messages_SendVote">Messages_SendVote</see>.</summary>
		public PollAnswer[] answers;
		/// <summary>Amount of time in seconds the poll will be active after creation, 5-600. Can't be used together with close_date.</summary>
		[IfFlag(4)] public int close_period;
		/// <summary>Point in time (Unix timestamp) when the poll will be automatically closed. Must be at least 5 and no more than 600 seconds in the future; can't be used together with close_period.</summary>
		[IfFlag(5)] public DateTime close_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the poll is closed and doesn't accept any more answers</summary>
			closed = 0x1,
			/// <summary>Whether cast votes are publicly visible to all users (non-anonymous poll)</summary>
			public_voters = 0x2,
			/// <summary>Whether multiple options can be chosen as answer</summary>
			multiple_choice = 0x4,
			/// <summary>Whether this is a quiz (with wrong and correct answers, results shown in the return type)</summary>
			quiz = 0x8,
			/// <summary>Field <see cref="close_period"/> has a value</summary>
			has_close_period = 0x10,
			/// <summary>Field <see cref="close_date"/> has a value</summary>
			has_close_date = 0x20,
		}
	}

	/// <summary>A poll answer, and how users voted on it		<para>See <a href="https://corefork.telegram.org/constructor/pollAnswerVoters"/></para></summary>
	[TLDef(0x3B6DDAD2)]
	public sealed partial class PollAnswerVoters : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The param that has to be passed to <see cref="SchemaExtensions.Messages_SendVote">Messages_SendVote</see>.</summary>
		public byte[] option;
		/// <summary>How many users voted for this option</summary>
		public int voters;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether we have chosen this answer</summary>
			chosen = 0x1,
			/// <summary>For quizzes, whether the option we have chosen is correct</summary>
			correct = 0x2,
		}
	}

	/// <summary>Results of poll		<para>See <a href="https://corefork.telegram.org/constructor/pollResults"/></para></summary>
	[TLDef(0x7ADF2420)]
	public sealed partial class PollResults : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Poll results</summary>
		[IfFlag(1)] public PollAnswerVoters[] results;
		/// <summary>Total number of people that voted in the poll</summary>
		[IfFlag(2)] public int total_voters;
		/// <summary>IDs of the last users that recently voted in the poll</summary>
		[IfFlag(3)] public Peer[] recent_voters;
		/// <summary>Explanation of quiz solution</summary>
		[IfFlag(4)] public string solution;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text in quiz solution</a></summary>
		[IfFlag(4)] public MessageEntity[] solution_entities;

		[Flags] public enum Flags : uint
		{
			/// <summary>Similar to <a href="https://corefork.telegram.org/api/min">min</a> objects, used for poll constructors that are the same for all users so they don't have the option chosen by the current user (you can use <see cref="SchemaExtensions.Messages_GetPollResults">Messages_GetPollResults</see> to get the full poll results).</summary>
			min = 0x1,
			/// <summary>Field <see cref="results"/> has a value</summary>
			has_results = 0x2,
			/// <summary>Field <see cref="total_voters"/> has a value</summary>
			has_total_voters = 0x4,
			/// <summary>Field <see cref="recent_voters"/> has a value</summary>
			has_recent_voters = 0x8,
			/// <summary>Fields <see cref="solution"/> and <see cref="solution_entities"/> have a value</summary>
			has_solution = 0x10,
		}
	}


	/// <summary>Settings used by telegram servers for sending the confirm code.		<para>See <a href="https://corefork.telegram.org/constructor/codeSettings"/></para></summary>
	[TLDef(0xAD253D78)]
	public sealed partial class CodeSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Previously stored future auth tokens, see <a href="https://corefork.telegram.org/api/auth#future-auth-tokens">the documentation for more info »</a></summary>
		[IfFlag(6)] public byte[][] logout_tokens;
		/// <summary>Used only by official iOS apps for Firebase auth: device token for apple push.</summary>
		[IfFlag(8)] public string token;
		/// <summary>Used only by official iOS apps for firebase auth: whether a sandbox-certificate will be used during transmission of the push notification.</summary>
		[IfFlag(8)] public bool app_sandbox;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to allow phone verification via <a href="https://corefork.telegram.org/api/auth">phone calls</a>.</summary>
			allow_flashcall = 0x1,
			/// <summary>Pass true if the phone number is used on the current device. Ignored if allow_flashcall is not set.</summary>
			current_number = 0x2,
			/// <summary>If a token that will be included in eventually sent SMSs is required: required in newer versions of android, to use the <a href="https://developers.google.com/identity/sms-retriever/overview">android SMS receiver APIs</a></summary>
			allow_app_hash = 0x10,
			/// <summary>Whether this device supports receiving the code using the <see cref="Auth_CodeType.MissedCall"/> method</summary>
			allow_missed_call = 0x20,
			/// <summary>Field <see cref="logout_tokens"/> has a value</summary>
			has_logout_tokens = 0x40,
			/// <summary>Whether Firebase auth is supported</summary>
			allow_firebase = 0x80,
			/// <summary>Fields <see cref="token"/> and <see cref="app_sandbox"/> have a value</summary>
			has_token = 0x100,
			/// <summary>Set this flag if there is a SIM card in the current device, but it is not possible to check whether the specified phone number matches the SIM's phone number.</summary>
			unknown_number = 0x200,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a> rendering information.		<para>See <a href="https://corefork.telegram.org/constructor/wallPaperSettings"/></para></summary>
	[TLDef(0x372EFCD0)]
	public sealed partial class WallPaperSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Used for <a href="https://corefork.telegram.org/api/wallpapers#solid-fill">solid »</a>, <a href="https://corefork.telegram.org/api/wallpapers#gradient-fill">gradient »</a> and <a href="https://corefork.telegram.org/api/wallpapers#freeform-gradient-fill">freeform gradient »</a> fills.</summary>
		[IfFlag(0)] public int background_color;
		/// <summary>Used for <a href="https://corefork.telegram.org/api/wallpapers#gradient-fill">gradient »</a> and <a href="https://corefork.telegram.org/api/wallpapers#freeform-gradient-fill">freeform gradient »</a> fills.</summary>
		[IfFlag(4)] public int second_background_color;
		/// <summary>Used for <a href="https://corefork.telegram.org/api/wallpapers#freeform-gradient-fill">freeform gradient »</a> fills.</summary>
		[IfFlag(5)] public int third_background_color;
		/// <summary>Used for <a href="https://corefork.telegram.org/api/wallpapers#freeform-gradient-fill">freeform gradient »</a> fills.</summary>
		[IfFlag(6)] public int fourth_background_color;
		/// <summary>Used for <a href="https://corefork.telegram.org/api/wallpapers#pattern-wallpapers">pattern wallpapers »</a>.</summary>
		[IfFlag(3)] public int intensity;
		/// <summary>Clockwise rotation angle of the gradient, in degrees; 0-359. Should be always divisible by 45.</summary>
		[IfFlag(4)] public int rotation;
		/// <summary>If set, this wallpaper can be used as a channel wallpaper and is represented by the specified UTF-8 emoji.</summary>
		[IfFlag(7)] public string emoticon;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="background_color"/> has a value</summary>
			has_background_color = 0x1,
			/// <summary>For <a href="https://corefork.telegram.org/api/wallpapers#image-wallpapers">image wallpapers »</a>: if set, the JPEG must be downscaled to fit in 450x450 square and then box-blurred with radius 12.</summary>
			blur = 0x2,
			/// <summary>If set, the background needs to be slightly moved when the device is rotated.</summary>
			motion = 0x4,
			/// <summary>Field <see cref="intensity"/> has a value</summary>
			has_intensity = 0x8,
			/// <summary>Fields <see cref="second_background_color"/> and <see cref="rotation"/> have a value</summary>
			has_second_background_color = 0x10,
			/// <summary>Field <see cref="third_background_color"/> has a value</summary>
			has_third_background_color = 0x20,
			/// <summary>Field <see cref="fourth_background_color"/> has a value</summary>
			has_fourth_background_color = 0x40,
			/// <summary>Field <see cref="emoticon"/> has a value</summary>
			has_emoticon = 0x80,
		}
	}

	/// <summary>Autodownload settings		<para>See <a href="https://corefork.telegram.org/constructor/autoDownloadSettings"/></para></summary>
	[TLDef(0xBAA57628)]
	public sealed partial class AutoDownloadSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Maximum size of photos to preload</summary>
		public int photo_size_max;
		/// <summary>Maximum size of videos to preload</summary>
		public long video_size_max;
		/// <summary>Maximum size of other files to preload</summary>
		public long file_size_max;
		/// <summary>Maximum suggested bitrate for <strong>uploading</strong> videos</summary>
		public int video_upload_maxbitrate;
		/// <summary>A limit, specifying the maximum number of files that should be downloaded in parallel from the same DC, for files smaller than 20MB.</summary>
		public int small_queue_active_operations_max;
		/// <summary>A limit, specifying the maximum number of files that should be downloaded in parallel from the same DC, for files bigger than 20MB.</summary>
		public int large_queue_active_operations_max;

		[Flags] public enum Flags : uint
		{
			/// <summary>Disable automatic media downloads?</summary>
			disabled = 0x1,
			/// <summary>Whether to preload the first seconds of videos larger than the specified limit</summary>
			video_preload_large = 0x2,
			/// <summary>Whether to preload the next audio track when you're listening to music</summary>
			audio_preload_next = 0x4,
			/// <summary>Whether to enable data saving mode in phone calls</summary>
			phonecalls_less_data = 0x8,
			/// <summary>Whether to preload <a href="https://corefork.telegram.org/api/stories">stories</a>; in particular, the first <see cref="DocumentAttributeVideo"/>.<c>preload_prefix_size</c> bytes of story videos should be preloaded.</summary>
			stories_preload = 0x10,
		}
	}


	/// <summary>Emoji keyword		<para>See <a href="https://corefork.telegram.org/constructor/emojiKeyword"/></para></summary>
	[TLDef(0xD5B3B9F9)]
	public partial class EmojiKeyword : IObject
	{
		/// <summary>Keyword</summary>
		public string keyword;
		/// <summary>Emojis associated to keyword</summary>
		public string[] emoticons;
	}
	/// <summary>Deleted emoji keyword		<para>See <a href="https://corefork.telegram.org/constructor/emojiKeywordDeleted"/></para></summary>
	[TLDef(0x236DF622)]
	public sealed partial class EmojiKeywordDeleted : EmojiKeyword { }

	/// <summary>Changes to emoji keywords		<para>See <a href="https://corefork.telegram.org/constructor/emojiKeywordsDifference"/></para></summary>
	[TLDef(0x5CC761BD)]
	public sealed partial class EmojiKeywordsDifference : IObject
	{
		/// <summary>Language code for keywords</summary>
		public string lang_code;
		/// <summary>Previous emoji keyword list version</summary>
		public int from_version;
		/// <summary>Current version of emoji keyword list</summary>
		public int version;
		/// <summary>Emojis associated to keywords</summary>
		public EmojiKeyword[] keywords;
	}

	/// <summary>An HTTP URL which can be used to automatically log in into translation platform and suggest new emoji replacements. The URL will be valid for 30 seconds after generation		<para>See <a href="https://corefork.telegram.org/constructor/emojiURL"/></para></summary>
	[TLDef(0xA575739D)]
	public sealed partial class EmojiURL : IObject
	{
		/// <summary>An HTTP URL which can be used to automatically log in into translation platform and suggest new emoji replacements. The URL will be valid for 30 seconds after generation</summary>
		public string url;
	}

	/// <summary>Emoji language		<para>See <a href="https://corefork.telegram.org/constructor/emojiLanguage"/></para></summary>
	[TLDef(0xB3FB5361)]
	public sealed partial class EmojiLanguage : IObject
	{
		/// <summary>Language code</summary>
		public string lang_code;
	}

	/// <summary>Folder		<para>See <a href="https://corefork.telegram.org/constructor/folder"/></para></summary>
	[TLDef(0xFF544E65)]
	public sealed partial class Folder : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Folder ID</summary>
		public int id;
		/// <summary>Folder title</summary>
		public string title;
		/// <summary>Folder picture</summary>
		[IfFlag(3)] public ChatPhoto photo;

		[Flags] public enum Flags : uint
		{
			/// <summary>Automatically add new channels to this folder</summary>
			autofill_new_broadcasts = 0x1,
			/// <summary>Automatically add joined new public supergroups to this folder</summary>
			autofill_public_groups = 0x2,
			/// <summary>Automatically add new private chats to this folder</summary>
			autofill_new_correspondents = 0x4,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x8,
		}
	}


	/// <summary>Peer in a folder		<para>See <a href="https://corefork.telegram.org/constructor/folderPeer"/></para></summary>
	[TLDef(0xE9BAA668)]
	public sealed partial class FolderPeer : IObject
	{
		/// <summary>Folder peer info</summary>
		public Peer peer;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public int folder_id;
	}

	/// <summary>URL authorization result		<para>See <a href="https://corefork.telegram.org/type/UrlAuthResult"/></para>		<para>Derived classes: <see cref="UrlAuthResultRequest"/>, <see cref="UrlAuthResultAccepted"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/urlAuthResultDefault">urlAuthResultDefault</a></remarks>
	public abstract partial class UrlAuthResult : IObject { }
	/// <summary>Details about the authorization request, for more info <a href="https://corefork.telegram.org/api/url-authorization">click here »</a>		<para>See <a href="https://corefork.telegram.org/constructor/urlAuthResultRequest"/></para></summary>
	[TLDef(0x92D33A0E)]
	public sealed partial class UrlAuthResultRequest : UrlAuthResult
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Username of a bot, which will be used for user authorization. If not specified, the current bot's username will be assumed. The url's domain must be the same as the domain linked with the bot. See <a href="https://corefork.telegram.org/widgets/login#linking-your-domain-to-the-bot">Linking your domain to the bot</a> for more details.</summary>
		public UserBase bot;
		/// <summary>The domain name of the website on which the user will log in.</summary>
		public string domain;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the bot would like to send messages to the user</summary>
			request_write_access = 0x1,
		}
	}
	/// <summary>Details about an accepted authorization request, for more info <a href="https://corefork.telegram.org/api/url-authorization">click here »</a>		<para>See <a href="https://corefork.telegram.org/constructor/urlAuthResultAccepted"/></para></summary>
	[TLDef(0x8F8C0E4E)]
	public sealed partial class UrlAuthResultAccepted : UrlAuthResult
	{
		/// <summary>The URL name of the website on which the user has logged in.</summary>
		public string url;
	}



	/// <summary>Restriction reason.		<para>See <a href="https://corefork.telegram.org/constructor/restrictionReason"/></para></summary>
	[TLDef(0xD072ACB4)]
	public sealed partial class RestrictionReason : IObject
	{
		/// <summary>Platform identifier (ios, android, wp, all, etc.), can be concatenated with a dash as separator (<c>android-ios</c>, <c>ios-wp</c>, etc)</summary>
		public string platform;
		/// <summary>Restriction reason (<c>porno</c>, <c>terms</c>, etc.). Ignore this restriction reason if it is contained in the <a href="https://corefork.telegram.org/api/config#ignore-restriction-reasons">ignore_restriction_reasons »</a> client configuration parameter.</summary>
		public string reason;
		/// <summary>Error message to be shown to the user</summary>
		public string text;
	}


	/// <summary>Theme		<para>See <a href="https://corefork.telegram.org/constructor/theme"/></para></summary>
	[TLDef(0xA00E67D6)]
	public sealed partial class Theme : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Theme ID</summary>
		public long id;
		/// <summary>Theme access hash</summary>
		public long access_hash;
		/// <summary>Unique theme ID</summary>
		public string slug;
		/// <summary>Theme name</summary>
		public string title;
		/// <summary>Theme</summary>
		[IfFlag(2)] public DocumentBase document;
		/// <summary>Theme settings</summary>
		[IfFlag(3)] public ThemeSettings[] settings;
		/// <summary>Theme emoji</summary>
		[IfFlag(6)] public string emoticon;
		/// <summary>Installation count</summary>
		[IfFlag(4)] public int installs_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the current user is the creator of this theme</summary>
			creator = 0x1,
			/// <summary>Whether this is the default theme</summary>
			default_ = 0x2,
			/// <summary>Field <see cref="document"/> has a value</summary>
			has_document = 0x4,
			/// <summary>Field <see cref="settings"/> has a value</summary>
			has_settings = 0x8,
			/// <summary>Field <see cref="installs_count"/> has a value</summary>
			has_installs_count = 0x10,
			/// <summary>Whether this theme is meant to be used as a <a href="https://telegram.org/blog/chat-themes-interactive-emoji-read-receipts">chat theme</a></summary>
			for_chat = 0x20,
			/// <summary>Field <see cref="emoticon"/> has a value</summary>
			has_emoticon = 0x40,
		}
	}




	/// <summary>Basic theme settings		<para>See <a href="https://corefork.telegram.org/type/BaseTheme"/></para></summary>
	public enum BaseTheme : uint
	{
		///<summary>Classic theme</summary>
		Classic = 0xC3A12462,
		///<summary>Day theme</summary>
		Day = 0xFBD81688,
		///<summary>Night theme</summary>
		Night = 0xB7B31EA8,
		///<summary>Tinted theme</summary>
		Tinted = 0x6D5F77EE,
		///<summary>Arctic theme</summary>
		Arctic = 0x5B11125A,
	}


	/// <summary>Theme settings		<para>See <a href="https://corefork.telegram.org/constructor/themeSettings"/></para></summary>
	[TLDef(0xFA58B6D4)]
	public sealed partial class ThemeSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Base theme</summary>
		public BaseTheme base_theme;
		/// <summary>Accent color, ARGB format</summary>
		public int accent_color;
		/// <summary>Accent color of outgoing messages in ARGB format</summary>
		[IfFlag(3)] public int outbox_accent_color;
		/// <summary>The fill to be used as a background for outgoing messages, in RGB24 format. <br/>If just one or two equal colors are provided, describes a solid fill of a background. <br/>If two different colors are provided, describes the top and bottom colors of a 0-degree gradient.<br/>If three or four colors are provided, describes a freeform gradient fill of a background.</summary>
		[IfFlag(0)] public int[] message_colors;
		/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a></summary>
		[IfFlag(1)] public WallPaperBase wallpaper;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="message_colors"/> has a value</summary>
			has_message_colors = 0x1,
			/// <summary>Field <see cref="wallpaper"/> has a value</summary>
			has_wallpaper = 0x2,
			/// <summary>If set, the freeform gradient fill needs to be animated on every sent message.</summary>
			message_colors_animated = 0x4,
			/// <summary>Field <see cref="outbox_accent_color"/> has a value</summary>
			has_outbox_accent_color = 0x8,
		}
	}

	/// <summary>Webpage attributes		<para>See <a href="https://corefork.telegram.org/type/WebPageAttribute"/></para>		<para>Derived classes: <see cref="WebPageAttributeTheme"/>, <see cref="WebPageAttributeStory"/>, <see cref="WebPageAttributeStickerSet"/></para></summary>
	public abstract partial class WebPageAttribute : IObject { }
	/// <summary>Page theme		<para>See <a href="https://corefork.telegram.org/constructor/webPageAttributeTheme"/></para></summary>
	[TLDef(0x54B56617)]
	public sealed partial class WebPageAttributeTheme : WebPageAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Theme files</summary>
		[IfFlag(0)] public DocumentBase[] documents;
		/// <summary>Theme settings</summary>
		[IfFlag(1)] public ThemeSettings settings;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="documents"/> has a value</summary>
			has_documents = 0x1,
			/// <summary>Field <see cref="settings"/> has a value</summary>
			has_settings = 0x2,
		}
	}
	/// <summary>Webpage preview of a Telegram story		<para>See <a href="https://corefork.telegram.org/constructor/webPageAttributeStory"/></para></summary>
	[TLDef(0x2E94C3E7)]
	public sealed partial class WebPageAttributeStory : WebPageAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer that posted the story</summary>
		public Peer peer;
		/// <summary><a href="https://corefork.telegram.org/api/stories#watching-stories">Story ID</a></summary>
		public int id;
		/// <summary>May contain the story, if not the story should be fetched when and if needed using <see cref="SchemaExtensions.Stories_GetStoriesByID">Stories_GetStoriesByID</see> with the above <c>id</c> and <c>peer</c>.</summary>
		[IfFlag(0)] public StoryItemBase story;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="story"/> has a value</summary>
			has_story = 0x1,
		}
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/stickers">stickerset »</a>, for a <see cref="WebPage"/> preview of a <a href="https://corefork.telegram.org/api/links#stickerset-links">stickerset deep link »</a> (the <see cref="WebPage"/> will have a <c>type</c> of <c>telegram_stickerset</c>).		<para>See <a href="https://corefork.telegram.org/constructor/webPageAttributeStickerSet"/></para></summary>
	[TLDef(0x50CC03D3)]
	public sealed partial class WebPageAttributeStickerSet : WebPageAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>A subset of the stickerset in the stickerset.</summary>
		public DocumentBase[] stickers;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this i s a <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji stickerset</a>.</summary>
			emojis = 0x1,
			/// <summary>Whether the color of this TGS custom emoji stickerset should be changed to the text color when used in messages, the accent color if used as emoji status, white on chat photos, or another appropriate color based on context.</summary>
			text_color = 0x2,
		}
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/webPageAttributeUniqueStarGift"/></para></summary>
	[TLDef(0xCF6F6DB8)]
	public sealed partial class WebPageAttributeUniqueStarGift : WebPageAttribute
	{
		public StarGiftBase gift;
	}

	/// <summary>Credit card info URL provided by the bank		<para>See <a href="https://corefork.telegram.org/constructor/bankCardOpenUrl"/></para></summary>
	[TLDef(0xF568028A)]
	public sealed partial class BankCardOpenUrl : IObject
	{
		/// <summary>Info URL</summary>
		public string url;
		/// <summary>Bank name</summary>
		public string name;
	}




	/// <summary>Represents an animated video thumbnail		<para>See <a href="https://corefork.telegram.org/type/VideoSize"/></para>		<para>Derived classes: <see cref="VideoSize"/>, <see cref="VideoSizeEmojiMarkup"/>, <see cref="VideoSizeStickerMarkup"/></para></summary>
	public abstract partial class VideoSizeBase : IObject { }
	/// <summary>An <a href="https://corefork.telegram.org/api/files#animated-profile-pictures">animated profile picture</a> in MPEG4 format		<para>See <a href="https://corefork.telegram.org/constructor/videoSize"/></para></summary>
	[TLDef(0xDE33B094)]
	public sealed partial class VideoSize : VideoSizeBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><c>u</c> for animated profile pictures, and <c>v</c> for trimmed and downscaled video previews</summary>
		public string type;
		/// <summary>Video width</summary>
		public int w;
		/// <summary>Video height</summary>
		public int h;
		/// <summary>File size</summary>
		public int size;
		/// <summary>Timestamp that should be shown as static preview to the user (seconds)</summary>
		[IfFlag(0)] public double video_start_ts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="video_start_ts"/> has a value</summary>
			has_video_start_ts = 0x1,
		}
	}
	/// <summary>An <a href="https://corefork.telegram.org/api/files#animated-profile-pictures">animated profile picture</a> based on a <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji sticker</a>.		<para>See <a href="https://corefork.telegram.org/constructor/videoSizeEmojiMarkup"/></para></summary>
	[TLDef(0xF85C413C)]
	public sealed partial class VideoSizeEmojiMarkup : VideoSizeBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/custom-emoji">Custom emoji ID</a>: the custom emoji sticker is shown at the center of the profile picture and occupies at most 67% of it.</summary>
		public long emoji_id;
		/// <summary>1, 2, 3 or 4 RBG-24 colors used to generate a solid (1), gradient (2) or freeform gradient (3, 4) background, similar to how <a href="https://corefork.telegram.org/api/wallpapers#fill-types">fill wallpapers</a> are generated. The rotation angle for gradient backgrounds is 0.</summary>
		public int[] background_colors;
	}
	/// <summary>An <a href="https://corefork.telegram.org/api/files#animated-profile-pictures">animated profile picture</a> based on a <a href="https://corefork.telegram.org/api/stickers">sticker</a>.		<para>See <a href="https://corefork.telegram.org/constructor/videoSizeStickerMarkup"/></para></summary>
	[TLDef(0x0DA082FE)]
	public sealed partial class VideoSizeStickerMarkup : VideoSizeBase
	{
		/// <summary>Stickerset</summary>
		public InputStickerSet stickerset;
		/// <summary>Sticker ID</summary>
		public long sticker_id;
		/// <summary>1, 2, 3 or 4 RBG-24 colors used to generate a solid (1), gradient (2) or freeform gradient (3, 4) background, similar to how <a href="https://corefork.telegram.org/api/wallpapers#fill-types">fill wallpapers</a> are generated. The rotation angle for gradient backgrounds is 0.</summary>
		public int[] background_colors;
	}


	/// <summary>Global privacy settings		<para>See <a href="https://corefork.telegram.org/constructor/globalPrivacySettings"/></para></summary>
	[TLDef(0x734C4CCB)]
	public sealed partial class GlobalPrivacySettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to archive and mute new chats from non-contacts</summary>
			archive_and_mute_new_noncontact_peers = 0x1,
			/// <summary>Whether unmuted chats will be kept in the Archive chat list when they get a new message.</summary>
			keep_archived_unmuted = 0x2,
			/// <summary>Whether unmuted chats that are always included or pinned in a <a href="https://corefork.telegram.org/api/folders">folder</a>, will be kept in the Archive chat list when they get a new message. Ignored if <c>keep_archived_unmuted</c> is set.</summary>
			keep_archived_folders = 0x4,
			/// <summary>If this flag is set, the <see cref="InputPrivacyKey.StatusTimestamp"/> key will also apply to the ability to use <see cref="SchemaExtensions.Messages_GetOutboxReadDate">Messages_GetOutboxReadDate</see> on messages sent to us. <br/>Meaning, users that cannot see <em>our</em> exact last online date due to the current value of the <see cref="InputPrivacyKey.StatusTimestamp"/> key will receive a <c>403 USER_PRIVACY_RESTRICTED</c> error when invoking <see cref="SchemaExtensions.Messages_GetOutboxReadDate">Messages_GetOutboxReadDate</see> to fetch the exact read date of a message they sent to us. <br/>The <see cref="UserFull"/>.<c>read_dates_private</c> flag will be set for users that have this flag enabled.</summary>
			hide_read_marks = 0x8,
			/// <summary>If set, only users that have a premium account, are in our contact list, or already have a private chat with us can write to us; a <c>403 PRIVACY_PREMIUM_REQUIRED</c> error will be emitted otherwise.  <br/>The <see cref="UserFull"/>.<c>contact_require_premium</c> flag will be set for users that have this flag enabled.  <br/>To check whether we can write to a user with this flag enabled, if we haven't yet cached all the required information (for example we don't have the <see cref="UserFull"/> or history of all users while displaying the chat list in the sharing UI) the <see cref="SchemaExtensions.Users_GetIsPremiumRequiredToContact">Users_GetIsPremiumRequiredToContact</see> method may be invoked, passing the list of users currently visible in the UI, returning a list of booleans that directly specify whether we can or cannot write to each user. <br/>This option may be enabled by both non-<a href="https://corefork.telegram.org/api/premium">Premium</a> and <a href="https://corefork.telegram.org/api/premium">Premium</a> users only if the <a href="https://corefork.telegram.org/api/config#new-noncontact-peers-require-premium-without-ownpremium">new_noncontact_peers_require_premium_without_ownpremium client configuration flag »</a> is equal to true, otherwise it may be enabled only by <a href="https://corefork.telegram.org/api/premium">Premium</a> users and non-Premium users will receive a <c>PREMIUM_ACCOUNT_REQUIRED</c> error when trying to enable this flag.</summary>
			new_noncontact_peers_require_premium = 0x10,
		}
	}




	/// <summary>Inline query peer type.		<para>See <a href="https://corefork.telegram.org/type/InlineQueryPeerType"/></para></summary>
	public enum InlineQueryPeerType : uint
	{
		///<summary>Peer type: private chat with the bot itself</summary>
		SameBotPM = 0x3081ED9D,
		///<summary>Peer type: private chat</summary>
		PM = 0x833C0FAC,
		///<summary>Peer type: <a href="https://corefork.telegram.org/api/channel">chat</a></summary>
		Chat = 0xD766C50A,
		///<summary>Peer type: <a href="https://corefork.telegram.org/api/channel">supergroup</a></summary>
		Megagroup = 0x5EC4BE43,
		///<summary>Peer type: <a href="https://corefork.telegram.org/api/channel">channel</a></summary>
		Broadcast = 0x6334EE9A,
		///<summary>Peer type: private chat with a bot.</summary>
		BotPM = 0x0E3B2D0C,
	}





	/// <summary>A <a href="https://corefork.telegram.org/api/sponsored-messages">sponsored message</a>.		<para>See <a href="https://corefork.telegram.org/constructor/sponsoredMessage"/></para></summary>
	[TLDef(0x4D93A990)]
	public sealed partial class SponsoredMessage : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Message ID</summary>
		public byte[] random_id;
		/// <summary>Contains the URL to open when the user clicks on the sponsored message.</summary>
		public string url;
		/// <summary>Contains the title of the sponsored message.</summary>
		public string title;
		/// <summary>Sponsored message</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a> in <c>message</c>.</summary>
		[IfFlag(1)] public MessageEntity[] entities;
		/// <summary>If set, contains a custom profile photo bubble that should be displayed for the sponsored message, like for messages sent in groups.</summary>
		[IfFlag(6)] public PhotoBase photo;
		/// <summary>If set, contains some media.</summary>
		[IfFlag(14)] public MessageMedia media;
		/// <summary>If set, the sponsored message should use the <a href="https://corefork.telegram.org/api/colors">message accent color »</a> specified in <c>color</c>.</summary>
		[IfFlag(13)] public PeerColor color;
		/// <summary>Label of the sponsored message button.</summary>
		public string button_text;
		/// <summary>If set, contains additional information about the sponsor to be shown along with the message.</summary>
		[IfFlag(7)] public string sponsor_info;
		/// <summary>If set, contains additional information about the sponsored message to be shown along with the message.</summary>
		[IfFlag(8)] public string additional_info;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,
			/// <summary>Whether the message needs to be labeled as "recommended" instead of "sponsored"</summary>
			recommended = 0x20,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x40,
			/// <summary>Field <see cref="sponsor_info"/> has a value</summary>
			has_sponsor_info = 0x80,
			/// <summary>Field <see cref="additional_info"/> has a value</summary>
			has_additional_info = 0x100,
			/// <summary>Whether this message can be <a href="https://corefork.telegram.org/api/sponsored-messages#reporting-sponsored-messages">reported as specified here »</a>.</summary>
			can_report = 0x1000,
			/// <summary>Field <see cref="color"/> has a value</summary>
			has_color = 0x2000,
			/// <summary>Field <see cref="media"/> has a value</summary>
			has_media = 0x4000,
		}
	}

	/// <summary>Information about found messages sent on a specific day, used to split the <c>messages</c> in <see cref="Messages_SearchResultsCalendar"/>s by days.		<para>See <a href="https://corefork.telegram.org/constructor/searchResultsCalendarPeriod"/></para></summary>
	[TLDef(0xC9B0539F)]
	public sealed partial class SearchResultsCalendarPeriod : IObject
	{
		/// <summary>The day this object is referring to.</summary>
		public DateTime date;
		/// <summary>First message ID that was sent on this day.</summary>
		public int min_msg_id;
		/// <summary>Last message ID that was sent on this day.</summary>
		public int max_msg_id;
		/// <summary>All messages that were sent on this day.</summary>
		public int count;
	}

	/// <summary>Information about a message in a specific position		<para>See <a href="https://corefork.telegram.org/type/SearchResultsPosition"/></para>		<para>Derived classes: <see cref="SearchResultPosition"/></para></summary>
	public abstract partial class SearchResultsPosition : IObject { }
	/// <summary>Information about a message in a specific position		<para>See <a href="https://corefork.telegram.org/constructor/searchResultPosition"/></para></summary>
	[TLDef(0x7F648B67)]
	public sealed partial class SearchResultPosition : SearchResultsPosition
	{
		/// <summary>Message ID</summary>
		public int msg_id;
		/// <summary>When was the message sent</summary>
		public DateTime date;
		/// <summary>0-based message position in the full list of suitable messages</summary>
		public int offset;
	}




	/// <summary>Reactions		<para>See <a href="https://corefork.telegram.org/constructor/reactionCount"/></para></summary>
	[TLDef(0xA3D1CB80)]
	public sealed partial class ReactionCount : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, indicates that the current user also sent this reaction. <br/>The integer value indicates when was the reaction added: the bigger the value, the newer the reaction.</summary>
		[IfFlag(0)] public int chosen_order;
		/// <summary>The reaction.</summary>
		public Reaction reaction;
		/// <summary>Number of users that reacted with this emoji.</summary>
		public int count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="chosen_order"/> has a value</summary>
			has_chosen_order = 0x1,
		}
	}
	
	/// <summary>Animations associated with a message reaction		<para>See <a href="https://corefork.telegram.org/constructor/availableReaction"/></para></summary>
	[TLDef(0xC077EC01)]
	public sealed partial class AvailableReaction : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Reaction emoji</summary>
		public string reaction;
		/// <summary>Reaction description</summary>
		public string title;
		/// <summary>Static icon for the reaction</summary>
		public DocumentBase static_icon;
		/// <summary>The animated sticker to show when the user opens the reaction dropdown</summary>
		public DocumentBase appear_animation;
		/// <summary>The animated sticker to show when the user hovers over the reaction</summary>
		public DocumentBase select_animation;
		/// <summary>The animated sticker to show when the reaction is chosen and activated</summary>
		public DocumentBase activate_animation;
		/// <summary>The background effect (still an animated sticker) to play under the <c>activate_animation</c>, when the reaction is chosen and activated</summary>
		public DocumentBase effect_animation;
		/// <summary>The animation that plays around the button when you press an existing reaction (played together with <c>center_icon</c>).</summary>
		[IfFlag(1)] public DocumentBase around_animation;
		/// <summary>The animation of the emoji inside the button when you press an existing reaction (played together with <c>around_animation</c>).</summary>
		[IfFlag(1)] public DocumentBase center_icon;

		[Flags] public enum Flags : uint
		{
			/// <summary>If not set, the reaction can be added to new messages and enabled in chats.</summary>
			inactive = 0x1,
			/// <summary>Fields <see cref="around_animation"/> and <see cref="center_icon"/> have a value</summary>
			has_around_animation = 0x2,
			/// <summary>Whether this reaction can only be used by Telegram Premium users</summary>
			premium = 0x4,
		}
	}



	/// <summary>Represents an attachment menu icon color for <a href="https://corefork.telegram.org/api/bots/attach">bot mini apps »</a>		<para>See <a href="https://corefork.telegram.org/constructor/attachMenuBotIconColor"/></para></summary>
	[TLDef(0x4576F3F0)]
	public sealed partial class AttachMenuBotIconColor : IObject
	{
		/// <summary>One of the following values: <br/><c>light_icon</c> - Color of the attachment menu icon (light mode) <br/><c>light_text</c> - Color of the attachment menu label, once selected (light mode) <br/><c>dark_icon</c> - Color of the attachment menu icon (dark mode) <br/><c>dark_text</c> - Color of the attachment menu label, once selected (dark mode)</summary>
		public string name;
		/// <summary>Color in RGB24 format</summary>
		public int color;
	}

	/// <summary>Represents an attachment menu icon for <a href="https://corefork.telegram.org/api/bots/attach">bot mini apps »</a>		<para>See <a href="https://corefork.telegram.org/constructor/attachMenuBotIcon"/></para></summary>
	[TLDef(0xB2A7386B)]
	public sealed partial class AttachMenuBotIcon : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>One of the following values: note that animated icons must be played when the user clicks on the button, activating the bot mini app. <br/><br/><c>default_static</c> - Default attachment menu icon in SVG format <br/><c>placeholder_static</c> - Default placeholder for opened Web Apps in SVG format <br/><c>ios_static</c> - Attachment menu icon in SVG format for the official iOS app <br/><c>ios_animated</c> - Animated attachment menu icon in TGS format for the official iOS app <br/><c>android_animated</c> - Animated attachment menu icon in TGS format for the official Android app <br/><c>macos_animated</c> - Animated attachment menu icon in TGS format for the official native Mac OS app <br/><c>ios_side_menu_static</c> - Side menu icon in PNG format for the official iOS app <br/><c>android_side_menu_static</c> - Side menu icon in SVG format for the official android app <br/><c>macos_side_menu_static</c> - Side menu icon in PNG format for the official native Mac OS app</summary>
		public string name;
		/// <summary>The actual icon file.</summary>
		public DocumentBase icon;
		/// <summary>Attachment menu icon colors.</summary>
		[IfFlag(0)] public AttachMenuBotIconColor[] colors;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="colors"/> has a value</summary>
			has_colors = 0x1,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/bots/attach">bot mini app that can be launched from the attachment/side menu »</a>		<para>See <a href="https://corefork.telegram.org/constructor/attachMenuBot"/></para></summary>
	[TLDef(0xD90D8DFE)]
	public sealed partial class AttachMenuBot : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Bot ID</summary>
		public long bot_id;
		/// <summary>Attachment menu item name</summary>
		public string short_name;
		/// <summary>List of dialog types where this attachment menu entry should be shown</summary>
		[IfFlag(3)] public AttachMenuPeerType[] peer_types;
		/// <summary>List of platform-specific static icons and animations to use for the attachment menu button</summary>
		public AttachMenuBotIcon[] icons;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, before launching the mini app the client should ask the user to add the mini app to the attachment/side menu, and only if the user accepts, after invoking <see cref="SchemaExtensions.Messages_ToggleBotInAttachMenu">Messages_ToggleBotInAttachMenu</see> the app should be opened.</summary>
			inactive = 0x1,
			/// <summary>Deprecated flag, can be ignored.</summary>
			has_settings = 0x2,
			/// <summary>Whether the bot would like to send messages to the user.</summary>
			request_write_access = 0x4,
			/// <summary>Whether, when installed, an attachment menu entry should be shown for the Mini App.</summary>
			show_in_attach_menu = 0x8,
			/// <summary>Whether, when installed, an entry in the main view side menu should be shown for the Mini App.</summary>
			show_in_side_menu = 0x10,
			/// <summary>If <c>inactive</c> if set and the user hasn't previously accepted the third-party mini apps <a href="https://telegram.org/tos/mini-apps">Terms of Service</a> for this bot, when showing the mini app installation prompt, an additional mandatory checkbox to accept the <a href="https://telegram.org/tos/mini-apps">mini apps TOS</a> and a disclaimer indicating that this Mini App is not affiliated to Telegram should be shown.</summary>
			side_menu_disclaimer_needed = 0x20,
		}
	}

	/// <summary>Represents a list of <a href="https://corefork.telegram.org/api/bots/attach">bot mini apps that can be launched from the attachment menu »</a>		<para>See <a href="https://corefork.telegram.org/constructor/attachMenuBots"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/attachMenuBotsNotModified">attachMenuBotsNotModified</a></remarks>
	[TLDef(0x3C4301C0)]
	public sealed partial class AttachMenuBots : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>List of <a href="https://corefork.telegram.org/api/bots/attach">bot mini apps that can be launched from the attachment menu »</a></summary>
		public AttachMenuBot[] bots;
		/// <summary>Info about related users/bots</summary>
		public Dictionary<long, User> users;
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/bots/attach">bot mini app that can be launched from the attachment menu »</a>		<para>See <a href="https://corefork.telegram.org/constructor/attachMenuBotsBot"/></para></summary>
	[TLDef(0x93BF667F)]
	public sealed partial class AttachMenuBotsBot : IObject
	{
		/// <summary>Represents a <a href="https://corefork.telegram.org/api/bots/attach">bot mini app that can be launched from the attachment menu »</a><br/></summary>
		public AttachMenuBot bot;
		/// <summary>Info about related users and bots</summary>
		public Dictionary<long, User> users;
	}

	/// <summary>Contains the webview URL with appropriate theme and user info parameters added		<para>See <a href="https://corefork.telegram.org/type/WebViewResult"/></para>		<para>Derived classes: <see cref="WebViewResultUrl"/></para></summary>
	public abstract partial class WebViewResult : IObject { }
	/// <summary>Contains the webview URL with appropriate theme and user info parameters added		<para>See <a href="https://corefork.telegram.org/constructor/webViewResultUrl"/></para></summary>
	[TLDef(0x4D22FF98)]
	public sealed partial class WebViewResultUrl : WebViewResult
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Webview session ID (only returned by <a href="https://corefork.telegram.org/api/bots/webapps#inline-button-mini-apps">inline button mini apps</a>, <a href="https://corefork.telegram.org/api/bots/webapps#menu-button-mini-apps">menu button mini apps</a>, <a href="https://corefork.telegram.org/api/bots/webapps#attachment-menu-mini-apps">attachment menu mini apps</a>).</summary>
		[IfFlag(0)] public long query_id;
		/// <summary>Webview URL to open</summary>
		public string url;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="query_id"/> has a value</summary>
			has_query_id = 0x1,
			/// <summary>If set, the app must be opened in fullsize mode instead of compact mode.</summary>
			fullsize = 0x2,
			/// <summary>If set, the app must be opened in fullscreen</summary>
			fullscreen = 0x4,
		}
	}

	/// <summary>Info about a sent inline webview message		<para>See <a href="https://corefork.telegram.org/constructor/webViewMessageSent"/></para></summary>
	[TLDef(0x0C94511C)]
	public sealed partial class WebViewMessageSent : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Message ID</summary>
		[IfFlag(0)] public InputBotInlineMessageIDBase msg_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="msg_id"/> has a value</summary>
			has_msg_id = 0x1,
		}
	}


	/// <summary>Represents a notification sound		<para>See <a href="https://corefork.telegram.org/type/NotificationSound"/></para>		<para>Derived classes: <see cref="NotificationSoundNone"/>, <see cref="NotificationSoundLocal"/>, <see cref="NotificationSoundRingtone"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/notificationSoundDefault">notificationSoundDefault</a></remarks>
	public abstract partial class NotificationSound : IObject { }
	/// <summary>No notification sound should be used		<para>See <a href="https://corefork.telegram.org/constructor/notificationSoundNone"/></para></summary>
	[TLDef(0x6F0C34DF)]
	public sealed partial class NotificationSoundNone : NotificationSound { }
	/// <summary>Indicates a specific local notification sound should be used		<para>See <a href="https://corefork.telegram.org/constructor/notificationSoundLocal"/></para></summary>
	[TLDef(0x830B9AE4)]
	public sealed partial class NotificationSoundLocal : NotificationSound
	{
		/// <summary>Notification sound title</summary>
		public string title;
		/// <summary>Notification sound identifier (arbitrary data used by the client to identify a specific local notification sound)</summary>
		public string data;
	}
	/// <summary>A specific previously uploaded notification sound should be used		<para>See <a href="https://corefork.telegram.org/constructor/notificationSoundRingtone"/></para></summary>
	[TLDef(0xFF6C8049)]
	public sealed partial class NotificationSoundRingtone : NotificationSound
	{
		/// <summary>Document ID of notification sound uploaded using <see cref="SchemaExtensions.Account_UploadRingtone">Account_UploadRingtone</see></summary>
		public long id;
	}


	/// <summary>Indicates a supported peer type for a <a href="https://corefork.telegram.org/bots/webapps#launching-mini-apps-from-the-attachment-menu">bot mini app attachment menu</a>		<para>See <a href="https://corefork.telegram.org/type/AttachMenuPeerType"/></para></summary>
	public enum AttachMenuPeerType : uint
	{
		///<summary>The bot attachment menu entry is available in the chat with the bot that offers it</summary>
		SameBotPM = 0x7D6BE90E,
		///<summary>The bot attachment menu entry is available in private chats with other bots (excluding the bot that offers the current attachment menu)</summary>
		BotPM = 0xC32BFA1A,
		///<summary>The bot attachment menu entry is available in private chats with other users (not bots)</summary>
		PM = 0xF146D31F,
		///<summary>The bot attachment menu entry is available in <a href="https://corefork.telegram.org/api/channel">groups and supergroups</a></summary>
		Chat = 0x0509113F,
		///<summary>The bot attachment menu entry is available in channels</summary>
		Broadcast = 0x7BFBDEFC,
	}






	/// <summary><a href="https://corefork.telegram.org/api/emoji-status">Emoji status</a>		<para>See <a href="https://corefork.telegram.org/type/EmojiStatus"/></para>		<para>Derived classes: <see cref="EmojiStatus"/>, <see cref="EmojiStatusUntil"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/emojiStatusEmpty">emojiStatusEmpty</a></remarks>
	public abstract partial class EmojiStatusBase : IObject
	{
		public virtual DateTime Until => default;
	}
	/// <summary>An <a href="https://corefork.telegram.org/api/emoji-status">emoji status</a>		<para>See <a href="https://corefork.telegram.org/constructor/emojiStatus"/></para></summary>
	[TLDef(0xE7FF068A)]
	public sealed partial class EmojiStatus : EmojiStatusBase
	{
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/custom-emoji">Custom emoji document ID</a></summary>
		public long document_id;
		[IfFlag(0)] public DateTime until;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="until"/> has a value</summary>
			has_until = 0x1,
		}

		public override DateTime Until => until;
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/emojiStatusCollectible"/></para></summary>
	[TLDef(0x7184603B)]
	public sealed partial class EmojiStatusCollectible : EmojiStatusBase
	{
		public Flags flags;
		public long collectible_id;
		public long document_id;
		public string title;
		public string slug;
		public long pattern_document_id;
		public int center_color;
		public int edge_color;
		public int pattern_color;
		public int text_color;
		[IfFlag(0)] public DateTime until;

		[Flags] public enum Flags : uint
		{
			has_until = 0x1,
		}

		public override DateTime Until => until;
	}


	/// <summary><a href="https://corefork.telegram.org/api/reactions">Message reaction</a>		<para>See <a href="https://corefork.telegram.org/type/Reaction"/></para>		<para>Derived classes: <see cref="ReactionEmoji"/>, <see cref="ReactionCustomEmoji"/>, <see cref="ReactionPaid"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/reactionEmpty">reactionEmpty</a></remarks>
	public abstract partial class Reaction : IObject { }
	/// <summary>Normal emoji message reaction		<para>See <a href="https://corefork.telegram.org/constructor/reactionEmoji"/></para></summary>
	[TLDef(0x1B2286B8)]
	public sealed partial class ReactionEmoji : Reaction
	{
		/// <summary>Emoji</summary>
		public string emoticon;
	}
	/// <summary><a href="https://corefork.telegram.org/api/custom-emoji">Custom emoji</a> message reaction		<para>See <a href="https://corefork.telegram.org/constructor/reactionCustomEmoji"/></para></summary>
	[TLDef(0x8935FC73)]
	public sealed partial class ReactionCustomEmoji : Reaction
	{
		/// <summary><a href="https://corefork.telegram.org/api/custom-emoji">Custom emoji document ID</a></summary>
		public long document_id;
	}
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/reactions#paid-reactions">paid Telegram Star reaction »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/reactionPaid"/></para></summary>
	[TLDef(0x523DA4EB)]
	public sealed partial class ReactionPaid : Reaction { }

	
	/// <summary>Email verification purpose		<para>See <a href="https://corefork.telegram.org/type/EmailVerifyPurpose"/></para>		<para>Derived classes: <see cref="EmailVerifyPurposeLoginSetup"/>, <see cref="EmailVerifyPurposeLoginChange"/>, <see cref="EmailVerifyPurposePassport"/></para></summary>
	public abstract partial class EmailVerifyPurpose : IObject { }
	/// <summary>Email verification purpose: setup login email		<para>See <a href="https://corefork.telegram.org/constructor/emailVerifyPurposeLoginSetup"/></para></summary>
	[TLDef(0x4345BE73)]
	public sealed partial class EmailVerifyPurposeLoginSetup : EmailVerifyPurpose
	{
		/// <summary>Phone number</summary>
		public string phone_number;
		/// <summary>Phone code hash as specified by the <a href="https://corefork.telegram.org/api/auth#email-verification">documentation</a></summary>
		public string phone_code_hash;
	}
	/// <summary>Email verification purpose: change login email		<para>See <a href="https://corefork.telegram.org/constructor/emailVerifyPurposeLoginChange"/></para></summary>
	[TLDef(0x527D22EB)]
	public sealed partial class EmailVerifyPurposeLoginChange : EmailVerifyPurpose { }
	/// <summary>Verify an email for use in <a href="https://corefork.telegram.org/api/passport">telegram passport</a>		<para>See <a href="https://corefork.telegram.org/constructor/emailVerifyPurposePassport"/></para></summary>
	[TLDef(0xBBF51685)]
	public sealed partial class EmailVerifyPurposePassport : EmailVerifyPurpose { }

	/// <summary>Email verification code or token		<para>See <a href="https://corefork.telegram.org/type/EmailVerification"/></para>		<para>Derived classes: <see cref="EmailVerificationCode"/>, <see cref="EmailVerificationGoogle"/>, <see cref="EmailVerificationApple"/></para></summary>
	public abstract partial class EmailVerification : IObject { }
	/// <summary>Email verification code		<para>See <a href="https://corefork.telegram.org/constructor/emailVerificationCode"/></para></summary>
	[TLDef(0x922E55A9)]
	public sealed partial class EmailVerificationCode : EmailVerification
	{
		/// <summary>Received verification code</summary>
		public string code;
	}
	/// <summary>Google ID email verification token		<para>See <a href="https://corefork.telegram.org/constructor/emailVerificationGoogle"/></para></summary>
	[TLDef(0xDB909EC2)]
	public sealed partial class EmailVerificationGoogle : EmailVerification
	{
		/// <summary>Token</summary>
		public string token;
	}
	/// <summary>Apple ID email verification token		<para>See <a href="https://corefork.telegram.org/constructor/emailVerificationApple"/></para></summary>
	[TLDef(0x96D074FD)]
	public sealed partial class EmailVerificationApple : EmailVerification
	{
		/// <summary>Token</summary>
		public string token;
	}

	/// <summary>Indicates a peer that can be used to send messages		<para>See <a href="https://corefork.telegram.org/constructor/sendAsPeer"/></para></summary>
	[TLDef(0xB81C7034)]
	public sealed partial class SendAsPeer : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer</summary>
		public Peer peer;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether a Telegram Premium account is required to send messages as this peer</summary>
			premium_required = 0x1,
		}
	}
	

	/// <summary>Contains information about a <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a>		<para>See <a href="https://corefork.telegram.org/type/ForumTopic"/></para>		<para>Derived classes: <see cref="ForumTopicDeleted"/>, <see cref="ForumTopic"/></para></summary>
	public abstract partial class ForumTopicBase : IObject
	{
		/// <summary>The ID of the deleted forum topic.</summary>
		public virtual int ID => default;
	}
	/// <summary>Represents a deleted forum topic.		<para>See <a href="https://corefork.telegram.org/constructor/forumTopicDeleted"/></para></summary>
	[TLDef(0x023F109B)]
	public sealed partial class ForumTopicDeleted : ForumTopicBase
	{
		/// <summary>The ID of the deleted forum topic.</summary>
		public int id;

		/// <summary>The ID of the deleted forum topic.</summary>
		public override int ID => id;
	}
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a>.		<para>See <a href="https://corefork.telegram.org/constructor/forumTopic"/></para></summary>
	[TLDef(0x71701DA9)]
	public sealed partial class ForumTopic : ForumTopicBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Topic ID</a></summary>
		public int id;
		/// <summary>Topic creation date</summary>
		public DateTime date;
		/// <summary>Topic title</summary>
		public string title;
		/// <summary>If no custom emoji icon is specified, specifies the color of the fallback topic icon (RGB), one of <c>0x6FB9F0</c>, <c>0xFFD67E</c>, <c>0xCB86DB</c>, <c>0x8EEE98</c>, <c>0xFF93B2</c>, or <c>0xFB6F5F</c>.</summary>
		public int icon_color;
		/// <summary>ID of the <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji</a> used as topic icon.</summary>
		[IfFlag(0)] public long icon_emoji_id;
		/// <summary>ID of the last message that was sent to this topic</summary>
		public int top_message;
		/// <summary>Position up to which all incoming messages are read.</summary>
		public int read_inbox_max_id;
		/// <summary>Position up to which all outgoing messages are read.</summary>
		public int read_outbox_max_id;
		/// <summary>Number of unread messages</summary>
		public int unread_count;
		/// <summary>Number of <a href="https://corefork.telegram.org/api/mentions">unread mentions</a></summary>
		public int unread_mentions_count;
		/// <summary>Number of unread reactions to messages you sent</summary>
		public int unread_reactions_count;
		/// <summary>ID of the peer that created the topic</summary>
		public Peer from_id;
		/// <summary>Notification settings</summary>
		public PeerNotifySettings notify_settings;
		/// <summary>Message <a href="https://corefork.telegram.org/api/drafts">draft</a></summary>
		[IfFlag(4)] public DraftMessageBase draft;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="icon_emoji_id"/> has a value</summary>
			has_icon_emoji_id = 0x1,
			/// <summary>Whether the topic was created by the current user</summary>
			my = 0x2,
			/// <summary>Whether the topic is closed (no messages can be sent to it)</summary>
			closed = 0x4,
			/// <summary>Whether the topic is pinned</summary>
			pinned = 0x8,
			/// <summary>Field <see cref="draft"/> has a value</summary>
			has_draft = 0x10,
			/// <summary>Whether this constructor is a reduced version of the full topic information. <br/>If set, only the <c>my</c>, <c>closed</c>, <c>id</c>, <c>date</c>, <c>title</c>, <c>icon_color</c>, <c>icon_emoji_id</c> and <c>from_id</c> parameters will contain valid information. <br/>Reduced info is usually only returned in topic-related <a href="https://corefork.telegram.org/api/recent-actions">admin log events »</a> and in the <see cref="Messages_ChannelMessages"/>: if needed, full information can be fetched using <see cref="SchemaExtensions.Channels_GetForumTopicsByID">Channels_GetForumTopicsByID</see>.</summary>
			short_ = 0x20,
			/// <summary>Whether the topic is hidden (only valid for the "General" topic, <c>id=1</c>)</summary>
			hidden = 0x40,
		}

		/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Topic ID</a></summary>
		public override int ID => id;
	}
	
	/// <summary>Contains info about the default value of the Time-To-Live setting, applied to all new chats.		<para>See <a href="https://corefork.telegram.org/constructor/defaultHistoryTTL"/></para></summary>
	[TLDef(0x43B46B20)]
	public sealed partial class DefaultHistoryTTL : IObject
	{
		/// <summary>Time-To-Live setting applied to all new chats.</summary>
		public int period;
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/links#temporary-profile-links">temporary profile link</a>.		<para>See <a href="https://corefork.telegram.org/constructor/exportedContactToken"/></para></summary>
	[TLDef(0x41BF109B)]
	public sealed partial class ExportedContactToken : IObject
	{
		/// <summary>The <a href="https://corefork.telegram.org/api/links#temporary-profile-links">temporary profile link</a>.</summary>
		public string url;
		/// <summary>Its expiration date</summary>
		public DateTime expires;
	}

	/// <summary>Filtering criteria to use for the peer selection list shown to the user.		<para>See <a href="https://corefork.telegram.org/type/RequestPeerType"/></para>		<para>Derived classes: <see cref="RequestPeerTypeUser"/>, <see cref="RequestPeerTypeChat"/>, <see cref="RequestPeerTypeBroadcast"/></para></summary>
	public abstract partial class RequestPeerType : IObject { }
	/// <summary>Choose a user.		<para>See <a href="https://corefork.telegram.org/constructor/requestPeerTypeUser"/></para></summary>
	[TLDef(0x5F3B8A00)]
	public sealed partial class RequestPeerTypeUser : RequestPeerType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Whether to allow choosing only bots.</summary>
		[IfFlag(0)] public bool bot;
		/// <summary>Whether to allow choosing only <a href="https://corefork.telegram.org/api/premium">Premium</a> users.</summary>
		[IfFlag(1)] public bool premium;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="bot"/> has a value</summary>
			has_bot = 0x1,
			/// <summary>Field <see cref="premium"/> has a value</summary>
			has_premium = 0x2,
		}
	}
	/// <summary>Choose a chat or supergroup		<para>See <a href="https://corefork.telegram.org/constructor/requestPeerTypeChat"/></para></summary>
	[TLDef(0xC9F06E1B)]
	public sealed partial class RequestPeerTypeChat : RequestPeerType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If specified, allows only choosing channels with or without a username, according to the value of <see cref="bool"/>.</summary>
		[IfFlag(3)] public bool has_username;
		/// <summary>If specified, allows only choosing chats or supergroups that are or aren't <a href="https://corefork.telegram.org/api/forum">forums</a>, according to the value of <see cref="bool"/>.</summary>
		[IfFlag(4)] public bool forum;
		/// <summary>If specified, allows only choosing chats or supergroups where the current user is an admin with at least the specified admin rights.</summary>
		[IfFlag(1)] public ChatAdminRights user_admin_rights;
		/// <summary>If specified, allows only choosing chats or supergroups where the bot is an admin with at least the specified admin rights.</summary>
		[IfFlag(2)] public ChatAdminRights bot_admin_rights;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to allow only choosing chats or supergroups that were created by the current user.</summary>
			creator = 0x1,
			/// <summary>Field <see cref="user_admin_rights"/> has a value</summary>
			has_user_admin_rights = 0x2,
			/// <summary>Field <see cref="bot_admin_rights"/> has a value</summary>
			has_bot_admin_rights = 0x4,
			/// <summary>Field <see cref="has_username"/> has a value</summary>
			has_has_username = 0x8,
			/// <summary>Field <see cref="forum"/> has a value</summary>
			has_forum = 0x10,
			/// <summary>Whether to allow only choosing chats or supergroups where the bot is a participant.</summary>
			bot_participant = 0x20,
		}
	}
	/// <summary>Choose a channel		<para>See <a href="https://corefork.telegram.org/constructor/requestPeerTypeBroadcast"/></para></summary>
	[TLDef(0x339BEF6C)]
	public sealed partial class RequestPeerTypeBroadcast : RequestPeerType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If specified, allows only choosing channels with or without a username, according to the value of <see cref="bool"/>.</summary>
		[IfFlag(3)] public bool has_username;
		/// <summary>If specified, allows only choosing channels where the current user is an admin with at least the specified admin rights.</summary>
		[IfFlag(1)] public ChatAdminRights user_admin_rights;
		/// <summary>If specified, allows only choosing channels where the bot is an admin with at least the specified admin rights.</summary>
		[IfFlag(2)] public ChatAdminRights bot_admin_rights;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to allow only choosing channels that were created by the current user.</summary>
			creator = 0x1,
			/// <summary>Field <see cref="user_admin_rights"/> has a value</summary>
			has_user_admin_rights = 0x2,
			/// <summary>Field <see cref="bot_admin_rights"/> has a value</summary>
			has_bot_admin_rights = 0x4,
			/// <summary>Field <see cref="has_username"/> has a value</summary>
			has_has_username = 0x8,
		}
	}

	/// <summary>Represents a list of <a href="https://corefork.telegram.org/api/custom-emoji">custom emojis</a>.		<para>See <a href="https://corefork.telegram.org/constructor/emojiList"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/emojiListNotModified">emojiListNotModified</a></remarks>
	[TLDef(0x7A1E11D1)]
	public sealed partial class EmojiList : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>Custom emoji IDs</summary>
		public long[] document_id;
	}

	/// <summary>Represents an <a href="https://corefork.telegram.org/api/emoji-categories">emoji category</a>.		<para>See <a href="https://corefork.telegram.org/type/EmojiGroup"/></para>		<para>Derived classes: <see cref="EmojiGroup"/>, <see cref="EmojiGroupGreeting"/>, <see cref="EmojiGroupPremium"/></para></summary>
	public abstract partial class EmojiGroupBase : IObject
	{
		/// <summary>Category name, i.e. "Animals", "Flags", "Faces" and so on...</summary>
		public virtual string Title => default;
		/// <summary>A single custom emoji used as preview for the category.</summary>
		public virtual long IconEmojiId => default;
	}
	/// <summary>Represents an <a href="https://corefork.telegram.org/api/emoji-categories">emoji category</a>.		<para>See <a href="https://corefork.telegram.org/constructor/emojiGroup"/></para></summary>
	[TLDef(0x7A9ABDA9)]
	public partial class EmojiGroup : EmojiGroupBase
	{
		/// <summary>Category name, i.e. "Animals", "Flags", "Faces" and so on...</summary>
		public string title;
		/// <summary>A single custom emoji used as preview for the category.</summary>
		public long icon_emoji_id;
		/// <summary>A list of UTF-8 emojis, matching the category.</summary>
		public string[] emoticons;

		/// <summary>Category name, i.e. "Animals", "Flags", "Faces" and so on...</summary>
		public override string Title => title;
		/// <summary>A single custom emoji used as preview for the category.</summary>
		public override long IconEmojiId => icon_emoji_id;
	}
	/// <summary>Represents an <a href="https://corefork.telegram.org/api/emoji-categories">emoji category</a>, that should be moved to the top of the list when choosing a sticker for a <a href="https://corefork.telegram.org/api/business#business-introduction">business introduction</a>		<para>See <a href="https://corefork.telegram.org/constructor/emojiGroupGreeting"/></para></summary>
	[TLDef(0x80D26CC7)]
	public sealed partial class EmojiGroupGreeting : EmojiGroup
	{
	}
	/// <summary>An <a href="https://corefork.telegram.org/api/emoji-categories">emoji category</a>, used to select all <a href="https://corefork.telegram.org/api/premium">Premium</a>-only stickers (i.e. those with a <a href="https://corefork.telegram.org/api/stickers#premium-animated-sticker-effects">Premium effect »</a>)/<a href="https://corefork.telegram.org/api/premium">Premium</a>-only <a href="https://corefork.telegram.org/api/custom-emoji">custom emojis</a> (i.e. those where the <see cref="DocumentAttributeCustomEmoji"/>.<c>free</c> flag is <strong>not</strong> set)		<para>See <a href="https://corefork.telegram.org/constructor/emojiGroupPremium"/></para></summary>
	[TLDef(0x093BCF34)]
	public sealed partial class EmojiGroupPremium : EmojiGroupBase
	{
		/// <summary>Category name, i.e. "Animals", "Flags", "Faces" and so on...</summary>
		public string title;
		/// <summary>A single custom emoji used as preview for the category.</summary>
		public long icon_emoji_id;

		/// <summary>Category name, i.e. "Animals", "Flags", "Faces" and so on...</summary>
		public override string Title => title;
		/// <summary>A single custom emoji used as preview for the category.</summary>
		public override long IconEmojiId => icon_emoji_id;
	}

	/// <summary>Styled text with <a href="https://corefork.telegram.org/api/entities">message entities</a>		<para>See <a href="https://corefork.telegram.org/constructor/textWithEntities"/></para></summary>
	[TLDef(0x751F3146)]
	public sealed partial class TextWithEntities : IObject
	{
		/// <summary>Text</summary>
		public string text;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		public MessageEntity[] entities;
	}
	
	/// <summary>Media autosave settings		<para>See <a href="https://corefork.telegram.org/constructor/autoSaveSettings"/></para></summary>
	[TLDef(0xC84834CE)]
	public sealed partial class AutoSaveSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, specifies a size limit for autosavable videos</summary>
		[IfFlag(2)] public long video_max_size;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether photos should be autosaved to the gallery.</summary>
			photos = 0x1,
			/// <summary>Whether videos should be autosaved to the gallery.</summary>
			videos = 0x2,
			/// <summary>Field <see cref="video_max_size"/> has a value</summary>
			has_video_max_size = 0x4,
		}
	}

	/// <summary>Peer-specific media autosave settings		<para>See <a href="https://corefork.telegram.org/constructor/autoSaveException"/></para></summary>
	[TLDef(0x81602D47)]
	public sealed partial class AutoSaveException : IObject
	{
		/// <summary>The peer</summary>
		public Peer peer;
		/// <summary>Media autosave settings</summary>
		public AutoSaveSettings settings;
	}





	/// <summary>Specifies an <a href="https://corefork.telegram.org/api/bots/webapps#inline-mode-mini-apps">inline mode mini app</a> button, shown on top of the inline query results list.		<para>See <a href="https://corefork.telegram.org/constructor/inlineBotWebView"/></para></summary>
	[TLDef(0xB57295D5)]
	public sealed partial class InlineBotWebView : IObject
	{
		/// <summary>Text of the button</summary>
		public string text;
		/// <summary>Webapp URL</summary>
		public string url;
	}

	/// <summary>Contains info about when a certain participant has read a message		<para>See <a href="https://corefork.telegram.org/constructor/readParticipantDate"/></para></summary>
	[TLDef(0x4A4FF172)]
	public sealed partial class ReadParticipantDate : IObject
	{
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>When the user read the message</summary>
		public DateTime date;
	}

	/// <summary>Exported <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/exportedChatlistInvite"/></para></summary>
	[TLDef(0x0C5181AC)]
	public sealed partial class ExportedChatlistInvite : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Name of the link</summary>
		public string title;
		/// <summary>The <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.</summary>
		public string url;
		/// <summary>Peers to import</summary>
		public Peer[] peers;

		[Flags] public enum Flags : uint
		{
		}
	}



	

	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/giveaways">prepaid giveaway »</a>.		<para>See <a href="https://corefork.telegram.org/type/PrepaidGiveaway"/></para>		<para>Derived classes: <see cref="PrepaidGiveaway"/>, <see cref="PrepaidStarsGiveaway"/></para></summary>
	public abstract partial class PrepaidGiveawayBase : IObject
	{
		/// <summary>Prepaid giveaway ID.</summary>
		public virtual long ID => default;
		/// <summary>Number of given away <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscriptions.</summary>
		public virtual int Quantity => default;
		/// <summary>Payment date.</summary>
		public virtual DateTime Date => default;
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/giveaways">prepaid giveaway »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/prepaidGiveaway"/></para></summary>
	[TLDef(0xB2539D54)]
	public sealed partial class PrepaidGiveaway : PrepaidGiveawayBase
	{
		/// <summary>Prepaid giveaway ID.</summary>
		public long id;
		/// <summary>Duration in months of each gifted <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription.</summary>
		public int months;
		/// <summary>Number of given away <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscriptions.</summary>
		public int quantity;
		/// <summary>Payment date.</summary>
		public DateTime date;

		/// <summary>Prepaid giveaway ID.</summary>
		public override long ID => id;
		/// <summary>Number of given away <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscriptions.</summary>
		public override int Quantity => quantity;
		/// <summary>Payment date.</summary>
		public override DateTime Date => date;
	}
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">prepaid Telegram Star giveaway »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/prepaidStarsGiveaway"/></para></summary>
	[TLDef(0x9A9D77E0)]
	public sealed partial class PrepaidStarsGiveaway : PrepaidGiveawayBase
	{
		/// <summary>Prepaid giveaway ID.</summary>
		public long id;
		/// <summary>Number of given away <a href="https://corefork.telegram.org/api/stars">Telegram Stars »</a></summary>
		public long stars;
		/// <summary>Number of giveaway winners</summary>
		public int quantity;
		/// <summary>Number of boosts the channel will gain by launching the giveaway.</summary>
		public int boosts;
		/// <summary>When was the giveaway paid for</summary>
		public DateTime date;

		/// <summary>Prepaid giveaway ID.</summary>
		public override long ID => id;
		/// <summary>Number of giveaway winners</summary>
		public override int Quantity => quantity;
		/// <summary>When was the giveaway paid for</summary>
		public override DateTime Date => date;
	}

	/// <summary>Info about one or more <a href="https://corefork.telegram.org/api/boost">boosts</a> applied by a specific user.		<para>See <a href="https://corefork.telegram.org/constructor/boost"/></para></summary>
	[TLDef(0x4B3E14D6)]
	public sealed partial class Boost : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Unique ID for this set of boosts.</summary>
		public string id;
		/// <summary>ID of the user that applied the boost.</summary>
		[IfFlag(0)] public long user_id;
		/// <summary>The message ID of the <a href="https://corefork.telegram.org/api/giveaways">giveaway</a></summary>
		[IfFlag(2)] public int giveaway_msg_id;
		/// <summary>When was the boost applied</summary>
		public DateTime date;
		/// <summary>When does the boost expire</summary>
		public DateTime expires;
		/// <summary>The created Telegram Premium gift code, only set if either <c>gift</c> or <c>giveaway</c> are set AND it is either a gift code for the currently logged in user or if it was already claimed.</summary>
		[IfFlag(4)] public string used_gift_slug;
		/// <summary>If set, this boost counts as <c>multiplier</c> boosts, otherwise it counts as a single boost.</summary>
		[IfFlag(5)] public int multiplier;
		/// <summary>Number of Telegram Stars distributed among the winners of the giveaway.</summary>
		[IfFlag(6)] public long stars;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="user_id"/> has a value</summary>
			has_user_id = 0x1,
			/// <summary>Whether this boost was applied because the channel/supergroup <a href="https://corefork.telegram.org/api/giveaways">directly gifted a subscription to the user</a>.</summary>
			gift = 0x2,
			/// <summary>Whether this boost was applied because the user was chosen in a <a href="https://corefork.telegram.org/api/giveaways">giveaway started by the channel/supergroup</a>.</summary>
			giveaway = 0x4,
			/// <summary>If set, the user hasn't yet invoked <see cref="SchemaExtensions.Payments_ApplyGiftCode">Payments_ApplyGiftCode</see> to claim a subscription gifted <a href="https://corefork.telegram.org/api/giveaways">directly or in a giveaway by the channel</a>.</summary>
			unclaimed = 0x8,
			/// <summary>Field <see cref="used_gift_slug"/> has a value</summary>
			has_used_gift_slug = 0x10,
			/// <summary>Field <see cref="multiplier"/> has a value</summary>
			has_multiplier = 0x20,
			/// <summary>Field <see cref="stars"/> has a value</summary>
			has_stars = 0x40,
		}
	}


	/// <summary>Contains information about a single <a href="https://corefork.telegram.org/api/boost">boost slot »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/myBoost"/></para></summary>
	[TLDef(0xC448415C)]
	public sealed partial class MyBoost : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/boost">Boost slot ID »</a></summary>
		public int slot;
		/// <summary>If set, indicates this slot is currently occupied, i.e. we are <a href="https://corefork.telegram.org/api/boost">boosting</a> this peer.  <br/>Note that we can assign multiple boost slots to the same peer.</summary>
		[IfFlag(0)] public Peer peer;
		/// <summary>When (unixtime) we started boosting the <c>peer</c>, <c>0</c> otherwise.</summary>
		public DateTime date;
		/// <summary>Indicates the (unixtime) expiration date of the boost in <c>peer</c> (<c>0</c> if <c>peer</c> is not set).</summary>
		public DateTime expires;
		/// <summary>If <c>peer</c> is set, indicates the (unixtime) date after which this boost can be reassigned to another channel.</summary>
		[IfFlag(1)] public DateTime cooldown_until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="peer"/> has a value</summary>
			has_peer = 0x1,
			/// <summary>Field <see cref="cooldown_until_date"/> has a value</summary>
			has_cooldown_until_date = 0x2,
		}
	}



	/// <summary>Interaction counters		<para>See <a href="https://corefork.telegram.org/type/PostInteractionCounters"/></para>		<para>Derived classes: <see cref="PostInteractionCountersMessage"/>, <see cref="PostInteractionCountersStory"/></para></summary>
	public abstract partial class PostInteractionCounters : IObject
	{
		/// <summary>Number of views</summary>
		public int views;
		/// <summary>Number of forwards to public channels</summary>
		public int forwards;
		/// <summary>Number of reactions</summary>
		public int reactions;
	}
	/// <summary>Interaction counters for a message.		<para>See <a href="https://corefork.telegram.org/constructor/postInteractionCountersMessage"/></para></summary>
	[TLDef(0xE7058E7F)]
	public sealed partial class PostInteractionCountersMessage : PostInteractionCounters
	{
		/// <summary>Message ID</summary>
		public int msg_id;
	}
	/// <summary>Interaction counters for a story.		<para>See <a href="https://corefork.telegram.org/constructor/postInteractionCountersStory"/></para></summary>
	[TLDef(0x8A480E27)]
	public sealed partial class PostInteractionCountersStory : PostInteractionCounters
	{
		/// <summary>Story ID</summary>
		public int story_id;
	}


	/// <summary>Contains info about the forwards of a <a href="https://corefork.telegram.org/api/stories">story</a> as a message to public chats and reposts by public channels.		<para>See <a href="https://corefork.telegram.org/type/PublicForward"/></para>		<para>Derived classes: <see cref="PublicForwardMessage"/>, <see cref="PublicForwardStory"/></para></summary>
	public abstract partial class PublicForward : IObject { }
	/// <summary>Contains info about a forward of a <a href="https://corefork.telegram.org/api/stories">story</a> as a message.		<para>See <a href="https://corefork.telegram.org/constructor/publicForwardMessage"/></para></summary>
	[TLDef(0x01F2BF4A)]
	public sealed partial class PublicForwardMessage : PublicForward
	{
		/// <summary>Info about the message with the reposted story.</summary>
		public MessageBase message;
	}
	/// <summary>Contains info about a forward of a <a href="https://corefork.telegram.org/api/stories">story</a> as a repost by a public channel.		<para>See <a href="https://corefork.telegram.org/constructor/publicForwardStory"/></para></summary>
	[TLDef(0xEDF3ADD0)]
	public sealed partial class PublicForwardStory : PublicForward
	{
		/// <summary>The channel that reposted the story.</summary>
		public Peer peer;
		/// <summary>The reposted story (may be different from the original story).</summary>
		public StoryItemBase story;
	}


	/// <summary>Represents a <a href="https://corefork.telegram.org/api/saved-messages">saved dialog »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/savedDialog"/></para></summary>
	[TLDef(0xBD87CB6C)]
	public sealed partial class SavedDialog : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The dialog</summary>
		public Peer peer;
		/// <summary>The latest message ID</summary>
		public int top_message;

		[Flags] public enum Flags : uint
		{
			/// <summary>Is the dialog pinned</summary>
			pinned = 0x4,
		}
	}


	/// <summary>Info about a <a href="https://corefork.telegram.org/api/saved-messages#tags">saved message reaction tag »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/savedReactionTag"/></para></summary>
	[TLDef(0xCB6FF828)]
	public sealed partial class SavedReactionTag : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/reactions">Reaction</a> associated to the tag.</summary>
		public Reaction reaction;
		/// <summary>Custom tag name assigned by the user (max 12 UTF-8 chars).</summary>
		[IfFlag(0)] public string title;
		/// <summary>Number of messages tagged with this tag.</summary>
		public int count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,
		}
	}
	
	/// <summary>Exact read date of a private message we sent to another user.		<para>See <a href="https://corefork.telegram.org/constructor/outboxReadDate"/></para></summary>
	[TLDef(0x3BB842AC)]
	public sealed partial class OutboxReadDate : IObject
	{
		/// <summary>UNIX timestamp with the read date.</summary>
		public DateTime date;
	}

	/// <summary>SMS jobs eligibility		<para>See <a href="https://corefork.telegram.org/type/smsjobs.EligibilityToJoin"/></para>		<para>Derived classes: <see cref="Smsjobs_EligibleToJoin"/></para></summary>
	public abstract partial class Smsjobs_EligibilityToJoin : IObject { }
	/// <summary>SMS jobs eligibility		<para>See <a href="https://corefork.telegram.org/constructor/smsjobs.eligibleToJoin"/></para></summary>
	[TLDef(0xDC8B44CF)]
	public sealed partial class Smsjobs_EligibleToJoin : Smsjobs_EligibilityToJoin
	{
		/// <summary>Terms of service URL</summary>
		public string terms_url;
		/// <summary>Monthly sent SMSes</summary>
		public int monthly_sent_sms;
	}

	/// <summary>Status		<para>See <a href="https://corefork.telegram.org/constructor/smsjobs.status"/></para></summary>
	[TLDef(0x2AEE9191)]
	public sealed partial class Smsjobs_Status : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Recently sent</summary>
		public int recent_sent;
		/// <summary>Since</summary>
		public int recent_since;
		/// <summary>Remaining</summary>
		public int recent_remains;
		/// <summary>Total sent</summary>
		public int total_sent;
		/// <summary>Total since</summary>
		public int total_since;
		/// <summary>Last gift deep link</summary>
		[IfFlag(1)] public string last_gift_slug;
		/// <summary>Terms of service URL</summary>
		public string terms_url;

		[Flags] public enum Flags : uint
		{
			/// <summary>Allow international numbers</summary>
			allow_international = 0x1,
			/// <summary>Field <see cref="last_gift_slug"/> has a value</summary>
			has_last_gift_slug = 0x2,
		}
	}

	/// <summary>Info about an SMS job.		<para>See <a href="https://corefork.telegram.org/constructor/smsJob"/></para></summary>
	[TLDef(0xE6A1EEB8)]
	public sealed partial class SmsJob : IObject
	{
		/// <summary>Job ID</summary>
		public string job_id;
		/// <summary>Destination phone number</summary>
		public string phone_number;
		/// <summary>Text</summary>
		public string text;
	}


	/// <summary>Timezone information.		<para>See <a href="https://corefork.telegram.org/constructor/timezone"/></para></summary>
	[TLDef(0xFF9289F5)]
	public sealed partial class Timezone : IObject
	{
		/// <summary>Unique timezone ID.</summary>
		public string id;
		/// <summary>Human-readable and localized timezone name.</summary>
		public string name;
		/// <summary>UTC offset in seconds, which may be displayed in hh:mm format by the client together with the human-readable name (i.e. <c>$name UTC -01:00</c>).</summary>
		public int utc_offset;
	}


	/// <summary>A <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut</a>.		<para>See <a href="https://corefork.telegram.org/constructor/quickReply"/></para></summary>
	[TLDef(0x0697102B)]
	public sealed partial class QuickReply : IObject
	{
		/// <summary>Unique shortcut ID.</summary>
		public int shortcut_id;
		/// <summary>Shortcut name.</summary>
		public string shortcut;
		/// <summary>ID of the last message in the shortcut.</summary>
		public int top_message;
		/// <summary>Total number of messages in the shortcut.</summary>
		public int count;
	}

	
	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business bot »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/connectedBot"/></para></summary>
	[TLDef(0xBD068601)]
	public sealed partial class ConnectedBot : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the connected bot</summary>
		public long bot_id;
		/// <summary>Specifies the private chats that a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business bot »</a> may receive messages and interact with.<br/></summary>
		public BusinessBotRecipients recipients;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the the bot can reply to messages it receives through the connection</summary>
			can_reply = 0x1,
		}
	}

	
	/// <summary><a href="https://corefork.telegram.org/api/profile#birthday">Birthday</a> information for a user.		<para>See <a href="https://corefork.telegram.org/constructor/birthday"/></para></summary>
	[TLDef(0x6C8E1E06)]
	public sealed partial class Birthday : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Birth day</summary>
		public int day;
		/// <summary>Birth month</summary>
		public int month;
		/// <summary>(Optional) birth year.</summary>
		[IfFlag(0)] public int year;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="year"/> has a value</summary>
			has_year = 0x1,
		}
	}





	/// <summary>Info about a <a href="https://corefork.telegram.org/api/fragment">fragment collectible</a>.		<para>See <a href="https://corefork.telegram.org/constructor/fragment.collectibleInfo"/></para></summary>
	[TLDef(0x6EBDFF91)]
	public sealed partial class Fragment_CollectibleInfo : IObject
	{
		/// <summary>Purchase date (unixtime)</summary>
		public DateTime purchase_date;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code for <c>amount</c></summary>
		public string currency;
		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;
		/// <summary>Cryptocurrency name.</summary>
		public string crypto_currency;
		/// <summary>Price, in the smallest units of the cryptocurrency.</summary>
		public long crypto_amount;
		/// <summary><a href="https://fragment.com">Fragment</a> URL with more info about the collectible</summary>
		public string url;
	}




	/// <summary>Info about why a specific user could not be <a href="https://corefork.telegram.org/api/invites#direct-invites">invited »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/missingInvitee"/></para></summary>
	[TLDef(0x628C9224)]
	public sealed partial class MissingInvitee : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the user. If neither of the flags below are set, we could not add the user because of their privacy settings, and we can create and directly share an <a href="https://corefork.telegram.org/api/invites#invite-links">invite link</a> with them using a normal message, instead.</summary>
		public long user_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, we could not add the user <em>only because</em> the current account needs to purchase a <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription to complete the operation.</summary>
			premium_would_allow_invite = 0x1,
			/// <summary>If set, we could not add the user because of their privacy settings, and additionally, the current account needs to purchase a <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscription to directly share an invite link with the user via a private message.</summary>
			premium_required_for_pm = 0x2,
		}
	}





	/// <summary>Info about a peer, shared by a user with the currently logged in bot using <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.		<para>See <a href="https://corefork.telegram.org/type/RequestedPeer"/></para>		<para>Derived classes: <see cref="RequestedPeerUser"/>, <see cref="RequestedPeerChat"/>, <see cref="RequestedPeerChannel"/></para></summary>
	public abstract partial class RequestedPeer : IObject { }
	/// <summary>Info about a user, shared by a user with the currently logged in bot using <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.		<para>See <a href="https://corefork.telegram.org/constructor/requestedPeerUser"/></para></summary>
	[TLDef(0xD62FF46A)]
	public sealed partial class RequestedPeerUser : RequestedPeer
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>User ID.</summary>
		public long user_id;
		/// <summary>First name.</summary>
		[IfFlag(0)] public string first_name;
		/// <summary>Last name.</summary>
		[IfFlag(0)] public string last_name;
		/// <summary>Username.</summary>
		[IfFlag(1)] public string username;
		/// <summary>Profile photo.</summary>
		[IfFlag(2)] public PhotoBase photo;

		[Flags] public enum Flags : uint
		{
			/// <summary>Fields <see cref="first_name"/> and <see cref="last_name"/> have a value</summary>
			has_first_name = 0x1,
			/// <summary>Field <see cref="username"/> has a value</summary>
			has_username = 0x2,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x4,
		}
	}
	/// <summary>Info about a <a href="https://corefork.telegram.org/api/channel">chat</a>, shared by a user with the currently logged in bot using <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.		<para>See <a href="https://corefork.telegram.org/constructor/requestedPeerChat"/></para></summary>
	[TLDef(0x7307544F)]
	public sealed partial class RequestedPeerChat : RequestedPeer
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Chat ID.</summary>
		public long chat_id;
		/// <summary>Chat title.</summary>
		[IfFlag(0)] public string title;
		/// <summary>Chat photo.</summary>
		[IfFlag(2)] public PhotoBase photo;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x4,
		}
	}
	/// <summary>Info about a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a>, shared by a user with the currently logged in bot using <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.		<para>See <a href="https://corefork.telegram.org/constructor/requestedPeerChannel"/></para></summary>
	[TLDef(0x8BA403E4)]
	public sealed partial class RequestedPeerChannel : RequestedPeer
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Channel/supergroup ID.</summary>
		public long channel_id;
		/// <summary>Channel/supergroup title.</summary>
		[IfFlag(0)] public string title;
		/// <summary>Channel/supergroup username.</summary>
		[IfFlag(1)] public string username;
		/// <summary>Channel/supergroup photo.</summary>
		[IfFlag(2)] public PhotoBase photo;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,
			/// <summary>Field <see cref="username"/> has a value</summary>
			has_username = 0x2,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x4,
		}
	}

	/// <summary>A <a href="https://corefork.telegram.org/api/sponsored-messages#reporting-sponsored-messages">report option for a sponsored message »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/sponsoredMessageReportOption"/></para></summary>
	[TLDef(0x430D3150)]
	public sealed partial class SponsoredMessageReportOption : IObject
	{
		/// <summary>Localized description of the option.</summary>
		public string text;
		/// <summary>Option identifier to pass to <see cref="SchemaExtensions.Channels_ReportSponsoredMessage">Channels_ReportSponsoredMessage</see>.</summary>
		public byte[] option;
	}



	/// <summary>A <a href="https://corefork.telegram.org/api/revenue">channel ad revenue »</a> transaction.		<para>See <a href="https://corefork.telegram.org/type/BroadcastRevenueTransaction"/></para>		<para>Derived classes: <see cref="BroadcastRevenueTransactionProceeds"/>, <see cref="BroadcastRevenueTransactionWithdrawal"/>, <see cref="BroadcastRevenueTransactionRefund"/></para></summary>
	public abstract partial class BroadcastRevenueTransaction : IObject { }
	/// <summary>Describes earnings from sponsored messages in a channel in some time frame, see <a href="https://corefork.telegram.org/api/revenue">here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/broadcastRevenueTransactionProceeds"/></para></summary>
	[TLDef(0x557E2CC4)]
	public sealed partial class BroadcastRevenueTransactionProceeds : BroadcastRevenueTransaction
	{
		/// <summary>Amount in the smallest unit of the cryptocurrency.</summary>
		public long amount;
		/// <summary>Start unixtime for the timeframe.</summary>
		public DateTime from_date;
		/// <summary>End unixtime for the timeframe.</summary>
		public DateTime to_date;
	}
	/// <summary>Describes a <a href="https://corefork.telegram.org/api/revenue#withdrawing-revenue">withdrawal of ad earnings »</a>		<para>See <a href="https://corefork.telegram.org/constructor/broadcastRevenueTransactionWithdrawal"/></para></summary>
	[TLDef(0x5A590978)]
	public sealed partial class BroadcastRevenueTransactionWithdrawal : BroadcastRevenueTransaction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Amount withdrawn</summary>
		public long amount;
		/// <summary>Withdrawal date</summary>
		public DateTime date;
		/// <summary>Payment provider name</summary>
		public string provider;
		/// <summary>If neither <c>pending</c> nor <c>failed</c> are set, the transaction was completed successfully, and this field will contain the point in time (Unix timestamp) when the withdrawal was completed successfully.</summary>
		[IfFlag(1)] public DateTime transaction_date;
		/// <summary>If neither <c>pending</c> nor <c>failed</c> are set, the transaction was completed successfully, and this field will contain a URL where the withdrawal transaction can be viewed.</summary>
		[IfFlag(1)] public string transaction_url;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the withdrawal is currently pending</summary>
			pending = 0x1,
			/// <summary>Fields <see cref="transaction_date"/> and <see cref="transaction_url"/> have a value</summary>
			has_transaction_date = 0x2,
			/// <summary>Whether the withdrawal has failed</summary>
			failed = 0x4,
		}
	}
	/// <summary>Describes a <a href="https://corefork.telegram.org/api/revenue#withdrawing-revenue">refund for failed withdrawal of ad earnings »</a>		<para>See <a href="https://corefork.telegram.org/constructor/broadcastRevenueTransactionRefund"/></para></summary>
	[TLDef(0x42D30D2E)]
	public sealed partial class BroadcastRevenueTransactionRefund : BroadcastRevenueTransaction
	{
		/// <summary>Amount refunded.</summary>
		public long amount;
		/// <summary>Date of refund.</summary>
		public DateTime date;
		/// <summary>Payment provider name.</summary>
		public string provider;
	}


	/// <summary>Reaction notification settings		<para>See <a href="https://corefork.telegram.org/type/ReactionNotificationsFrom"/></para></summary>
	public enum ReactionNotificationsFrom : uint
	{
		///<summary>Receive notifications about reactions made only by our contacts.</summary>
		Contacts = 0xBAC3A61A,
		///<summary>Receive notifications about reactions made by any user.</summary>
		All = 0x4B9E22A0,
	}

	/// <summary>Reaction notification settings, see <a href="https://corefork.telegram.org/api/reactions#notifications-about-reactions">here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/reactionsNotifySettings"/></para></summary>
	[TLDef(0x56E34970)]
	public sealed partial class ReactionsNotifySettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Message reaction notification settings, if not set completely disables notifications/updates about message reactions.</summary>
		[IfFlag(0)] public ReactionNotificationsFrom messages_notify_from;
		/// <summary>Story reaction notification settings, if not set completely disables notifications/updates about reactions to stories.</summary>
		[IfFlag(1)] public ReactionNotificationsFrom stories_notify_from;
		/// <summary><a href="https://corefork.telegram.org/api/ringtones">Notification sound for reactions »</a></summary>
		public NotificationSound sound;
		/// <summary>If false, <a href="https://corefork.telegram.org/api/push-updates">push notifications »</a> about message/story reactions will only be of type <c>REACT_HIDDEN</c>/<c>REACT_STORY_HIDDEN</c>, without any information about the reacted-to story or the reaction itself.</summary>
		public bool show_previews;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="messages_notify_from"/> has a value</summary>
			has_messages_notify_from = 0x1,
			/// <summary>Field <see cref="stories_notify_from"/> has a value</summary>
			has_stories_notify_from = 0x2,
		}
	}

	/// <summary>Describes <a href="https://corefork.telegram.org/api/revenue">channel ad revenue balances »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/broadcastRevenueBalances"/></para></summary>
	[TLDef(0xC3FF71E7)]
	public sealed partial class BroadcastRevenueBalances : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Amount of not-yet-withdrawn cryptocurrency.</summary>
		public long current_balance;
		/// <summary>Amount of withdrawable cryptocurrency, out of the currently available balance (<c>available_balance &lt;= current_balance</c>).</summary>
		public long available_balance;
		/// <summary>Total amount of earned cryptocurrency.</summary>
		public long overall_revenue;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, the available balance can be <a href="https://corefork.telegram.org/api/revenue#withdrawing-revenue">withdrawn »</a>.</summary>
			withdrawal_enabled = 0x1,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/effects">message effect »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/availableEffect"/></para></summary>
	[TLDef(0x93C3E27E)]
	public sealed partial class AvailableEffect : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Unique effect ID.</summary>
		public long id;
		/// <summary>Emoji corresponding to the effect, to be used as icon for the effect if <c>static_icon_id</c> is not set.</summary>
		public string emoticon;
		/// <summary>ID of the document containing the static icon (WEBP) of the effect.</summary>
		[IfFlag(0)] public long static_icon_id;
		/// <summary>Contains the preview <a href="https://corefork.telegram.org/api/stickers#animated-stickers">animation (TGS format »)</a>, used for the effect selection menu.</summary>
		public long effect_sticker_id;
		/// <summary>If set, contains the actual animated effect <a href="https://corefork.telegram.org/api/stickers#animated-stickers">(TGS format »)</a>. If not set, the animated effect must be set equal to the <a href="https://corefork.telegram.org/api/stickers#premium-animated-sticker-effects">premium animated sticker effect</a> associated to the animated sticker specified in <c>effect_sticker_id</c> (always different from the preview animation, fetched thanks to the <see cref="VideoSize"/> of type <c>f</c> as specified <a href="https://corefork.telegram.org/api/stickers#premium-animated-sticker-effects">here »</a>).</summary>
		[IfFlag(1)] public long effect_animation_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="static_icon_id"/> has a value</summary>
			has_static_icon_id = 0x1,
			/// <summary>Field <see cref="effect_animation_id"/> has a value</summary>
			has_effect_animation_id = 0x2,
			/// <summary>Whether a <a href="https://corefork.telegram.org/api/premium">Premium</a> subscription is required to use this effect.</summary>
			premium_required = 0x4,
		}
	}


	/// <summary>Represents a <a href="https://corefork.telegram.org/api/factcheck">fact-check »</a> created by an independent fact-checker.		<para>See <a href="https://corefork.telegram.org/constructor/factCheck"/></para></summary>
	[TLDef(0xB89BFCCF)]
	public sealed partial class FactCheck : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>A two-letter ISO 3166-1 alpha-2 country code of the country for which the fact-check should be shown.</summary>
		[IfFlag(1)] public string country;
		/// <summary>The fact-check.</summary>
		[IfFlag(1)] public TextWithEntities text;
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, the <c>country</c>/<c>text</c> fields will <strong>not</strong> be set, and the fact check must be fetched manually by the client (if it isn't already cached with the key specified in <c>hash</c>) using bundled <see cref="SchemaExtensions.Messages_GetFactCheck">Messages_GetFactCheck</see> requests, when the message with the factcheck scrolls into view.</summary>
			need_check = 0x1,
			/// <summary>Fields <see cref="country"/> and <see cref="text"/> have a value</summary>
			has_country = 0x2,
		}
	}



	/// <summary>A story found using <a href="https://corefork.telegram.org/api/stories#searching-stories">global story search »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/foundStory"/></para></summary>
	[TLDef(0xE87ACBC0)]
	public sealed partial class FoundStory : IObject
	{
		/// <summary>The peer that posted the story.</summary>
		public Peer peer;
		/// <summary>The story.</summary>
		public StoryItemBase story;
	}


	/// <summary>Address optionally associated to a <see cref="GeoPoint"/>.		<para>See <a href="https://corefork.telegram.org/constructor/geoPointAddress"/></para></summary>
	[TLDef(0xDE4C5D93)]
	public sealed partial class GeoPointAddress : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Two-letter ISO 3166-1 alpha-2 country code</summary>
		public string country_iso2;
		/// <summary>State</summary>
		[IfFlag(0)] public string state;
		/// <summary>City</summary>
		[IfFlag(1)] public string city;
		/// <summary>Street</summary>
		[IfFlag(2)] public string street;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="state"/> has a value</summary>
			has_state = 0x1,
			/// <summary>Field <see cref="city"/> has a value</summary>
			has_city = 0x2,
			/// <summary>Field <see cref="street"/> has a value</summary>
			has_street = 0x4,
		}
	}


	
	/// <summary>Represents a report menu or result		<para>See <a href="https://corefork.telegram.org/type/ReportResult"/></para>		<para>Derived classes: <see cref="ReportResultChooseOption"/>, <see cref="ReportResultAddComment"/>, <see cref="ReportResultReported"/></para></summary>
	public abstract partial class ReportResult : IObject { }
	/// <summary>The user must choose one of the following options, and then <see cref="SchemaExtensions.Messages_Report">Messages_Report</see> must be re-invoked, passing the option's <c>option</c> identifier to <see cref="SchemaExtensions.Messages_Report">Messages_Report</see>.<c>option</c>.		<para>See <a href="https://corefork.telegram.org/constructor/reportResultChooseOption"/></para></summary>
	[TLDef(0xF0E4E0B6)]
	public sealed partial class ReportResultChooseOption : ReportResult
	{
		/// <summary>Title of the option popup</summary>
		public string title;
		/// <summary>Available options, rendered as menu entries.</summary>
		public MessageReportOption[] options;
	}
	/// <summary>The user should enter an additional comment for the moderators, and then <see cref="SchemaExtensions.Messages_Report">Messages_Report</see> must be re-invoked, passing the comment to <see cref="SchemaExtensions.Messages_Report">Messages_Report</see>.<c>message</c>.		<para>See <a href="https://corefork.telegram.org/constructor/reportResultAddComment"/></para></summary>
	[TLDef(0x6F09AC31)]
	public sealed partial class ReportResultAddComment : ReportResult
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The <see cref="SchemaExtensions.Messages_Report">Messages_Report</see> method must be re-invoked, passing this option to <c>option</c></summary>
		public byte[] option;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this step can be skipped by the user, passing an empty <c>message</c> to <see cref="SchemaExtensions.Messages_Report">Messages_Report</see>, or if a non-empty <c>message</c> is mandatory.</summary>
			optional = 0x1,
		}
	}
	/// <summary>The report was sent successfully, no further actions are required.		<para>See <a href="https://corefork.telegram.org/constructor/reportResultReported"/></para></summary>
	[TLDef(0x8DB33C4B)]
	public sealed partial class ReportResultReported : ReportResult { }
	


	/// <summary>Info about an <a href="https://corefork.telegram.org/api/bots/referrals#becoming-an-affiliate">active affiliate program we have with a Mini App</a>		<para>See <a href="https://corefork.telegram.org/constructor/connectedBotStarRef"/></para></summary>
	[TLDef(0x19A13F71)]
	public sealed partial class ConnectedBotStarRef : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/links#referral-links">Referral link</a> to be shared</summary>
		public string url;
		/// <summary>When did we affiliate with <c>bot_id</c></summary>
		public DateTime date;
		/// <summary>ID of the mini app that created the affiliate program</summary>
		public long bot_id;
		/// <summary>The number of Telegram Stars received by the affiliate for each 1000 Telegram Stars received by <c>bot_id</c></summary>
		public int commission_permille;
		/// <summary>Number of months the program will be active; if not set, there is no expiration date.</summary>
		[IfFlag(0)] public int duration_months;
		/// <summary>The number of users that used the affiliate program</summary>
		public long participants;
		/// <summary>The number of Telegram Stars that were earned by the affiliate program</summary>
		public long revenue;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="duration_months"/> has a value</summary>
			has_duration_months = 0x1,
			/// <summary>If set, this affiliation was revoked by the affiliate using <see cref="SchemaExtensions.Payments_EditConnectedStarRefBot">Payments_EditConnectedStarRefBot</see>, or by the affiliation program owner using <see cref="SchemaExtensions.Bots_UpdateStarRefProgram">Bots_UpdateStarRefProgram</see></summary>
			revoked = 0x2,
		}
	}
	
	/// <summary><para>See <a href="https://corefork.telegram.org/type/PaidReactionPrivacy"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/paidReactionPrivacyDefault">paidReactionPrivacyDefault</a></remarks>
	public abstract partial class PaidReactionPrivacy : IObject { }
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/paidReactionPrivacyAnonymous"/></para></summary>
	[TLDef(0x1F0C1AD9)]
	public sealed partial class PaidReactionPrivacyAnonymous : PaidReactionPrivacy { }
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/paidReactionPrivacyPeer"/></para></summary>
	[TLDef(0xDC6CFCF0)]
	public sealed partial class PaidReactionPrivacyPeer : PaidReactionPrivacy
	{
		public InputPeer peer;
	}
}
