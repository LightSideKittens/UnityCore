using System;

namespace TL
{
#pragma warning disable CS1574
	/// <summary>A document.		<para>See <a href="https://corefork.telegram.org/type/Document"/></para>		<para>Derived classes: <see cref="DocumentEmpty"/>, <see cref="Document"/></para></summary>
	public abstract partial class DocumentBase : IObject
	{
	}

	/// <summary>Empty constructor, document doesn't exist.		<para>See <a href="https://corefork.telegram.org/constructor/documentEmpty"/></para></summary>
	[TLDef(0x36F8C871)]
	public sealed partial class DocumentEmpty : DocumentBase
	{
		/// <summary>Document ID or <c>0</c></summary>
		public long id;
	}

	/// <summary>Document		<para>See <a href="https://corefork.telegram.org/constructor/document"/></para></summary>
	[TLDef(0x8FD4C4D8)]
	public sealed partial class Document : DocumentBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Document ID</summary>
		public long id;

		/// <summary>Check sum, dependent on document ID</summary>
		public long access_hash;

		/// <summary><a href="https://corefork.telegram.org/api/file_reference">File reference</a></summary>
		public byte[] file_reference;

		/// <summary>Creation date</summary>
		public DateTime date;

		/// <summary>MIME type</summary>
		public string mime_type;

		/// <summary>Size</summary>
		public long size;

		/// <summary>Thumbnails</summary>
		[IfFlag(0)] public PhotoSizeBase[] thumbs;

		/// <summary>Video thumbnails</summary>
		[IfFlag(1)] public VideoSizeBase[] video_thumbs;

		/// <summary>DC ID</summary>
		public int dc_id;

		/// <summary>Attributes</summary>
		public DocumentAttribute[] attributes;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="thumbs"/> has a value</summary>
			has_thumbs = 0x1,

			/// <summary>Field <see cref="video_thumbs"/> has a value</summary>
			has_video_thumbs = 0x2,
		}
	}
	
		/// <summary>Various possible attributes of a document (used to define if it's a sticker, a GIF, a video, a mask sticker, an image, an audio, and so on)		<para>See <a href="https://corefork.telegram.org/type/DocumentAttribute"/></para>		<para>Derived classes: <see cref="DocumentAttributeImageSize"/>, <see cref="DocumentAttributeAnimated"/>, <see cref="DocumentAttributeSticker"/>, <see cref="DocumentAttributeVideo"/>, <see cref="DocumentAttributeAudio"/>, <see cref="DocumentAttributeFilename"/>, <see cref="DocumentAttributeHasStickers"/>, <see cref="DocumentAttributeCustomEmoji"/></para></summary>
	public abstract partial class DocumentAttribute : IObject { }
	/// <summary>Defines the width and height of an image uploaded as document		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeImageSize"/></para></summary>
	[TLDef(0x6C37C15C)]
	public sealed partial class DocumentAttributeImageSize : DocumentAttribute
	{
		/// <summary>Width of image</summary>
		public int w;
		/// <summary>Height of image</summary>
		public int h;
	}
	/// <summary>Defines an animated GIF		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeAnimated"/></para></summary>
	[TLDef(0x11B58939)]
	public sealed partial class DocumentAttributeAnimated : DocumentAttribute { }
	/// <summary>Defines a sticker		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeSticker"/></para></summary>
	[TLDef(0x6319D612)]
	public sealed partial class DocumentAttributeSticker : DocumentAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Alternative emoji representation of sticker</summary>
		public string alt;
		/// <summary>Associated stickerset</summary>
		public InputStickerSet stickerset;
		/// <summary>Mask coordinates (if this is a mask sticker, attached to a photo)</summary>
		[IfFlag(0)] public MaskCoords mask_coords;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="mask_coords"/> has a value</summary>
			has_mask_coords = 0x1,
			/// <summary>Whether this is a mask sticker</summary>
			mask = 0x2,
		}
	}
	/// <summary>Defines a video		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeVideo"/></para></summary>
	[TLDef(0x43C57C48)]
	public sealed partial class DocumentAttributeVideo : DocumentAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Duration in seconds</summary>
		public double duration;
		/// <summary>Video width</summary>
		public int w;
		/// <summary>Video height</summary>
		public int h;
		/// <summary>Number of bytes to preload when preloading videos (particularly <a href="https://corefork.telegram.org/api/stories">video stories</a>).</summary>
		[IfFlag(2)] public int preload_prefix_size;
		/// <summary>Floating point UNIX timestamp in seconds, indicating the frame of the video that should be used as static preview and thumbnail.</summary>
		[IfFlag(4)] public double video_start_ts;
		/// <summary>Codec used for the video, i.e. “h264”, “h265”, or “av1”</summary>
		[IfFlag(5)] public string video_codec;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is a round video</summary>
			round_message = 0x1,
			/// <summary>Whether the video supports streaming</summary>
			supports_streaming = 0x2,
			/// <summary>Field <see cref="preload_prefix_size"/> has a value</summary>
			has_preload_prefix_size = 0x4,
			/// <summary>Whether the specified document is a video file with no audio tracks</summary>
			nosound = 0x8,
			/// <summary>Field <see cref="video_start_ts"/> has a value</summary>
			has_video_start_ts = 0x10,
			/// <summary>Field <see cref="video_codec"/> has a value</summary>
			has_video_codec = 0x20,
		}
	}
	/// <summary>Represents an audio file		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeAudio"/></para></summary>
	[TLDef(0x9852F9C6)]
	public sealed partial class DocumentAttributeAudio : DocumentAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Duration in seconds</summary>
		public int duration;
		/// <summary>Name of song</summary>
		[IfFlag(0)] public string title;
		/// <summary>Performer</summary>
		[IfFlag(1)] public string performer;
		/// <summary>Waveform: consists in a series of bitpacked 5-bit values. <br/>Example implementation: <a href="https://github.com/DrKLO/Telegram/blob/96dce2c9aabc33b87db61d830aa087b6b03fe397/TMessagesProj/jni/audio.c#L546">android</a>.</summary>
		[IfFlag(2)] public byte[] waveform;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,
			/// <summary>Field <see cref="performer"/> has a value</summary>
			has_performer = 0x2,
			/// <summary>Field <see cref="waveform"/> has a value</summary>
			has_waveform = 0x4,
			/// <summary>Whether this is a voice message</summary>
			voice = 0x400,
		}
	}
	/// <summary>A simple document with a file name		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeFilename"/></para></summary>
	[TLDef(0x15590068)]
	public sealed partial class DocumentAttributeFilename : DocumentAttribute
	{
		/// <summary>The file name</summary>
		public string file_name;
	}
	/// <summary>Whether the current document has stickers attached		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeHasStickers"/></para></summary>
	[TLDef(0x9801D2F7)]
	public sealed partial class DocumentAttributeHasStickers : DocumentAttribute { }
	/// <summary>Info about a custom emoji		<para>See <a href="https://corefork.telegram.org/constructor/documentAttributeCustomEmoji"/></para></summary>
	[TLDef(0xFD149899)]
	public sealed partial class DocumentAttributeCustomEmoji : DocumentAttribute
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The actual emoji</summary>
		public string alt;
		/// <summary>The emoji stickerset to which this emoji belongs.</summary>
		public InputStickerSet stickerset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this custom emoji can be sent by non-Premium users</summary>
			free = 0x1,
			/// <summary>Whether the color of this TGS custom emoji should be changed to the text color when used in messages, the accent color if used as emoji status, white on chat photos, or another appropriate color based on context.</summary>
			text_color = 0x2,
		}
	}
}