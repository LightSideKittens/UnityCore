using System;

namespace TL
{
#pragma warning disable CS1574
	/// <summary>Peer		<para>See <a href="https://corefork.telegram.org/type/InputPeer"/></para>		<para>Derived classes: <see cref="InputPeerSelf"/>, <see cref="InputPeerChat"/>, <see cref="InputPeerUser"/>, <see cref="InputPeerChannel"/>, <see cref="InputPeerUserFromMessage"/>, <see cref="InputPeerChannelFromMessage"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputPeerEmpty">inputPeerEmpty</a></remarks>
	public abstract partial class InputPeer : IObject
	{
	}

	/// <summary>Defines the current user.		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerSelf"/></para></summary>
	[TLDef(0x7DA07EC9)]
	public sealed partial class InputPeerSelf : InputPeer
	{
	}

	/// <summary>Defines a chat for further interaction.		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerChat"/></para></summary>
	[TLDef(0x35A95CB9)]
	public sealed partial class InputPeerChat : InputPeer
	{
		/// <summary>Chat identifier</summary>
		public long chat_id;
	}

	/// <summary>Defines a user for further interaction.		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerUser"/></para></summary>
	[TLDef(0xDDE8A54C)]
	public sealed partial class InputPeerUser : InputPeer
	{
		/// <summary>User identifier</summary>
		public long user_id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/><strong>access_hash</strong> value from the <see cref="User"/></summary>
		public long access_hash;
	}

	/// <summary>Defines a channel for further interaction.		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerChannel"/></para></summary>
	[TLDef(0x27BCBBFC)]
	public sealed partial class InputPeerChannel : InputPeer
	{
		/// <summary>Channel identifier</summary>
		public long channel_id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/><strong>access_hash</strong> value from the <see cref="Channel"/></summary>
		public long access_hash;
	}

	/// <summary>Defines a <a href="https://corefork.telegram.org/api/min">min</a> user that was seen in a certain message of a certain chat.		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerUserFromMessage"/></para></summary>
	[TLDef(0xA87B0A1C)]
	public sealed partial class InputPeerUserFromMessage : InputPeer
	{
		/// <summary>The chat where the user was seen</summary>
		public InputPeer peer;

		/// <summary>The message ID</summary>
		public int msg_id;

		/// <summary>The identifier of the user that was seen</summary>
		public long user_id;
	}

	/// <summary>Defines a <a href="https://corefork.telegram.org/api/min">min</a> channel that was seen in a certain message of a certain chat.		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerChannelFromMessage"/></para></summary>
	[TLDef(0xBD2A0840)]
	public sealed partial class InputPeerChannelFromMessage : InputPeer
	{
		/// <summary>The chat where the channel's message was seen</summary>
		public InputPeer peer;

		/// <summary>The message ID</summary>
		public int msg_id;

		/// <summary>The identifier of the channel that was seen</summary>
		public long channel_id;
	}

	/// <summary>Defines a user for subsequent interaction.		<para>See <a href="https://corefork.telegram.org/type/InputUser"/></para>		<para>Derived classes: <see cref="InputUserSelf"/>, <see cref="InputUser"/>, <see cref="InputUserFromMessage"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputUserEmpty">inputUserEmpty</a></remarks>
	public abstract partial class InputUserBase : IObject
	{
	}

	/// <summary>Defines the current user.		<para>See <a href="https://corefork.telegram.org/constructor/inputUserSelf"/></para></summary>
	[TLDef(0xF7C1B13F)]
	public sealed partial class InputUserSelf : InputUserBase
	{
	}

	/// <summary>Defines a user for further interaction.		<para>See <a href="https://corefork.telegram.org/constructor/inputUser"/></para></summary>
	[TLDef(0xF21158C6)]
	public sealed partial class InputUser : InputUserBase
	{
		/// <summary>User identifier</summary>
		public long user_id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/><strong>access_hash</strong> value from the <see cref="User"/></summary>
		public long access_hash;
	}

	/// <summary>Defines a <a href="https://corefork.telegram.org/api/min">min</a> user that was seen in a certain message of a certain chat.		<para>See <a href="https://corefork.telegram.org/constructor/inputUserFromMessage"/></para></summary>
	[TLDef(0x1DA448E2)]
	public sealed partial class InputUserFromMessage : InputUserBase
	{
		/// <summary>The chat where the user was seen</summary>
		public InputPeer peer;

		/// <summary>The message ID</summary>
		public int msg_id;

		/// <summary>The identifier of the user that was seen</summary>
		public long user_id;
	}

	/// <summary>Object defines a contact from the user's phone book.		<para>See <a href="https://corefork.telegram.org/type/InputContact"/></para>		<para>Derived classes: <see cref="InputPhoneContact"/></para></summary>
	public abstract partial class InputContact : IObject
	{
	}

	/// <summary>Phone contact.		<para>See <a href="https://corefork.telegram.org/constructor/inputPhoneContact"/></para></summary>
	[TLDef(0xF392B7F4)]
	public sealed partial class InputPhoneContact : InputContact
	{
		/// <summary>An arbitrary 64-bit integer: it should be set, for example, to an incremental number when using <see cref="SchemaExtensions.Contacts_ImportContacts">Contacts_ImportContacts</see>, in order to retry importing only the contacts that weren't imported successfully, according to the client_ids returned in <see cref="Contacts_ImportedContacts"/>.<c>retry_contacts</c>.</summary>
		public long client_id;

		/// <summary>Phone number</summary>
		public string phone;

		/// <summary>Contact's first name</summary>
		public string first_name;

		/// <summary>Contact's last name</summary>
		public string last_name;
	}

	/// <summary>Defines a file uploaded by the client.		<para>See <a href="https://corefork.telegram.org/type/InputFile"/></para>		<para>Derived classes: <see cref="InputFile"/>, <see cref="InputFileBig"/>, <see cref="InputFileStoryDocument"/></para></summary>
	public abstract partial class InputFileBase : IObject
	{
	}

	/// <summary>Defines a file saved in parts using the method <see cref="SchemaExtensions.Upload_SaveFilePart">Upload_SaveFilePart</see>.		<para>See <a href="https://corefork.telegram.org/constructor/inputFile"/></para></summary>
	[TLDef(0xF52FF27F)]
	public sealed partial class InputFile : InputFileBase
	{
		/// <summary>Random file identifier created by the client</summary>
		public long id;

		/// <summary>Number of parts saved</summary>
		public int parts;

		/// <summary>Full name of the file</summary>
		public string name;

		/// <summary>In case the file's <a href="https://en.wikipedia.org/wiki/MD5#MD5_hashes">md5-hash</a> was passed, contents of the file will be checked prior to use</summary>
		public string md5_checksum;
	}

	/// <summary>Assigns a big file (over 10 MB in size), saved in part using the method <see cref="SchemaExtensions.Upload_SaveBigFilePart">Upload_SaveBigFilePart</see>.		<para>See <a href="https://corefork.telegram.org/constructor/inputFileBig"/></para></summary>
	[TLDef(0xFA4F0BB5)]
	public sealed partial class InputFileBig : InputFileBase
	{
		/// <summary>Random file id, created by the client</summary>
		public long id;

		/// <summary>Number of parts saved</summary>
		public int parts;

		/// <summary>Full file name</summary>
		public string name;
	}

	/// <summary>Used to <a href="https://corefork.telegram.org/api/stories#editing-stories">edit the thumbnail/static preview of a story, see here »</a> for more info on the full flow.		<para>See <a href="https://corefork.telegram.org/constructor/inputFileStoryDocument"/></para></summary>
	[TLDef(0x62DC8B48)]
	public sealed partial class InputFileStoryDocument : InputFileBase
	{
		/// <summary>The old story video.</summary>
		public InputDocument id;
	}

	/// <summary>Defines media content of a message.		<para>See <a href="https://corefork.telegram.org/type/InputMedia"/></para>		<para>Derived classes: <see cref="InputMediaUploadedPhoto"/>, <see cref="InputMediaPhoto"/>, <see cref="InputMediaGeoPoint"/>, <see cref="InputMediaContact"/>, <see cref="InputMediaUploadedDocument"/>, <see cref="InputMediaDocument"/>, <see cref="InputMediaVenue"/>, <see cref="InputMediaPhotoExternal"/>, <see cref="InputMediaDocumentExternal"/>, <see cref="InputMediaGame"/>, <see cref="InputMediaInvoice"/>, <see cref="InputMediaGeoLive"/>, <see cref="InputMediaPoll"/>, <see cref="InputMediaDice"/>, <see cref="InputMediaStory"/>, <see cref="InputMediaWebPage"/>, <see cref="InputMediaPaidMedia"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputMediaEmpty">inputMediaEmpty</a></remarks>
	public abstract partial class InputMedia : IObject
	{
	}

	/// <summary>Photo		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaUploadedPhoto"/></para></summary>
	[TLDef(0x1E287D04)]
	public sealed partial class InputMediaUploadedPhoto : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The <a href="https://corefork.telegram.org/api/files">uploaded file</a></summary>
		public InputFileBase file;

		/// <summary>Attached mask stickers</summary>
		[IfFlag(0)] public InputDocument[] stickers;

		/// <summary>Time to live in seconds of self-destructing photo</summary>
		[IfFlag(1)] public int ttl_seconds;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="stickers"/> has a value</summary>
			has_stickers = 0x1,

			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x2,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x4,
		}
	}

	/// <summary>Forwarded photo		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaPhoto"/></para></summary>
	[TLDef(0xB3BA0635)]
	public sealed partial class InputMediaPhoto : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Photo to be forwarded</summary>
		public InputPhoto id;

		/// <summary>Time to live in seconds of self-destructing photo</summary>
		[IfFlag(0)] public int ttl_seconds;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x1,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x2,
		}
	}

	/// <summary>Map.		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaGeoPoint"/></para></summary>
	[TLDef(0xF9C44144)]
	public sealed partial class InputMediaGeoPoint : InputMedia
	{
		/// <summary>GeoPoint</summary>
		public InputGeoPoint geo_point;
	}

	/// <summary>Phone book contact		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaContact"/></para></summary>
	[TLDef(0xF8AB7DFB)]
	public sealed partial class InputMediaContact : InputMedia
	{
		/// <summary>Phone number</summary>
		public string phone_number;

		/// <summary>Contact's first name</summary>
		public string first_name;

		/// <summary>Contact's last name</summary>
		public string last_name;

		/// <summary>Contact vcard</summary>
		public string vcard;
	}

	/// <summary>New document		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaUploadedDocument"/></para></summary>
	[TLDef(0x037C9330)]
	public sealed partial class InputMediaUploadedDocument : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The <a href="https://corefork.telegram.org/api/files">uploaded file</a></summary>
		public InputFileBase file;

		/// <summary>Thumbnail of the document, uploaded as for the file</summary>
		[IfFlag(2)] public InputFileBase thumb;

		/// <summary>MIME type of document</summary>
		public string mime_type;

		/// <summary>Attributes that specify the type of the document (video, audio, voice, sticker, etc.)</summary>
		public DocumentAttribute[] attributes;

		/// <summary>Attached stickers</summary>
		[IfFlag(0)] public InputDocument[] stickers;

		[IfFlag(6)] public InputPhoto video_cover;
		[IfFlag(7)] public int video_timestamp;

		/// <summary>Time to live in seconds of self-destructing document</summary>
		[IfFlag(1)] public int ttl_seconds;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="stickers"/> has a value</summary>
			has_stickers = 0x1,

			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x2,

			/// <summary>Field <see cref="thumb"/> has a value</summary>
			has_thumb = 0x4,

			/// <summary>Whether to send the file as a video even if it doesn&#39;t have an audio track (i.e. if set, the <see cref="DocumentAttributeAnimated"/> attribute will <strong>not</strong> be set even for videos without audio)</summary>
			nosound_video = 0x8,

			/// <summary>Force the media file to be uploaded as document</summary>
			force_file = 0x10,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x20,

			/// <summary>Field <see cref="video_cover"/> has a value</summary>
			has_video_cover = 0x40,

			/// <summary>Field <see cref="video_timestamp"/> has a value</summary>
			has_video_timestamp = 0x80,
		}
	}

	/// <summary>Forwarded document		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaDocument"/></para></summary>
	[TLDef(0xA8763AB5)]
	public sealed partial class InputMediaDocument : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The document to be forwarded.</summary>
		public InputDocument id;

		[IfFlag(3)] public InputPhoto video_cover;
		[IfFlag(4)] public int video_timestamp;

		/// <summary>Time to live of self-destructing document</summary>
		[IfFlag(0)] public int ttl_seconds;

		/// <summary>Text query or emoji that was used by the user to find this sticker or GIF: used to improve search result relevance.</summary>
		[IfFlag(1)] public string query;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x1,

			/// <summary>Field <see cref="query"/> has a value</summary>
			has_query = 0x2,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x4,

			/// <summary>Field <see cref="video_cover"/> has a value</summary>
			has_video_cover = 0x8,

			/// <summary>Field <see cref="video_timestamp"/> has a value</summary>
			has_video_timestamp = 0x10,
		}
	}

	/// <summary>Can be used to send a venue geolocation.		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaVenue"/></para></summary>
	[TLDef(0xC13D1C11)]
	public sealed partial class InputMediaVenue : InputMedia
	{
		/// <summary>Geolocation</summary>
		public InputGeoPoint geo_point;

		/// <summary>Venue name</summary>
		public string title;

		/// <summary>Physical address of the venue</summary>
		public string address;

		/// <summary>Venue provider: currently only "foursquare" and "gplaces" (Google Places) need to be supported</summary>
		public string provider;

		/// <summary>Venue ID in the provider's database</summary>
		public string venue_id;

		/// <summary>Venue type in the provider's database</summary>
		public string venue_type;
	}

	/// <summary>New photo that will be uploaded by the server using the specified URL		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaPhotoExternal"/></para></summary>
	[TLDef(0xE5BBFE1A)]
	public sealed partial class InputMediaPhotoExternal : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>URL of the photo</summary>
		public string url;

		/// <summary>Self-destruct time to live of photo</summary>
		[IfFlag(0)] public int ttl_seconds;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x1,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x2,
		}
	}

	/// <summary>Document that will be downloaded by the telegram servers		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaDocumentExternal"/></para></summary>
	[TLDef(0x779600F9)]
	public sealed partial class InputMediaDocumentExternal : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>URL of the document</summary>
		public string url;

		/// <summary>Self-destruct time to live of document</summary>
		[IfFlag(0)] public int ttl_seconds;

		[IfFlag(2)] public InputPhoto video_cover;
		[IfFlag(3)] public int video_timestamp;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="ttl_seconds"/> has a value</summary>
			has_ttl_seconds = 0x1,

			/// <summary>Whether this media should be hidden behind a spoiler warning</summary>
			spoiler = 0x2,

			/// <summary>Field <see cref="video_cover"/> has a value</summary>
			has_video_cover = 0x4,

			/// <summary>Field <see cref="video_timestamp"/> has a value</summary>
			has_video_timestamp = 0x8,
		}
	}

	/// <summary>A game		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaGame"/></para></summary>
	[TLDef(0xD33F43F3)]
	public sealed partial class InputMediaGame : InputMedia
	{
		/// <summary>The game to forward</summary>
		public InputGame id;
	}

	/// <summary>Generated invoice of a <a href="https://corefork.telegram.org/bots/payments">bot payment</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaInvoice"/></para></summary>
	[TLDef(0x405FEF0D)]
	public sealed partial class InputMediaInvoice : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Product name, 1-32 characters</summary>
		public string title;

		/// <summary>Product description, 1-255 characters</summary>
		public string description;

		/// <summary>URL of the product photo for the invoice. Can be a photo of the goods or a marketing image for a service. People like it better when they see what they are paying for.</summary>
		[IfFlag(0)] public InputWebDocument photo;

		/// <summary>The actual invoice</summary>
		public Invoice invoice;

		/// <summary>Bot-defined invoice payload, 1-128 bytes. This will not be displayed to the user, use for your internal processes.</summary>
		public byte[] payload;

		/// <summary>Payments provider token, obtained via <a href="https://t.me/botfather">Botfather</a></summary>
		[IfFlag(3)] public string provider;

		/// <summary>JSON-encoded data about the invoice, which will be shared with the payment provider. A detailed description of required fields should be provided by the payment provider.</summary>
		public DataJSON provider_data;

		/// <summary>Unique <a href="https://corefork.telegram.org/api/links#bot-links">bot deep links start parameter</a>. If present, forwarded copies of the sent message will have a URL button with a <a href="https://corefork.telegram.org/api/links#bot-links">deep link</a> to the bot (instead of a Pay button), with the value used as the start parameter. If absent, forwarded copies of the sent message will have a Pay button, allowing multiple users to pay directly from the forwarded message, using the same invoice.</summary>
		[IfFlag(1)] public string start_param;

		/// <summary>Deprecated</summary>
		[IfFlag(2)] public InputMedia extended_media;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x1,

			/// <summary>Field <see cref="start_param"/> has a value</summary>
			has_start_param = 0x2,

			/// <summary>Field <see cref="extended_media"/> has a value</summary>
			has_extended_media = 0x4,

			/// <summary>Field <see cref="provider"/> has a value</summary>
			has_provider = 0x8,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/live-location">Live geolocation</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaGeoLive"/></para></summary>
	[TLDef(0x971FA843)]
	public sealed partial class InputMediaGeoLive : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Current geolocation</summary>
		public InputGeoPoint geo_point;

		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a direction in which the location moves, in degrees; 1-360.</summary>
		[IfFlag(2)] public int heading;

		/// <summary>Validity period of the current location</summary>
		[IfFlag(1)] public int period;

		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a maximum distance to another chat member for proximity alerts, in meters (0-100000)</summary>
		[IfFlag(3)] public int proximity_notification_radius;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether sending of the geolocation was stopped</summary>
			stopped = 0x1,

			/// <summary>Field <see cref="period"/> has a value</summary>
			has_period = 0x2,

			/// <summary>Field <see cref="heading"/> has a value</summary>
			has_heading = 0x4,

			/// <summary>Field <see cref="proximity_notification_radius"/> has a value</summary>
			has_proximity_notification_radius = 0x8,
		}
	}

	/// <summary>A poll		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaPoll"/></para></summary>
	[TLDef(0x0F94E5F1)]
	public sealed partial class InputMediaPoll : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The poll to send</summary>
		public Poll poll;

		/// <summary>Correct answer IDs (for quiz polls)</summary>
		[IfFlag(0)] public byte[][] correct_answers;

		/// <summary>Explanation of quiz solution</summary>
		[IfFlag(1)] public string solution;

		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] solution_entities;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="correct_answers"/> has a value</summary>
			has_correct_answers = 0x1,

			/// <summary>Fields <see cref="solution"/> and <see cref="solution_entities"/> have a value</summary>
			has_solution = 0x2,
		}
	}

	/// <summary>Send a <a href="https://corefork.telegram.org/api/dice">dice-based animated sticker</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaDice"/></para></summary>
	[TLDef(0xE66FBF7B)]
	public sealed partial class InputMediaDice : InputMedia
	{
		/// <summary>The emoji, for now 🏀, 🎲 and 🎯 are supported</summary>
		public string emoticon;
	}

	/// <summary>Forwarded story		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaStory"/></para></summary>
	[TLDef(0x89FDD778)]
	public sealed partial class InputMediaStory : InputMedia
	{
		/// <summary>Peer where the story was posted</summary>
		public InputPeer peer;

		/// <summary>Story ID</summary>
		public int id;
	}

	/// <summary>Specifies options that will be used to generate the link preview for the caption, or even a standalone link preview without an attached message.		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaWebPage"/></para></summary>
	[TLDef(0xC21B8849)]
	public sealed partial class InputMediaWebPage : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The URL to use for the link preview.</summary>
		public string url;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, specifies that a large media preview should be used.</summary>
			force_large_media = 0x1,

			/// <summary>If set, specifies that a small media preview should be used.</summary>
			force_small_media = 0x2,

			/// <summary>If <strong>not</strong> set, a <c>WEBPAGE_NOT_FOUND</c> RPC error will be emitted if a webpage preview cannot be generated for the specified <c>url</c>; otherwise, no error will be emitted (unless the provided message is also empty, in which case a <c>MESSAGE_EMPTY</c> will be emitted, instead).</summary>
			optional = 0x4,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/paid-media">Paid media, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaPaidMedia"/></para></summary>
	[TLDef(0xC4103386)]
	public sealed partial class InputMediaPaidMedia : InputMedia
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The price of the media in <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public long stars_amount;

		/// <summary>Photos or videos.</summary>
		public InputMedia[] extended_media;

		/// <summary>Bots only, specifies a custom payload that will then be passed in <see cref="UpdateBotPurchasedPaidMedia"/> when a payment is made (this field will not be visible to the user)</summary>
		[IfFlag(0)] public string payload;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="payload"/> has a value</summary>
			has_payload = 0x1,
		}
	}

	/// <summary>Defines a new group profile photo.		<para>See <a href="https://corefork.telegram.org/type/InputChatPhoto"/></para>		<para>Derived classes: <see cref="InputChatUploadedPhoto"/>, <see cref="InputChatPhoto"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputChatPhotoEmpty">inputChatPhotoEmpty</a></remarks>
	public abstract partial class InputChatPhotoBase : IObject
	{
	}

	/// <summary>New photo to be set as group profile photo.		<para>See <a href="https://corefork.telegram.org/constructor/inputChatUploadedPhoto"/></para></summary>
	[TLDef(0xBDCDAEC0)]
	public sealed partial class InputChatUploadedPhoto : InputChatPhotoBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>File saved in parts using the method <see cref="SchemaExtensions.Upload_SaveFilePart">Upload_SaveFilePart</see></summary>
		[IfFlag(0)] public InputFileBase file;

		/// <summary>Square video for animated profile picture</summary>
		[IfFlag(1)] public InputFileBase video;

		/// <summary>Floating point UNIX timestamp in seconds, indicating the frame of the video/sticker that should be used as static preview; can only be used if <c>video</c> or <c>video_emoji_markup</c> is set.</summary>
		[IfFlag(2)] public double video_start_ts;

		/// <summary>Animated sticker profile picture, must contain either a <see cref="VideoSizeEmojiMarkup"/> or a <see cref="VideoSizeStickerMarkup"/>.</summary>
		[IfFlag(3)] public VideoSizeBase video_emoji_markup;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="file"/> has a value</summary>
			has_file = 0x1,

			/// <summary>Field <see cref="video"/> has a value</summary>
			has_video = 0x2,

			/// <summary>Field <see cref="video_start_ts"/> has a value</summary>
			has_video_start_ts = 0x4,

			/// <summary>Field <see cref="video_emoji_markup"/> has a value</summary>
			has_video_emoji_markup = 0x8,
		}
	}

	/// <summary>Existing photo to be set as a chat profile photo.		<para>See <a href="https://corefork.telegram.org/constructor/inputChatPhoto"/></para></summary>
	[TLDef(0x8953AD37)]
	public sealed partial class InputChatPhoto : InputChatPhotoBase
	{
		/// <summary>Existing photo</summary>
		public InputPhoto id;
	}

	/// <summary>Defines a GeoPoint by its coordinates.		<para>See <a href="https://corefork.telegram.org/constructor/inputGeoPoint"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputGeoPointEmpty">inputGeoPointEmpty</a></remarks>
	[TLDef(0x48222FAF)]
	public sealed partial class InputGeoPoint : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Latitude</summary>
		public double lat;

		/// <summary>Longitude</summary>
		public double lon;

		/// <summary>The estimated horizontal accuracy of the location, in meters; as defined by the sender.</summary>
		[IfFlag(0)] public int accuracy_radius;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="accuracy_radius"/> has a value</summary>
			has_accuracy_radius = 0x1,
		}
	}

	/// <summary>Defines a photo for further interaction.		<para>See <a href="https://corefork.telegram.org/constructor/inputPhoto"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputPhotoEmpty">inputPhotoEmpty</a></remarks>
	[TLDef(0x3BB3B94A)]
	public sealed partial class InputPhoto : IObject
	{
		/// <summary>Photo identifier</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/><strong>access_hash</strong> value from the <see cref="Photo"/></summary>
		public long access_hash;

		/// <summary><a href="https://corefork.telegram.org/api/file_reference">File reference</a></summary>
		public byte[] file_reference;
	}

	/// <summary>Defines the location of a file for download.		<para>See <a href="https://corefork.telegram.org/type/InputFileLocation"/></para>		<para>Derived classes: <see cref="InputFileLocation"/>, <see cref="InputEncryptedFileLocation"/>, <see cref="InputDocumentFileLocation"/>, <see cref="InputSecureFileLocation"/>, <see cref="InputTakeoutFileLocation"/>, <see cref="InputPhotoFileLocation"/>, <see cref="InputPhotoLegacyFileLocation"/>, <see cref="InputPeerPhotoFileLocation"/>, <see cref="InputStickerSetThumb"/>, <see cref="InputGroupCallStream"/></para></summary>
	public abstract partial class InputFileLocationBase : IObject
	{
	}

	/// <summary>DEPRECATED location of a photo		<para>See <a href="https://corefork.telegram.org/constructor/inputFileLocation"/></para></summary>
	[TLDef(0xDFDAABE1)]
	public sealed partial class InputFileLocation : InputFileLocationBase
	{
		/// <summary>Server volume</summary>
		public long volume_id;

		/// <summary>File identifier</summary>
		public int local_id;

		/// <summary>Check sum to access the file</summary>
		public long secret;

		/// <summary><a href="https://corefork.telegram.org/api/file_reference">File reference</a></summary>
		public byte[] file_reference;
	}

	/// <summary>Location of encrypted secret chat file.		<para>See <a href="https://corefork.telegram.org/constructor/inputEncryptedFileLocation"/></para></summary>
	[TLDef(0xF5235D55)]
	public sealed partial class InputEncryptedFileLocation : InputFileLocationBase
	{
		/// <summary>File ID, <strong>id</strong> parameter value from <see cref="EncryptedFile"/></summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Checksum, <strong>access_hash</strong> parameter value from <see cref="EncryptedFile"/></summary>
		public long access_hash;
	}

	/// <summary>Document location (video, voice, audio, basically every type except photo)		<para>See <a href="https://corefork.telegram.org/constructor/inputDocumentFileLocation"/></para></summary>
	[TLDef(0xBAD07584)]
	public sealed partial class InputDocumentFileLocation : InputFileLocationBase
	{
		/// <summary>Document ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/><strong>access_hash</strong> parameter from the <see cref="Document"/></summary>
		public long access_hash;

		/// <summary><a href="https://corefork.telegram.org/api/file_reference">File reference</a></summary>
		public byte[] file_reference;

		/// <summary>Thumbnail size to download the thumbnail</summary>
		public string thumb_size;
	}

	/// <summary>Location of encrypted telegram <a href="https://corefork.telegram.org/passport">passport</a> file.		<para>See <a href="https://corefork.telegram.org/constructor/inputSecureFileLocation"/></para></summary>
	[TLDef(0xCBC7EE28)]
	public sealed partial class InputSecureFileLocation : InputFileLocationBase
	{
		/// <summary>File ID, <strong>id</strong> parameter value from <see cref="SecureFile"/></summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Checksum, <strong>access_hash</strong> parameter value from <see cref="SecureFile"/></summary>
		public long access_hash;
	}

	/// <summary>Used to download a JSON file that will contain all personal data related to features that do not have a specialized <a href="https://corefork.telegram.org/api/takeout">takeout method</a> yet, see <a href="https://corefork.telegram.org/api/takeout">here »</a> for more info on the takeout API.		<para>See <a href="https://corefork.telegram.org/constructor/inputTakeoutFileLocation"/></para></summary>
	[TLDef(0x29BE5899)]
	public sealed partial class InputTakeoutFileLocation : InputFileLocationBase
	{
	}

	/// <summary>Use this object to download a photo with <see cref="SchemaExtensions.Upload_GetFile">Upload_GetFile</see> method		<para>See <a href="https://corefork.telegram.org/constructor/inputPhotoFileLocation"/></para></summary>
	[TLDef(0x40181FFE)]
	public sealed partial class InputPhotoFileLocation : InputFileLocationBase
	{
		/// <summary>Photo ID, obtained from the <see cref="Photo"/> object</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Photo's access hash, obtained from the <see cref="Photo"/> object</summary>
		public long access_hash;

		/// <summary><a href="https://corefork.telegram.org/api/file_reference">File reference</a></summary>
		public byte[] file_reference;

		/// <summary>The <see cref="PhotoSizeBase"/> to download: must be set to the <c>type</c> field of the desired PhotoSize object of the <see cref="Photo"/></summary>
		public string thumb_size;
	}

	/// <summary>DEPRECATED legacy photo file location		<para>See <a href="https://corefork.telegram.org/constructor/inputPhotoLegacyFileLocation"/></para></summary>
	[TLDef(0xD83466F3)]
	public sealed partial class InputPhotoLegacyFileLocation : InputFileLocationBase
	{
		/// <summary>Photo ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash</summary>
		public long access_hash;

		/// <summary>File reference</summary>
		public byte[] file_reference;

		/// <summary>Volume ID</summary>
		public long volume_id;

		/// <summary>Local ID</summary>
		public int local_id;

		/// <summary>Secret</summary>
		public long secret;
	}

	/// <summary>Location of profile photo of channel/group/supergroup/user		<para>See <a href="https://corefork.telegram.org/constructor/inputPeerPhotoFileLocation"/></para></summary>
	[TLDef(0x37257E99)]
	public sealed partial class InputPeerPhotoFileLocation : InputFileLocationBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The peer whose profile picture should be downloaded</summary>
		public InputPeer peer;

		/// <summary>Photo ID</summary>
		public long photo_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether to download the high-quality version of the picture</summary>
			big = 0x1,
		}
	}

	/// <summary>Location of stickerset thumbnail (see <a href="https://corefork.telegram.org/api/files">files</a>)		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetThumb"/></para></summary>
	[TLDef(0x9D84F3DB)]
	public sealed partial class InputStickerSetThumb : InputFileLocationBase
	{
		/// <summary>Sticker set</summary>
		public InputStickerSet stickerset;

		/// <summary>Thumbnail version</summary>
		public int thumb_version;
	}

	/// <summary>Chunk of a livestream		<para>See <a href="https://corefork.telegram.org/constructor/inputGroupCallStream"/></para></summary>
	[TLDef(0x0598A92A)]
	public sealed partial class InputGroupCallStream : InputFileLocationBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Livestream info</summary>
		public InputGroupCall call;

		/// <summary>Timestamp in milliseconds</summary>
		public long time_ms;

		/// <summary>Specifies the duration of the video segment to fetch in milliseconds, by bitshifting <c>1000</c> to the right <c>scale</c> times: <c>duration_ms := 1000 &gt;&gt; scale</c></summary>
		public int scale;

		/// <summary>Selected video channel</summary>
		[IfFlag(0)] public int video_channel;

		/// <summary>Selected video quality (0 = lowest, 1 = medium, 2 = best)</summary>
		[IfFlag(0)] public int video_quality;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Fields <see cref="video_channel"/> and <see cref="video_quality"/> have a value</summary>
			has_video_channel = 0x1,
		}
	}

	/// <summary>Defines a document for subsequent interaction.		<para>See <a href="https://corefork.telegram.org/constructor/inputDocument"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputDocumentEmpty">inputDocumentEmpty</a></remarks>
	[TLDef(0x1ABFB575)]
	public sealed partial class InputDocument : IObject
	{
		/// <summary>Document ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/><strong>access_hash</strong> parameter from the <see cref="Document"/></summary>
		public long access_hash;

		/// <summary><a href="https://corefork.telegram.org/api/file_reference">File reference</a></summary>
		public byte[] file_reference;
	}

	/// <summary>Privacy <strong>rules</strong> indicate <em>who</em> can or can't do something and are specified by a <see cref="PrivacyRule"/>, and its input counterpart <see cref="InputPrivacyRule"/>.		<para>See <a href="https://corefork.telegram.org/type/InputPrivacyRule"/></para>		<para>Derived classes: <see cref="InputPrivacyValueAllowContacts"/>, <see cref="InputPrivacyValueAllowAll"/>, <see cref="InputPrivacyValueAllowUsers"/>, <see cref="InputPrivacyValueDisallowContacts"/>, <see cref="InputPrivacyValueDisallowAll"/>, <see cref="InputPrivacyValueDisallowUsers"/>, <see cref="InputPrivacyValueAllowChatParticipants"/>, <see cref="InputPrivacyValueDisallowChatParticipants"/>, <see cref="InputPrivacyValueAllowCloseFriends"/>, <see cref="InputPrivacyValueAllowPremium"/>, <see cref="InputPrivacyValueAllowBots"/>, <see cref="InputPrivacyValueDisallowBots"/></para></summary>
	public abstract partial class InputPrivacyRule : IObject
	{
	}

	/// <summary>Allow only contacts		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowContacts"/></para></summary>
	[TLDef(0x0D09E07B)]
	public sealed partial class InputPrivacyValueAllowContacts : InputPrivacyRule
	{
	}

	/// <summary>Allow all users		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowAll"/></para></summary>
	[TLDef(0x184B35CE)]
	public sealed partial class InputPrivacyValueAllowAll : InputPrivacyRule
	{
	}

	/// <summary>Allow only certain users		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowUsers"/></para></summary>
	[TLDef(0x131CC67F)]
	public sealed partial class InputPrivacyValueAllowUsers : InputPrivacyRule
	{
		/// <summary>Allowed users</summary>
		public InputUserBase[] users;
	}

	/// <summary>Disallow only contacts		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueDisallowContacts"/></para></summary>
	[TLDef(0x0BA52007)]
	public sealed partial class InputPrivacyValueDisallowContacts : InputPrivacyRule
	{
	}

	/// <summary>Disallow all		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueDisallowAll"/></para></summary>
	[TLDef(0xD66B66C9)]
	public sealed partial class InputPrivacyValueDisallowAll : InputPrivacyRule
	{
	}

	/// <summary>Disallow only certain users		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueDisallowUsers"/></para></summary>
	[TLDef(0x90110467)]
	public sealed partial class InputPrivacyValueDisallowUsers : InputPrivacyRule
	{
		/// <summary>Users to disallow</summary>
		public InputUserBase[] users;
	}

	/// <summary>Allow only participants of certain chats		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowChatParticipants"/></para></summary>
	[TLDef(0x840649CF)]
	public sealed partial class InputPrivacyValueAllowChatParticipants : InputPrivacyRule
	{
		/// <summary>Allowed chat IDs</summary>
		public long[] chats;
	}

	/// <summary>Disallow only participants of certain chats		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueDisallowChatParticipants"/></para></summary>
	[TLDef(0xE94F0F86)]
	public sealed partial class InputPrivacyValueDisallowChatParticipants : InputPrivacyRule
	{
		/// <summary>Disallowed chat IDs</summary>
		public long[] chats;
	}

	/// <summary>Allow only <a href="https://corefork.telegram.org/api/privacy">close friends »</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowCloseFriends"/></para></summary>
	[TLDef(0x2F453E49)]
	public sealed partial class InputPrivacyValueAllowCloseFriends : InputPrivacyRule
	{
	}

	/// <summary>Allow only users with a <a href="https://corefork.telegram.org/api/premium">Premium subscription »</a>, currently only usable for <see cref="InputPrivacyKey.ChatInvite"/>.		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowPremium"/></para></summary>
	[TLDef(0x77CDC9F1)]
	public sealed partial class InputPrivacyValueAllowPremium : InputPrivacyRule
	{
	}

	/// <summary>Allow bots and mini apps		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueAllowBots"/></para></summary>
	[TLDef(0x5A4FCCE5)]
	public sealed partial class InputPrivacyValueAllowBots : InputPrivacyRule
	{
	}

	/// <summary>Disallow bots and mini apps		<para>See <a href="https://corefork.telegram.org/constructor/inputPrivacyValueDisallowBots"/></para></summary>
	[TLDef(0xC4E57915)]
	public sealed partial class InputPrivacyValueDisallowBots : InputPrivacyRule
	{
	}

	/// <summary>Represents a stickerset		<para>See <a href="https://corefork.telegram.org/type/InputStickerSet"/></para>		<para>Derived classes: <see cref="InputStickerSetID"/>, <see cref="InputStickerSetShortName"/>, <see cref="InputStickerSetAnimatedEmoji"/>, <see cref="InputStickerSetDice"/>, <see cref="InputStickerSetAnimatedEmojiAnimations"/>, <see cref="InputStickerSetPremiumGifts"/>, <see cref="InputStickerSetEmojiGenericAnimations"/>, <see cref="InputStickerSetEmojiDefaultStatuses"/>, <see cref="InputStickerSetEmojiDefaultTopicIcons"/>, <see cref="InputStickerSetEmojiChannelDefaultStatuses"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputStickerSetEmpty">inputStickerSetEmpty</a></remarks>
	public abstract partial class InputStickerSet : IObject
	{
	}

	/// <summary>Stickerset by ID		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetID"/></para></summary>
	[TLDef(0x9DE7A269)]
	public sealed partial class InputStickerSetID : InputStickerSet
	{
		/// <summary>ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash</summary>
		public long access_hash;
	}

	/// <summary>Stickerset by short name, from a <a href="https://corefork.telegram.org/api/links#stickerset-links">stickerset deep link »</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetShortName"/></para></summary>
	[TLDef(0x861CC8A0)]
	public sealed partial class InputStickerSetShortName : InputStickerSet
	{
		/// <summary>Short name from a <a href="https://corefork.telegram.org/api/links#stickerset-links">stickerset deep link »</a></summary>
		public string short_name;
	}

	/// <summary>Animated emojis stickerset		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetAnimatedEmoji"/></para></summary>
	[TLDef(0x028703C8)]
	public sealed partial class InputStickerSetAnimatedEmoji : InputStickerSet
	{
	}

	/// <summary>Used for fetching <a href="https://corefork.telegram.org/api/dice">animated dice stickers</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetDice"/></para></summary>
	[TLDef(0xE67F520E)]
	public sealed partial class InputStickerSetDice : InputStickerSet
	{
		/// <summary>The emoji, for now 🏀, 🎲 and 🎯 are supported</summary>
		public string emoticon;
	}

	/// <summary>Animated emoji reaction stickerset (contains animations to play when a user clicks on a given animated emoji)		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetAnimatedEmojiAnimations"/></para></summary>
	[TLDef(0x0CDE3739)]
	public sealed partial class InputStickerSetAnimatedEmojiAnimations : InputStickerSet
	{
	}

	/// <summary>Stickers to show when receiving a gifted Telegram Premium subscription		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetPremiumGifts"/></para></summary>
	[TLDef(0xC88B3B02)]
	public sealed partial class InputStickerSetPremiumGifts : InputStickerSet
	{
	}

	/// <summary>Generic animation stickerset containing animations to play when <a href="https://corefork.telegram.org/api/reactions">reacting to messages using a normal emoji without a custom animation</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetEmojiGenericAnimations"/></para></summary>
	[TLDef(0x04C4D4CE)]
	public sealed partial class InputStickerSetEmojiGenericAnimations : InputStickerSet
	{
	}

	/// <summary>Default <a href="https://corefork.telegram.org/api/emoji-status">custom emoji status</a> stickerset		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetEmojiDefaultStatuses"/></para></summary>
	[TLDef(0x29D0F5EE)]
	public sealed partial class InputStickerSetEmojiDefaultStatuses : InputStickerSet
	{
	}

	/// <summary>Default <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji</a> stickerset for <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic icons</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetEmojiDefaultTopicIcons"/></para></summary>
	[TLDef(0x44C1F8E9)]
	public sealed partial class InputStickerSetEmojiDefaultTopicIcons : InputStickerSet
	{
	}

	/// <summary>Default <a href="https://corefork.telegram.org/api/emoji-status">custom emoji status</a> stickerset for channel statuses		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetEmojiChannelDefaultStatuses"/></para></summary>
	[TLDef(0x49748553)]
	public sealed partial class InputStickerSetEmojiChannelDefaultStatuses : InputStickerSet
	{
	}

	/// <summary>Button to request a user to <see cref="SchemaExtensions.Messages_AcceptUrlAuth">Messages_AcceptUrlAuth</see> via URL using <a href="https://telegram.org/blog/privacy-discussions-web-bots#meet-seamless-web-bots">Seamless Telegram Login</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputKeyboardButtonUrlAuth"/></para></summary>
	[TLDef(0xD02E7FD4)]
	public sealed partial class InputKeyboardButtonUrlAuth : KeyboardButtonBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Button text</summary>
		public string text;

		/// <summary>New text of the button in forwarded messages.</summary>
		[IfFlag(1)] public string fwd_text;

		/// <summary>An HTTP URL to be opened with user authorization data added to the query string when the button is pressed. If the user refuses to provide authorization data, the original URL without information about the user will be opened. The data added is the same as described in <a href="https://corefork.telegram.org/widgets/login#receiving-authorization-data">Receiving authorization data</a>.<br/>NOTE: You must always check the hash of the received data to verify the authentication and the integrity of the data as described in <a href="https://corefork.telegram.org/widgets/login#checking-authorization">Checking authorization</a>.</summary>
		public string url;

		/// <summary>Username of a bot, which will be used for user authorization. See <a href="https://corefork.telegram.org/widgets/login#setting-up-a-bot">Setting up a bot</a> for more details. If not specified, the current bot's username will be assumed. The url's domain must be the same as the domain linked with the bot. See <a href="https://corefork.telegram.org/widgets/login#linking-your-domain-to-the-bot">Linking your domain to the bot</a> for more details.</summary>
		public InputUserBase bot;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Set this flag to request the permission for your bot to send messages to the user.</summary>
			request_write_access = 0x1,

			/// <summary>Field <see cref="fwd_text"/> has a value</summary>
			has_fwd_text = 0x2,
		}

		/// <summary>Button text</summary>
		public override string Text => text;
	}

	/// <summary>Button that links directly to a user profile		<para>See <a href="https://corefork.telegram.org/constructor/inputKeyboardButtonUserProfile"/></para></summary>
	[TLDef(0xE988037B)]
	public sealed partial class InputKeyboardButtonUserProfile : KeyboardButtonBase
	{
		/// <summary>Button text</summary>
		public string text;

		/// <summary>User ID</summary>
		public InputUserBase user_id;

		/// <summary>Button text</summary>
		public override string Text => text;
	}

	/// <summary>Prompts the user to select and share one or more peers with the bot using <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.		<para>See <a href="https://corefork.telegram.org/constructor/inputKeyboardButtonRequestPeer"/></para></summary>
	[TLDef(0xC9662D05)]
	public sealed partial class InputKeyboardButtonRequestPeer : KeyboardButtonBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Button text</summary>
		public string text;

		/// <summary>Button ID, to be passed to <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.</summary>
		public int button_id;

		/// <summary>Filtering criteria to use for the peer selection list shown to the user. <br/>The list should display all existing peers of the specified type, and should also offer an option for the user to create and immediately use one or more (up to <c>max_quantity</c>) peers of the specified type, if needed.</summary>
		public RequestPeerType peer_type;

		/// <summary>Maximum number of peers that can be chosen.</summary>
		public int max_quantity;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Set this flag to request the peer's name.</summary>
			name_requested = 0x1,

			/// <summary>Set this flag to request the peer's <c>@username</c> (if any).</summary>
			username_requested = 0x2,

			/// <summary>Set this flag to request the peer's photo (if any).</summary>
			photo_requested = 0x4,
		}

		/// <summary>Button text</summary>
		public override string Text => text;
	}

	/// <summary>Represents a channel		<para>See <a href="https://corefork.telegram.org/type/InputChannel"/></para>		<para>Derived classes: <see cref="InputChannel"/>, <see cref="InputChannelFromMessage"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputChannelEmpty">inputChannelEmpty</a></remarks>
	public abstract partial class InputChannelBase : IObject
	{
		/// <summary>Channel ID</summary>
		public abstract long ChannelId { get; set; }
	}

	/// <summary>Represents a channel		<para>See <a href="https://corefork.telegram.org/constructor/inputChannel"/></para></summary>
	[TLDef(0xF35AEC28)]
	public sealed partial class InputChannel : InputChannelBase
	{
		/// <summary>Channel ID</summary>
		public long channel_id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash taken from the <see cref="Channel"/></summary>
		public long access_hash;

		/// <summary>Channel ID</summary>
		public override long ChannelId
		{
			get => channel_id;
			set => channel_id = value;
		}
	}

	/// <summary>Defines a <a href="https://corefork.telegram.org/api/min">min</a> channel that was seen in a certain message of a certain chat.		<para>See <a href="https://corefork.telegram.org/constructor/inputChannelFromMessage"/></para></summary>
	[TLDef(0x5B934F9D)]
	public sealed partial class InputChannelFromMessage : InputChannelBase
	{
		/// <summary>The chat where the channel was seen</summary>
		public InputPeer peer;

		/// <summary>The message ID in the chat where the channel was seen</summary>
		public int msg_id;

		/// <summary>The channel ID</summary>
		public long channel_id;

		/// <summary>The channel ID</summary>
		public override long ChannelId
		{
			get => channel_id;
			set => channel_id = value;
		}
	}

	/// <summary>Represents a sent inline message from the perspective of a bot		<para>See <a href="https://corefork.telegram.org/type/InputBotInlineMessage"/></para>		<para>Derived classes: <see cref="InputBotInlineMessageMediaAuto"/>, <see cref="InputBotInlineMessageText"/>, <see cref="InputBotInlineMessageMediaGeo"/>, <see cref="InputBotInlineMessageMediaVenue"/>, <see cref="InputBotInlineMessageMediaContact"/>, <see cref="InputBotInlineMessageGame"/>, <see cref="InputBotInlineMessageMediaInvoice"/>, <see cref="InputBotInlineMessageMediaWebPage"/></para></summary>
	public abstract partial class InputBotInlineMessage : IObject
	{
	}

	/// <summary>A media		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageMediaAuto"/></para></summary>
	[TLDef(0x3380C786)]
	public sealed partial class InputBotInlineMessageMediaAuto : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Caption</summary>
		public string message;

		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;

		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,

			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,

			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x8,
		}
	}

	/// <summary>Simple text message		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageText"/></para></summary>
	[TLDef(0x3DCD7A87)]
	public sealed partial class InputBotInlineMessageText : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Message</summary>
		public string message;

		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;

		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags]
		public enum Flags : uint
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

	/// <summary>Geolocation		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageMediaGeo"/></para></summary>
	[TLDef(0x96929A85)]
	public sealed partial class InputBotInlineMessageMediaGeo : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Geolocation</summary>
		public InputGeoPoint geo_point;

		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a direction in which the location moves, in degrees; 1-360</summary>
		[IfFlag(0)] public int heading;

		/// <summary>Validity period</summary>
		[IfFlag(1)] public int period;

		/// <summary>For <a href="https://corefork.telegram.org/api/live-location">live locations</a>, a maximum distance to another chat member for proximity alerts, in meters (0-100000)</summary>
		[IfFlag(3)] public int proximity_notification_radius;

		/// <summary>Reply markup for bot/inline keyboards</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags]
		public enum Flags : uint
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

	/// <summary>Venue		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageMediaVenue"/></para></summary>
	[TLDef(0x417BBF11)]
	public sealed partial class InputBotInlineMessageMediaVenue : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Geolocation</summary>
		public InputGeoPoint geo_point;

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

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
		}
	}

	/// <summary>A contact		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageMediaContact"/></para></summary>
	[TLDef(0xA6EDBFFD)]
	public sealed partial class InputBotInlineMessageMediaContact : InputBotInlineMessage
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

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
		}
	}

	/// <summary>A game		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageGame"/></para></summary>
	[TLDef(0x4B425864)]
	public sealed partial class InputBotInlineMessageGame : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
		}
	}

	/// <summary>An invoice		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageMediaInvoice"/></para></summary>
	[TLDef(0xD7E78225)]
	public sealed partial class InputBotInlineMessageMediaInvoice : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Product name, 1-32 characters</summary>
		public string title;

		/// <summary>Product description, 1-255 characters</summary>
		public string description;

		/// <summary>Invoice photo</summary>
		[IfFlag(0)] public InputWebDocument photo;

		/// <summary>The invoice</summary>
		public Invoice invoice;

		/// <summary>Bot-defined invoice payload, 1-128 bytes. This will not be displayed to the user, use for your internal processes.</summary>
		public byte[] payload;

		/// <summary>Payments provider token, obtained via <a href="https://t.me/botfather">Botfather</a></summary>
		public string provider;

		/// <summary>A JSON-serialized object for data about the invoice, which will be shared with the payment provider. A detailed description of the required fields should be provided by the payment provider.</summary>
		public DataJSON provider_data;

		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x1,

			/// <summary>Field <see cref="reply_markup"/> has a value</summary>
			has_reply_markup = 0x4,
		}
	}

	/// <summary>Specifies options that will be used to generate the link preview for the message, or even a standalone link preview without an attached message.		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageMediaWebPage"/></para></summary>
	[TLDef(0xBDDCC510)]
	public sealed partial class InputBotInlineMessageMediaWebPage : InputBotInlineMessage
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The message, can be empty.</summary>
		public string message;

		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;

		/// <summary>The URL to use for the link preview.</summary>
		public string url;

		/// <summary>Inline keyboard</summary>
		[IfFlag(2)] public ReplyMarkup reply_markup;

		[Flags]
		public enum Flags : uint
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

			/// <summary>If <strong>not</strong> set, a <c>WEBPAGE_NOT_FOUND</c> RPC error will be emitted if a webpage preview cannot be generated for the specified <c>url</c>; otherwise, no error will be emitted (unless the provided message is also empty, in which case a <c>MESSAGE_EMPTY</c> will be emitted, instead).</summary>
			optional = 0x40,
		}
	}

	/// <summary>Inline bot result		<para>See <a href="https://corefork.telegram.org/type/InputBotInlineResult"/></para>		<para>Derived classes: <see cref="InputBotInlineResult"/>, <see cref="InputBotInlineResultPhoto"/>, <see cref="InputBotInlineResultDocument"/>, <see cref="InputBotInlineResultGame"/></para></summary>
	public abstract partial class InputBotInlineResultBase : IObject
	{
		/// <summary>ID of result</summary>
		public abstract string ID { get; set; }

		/// <summary>Message to send when the result is selected</summary>
		public abstract InputBotInlineMessage SendMessage { get; set; }
	}

	/// <summary>An inline bot result		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineResult"/></para></summary>
	[TLDef(0x88BF9319)]
	public sealed partial class InputBotInlineResult : InputBotInlineResultBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>ID of result</summary>
		public string id;

		/// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
		public string type;

		/// <summary>Result title</summary>
		[IfFlag(1)] public string title;

		/// <summary>Result description</summary>
		[IfFlag(2)] public string description;

		/// <summary>URL of result</summary>
		[IfFlag(3)] public string url;

		/// <summary>Thumbnail for result</summary>
		[IfFlag(4)] public InputWebDocument thumb;

		/// <summary>Result contents</summary>
		[IfFlag(5)] public InputWebDocument content;

		/// <summary>Message to send when the result is selected</summary>
		public InputBotInlineMessage send_message;

		[Flags]
		public enum Flags : uint
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

		/// <summary>ID of result</summary>
		public override string ID
		{
			get => id;
			set => id = value;
		}

		/// <summary>Message to send when the result is selected</summary>
		public override InputBotInlineMessage SendMessage
		{
			get => send_message;
			set => send_message = value;
		}
	}

	/// <summary>Photo		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineResultPhoto"/></para></summary>
	[TLDef(0xA8D864A7)]
	public sealed partial class InputBotInlineResultPhoto : InputBotInlineResultBase
	{
		/// <summary>Result ID</summary>
		public string id;

		/// <summary>Result type (see <a href="https://corefork.telegram.org/bots/api#inlinequeryresult">bot API docs</a>)</summary>
		public string type;

		/// <summary>Photo to send</summary>
		public InputPhoto photo;

		/// <summary>Message to send when the result is selected</summary>
		public InputBotInlineMessage send_message;

		/// <summary>Result ID</summary>
		public override string ID
		{
			get => id;
			set => id = value;
		}

		/// <summary>Message to send when the result is selected</summary>
		public override InputBotInlineMessage SendMessage
		{
			get => send_message;
			set => send_message = value;
		}
	}

	/// <summary>Document (media of any type except for photos)		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineResultDocument"/></para></summary>
	[TLDef(0xFFF8FDC4)]
	public sealed partial class InputBotInlineResultDocument : InputBotInlineResultBase
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

		/// <summary>Document to send</summary>
		public InputDocument document;

		/// <summary>Message to send when the result is selected</summary>
		public InputBotInlineMessage send_message;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x2,

			/// <summary>Field <see cref="description"/> has a value</summary>
			has_description = 0x4,
		}

		/// <summary>Result ID</summary>
		public override string ID
		{
			get => id;
			set => id = value;
		}

		/// <summary>Message to send when the result is selected</summary>
		public override InputBotInlineMessage SendMessage
		{
			get => send_message;
			set => send_message = value;
		}
	}

	/// <summary>Game		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineResultGame"/></para></summary>
	[TLDef(0x4FA417F2)]
	public sealed partial class InputBotInlineResultGame : InputBotInlineResultBase
	{
		/// <summary>Result ID</summary>
		public string id;

		/// <summary>Game short name</summary>
		public string short_name;

		/// <summary>Message to send when the result is selected</summary>
		public InputBotInlineMessage send_message;

		/// <summary>Result ID</summary>
		public override string ID
		{
			get => id;
			set => id = value;
		}

		/// <summary>Message to send when the result is selected</summary>
		public override InputBotInlineMessage SendMessage
		{
			get => send_message;
			set => send_message = value;
		}
	}

	/// <summary>Represents a sent inline message from the perspective of a bot		<para>See <a href="https://corefork.telegram.org/type/InputBotInlineMessageID"/></para>		<para>Derived classes: <see cref="InputBotInlineMessageID"/>, <see cref="InputBotInlineMessageID64"/></para></summary>
	public abstract partial class InputBotInlineMessageIDBase : IObject
	{
		/// <summary>DC ID to use when working with this inline message</summary>
		public abstract int DcId { get; set; }

		/// <summary>Access hash of message</summary>
		public abstract long AccessHash { get; set; }
	}

	/// <summary>Represents a sent inline message from the perspective of a bot (legacy constructor)		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageID"/></para></summary>
	[TLDef(0x890C3D89)]
	public sealed partial class InputBotInlineMessageID : InputBotInlineMessageIDBase
	{
		/// <summary>DC ID to use when working with this inline message</summary>
		public int dc_id;

		/// <summary>ID of message, contains both the (32-bit, legacy) owner ID and the message ID, used only for Bot API backwards compatibility with 32-bit user ID.</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash of message</summary>
		public long access_hash;

		/// <summary>DC ID to use when working with this inline message</summary>
		public override int DcId
		{
			get => dc_id;
			set => dc_id = value;
		}

		/// <summary>Access hash of message</summary>
		public override long AccessHash
		{
			get => access_hash;
			set => access_hash = value;
		}
	}

	/// <summary>Represents a sent inline message from the perspective of a bot		<para>See <a href="https://corefork.telegram.org/constructor/inputBotInlineMessageID64"/></para></summary>
	[TLDef(0xB6D915D7)]
	public sealed partial class InputBotInlineMessageID64 : InputBotInlineMessageIDBase
	{
		/// <summary>DC ID to use when working with this inline message</summary>
		public int dc_id;

		/// <summary>ID of the owner of this message</summary>
		public long owner_id;

		/// <summary>ID of message</summary>
		public int id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash of message</summary>
		public long access_hash;

		/// <summary>DC ID to use when working with this inline message</summary>
		public override int DcId
		{
			get => dc_id;
			set => dc_id = value;
		}

		/// <summary>Access hash of message</summary>
		public override long AccessHash
		{
			get => access_hash;
			set => access_hash = value;
		}
	}

	/// <summary>Represents a media with attached stickers		<para>See <a href="https://corefork.telegram.org/type/InputStickeredMedia"/></para>		<para>Derived classes: <see cref="InputStickeredMediaPhoto"/>, <see cref="InputStickeredMediaDocument"/></para></summary>
	public abstract partial class InputStickeredMedia : IObject
	{
	}

	/// <summary>A photo with stickers attached		<para>See <a href="https://corefork.telegram.org/constructor/inputStickeredMediaPhoto"/></para></summary>
	[TLDef(0x4A992157)]
	public sealed partial class InputStickeredMediaPhoto : InputStickeredMedia
	{
		/// <summary>The photo</summary>
		public InputPhoto id;
	}

	/// <summary>A document with stickers attached		<para>See <a href="https://corefork.telegram.org/constructor/inputStickeredMediaDocument"/></para></summary>
	[TLDef(0x0438865B)]
	public sealed partial class InputStickeredMediaDocument : InputStickeredMedia
	{
		/// <summary>The document</summary>
		public InputDocument id;
	}

	/// <summary>A game to send		<para>See <a href="https://corefork.telegram.org/type/InputGame"/></para>		<para>Derived classes: <see cref="InputGameID"/>, <see cref="InputGameShortName"/></para></summary>
	public abstract partial class InputGame : IObject
	{
	}

	/// <summary>Indicates an already sent game		<para>See <a href="https://corefork.telegram.org/constructor/inputGameID"/></para></summary>
	[TLDef(0x032C3E77)]
	public sealed partial class InputGameID : InputGame
	{
		/// <summary>game ID from <see cref="Game"/> constructor</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>access hash from <see cref="Game"/> constructor</summary>
		public long access_hash;
	}

	/// <summary>Game by short name		<para>See <a href="https://corefork.telegram.org/constructor/inputGameShortName"/></para></summary>
	[TLDef(0xC331E80A)]
	public sealed partial class InputGameShortName : InputGame
	{
		/// <summary>The bot that provides the game</summary>
		public InputUserBase bot_id;

		/// <summary>The game's short name, usually obtained from a <a href="https://corefork.telegram.org/api/links#game-links">game link »</a></summary>
		public string short_name;
	}

	/// <summary>The document		<para>See <a href="https://corefork.telegram.org/constructor/inputWebDocument"/></para></summary>
	[TLDef(0x9BED434D)]
	public sealed partial class InputWebDocument : IObject
	{
		/// <summary>Remote document URL to be downloaded using the appropriate <a href="https://corefork.telegram.org/api/files">method</a></summary>
		public string url;

		/// <summary>Remote file size</summary>
		public int size;

		/// <summary>Mime type</summary>
		public string mime_type;

		/// <summary>Attributes for media types</summary>
		public DocumentAttribute[] attributes;
	}

	/// <summary>Location of remote file		<para>See <a href="https://corefork.telegram.org/type/InputWebFileLocation"/></para>		<para>Derived classes: <see cref="InputWebFileLocation"/>, <see cref="InputWebFileGeoPointLocation"/>, <see cref="InputWebFileAudioAlbumThumbLocation"/></para></summary>
	public abstract partial class InputWebFileLocationBase : IObject
	{
	}

	/// <summary>Location of a remote HTTP(s) file		<para>See <a href="https://corefork.telegram.org/constructor/inputWebFileLocation"/></para></summary>
	[TLDef(0xC239D686)]
	public sealed partial class InputWebFileLocation : InputWebFileLocationBase
	{
		/// <summary>HTTP URL of file</summary>
		public string url;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash</summary>
		public long access_hash;
	}

	/// <summary>Used to download a server-generated image with the map preview from a <see cref="GeoPoint"/>, see the <a href="https://corefork.telegram.org/api/files#downloading-webfiles">webfile docs for more info »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputWebFileGeoPointLocation"/></para></summary>
	[TLDef(0x9F2221C9)]
	public sealed partial class InputWebFileGeoPointLocation : InputWebFileLocationBase
	{
		/// <summary>Generated from the <c>lat</c>, <c>long</c> and <c>accuracy_radius</c> parameters of the <see cref="GeoPoint"/></summary>
		public InputGeoPoint geo_point;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash of the <see cref="GeoPoint"/></summary>
		public long access_hash;

		/// <summary>Map width in pixels before applying scale; 16-1024</summary>
		public int w;

		/// <summary>Map height in pixels before applying scale; 16-1024</summary>
		public int h;

		/// <summary>Map zoom level; 13-20</summary>
		public int zoom;

		/// <summary>Map scale; 1-3</summary>
		public int scale;
	}

	/// <summary>Used to download an album cover for any music file using <see cref="SchemaExtensions.Upload_GetWebFile">Upload_GetWebFile</see>, see the <a href="https://corefork.telegram.org/api/files#downloading-webfiles">webfile docs for more info »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputWebFileAudioAlbumThumbLocation"/></para></summary>
	[TLDef(0xF46FE924)]
	public sealed partial class InputWebFileAudioAlbumThumbLocation : InputWebFileLocationBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The audio file in question: must NOT be provided in secret chats, provide the <c>title</c> and <c>performer</c> fields instead.</summary>
		[IfFlag(0)] public InputDocument document;

		/// <summary>Song title: should only be used in secret chats, in normal chats provide <c>document</c> instead, as it has more lax rate limits.</summary>
		[IfFlag(1)] public string title;

		/// <summary>Song performer: should only be used in secret chats, in normal chats provide <c>document</c> instead, as it has more lax rate limits.</summary>
		[IfFlag(1)] public string performer;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="document"/> has a value</summary>
			has_document = 0x1,

			/// <summary>Fields <see cref="title"/> and <see cref="performer"/> have a value</summary>
			has_title = 0x2,

			/// <summary>Used to return a thumbnail with <c>100x100</c> resolution (instead of the default <c>600x600</c>)</summary>
			small = 0x4,
		}
	}

	/// <summary>Payment credentials		<para>See <a href="https://corefork.telegram.org/type/InputPaymentCredentials"/></para>		<para>Derived classes: <see cref="InputPaymentCredentialsSaved"/>, <see cref="InputPaymentCredentials"/>, <see cref="InputPaymentCredentialsApplePay"/>, <see cref="InputPaymentCredentialsGooglePay"/></para></summary>
	public abstract partial class InputPaymentCredentialsBase : IObject
	{
	}

	/// <summary>Saved payment credentials		<para>See <a href="https://corefork.telegram.org/constructor/inputPaymentCredentialsSaved"/></para></summary>
	[TLDef(0xC10EB2CF)]
	public sealed partial class InputPaymentCredentialsSaved : InputPaymentCredentialsBase
	{
		/// <summary>Credential ID</summary>
		public string id;

		/// <summary>Temporary password</summary>
		public byte[] tmp_password;
	}

	/// <summary>Payment credentials		<para>See <a href="https://corefork.telegram.org/constructor/inputPaymentCredentials"/></para></summary>
	[TLDef(0x3417D728)]
	public sealed partial class InputPaymentCredentials : InputPaymentCredentialsBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Payment credentials</summary>
		public DataJSON data;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Save payment credential for future use</summary>
			save = 0x1,
		}
	}

	/// <summary>Apple pay payment credentials		<para>See <a href="https://corefork.telegram.org/constructor/inputPaymentCredentialsApplePay"/></para></summary>
	[TLDef(0x0AA1C39F)]
	public sealed partial class InputPaymentCredentialsApplePay : InputPaymentCredentialsBase
	{
		/// <summary>Payment data</summary>
		public DataJSON payment_data;
	}

	/// <summary>Google Pay payment credentials		<para>See <a href="https://corefork.telegram.org/constructor/inputPaymentCredentialsGooglePay"/></para></summary>
	[TLDef(0x8AC32801)]
	public sealed partial class InputPaymentCredentialsGooglePay : InputPaymentCredentialsBase
	{
		/// <summary>Payment token</summary>
		public DataJSON payment_token;
	}

	/// <summary>Sticker in a stickerset		<para>See <a href="https://corefork.telegram.org/constructor/inputStickerSetItem"/></para></summary>
	[TLDef(0x32DA9E9C)]
	public sealed partial class InputStickerSetItem : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The sticker</summary>
		public InputDocument document;

		/// <summary>Associated emoji</summary>
		public string emoji;

		/// <summary>Coordinates for mask sticker</summary>
		[IfFlag(0)] public MaskCoords mask_coords;

		/// <summary>Set of keywords, separated by commas (can't be provided for mask stickers)</summary>
		[IfFlag(1)] public string keywords;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="mask_coords"/> has a value</summary>
			has_mask_coords = 0x1,

			/// <summary>Field <see cref="keywords"/> has a value</summary>
			has_keywords = 0x2,
		}
	}

	/// <summary>Phone call		<para>See <a href="https://corefork.telegram.org/constructor/inputPhoneCall"/></para></summary>
	[TLDef(0x1E36FDED)]
	public sealed partial class InputPhoneCall : IObject
	{
		/// <summary>Call ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash</summary>
		public long access_hash;
	}

	/// <summary>A single media in an <a href="https://corefork.telegram.org/api/files#albums-grouped-media">album or grouped media</a> sent with <see cref="SchemaExtensions.Messages_SendMultiMedia">Messages_SendMultiMedia</see>.		<para>See <a href="https://corefork.telegram.org/constructor/inputSingleMedia"/></para></summary>
	[TLDef(0x1CC6E91F)]
	public sealed partial class InputSingleMedia : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The media</summary>
		public InputMedia media;

		/// <summary>Unique client media ID required to prevent message resending</summary>
		public long random_id;

		/// <summary>A caption for the media</summary>
		public string message;

		/// <summary>Message <a href="https://corefork.telegram.org/api/entities">entities</a> for styled text</summary>
		[IfFlag(0)] public MessageEntity[] entities;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x1,
		}
	}

	/// <summary>A message		<para>See <a href="https://corefork.telegram.org/type/InputMessage"/></para>		<para>Derived classes: <see cref="InputMessageID"/>, <see cref="InputMessageReplyTo"/>, <see cref="InputMessagePinned"/>, <see cref="InputMessageCallbackQuery"/></para></summary>
	public abstract partial class InputMessage : IObject
	{
	}

	/// <summary>Message by ID		<para>See <a href="https://corefork.telegram.org/constructor/inputMessageID"/></para></summary>
	[TLDef(0xA676A322)]
	public sealed partial class InputMessageID : InputMessage
	{
		/// <summary>Message ID</summary>
		public int id;
	}

	/// <summary>Message to which the specified message replies to		<para>See <a href="https://corefork.telegram.org/constructor/inputMessageReplyTo"/></para></summary>
	[TLDef(0xBAD88395)]
	public sealed partial class InputMessageReplyTo : InputMessage
	{
		/// <summary>ID of the message that replies to the message we need</summary>
		public int id;
	}

	/// <summary>Pinned message		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagePinned"/></para></summary>
	[TLDef(0x86872538)]
	public sealed partial class InputMessagePinned : InputMessage
	{
	}

	/// <summary>Used by bots for fetching information about the message that originated a callback query		<para>See <a href="https://corefork.telegram.org/constructor/inputMessageCallbackQuery"/></para></summary>
	[TLDef(0xACFA1A7E)]
	public sealed partial class InputMessageCallbackQuery : InputMessage
	{
		/// <summary>Message ID</summary>
		public int id;

		/// <summary>Callback query ID</summary>
		public long query_id;
	}

	/// <summary>Peer, or all peers in a certain folder		<para>See <a href="https://corefork.telegram.org/type/InputDialogPeer"/></para>		<para>Derived classes: <see cref="InputDialogPeer"/>, <see cref="InputDialogPeerFolder"/></para></summary>
	public abstract partial class InputDialogPeerBase : IObject
	{
	}

	/// <summary>A peer		<para>See <a href="https://corefork.telegram.org/constructor/inputDialogPeer"/></para></summary>
	[TLDef(0xFCAAFEB7)]
	public sealed partial class InputDialogPeer : InputDialogPeerBase
	{
		/// <summary>Peer</summary>
		public InputPeer peer;
	}

	/// <summary>All peers in a <a href="https://corefork.telegram.org/api/folders#peer-folders">peer folder</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputDialogPeerFolder"/></para></summary>
	[TLDef(0x64600527)]
	public sealed partial class InputDialogPeerFolder : InputDialogPeerBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public int folder_id;
	}

	/// <summary>Info about an <a href="https://corefork.telegram.org/mtproto/mtproto-transports#transport-obfuscation">MTProxy</a> used to connect.		<para>See <a href="https://corefork.telegram.org/constructor/inputClientProxy"/></para></summary>
	[TLDef(0x75588B3F)]
	public sealed partial class InputClientProxy : IObject
	{
		/// <summary>Proxy address</summary>
		public string address;

		/// <summary>Proxy port</summary>
		public int port;
	}

	/// <summary>Secure <a href="https://corefork.telegram.org/passport">passport</a> file, for more info <a href="https://corefork.telegram.org/passport/encryption#inputsecurefile">see the passport docs »</a>		<para>See <a href="https://corefork.telegram.org/type/InputSecureFile"/></para>		<para>Derived classes: <see cref="InputSecureFileUploaded"/>, <see cref="InputSecureFile"/></para></summary>
	public abstract partial class InputSecureFileBase : IObject
	{
		/// <summary>Secure file ID</summary>
		public abstract long ID { get; set; }
	}

	/// <summary>Uploaded secure file, for more info <a href="https://corefork.telegram.org/passport/encryption#inputsecurefile">see the passport docs »</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputSecureFileUploaded"/></para></summary>
	[TLDef(0x3334B0F0)]
	public sealed partial class InputSecureFileUploaded : InputSecureFileBase
	{
		/// <summary>Secure file ID</summary>
		public long id;

		/// <summary>Secure file part count</summary>
		public int parts;

		/// <summary>MD5 hash of encrypted uploaded file, to be checked server-side</summary>
		public string md5_checksum;

		/// <summary>File hash</summary>
		public byte[] file_hash;

		/// <summary>Secret</summary>
		public byte[] secret;

		/// <summary>Secure file ID</summary>
		public override long ID
		{
			get => id;
			set => id = value;
		}
	}

	/// <summary>Pre-uploaded <a href="https://corefork.telegram.org/passport">passport</a> file, for more info <a href="https://corefork.telegram.org/passport/encryption#inputsecurefile">see the passport docs »</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputSecureFile"/></para></summary>
	[TLDef(0x5367E5BE)]
	public sealed partial class InputSecureFile : InputSecureFileBase
	{
		/// <summary>Secure file ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Secure file access hash</summary>
		public long access_hash;

		/// <summary>Secure file ID</summary>
		public override long ID
		{
			get => id;
			set => id = value;
		}
	}

	/// <summary>Secure value, <a href="https://corefork.telegram.org/passport/encryption#encryption">for more info see the passport docs »</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputSecureValue"/></para></summary>
	[TLDef(0xDB21D0A7)]
	public sealed partial class InputSecureValue : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Secure <a href="https://corefork.telegram.org/passport">passport</a> value type</summary>
		public SecureValueType type;

		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">Telegram Passport</a> element data</summary>
		[IfFlag(0)] public SecureData data;

		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">passport</a> file with the front side of the document</summary>
		[IfFlag(1)] public InputSecureFileBase front_side;

		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">passport</a> file with the reverse side of the document</summary>
		[IfFlag(2)] public InputSecureFileBase reverse_side;

		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">passport</a> file with a selfie of the user holding the document</summary>
		[IfFlag(3)] public InputSecureFileBase selfie;

		/// <summary>Array of encrypted <a href="https://corefork.telegram.org/passport">passport</a> files with translated versions of the provided documents</summary>
		[IfFlag(6)] public InputSecureFileBase[] translation;

		/// <summary>Array of encrypted <a href="https://corefork.telegram.org/passport">passport</a> files with photos the of the documents</summary>
		[IfFlag(4)] public InputSecureFileBase[] files;

		/// <summary>Plaintext verified <a href="https://corefork.telegram.org/passport">passport</a> data</summary>
		[IfFlag(5)] public SecurePlainData plain_data;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="data"/> has a value</summary>
			has_data = 0x1,

			/// <summary>Field <see cref="front_side"/> has a value</summary>
			has_front_side = 0x2,

			/// <summary>Field <see cref="reverse_side"/> has a value</summary>
			has_reverse_side = 0x4,

			/// <summary>Field <see cref="selfie"/> has a value</summary>
			has_selfie = 0x8,

			/// <summary>Field <see cref="files"/> has a value</summary>
			has_files = 0x10,

			/// <summary>Field <see cref="plain_data"/> has a value</summary>
			has_plain_data = 0x20,

			/// <summary>Field <see cref="translation"/> has a value</summary>
			has_translation = 0x40,
		}
	}

	/// <summary>Constructor for checking the validity of a 2FA SRP password (see <a href="https://corefork.telegram.org/api/srp">SRP</a>)		<para>See <a href="https://corefork.telegram.org/constructor/inputCheckPasswordSRP"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputCheckPasswordEmpty">inputCheckPasswordEmpty</a></remarks>
	[TLDef(0xD27FF082)]
	public sealed partial class InputCheckPasswordSRP : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/srp">SRP ID</a></summary>
		public long srp_id;

		/// <summary><c>A</c> parameter (see <a href="https://corefork.telegram.org/api/srp">SRP</a>)</summary>
		public byte[] A;

		/// <summary><c>M1</c> parameter (see <a href="https://corefork.telegram.org/api/srp">SRP</a>)</summary>
		public byte[] M1;
	}

	/// <summary>Event that occurred in the application.		<para>See <a href="https://corefork.telegram.org/constructor/inputAppEvent"/></para></summary>
	[TLDef(0x1D1B1245)]
	public sealed partial class InputAppEvent : IObject
	{
		/// <summary>Client's exact timestamp for the event</summary>
		public double time;

		/// <summary>Type of event</summary>
		public string type;

		/// <summary>Arbitrary numeric value for more convenient selection of certain event types, or events referring to a certain object</summary>
		public long peer;

		/// <summary>Details of the event</summary>
		public JSONValue data;
	}

	/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a>		<para>See <a href="https://corefork.telegram.org/type/InputWallPaper"/></para>		<para>Derived classes: <see cref="InputWallPaper"/>, <see cref="InputWallPaperSlug"/>, <see cref="InputWallPaperNoFile"/></para></summary>
	public abstract partial class InputWallPaperBase : IObject
	{
	}

	/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputWallPaper"/></para></summary>
	[TLDef(0xE630B979)]
	public sealed partial class InputWallPaper : InputWallPaperBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a> ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash</summary>
		public long access_hash;
	}

	/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a> by slug (a unique ID, obtained from a <a href="https://corefork.telegram.org/api/links#wallpaper-links">wallpaper link »</a>)		<para>See <a href="https://corefork.telegram.org/constructor/inputWallPaperSlug"/></para></summary>
	[TLDef(0x72091C80)]
	public sealed partial class InputWallPaperSlug : InputWallPaperBase
	{
		/// <summary>Unique wallpaper ID</summary>
		public string slug;
	}

	/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a> with no file access hash, used for example when deleting (<c>unsave=true</c>) wallpapers using <see cref="SchemaExtensions.Account_SaveWallPaper">Account_SaveWallPaper</see>, specifying just the wallpaper ID.		<para>See <a href="https://corefork.telegram.org/constructor/inputWallPaperNoFile"/></para></summary>
	[TLDef(0x967A462E)]
	public sealed partial class InputWallPaperNoFile : InputWallPaperBase
	{
		/// <summary>Wallpaper ID</summary>
		public long id;
	}

	/// <summary>Peer in a folder		<para>See <a href="https://corefork.telegram.org/constructor/inputFolderPeer"/></para></summary>
	[TLDef(0xFBD2C296)]
	public sealed partial class InputFolderPeer : IObject
	{
		/// <summary>Peer</summary>
		public InputPeer peer;

		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public int folder_id;
	}

	/// <summary>Cloud theme		<para>See <a href="https://corefork.telegram.org/type/InputTheme"/></para>		<para>Derived classes: <see cref="InputTheme"/>, <see cref="InputThemeSlug"/></para></summary>
	public abstract partial class InputThemeBase : IObject
	{
	}

	/// <summary>Theme		<para>See <a href="https://corefork.telegram.org/constructor/inputTheme"/></para></summary>
	[TLDef(0x3C5693E9)]
	public sealed partial class InputTheme : InputThemeBase
	{
		/// <summary>ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash</summary>
		public long access_hash;
	}

	/// <summary>Theme by theme ID		<para>See <a href="https://corefork.telegram.org/constructor/inputThemeSlug"/></para></summary>
	[TLDef(0xF5890DF1)]
	public sealed partial class InputThemeSlug : InputThemeBase
	{
		/// <summary>Unique theme ID obtained from a <a href="https://corefork.telegram.org/api/links#theme-links">theme deep link »</a></summary>
		public string slug;
	}

	/// <summary>Theme settings		<para>See <a href="https://corefork.telegram.org/constructor/inputThemeSettings"/></para></summary>
	[TLDef(0x8FDE504F)]
	public sealed partial class InputThemeSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Default theme on which this theme is based</summary>
		public BaseTheme base_theme;

		/// <summary>Accent color, ARGB format</summary>
		public int accent_color;

		/// <summary>Accent color of outgoing messages in ARGB format</summary>
		[IfFlag(3)] public int outbox_accent_color;

		/// <summary>The fill to be used as a background for outgoing messages, in RGB24 format. <br/>If just one or two equal colors are provided, describes a solid fill of a background. <br/>If two different colors are provided, describes the top and bottom colors of a 0-degree gradient.<br/>If three or four colors are provided, describes a freeform gradient fill of a background.</summary>
		[IfFlag(0)] public int[] message_colors;

		/// <summary><see cref="InputWallPaper"/> or <see cref="InputWallPaper">inputWallPaperSlug</see> when passing wallpaper files for <a href="https://corefork.telegram.org/api/wallpapers#image-wallpapers">image</a> or <a href="https://corefork.telegram.org/api/wallpapers#pattern-wallpapers">pattern</a> wallpapers, <see cref="InputWallPaperNoFile"/> with <c>id=0</c> otherwise.</summary>
		[IfFlag(1)] public InputWallPaperBase wallpaper;

		/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a> settings.</summary>
		[IfFlag(1)] public WallPaperSettings wallpaper_settings;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="message_colors"/> has a value</summary>
			has_message_colors = 0x1,

			/// <summary>Fields <see cref="wallpaper"/> and <see cref="wallpaper_settings"/> have a value</summary>
			has_wallpaper = 0x2,

			/// <summary>If set, the freeform gradient fill needs to be animated on every sent message</summary>
			message_colors_animated = 0x4,

			/// <summary>Field <see cref="outbox_accent_color"/> has a value</summary>
			has_outbox_accent_color = 0x8,
		}
	}

	/// <summary>Points to a specific group call		<para>See <a href="https://corefork.telegram.org/constructor/inputGroupCall"/></para></summary>
	[TLDef(0xD8AA840F)]
	public sealed partial class InputGroupCall : IObject
	{
		/// <summary>Group call ID</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Group call access hash</summary>
		public long access_hash;
	}

	/// <summary>An invoice		<para>See <a href="https://corefork.telegram.org/type/InputInvoice"/></para>		<para>Derived classes: <see cref="InputInvoiceMessage"/>, <see cref="InputInvoiceSlug"/>, <see cref="InputInvoicePremiumGiftCode"/>, <see cref="InputInvoiceStars"/>, <see cref="InputInvoiceChatInviteSubscription"/>, <see cref="InputInvoiceStarGift"/></para></summary>
	public abstract partial class InputInvoice : IObject
	{
	}

	/// <summary>An invoice contained in a <see cref="MessageMediaInvoice"/> message or <a href="https://corefork.telegram.org/api/paid-media">paid media »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceMessage"/></para></summary>
	[TLDef(0xC5B56859)]
	public sealed partial class InputInvoiceMessage : InputInvoice
	{
		/// <summary>Chat where the invoice/paid media was sent</summary>
		public InputPeer peer;

		/// <summary>Message ID</summary>
		public int msg_id;
	}

	/// <summary>An invoice slug taken from an <a href="https://corefork.telegram.org/api/links#invoice-links">invoice deep link</a> or from the <a href="https://corefork.telegram.org/api/config#premium-invoice-slug"><c>premium_invoice_slug</c> app config parameter »</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceSlug"/></para></summary>
	[TLDef(0xC326CAEF)]
	public sealed partial class InputInvoiceSlug : InputInvoice
	{
		/// <summary>The invoice slug</summary>
		public string slug;
	}

	/// <summary>Used if the user wishes to start a channel/supergroup <a href="https://corefork.telegram.org/api/giveaways">giveaway</a> or send some <a href="https://corefork.telegram.org/api/giveaways">giftcodes</a> to members of a channel/supergroup, in exchange for <a href="https://corefork.telegram.org/api/boost">boosts</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputInvoicePremiumGiftCode"/></para></summary>
	[TLDef(0x98986C0D)]
	public sealed partial class InputInvoicePremiumGiftCode : InputInvoice
	{
		/// <summary>Should be populated with <see cref="InputStorePaymentPremiumGiveaway"/> for <a href="https://corefork.telegram.org/api/giveaways">giveaways</a> and <see cref="InputStorePaymentPremiumGiftCode"/> for <a href="https://corefork.telegram.org/api/giveaways">gifts</a>.</summary>
		public InputStorePaymentPurpose purpose;

		/// <summary>Should be populated with one of the giveaway options returned by <see cref="SchemaExtensions.Payments_GetPremiumGiftCodeOptions">Payments_GetPremiumGiftCodeOptions</see>, see the <a href="https://corefork.telegram.org/api/giveaways">giveaways »</a> documentation for more info.</summary>
		public PremiumGiftCodeOption option;
	}

	/// <summary>Used to top up the <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> balance of the current account or someone else's account, or to start a <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">Telegram Star giveaway »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceStars"/></para></summary>
	[TLDef(0x65F00CE3)]
	public sealed partial class InputInvoiceStars : InputInvoice
	{
		/// <summary>An <see cref="InputStorePaymentStarsGiveaway"/>, <see cref="InputStorePaymentStarsTopup"/> or <see cref="InputStorePaymentStarsGift"/>.</summary>
		public InputStorePaymentPurpose purpose;
	}

	/// <summary>Used to pay for a <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceChatInviteSubscription"/></para></summary>
	[TLDef(0x34E793F1)]
	public sealed partial class InputInvoiceChatInviteSubscription : InputInvoice
	{
		/// <summary>The <a href="https://corefork.telegram.org/api/stars#star-subscriptions">invitation link of the Telegram Star subscription »</a></summary>
		public string hash;
	}

	/// <summary>Used to buy a <a href="https://corefork.telegram.org/api/gifts">Telegram Star Gift, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceStarGift"/></para></summary>
	[TLDef(0xE8625E92)]
	public sealed partial class InputInvoiceStarGift : InputInvoice
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		public InputPeer peer;

		/// <summary>Identifier of the gift, from <see cref="StarGift"/>.<c>id</c></summary>
		public long gift_id;

		/// <summary>Optional message, attached with the gift. <br/>The maximum length for this field is specified in the <a href="https://corefork.telegram.org/api/config#stargifts-message-length-max">stargifts_message_length_max client configuration value »</a>.</summary>
		[IfFlag(1)] public TextWithEntities message;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, your name will be hidden if the destination user decides to display the gift on their profile (they will still see that you sent the gift)</summary>
			hide_name = 0x1,

			/// <summary>Field <see cref="message"/> has a value</summary>
			has_message = 0x2,
			include_upgrade = 0x4,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceStarGiftUpgrade"/></para></summary>
	[TLDef(0x4D818D5D)]
	public sealed partial class InputInvoiceStarGiftUpgrade : InputInvoice
	{
		public Flags flags;
		public InputSavedStarGift stargift;

		[Flags]
		public enum Flags : uint
		{
			keep_original_details = 0x1,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/inputInvoiceStarGiftTransfer"/></para></summary>
	[TLDef(0x4A5F5BD9)]
	public sealed partial class InputInvoiceStarGiftTransfer : InputInvoice
	{
		public InputSavedStarGift stargift;
		public InputPeer to_id;
	}

	/// <summary>Info about a Telegram Premium purchase		<para>See <a href="https://corefork.telegram.org/type/InputStorePaymentPurpose"/></para>		<para>Derived classes: <see cref="InputStorePaymentPremiumSubscription"/>, <see cref="InputStorePaymentGiftPremium"/>, <see cref="InputStorePaymentPremiumGiftCode"/>, <see cref="InputStorePaymentPremiumGiveaway"/>, <see cref="InputStorePaymentStarsTopup"/>, <see cref="InputStorePaymentStarsGift"/>, <see cref="InputStorePaymentStarsGiveaway"/></para></summary>
	public abstract partial class InputStorePaymentPurpose : IObject
	{
	}

	/// <summary>Info about a Telegram Premium purchase		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentPremiumSubscription"/></para></summary>
	[TLDef(0xA6751E66)]
	public sealed partial class InputStorePaymentPremiumSubscription : InputStorePaymentPurpose
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Pass true if this is a restore of a Telegram Premium purchase; only for the App Store</summary>
			restore = 0x1,

			/// <summary>Pass true if this is an upgrade from a monthly subscription to a yearly subscription; only for App Store</summary>
			upgrade = 0x2,
		}
	}

	/// <summary>Info about a gifted Telegram Premium purchase		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentGiftPremium"/></para></summary>
	[TLDef(0x616F7FE8)]
	public sealed partial class InputStorePaymentGiftPremium : InputStorePaymentPurpose
	{
		/// <summary>The user to which the Telegram Premium subscription was gifted</summary>
		public InputUserBase user_id;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Price of the product in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;
	}

	/// <summary>Used to gift <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscriptions only to some specific subscribers of a channel/supergroup or to some of our contacts, see <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on giveaways and gifts.		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentPremiumGiftCode"/></para></summary>
	[TLDef(0xFB790393)]
	public sealed partial class InputStorePaymentPremiumGiftCode : InputStorePaymentPurpose
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The users that will receive the <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> subscriptions.</summary>
		public InputUserBase[] users;

		/// <summary>If set, the gifts will be sent on behalf of a channel/supergroup we are an admin of, which will also assign some <a href="https://corefork.telegram.org/api/boost">boosts</a> to it. Otherwise, the gift will be sent directly from the currently logged in user, and we will gain some extra <a href="https://corefork.telegram.org/api/boost">boost slots</a>. See <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on giveaways and gifts.</summary>
		[IfFlag(0)] public InputPeer boost_peer;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		/// <summary>Message attached with the gift</summary>
		[IfFlag(1)] public TextWithEntities message;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="boost_peer"/> has a value</summary>
			has_boost_peer = 0x1,

			/// <summary>Field <see cref="message"/> has a value</summary>
			has_message = 0x2,
		}
	}

	/// <summary>Used to pay for a <a href="https://corefork.telegram.org/api/giveaways">giveaway, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentPremiumGiveaway"/></para></summary>
	[TLDef(0x160544CA)]
	public sealed partial class InputStorePaymentPremiumGiveaway : InputStorePaymentPurpose
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The channel/supergroup starting the giveaway, that the user must join to participate, that will receive the giveaway <a href="https://corefork.telegram.org/api/boost">boosts</a>; see <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on giveaways.</summary>
		public InputPeer boost_peer;

		/// <summary>Additional channels that the user must join to participate to the giveaway can be specified here.</summary>
		[IfFlag(1)] public InputPeer[] additional_peers;

		/// <summary>The set of users that can participate to the giveaway can be restricted by passing here an explicit whitelist of up to <a href="https://corefork.telegram.org/api/config#giveaway-countries-max">giveaway_countries_max</a> countries, specified as two-letter ISO 3166-1 alpha-2 country codes.</summary>
		[IfFlag(2)] public string[] countries_iso2;

		/// <summary>Can contain a textual description of additional giveaway prizes.</summary>
		[IfFlag(4)] public string prize_description;

		/// <summary>Random ID to avoid resending the giveaway</summary>
		public long random_id;

		/// <summary>The end date of the giveaway, must be at most <a href="https://corefork.telegram.org/api/config#giveaway-period-max">giveaway_period_max</a> seconds in the future; see <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on giveaways.</summary>
		public DateTime until_date;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, only new subscribers starting from the giveaway creation date will be able to participate to the giveaway.</summary>
			only_new_subscribers = 0x1,

			/// <summary>Field <see cref="additional_peers"/> has a value</summary>
			has_additional_peers = 0x2,

			/// <summary>Field <see cref="countries_iso2"/> has a value</summary>
			has_countries_iso2 = 0x4,

			/// <summary>If set, giveaway winners are public and will be listed in a <see cref="MessageMediaGiveawayResults"/> message that will be automatically sent to the channel once the giveaway ends.</summary>
			winners_are_visible = 0x8,

			/// <summary>Field <see cref="prize_description"/> has a value</summary>
			has_prize_description = 0x10,
		}
	}

	/// <summary>Used to top up the <a href="https://corefork.telegram.org/api/stars">Telegram Stars balance</a> of the current account.		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentStarsTopup"/></para></summary>
	[TLDef(0xDDDD0F56)]
	public sealed partial class InputStorePaymentStarsTopup : InputStorePaymentPurpose
	{
		/// <summary>Amount of stars to topup</summary>
		public long stars;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;
	}

	/// <summary>Used to gift <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a> to a friend.		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentStarsGift"/></para></summary>
	[TLDef(0x1D741EF7)]
	public sealed partial class InputStorePaymentStarsGift : InputStorePaymentPurpose
	{
		/// <summary>The user to which the stars should be gifted.</summary>
		public InputUserBase user_id;

		/// <summary>Amount of stars to gift</summary>
		public long stars;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;
	}

	/// <summary>Used to pay for a <a href="https://corefork.telegram.org/api/giveaways#star-giveaways">star giveaway, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/inputStorePaymentStarsGiveaway"/></para></summary>
	[TLDef(0x751F08FA)]
	public sealed partial class InputStorePaymentStarsGiveaway : InputStorePaymentPurpose
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Total number of Telegram Stars being given away (each user will receive <c>stars/users</c> stars).</summary>
		public long stars;

		/// <summary>The channel/supergroup starting the giveaway, that the user must join to participate, that will receive the giveaway <a href="https://corefork.telegram.org/api/boost">boosts</a>; see <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on giveaways.</summary>
		public InputPeer boost_peer;

		/// <summary>Additional channels that the user must join to participate to the giveaway can be specified here.</summary>
		[IfFlag(1)] public InputPeer[] additional_peers;

		/// <summary>The set of users that can participate to the giveaway can be restricted by passing here an explicit whitelist of up to <a href="https://corefork.telegram.org/api/config#giveaway-countries-max">giveaway_countries_max</a> countries, specified as two-letter ISO 3166-1 alpha-2 country codes.</summary>
		[IfFlag(2)] public string[] countries_iso2;

		/// <summary>Can contain a textual description of additional giveaway prizes.</summary>
		[IfFlag(4)] public string prize_description;

		/// <summary>Random ID to avoid resending the giveaway</summary>
		public long random_id;

		/// <summary>The end date of the giveaway, must be at most <a href="https://corefork.telegram.org/api/config#giveaway-period-max">giveaway_period_max</a> seconds in the future; see <a href="https://corefork.telegram.org/api/giveaways">here »</a> for more info on giveaways.</summary>
		public DateTime until_date;

		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code</summary>
		public string currency;

		/// <summary>Total price in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long amount;

		/// <summary>Number of winners.</summary>
		public int users;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, only new subscribers starting from the giveaway creation date will be able to participate to the giveaway.</summary>
			only_new_subscribers = 0x1,

			/// <summary>Field <see cref="additional_peers"/> has a value</summary>
			has_additional_peers = 0x2,

			/// <summary>Field <see cref="countries_iso2"/> has a value</summary>
			has_countries_iso2 = 0x4,

			/// <summary>If set, giveaway winners are public and will be listed in a <see cref="MessageMediaGiveawayResults"/> message that will be automatically sent to the channel once the giveaway ends.</summary>
			winners_are_visible = 0x8,

			/// <summary>Field <see cref="prize_description"/> has a value</summary>
			has_prize_description = 0x10,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/inputEmojiStatusCollectible"/></para></summary>
	[TLDef(0x07141DBF)]
	public sealed partial class InputEmojiStatusCollectible : EmojiStatusBase
	{
		public Flags flags;
		public long collectible_id;
		[IfFlag(0)] public DateTime until;

		[Flags]
		public enum Flags : uint
		{
			has_until = 0x1,
		}

		public override DateTime Until => until;
	}

	/// <summary>Used to fetch information about a <a href="https://corefork.telegram.org/api/bots/webapps#direct-link-mini-apps">direct link Mini App</a>		<para>See <a href="https://corefork.telegram.org/type/InputBotApp"/></para>		<para>Derived classes: <see cref="InputBotAppID"/>, <see cref="InputBotAppShortName"/></para></summary>
	public abstract partial class InputBotApp : IObject
	{
	}

	/// <summary>Used to fetch information about a <a href="https://corefork.telegram.org/api/bots/webapps#direct-link-mini-apps">direct link Mini App</a> by its ID		<para>See <a href="https://corefork.telegram.org/constructor/inputBotAppID"/></para></summary>
	[TLDef(0xA920BD7A)]
	public sealed partial class InputBotAppID : InputBotApp
	{
		/// <summary><a href="https://corefork.telegram.org/api/bots/webapps#direct-link-mini-apps">direct link Mini App</a> ID.</summary>
		public long id;

		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Access hash, obtained from the <see cref="BotApp"/>.</summary>
		public long access_hash;
	}

	/// <summary>Used to fetch information about a <a href="https://corefork.telegram.org/api/bots/webapps#direct-link-mini-apps">direct link Mini App</a> by its short name		<para>See <a href="https://corefork.telegram.org/constructor/inputBotAppShortName"/></para></summary>
	[TLDef(0x908C0407)]
	public sealed partial class InputBotAppShortName : InputBotApp
	{
		/// <summary>ID of the bot that owns the bot mini app</summary>
		public InputUserBase bot_id;

		/// <summary>Short name, obtained from a <a href="https://corefork.telegram.org/api/links#direct-mini-app-links">Direct Mini App deep link</a></summary>
		public string short_name;
	}

	/// <summary>Represents a folder		<para>See <a href="https://corefork.telegram.org/type/InputChatlist"/></para>		<para>Derived classes: <see cref="InputChatlistDialogFilter"/></para></summary>
	public abstract partial class InputChatlist : IObject
	{
	}

	/// <summary>Folder ID		<para>See <a href="https://corefork.telegram.org/constructor/inputChatlistDialogFilter"/></para></summary>
	[TLDef(0xF3E0DA33)]
	public sealed partial class InputChatlistDialogFilter : InputChatlist
	{
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> ID</summary>
		public int filter_id;
	}

	/// <summary>Contains info about a message or story to reply to.		<para>See <a href="https://corefork.telegram.org/type/InputReplyTo"/></para>		<para>Derived classes: <see cref="InputReplyToMessage"/>, <see cref="InputReplyToStory"/></para></summary>
	public abstract partial class InputReplyTo : IObject
	{
	}

	/// <summary>Reply to a message.		<para>See <a href="https://corefork.telegram.org/constructor/inputReplyToMessage"/></para></summary>
	[TLDef(0x22C0F6D5)]
	public sealed partial class InputReplyToMessage : InputReplyTo
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>The message ID to reply to.</summary>
		public int reply_to_msg_id;

		/// <summary>This field must contain the topic ID <strong>only</strong> when replying to messages in forum topics different from the "General" topic (i.e. <c>reply_to_msg_id</c> is set and <c>reply_to_msg_id != topicID</c> and <c>topicID != 1</c>).  <br/>If the replied-to message is deleted before the method finishes execution, the value in this field will be used to send the message to the correct topic, instead of the "General" topic.</summary>
		[IfFlag(0)] public int top_msg_id;

		/// <summary>Used to reply to messages sent to another chat (specified here), can only be used for non-<c>protected</c> chats and messages.</summary>
		[IfFlag(1)] public InputPeer reply_to_peer_id;

		/// <summary>Used to quote-reply to only a certain section (specified here) of the original message. The maximum UTF-8 length for quotes is specified in the <a href="https://corefork.telegram.org/api/config#quote-length-max">quote_length_max</a> config key.</summary>
		[IfFlag(2)] public string quote_text;

		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a> from the <c>quote_text</c> field.</summary>
		[IfFlag(3)] public MessageEntity[] quote_entities;

		/// <summary>Offset of the message <c>quote_text</c> within the original message (in <a href="https://corefork.telegram.org/api/entities#entity-length">UTF-16 code units</a>).</summary>
		[IfFlag(4)] public int quote_offset;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="top_msg_id"/> has a value</summary>
			has_top_msg_id = 0x1,

			/// <summary>Field <see cref="reply_to_peer_id"/> has a value</summary>
			has_reply_to_peer_id = 0x2,

			/// <summary>Field <see cref="quote_text"/> has a value</summary>
			has_quote_text = 0x4,

			/// <summary>Field <see cref="quote_entities"/> has a value</summary>
			has_quote_entities = 0x8,

			/// <summary>Field <see cref="quote_offset"/> has a value</summary>
			has_quote_offset = 0x10,
		}
	}

	/// <summary>Reply to a story.		<para>See <a href="https://corefork.telegram.org/constructor/inputReplyToStory"/></para></summary>
	[TLDef(0x5881323A)]
	public sealed partial class InputReplyToStory : InputReplyTo
	{
		/// <summary>Sender of the story</summary>
		public InputPeer peer;

		/// <summary>ID of the story to reply to.</summary>
		public int story_id;
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories#media-areas">location tag</a> attached to a <a href="https://corefork.telegram.org/api/stories">story</a>, with additional venue information.		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaAreaVenue"/></para></summary>
	[TLDef(0xB282217F)]
	public sealed partial class InputMediaAreaVenue : MediaArea
	{
		/// <summary>The size and location of the media area corresponding to the location sticker on top of the story media.</summary>
		public MediaAreaCoordinates coordinates;

		/// <summary>The <c>query_id</c> from <see cref="Messages_BotResults"/>, see <a href="https://corefork.telegram.org/api/stories#media-areas">here »</a> for more info.</summary>
		public long query_id;

		/// <summary>The <c>id</c> of the chosen result, see <a href="https://corefork.telegram.org/api/stories#media-areas">here »</a> for more info.</summary>
		public string result_id;
	}

	/// <summary>Represents a channel post		<para>See <a href="https://corefork.telegram.org/constructor/inputMediaAreaChannelPost"/></para></summary>
	[TLDef(0x2271F2BF)]
	public sealed partial class InputMediaAreaChannelPost : MediaArea
	{
		/// <summary>The size and location of the media area corresponding to the location sticker on top of the story media.</summary>
		public MediaAreaCoordinates coordinates;

		/// <summary>The channel that posted the message</summary>
		public InputChannelBase channel;

		/// <summary>ID of the channel message</summary>
		public int msg_id;
	}

	/// <summary>Specifies the chats that <strong>can</strong> receive Telegram Business <a href="https://corefork.telegram.org/api/business#away-messages">away »</a> and <a href="https://corefork.telegram.org/api/business#greeting-messages">greeting »</a> messages.		<para>See <a href="https://corefork.telegram.org/constructor/inputBusinessRecipients"/></para></summary>
	[TLDef(0x6F8B32AA)]
	public sealed partial class InputBusinessRecipients : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Only private chats with the specified users.</summary>
		[IfFlag(4)] public InputUserBase[] users;

		[Flags]
		public enum Flags : uint
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

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/business#greeting-messages">Telegram Business greeting</a>, automatically sent to new users writing to us in private for the first time, or after a certain inactivity period.		<para>See <a href="https://corefork.telegram.org/constructor/inputBusinessGreetingMessage"/></para></summary>
	[TLDef(0x0194CB3B)]
	public sealed partial class InputBusinessGreetingMessage : IObject
	{
		/// <summary>ID of a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shorcut, containing the greeting messages to send, see here » for more info</a>.</summary>
		public int shortcut_id;

		/// <summary>Allowed recipients for the greeting messages.</summary>
		public InputBusinessRecipients recipients;

		/// <summary>The number of days after which a private chat will be considered as inactive; currently, must be one of 7, 14, 21, or 28.</summary>
		public int no_activity_days;
	}

	/// <summary>Describes a <a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away message</a>, automatically sent to users writing to us when we're offline, during closing hours, while we're on vacation, or in some other custom time period when we cannot immediately answer to the user.		<para>See <a href="https://corefork.telegram.org/constructor/inputBusinessAwayMessage"/></para></summary>
	[TLDef(0x832175E0)]
	public sealed partial class InputBusinessAwayMessage : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>ID of a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shorcut, containing the away messages to send, see here » for more info</a>.</summary>
		public int shortcut_id;

		/// <summary>Specifies when should the away messages be sent.</summary>
		public BusinessAwayMessageSchedule schedule;

		/// <summary>Allowed recipients for the away messages.</summary>
		public InputBusinessRecipients recipients;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, the messages will not be sent if the account was online in the last 10 minutes.</summary>
			offline_only = 0x1,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut »</a>.		<para>See <a href="https://corefork.telegram.org/type/InputQuickReplyShortcut"/></para>		<para>Derived classes: <see cref="InputQuickReplyShortcut"/>, <see cref="InputQuickReplyShortcutId"/></para></summary>
	public abstract partial class InputQuickReplyShortcutBase : IObject
	{
	}

	/// <summary>Selects a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut</a> by name.		<para>See <a href="https://corefork.telegram.org/constructor/inputQuickReplyShortcut"/></para></summary>
	[TLDef(0x24596D41)]
	public sealed partial class InputQuickReplyShortcut : InputQuickReplyShortcutBase
	{
		/// <summary>Shortcut name.</summary>
		public string shortcut;
	}

	/// <summary>Selects a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut</a> by its numeric ID.		<para>See <a href="https://corefork.telegram.org/constructor/inputQuickReplyShortcutId"/></para></summary>
	[TLDef(0x01190CF1)]
	public sealed partial class InputQuickReplyShortcutId : InputQuickReplyShortcutBase
	{
		/// <summary>Shortcut ID.</summary>
		public int shortcut_id;
	}

	/// <summary><a href="https://corefork.telegram.org/api/business#business-introduction">Telegram Business introduction »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputBusinessIntro"/></para></summary>
	[TLDef(0x09C469CD)]
	public sealed partial class InputBusinessIntro : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Title of the introduction message</summary>
		public string title;

		/// <summary>Profile introduction</summary>
		public string description;

		/// <summary>Optional introduction <a href="https://corefork.telegram.org/api/stickers">sticker</a>.</summary>
		[IfFlag(0)] public InputDocument sticker;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="sticker"/> has a value</summary>
			has_sticker = 0x1,
		}
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/fragment">Fragment collectible »</a>.		<para>See <a href="https://corefork.telegram.org/type/InputCollectible"/></para>		<para>Derived classes: <see cref="InputCollectibleUsername"/>, <see cref="InputCollectiblePhone"/></para></summary>
	public abstract partial class InputCollectible : IObject
	{
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/fragment">username fragment collectible</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputCollectibleUsername"/></para></summary>
	[TLDef(0xE39460A9)]
	public sealed partial class InputCollectibleUsername : InputCollectible
	{
		/// <summary>Username</summary>
		public string username;
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/fragment">phone number fragment collectible</a>		<para>See <a href="https://corefork.telegram.org/constructor/inputCollectiblePhone"/></para></summary>
	[TLDef(0xA2E214A4)]
	public sealed partial class InputCollectiblePhone : InputCollectible
	{
		/// <summary>Phone number</summary>
		public string phone;
	}

	/// <summary>Specifies the private chats that a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business bot »</a> may interact with.		<para>See <a href="https://corefork.telegram.org/constructor/inputBusinessBotRecipients"/></para></summary>
	[TLDef(0xC4E5921E)]
	public sealed partial class InputBusinessBotRecipients : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Explicitly selected private chats.</summary>
		[IfFlag(4)] public InputUserBase[] users;

		/// <summary>Identifiers of private chats that are always excluded.</summary>
		[IfFlag(6)] public InputUserBase[] exclude_users;

		[Flags]
		public enum Flags : uint
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

			/// <summary>If set, then all private chats <em>except</em> the ones selected by <c>existing_chats</c>, <c>new_chats</c>, <c>contacts</c>, <c>non_contacts</c> and <c>users</c> are chosen. <br/>Note that if this flag is set, any values passed in <c>exclude_users</c> will be merged and moved into <c>users</c> by the server.</summary>
			exclude_selected = 0x20,

			/// <summary>Field <see cref="exclude_users"/> has a value</summary>
			has_exclude_users = 0x40,
		}
	}

	/// <summary>Contains info about a <a href="https://corefork.telegram.org/api/business#business-chat-links">business chat deep link »</a> to be created by the current account.		<para>See <a href="https://corefork.telegram.org/constructor/inputBusinessChatLink"/></para></summary>
	[TLDef(0x11679FA7)]
	public sealed partial class InputBusinessChatLink : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Message to pre-fill in the message input field.</summary>
		public string message;

		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(0)] public MessageEntity[] entities;

		/// <summary>Human-readable name of the link, to simplify management in the UI (only visible to the creator of the link).</summary>
		[IfFlag(1)] public string title;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x1,

			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x2,
		}
	}

	/// <summary>Used to fetch info about a <a href="https://corefork.telegram.org/api/stars#balance-and-transaction-history">Telegram Star transaction »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputStarsTransaction"/></para></summary>
	[TLDef(0x206AE6D1)]
	public sealed partial class InputStarsTransaction : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Transaction ID.</summary>
		public string id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, fetches info about the refund transaction for this transaction.</summary>
			refund = 0x1,
		}
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/type/InputSavedStarGift"/></para></summary>
	public abstract partial class InputSavedStarGift : IObject
	{
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/inputSavedStarGiftUser"/></para></summary>
	[TLDef(0x69279795)]
	public sealed partial class InputSavedStarGiftUser : InputSavedStarGift
	{
		public int msg_id;
	}

	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/inputSavedStarGiftChat"/></para></summary>
	[TLDef(0xF101AA7F)]
	public sealed partial class InputSavedStarGiftChat : InputSavedStarGift
	{
		public InputPeer peer;
		public long saved_id;
	}
}