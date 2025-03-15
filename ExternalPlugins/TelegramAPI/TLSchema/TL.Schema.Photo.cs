using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Object describes a photo.		<para>See <a href="https://corefork.telegram.org/type/Photo"/></para>		<para>Derived classes: <see cref="PhotoEmpty"/>, <see cref="Photo"/></para></summary>
    public abstract partial class PhotoBase : IObject { }
    
    /// <summary>Empty constructor, non-existent photo		<para>See <a href="https://corefork.telegram.org/constructor/photoEmpty"/></para></summary>
	[TLDef(0x2331B22D)]
	public sealed partial class PhotoEmpty : PhotoBase
	{
		/// <summary>Photo identifier</summary>
		public long id;
	}
	/// <summary>Photo		<para>See <a href="https://corefork.telegram.org/constructor/photo"/></para></summary>
	[TLDef(0xFB197A65)]
	public sealed partial class Photo : PhotoBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID</summary>
		public long id;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary><a href="https://corefork.telegram.org/api/file_reference">file reference</a></summary>
		public byte[] file_reference;
		/// <summary>Date of upload</summary>
		public DateTime date;
		/// <summary>Available sizes for download</summary>
		public PhotoSizeBase[] sizes;
		/// <summary><a href="https://corefork.telegram.org/api/files#animated-profile-pictures">For animated profiles</a>, the MPEG4 videos</summary>
		[IfFlag(1)] public VideoSizeBase[] video_sizes;
		/// <summary>DC ID to use for download</summary>
		public int dc_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the photo has mask stickers attached to it</summary>
			has_stickers = 0x1,
			/// <summary>Field <see cref="video_sizes"/> has a value</summary>
			has_video_sizes = 0x2,
		}
	}

	/// <summary>Location of a certain size of a picture		<para>See <a href="https://corefork.telegram.org/type/PhotoSize"/></para>		<para>Derived classes: <see cref="PhotoSizeEmpty"/>, <see cref="PhotoSize"/>, <see cref="PhotoCachedSize"/>, <see cref="PhotoStrippedSize"/>, <see cref="PhotoSizeProgressive"/>, <see cref="PhotoPathSize"/></para></summary>
	public abstract partial class PhotoSizeBase : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Thumbnail type »</a></summary>
		public virtual string Type => default;
	}
	/// <summary>Empty constructor. Image with this thumbnail is unavailable.		<para>See <a href="https://corefork.telegram.org/constructor/photoSizeEmpty"/></para></summary>
	[TLDef(0x0E17E23C)]
	public sealed partial class PhotoSizeEmpty : PhotoSizeBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Thumbnail type »</a></summary>
		public string type;

		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Thumbnail type »</a></summary>
		public override string Type => type;
	}
	/// <summary>Image description.		<para>See <a href="https://corefork.telegram.org/constructor/photoSize"/></para></summary>
	[TLDef(0x75C78E60)]
	public sealed partial class PhotoSize : PhotoSizeBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Thumbnail type »</a></summary>
		public string type;
		/// <summary>Image width</summary>
		public int w;
		/// <summary>Image height</summary>
		public int h;
		/// <summary>File size</summary>
		public int size;

		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Thumbnail type »</a></summary>
		public override string Type => type;
	}
	/// <summary>Description of an image and its content.		<para>See <a href="https://corefork.telegram.org/constructor/photoCachedSize"/></para></summary>
	[TLDef(0x021E1AD6)]
	public sealed partial class PhotoCachedSize : PhotoSizeBase
	{
		/// <summary>Thumbnail type</summary>
		public string type;
		/// <summary>Image width</summary>
		public int w;
		/// <summary>Image height</summary>
		public int h;
		/// <summary>Binary data, file content</summary>
		public byte[] bytes;

		/// <summary>Thumbnail type</summary>
		public override string Type => type;
	}
	/// <summary>A low-resolution compressed JPG payload		<para>See <a href="https://corefork.telegram.org/constructor/photoStrippedSize"/></para></summary>
	[TLDef(0xE0B0BC2E)]
	public sealed partial class PhotoStrippedSize : PhotoSizeBase
	{
		/// <summary>Thumbnail type</summary>
		public string type;
		/// <summary>Thumbnail data, see <a href="https://corefork.telegram.org/api/files#stripped-thumbnails">here for more info on decompression »</a></summary>
		public byte[] bytes;

		/// <summary>Thumbnail type</summary>
		public override string Type => type;
	}
	/// <summary>Progressively encoded photosize		<para>See <a href="https://corefork.telegram.org/constructor/photoSizeProgressive"/></para></summary>
	[TLDef(0xFA3EFB95)]
	public sealed partial class PhotoSizeProgressive : PhotoSizeBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Photosize type »</a></summary>
		public string type;
		/// <summary>Photo width</summary>
		public int w;
		/// <summary>Photo height</summary>
		public int h;
		/// <summary>Sizes of progressive JPEG file prefixes, which can be used to preliminarily show the image.</summary>
		public int[] sizes;

		/// <summary><a href="https://corefork.telegram.org/api/files#image-thumbnail-types">Photosize type »</a></summary>
		public override string Type => type;
	}
	/// <summary>Messages with animated stickers can have a compressed svg (&lt; 300 bytes) to show the outline of the sticker before fetching the actual lottie animation.		<para>See <a href="https://corefork.telegram.org/constructor/photoPathSize"/></para></summary>
	[TLDef(0xD8214D41)]
	public sealed partial class PhotoPathSize : PhotoSizeBase
	{
		/// <summary>Always <c>j</c></summary>
		public string type;
		/// <summary>Compressed SVG path payload, <a href="https://corefork.telegram.org/api/files#vector-thumbnails">see here for decompression instructions</a></summary>
		public byte[] bytes;

		/// <summary>Always <c>j</c></summary>
		public override string Type => type;
	}
}