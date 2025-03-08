using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary><a href="https://instantview.telegram.org">Instant View</a> webpage preview		<para>See <a href="https://corefork.telegram.org/type/WebPage"/></para>		<para>Derived classes: <see cref="WebPageEmpty"/>, <see cref="WebPagePending"/>, <see cref="WebPage"/>, <see cref="WebPageNotModified"/></para></summary>
    public abstract partial class WebPageBase : IObject
    {
        /// <summary>Preview ID</summary>
        public virtual long ID => default;
        /// <summary>URL of the webpage.</summary>
        public virtual string Url => default;
    }
    
    /// <summary>No preview is available for the webpage		<para>See <a href="https://corefork.telegram.org/constructor/webPageEmpty"/></para></summary>
	[TLDef(0x211A1788)]
	public sealed partial class WebPageEmpty : WebPageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Preview ID</summary>
		public long id;
		/// <summary>URL of the webpage.</summary>
		[IfFlag(0)] public string url;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="url"/> has a value</summary>
			has_url = 0x1,
		}

		/// <summary>Preview ID</summary>
		public override long ID => id;
		/// <summary>URL of the webpage.</summary>
		public override string Url => url;
	}
	/// <summary>A preview of the webpage is currently being generated		<para>See <a href="https://corefork.telegram.org/constructor/webPagePending"/></para></summary>
	[TLDef(0xB0D13E47)]
	public sealed partial class WebPagePending : WebPageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of preview</summary>
		public long id;
		/// <summary>URL of the webpage</summary>
		[IfFlag(0)] public string url;
		/// <summary>When was the processing started</summary>
		public DateTime date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="url"/> has a value</summary>
			has_url = 0x1,
		}

		/// <summary>ID of preview</summary>
		public override long ID => id;
		/// <summary>URL of the webpage</summary>
		public override string Url => url;
	}
	/// <summary>Webpage preview		<para>See <a href="https://corefork.telegram.org/constructor/webPage"/></para></summary>
	[TLDef(0xE89C45B2)]
	public sealed partial class WebPage : WebPageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Preview ID</summary>
		public long id;
		/// <summary>URL of previewed webpage</summary>
		public string url;
		/// <summary>Webpage URL to be displayed to the user</summary>
		public string display_url;
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary>Type of the web page. One of the following: <!-- start type --><br/><br/>- <c>video</c><br/>- <c>gif</c><br/>- <c>photo</c><br/>- <c>document</c><br/>- <c>profile</c><br/>- <c>telegram_background</c><br/>- <c>telegram_theme</c><br/>- <c>telegram_story</c><br/>- <c>telegram_channel</c><br/>- <c>telegram_channel_request</c><br/>- <c>telegram_megagroup</c><br/>- <c>telegram_chat</c><br/>- <c>telegram_megagroup_request</c><br/>- <c>telegram_chat_request</c><br/>- <c>telegram_album</c><br/>- <c>telegram_message</c><br/>- <c>telegram_bot</c><br/>- <c>telegram_voicechat</c><br/>- <c>telegram_livestream</c><br/>- <c>telegram_user</c><br/>- <c>telegram_botapp</c><br/>- <c>telegram_channel_boost</c><br/>- <c>telegram_group_boost</c><br/>- <c>telegram_giftcode</c><br/>- <c>telegram_stickerset</c><br/><br/><!-- end type --></summary>
		[IfFlag(0)] public string type;
		/// <summary>Short name of the site (e.g., Google Docs, App Store)</summary>
		[IfFlag(1)] public string site_name;
		/// <summary>Title of the content</summary>
		[IfFlag(2)] public string title;
		/// <summary>Content description</summary>
		[IfFlag(3)] public string description;
		/// <summary>Image representing the content</summary>
		[IfFlag(4)] public PhotoBase photo;
		/// <summary>URL to show in the embedded preview</summary>
		[IfFlag(5)] public string embed_url;
		/// <summary>MIME type of the embedded preview, (e.g., text/html or video/mp4)</summary>
		[IfFlag(5)] public string embed_type;
		/// <summary>Width of the embedded preview</summary>
		[IfFlag(6)] public int embed_width;
		/// <summary>Height of the embedded preview</summary>
		[IfFlag(6)] public int embed_height;
		/// <summary>Duration of the content, in seconds</summary>
		[IfFlag(7)] public int duration;
		/// <summary>Author of the content</summary>
		[IfFlag(8)] public string author;
		/// <summary>Preview of the content as a media file</summary>
		[IfFlag(9)] public DocumentBase document;
		/// <summary>Page contents in <a href="https://instantview.telegram.org">instant view</a> format</summary>
		[IfFlag(10)] public Page cached_page;
		/// <summary>Webpage attributes</summary>
		[IfFlag(12)] public WebPageAttribute[] attributes;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="type"/> has a value</summary>
			has_type = 0x1,
			/// <summary>Field <see cref="site_name"/> has a value</summary>
			has_site_name = 0x2,
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x4,
			/// <summary>Field <see cref="description"/> has a value</summary>
			has_description = 0x8,
			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x10,
			/// <summary>Fields <see cref="embed_url"/> and <see cref="embed_type"/> have a value</summary>
			has_embed_url = 0x20,
			/// <summary>Fields <see cref="embed_width"/> and <see cref="embed_height"/> have a value</summary>
			has_embed_width = 0x40,
			/// <summary>Field <see cref="duration"/> has a value</summary>
			has_duration = 0x80,
			/// <summary>Field <see cref="author"/> has a value</summary>
			has_author = 0x100,
			/// <summary>Field <see cref="document"/> has a value</summary>
			has_document = 0x200,
			/// <summary>Field <see cref="cached_page"/> has a value</summary>
			has_cached_page = 0x400,
			/// <summary>Field <see cref="attributes"/> has a value</summary>
			has_attributes = 0x1000,
			/// <summary>Whether the size of the media in the preview can be changed.</summary>
			has_large_media = 0x2000,
			video_cover_photo = 0x4000,
		}

		/// <summary>Preview ID</summary>
		public override long ID => id;
		/// <summary>URL of previewed webpage</summary>
		public override string Url => url;
	}
	
	/// <summary>The preview of the webpage hasn't changed		<para>See <a href="https://corefork.telegram.org/constructor/webPageNotModified"/></para></summary>
	[TLDef(0x7311CA11)]
	public sealed partial class WebPageNotModified : WebPageBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Page view count</summary>
		[IfFlag(0)] public int cached_page_views;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="cached_page_views"/> has a value</summary>
			has_cached_page_views = 0x1,
		}
	}
}