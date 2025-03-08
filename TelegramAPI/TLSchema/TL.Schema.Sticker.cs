using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Represents a stickerset (stickerpack)		<para>See <a href="https://corefork.telegram.org/constructor/stickerSet"/></para></summary>
    [TLDef(0x2DD14EDC)]
    public sealed partial class StickerSet : IObject
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>When was this stickerset installed</summary>
        [IfFlag(0)] public DateTime installed_date;
        /// <summary>ID of the stickerset</summary>
        public long id;
        /// <summary>Access hash of stickerset</summary>
        public long access_hash;
        /// <summary>Title of stickerset</summary>
        public string title;
        /// <summary>Short name of stickerset, used when sharing stickerset using <a href="https://corefork.telegram.org/api/links#stickerset-links">stickerset deep links</a>.</summary>
        public string short_name;
        /// <summary>Stickerset thumbnail</summary>
        [IfFlag(4)] public PhotoSizeBase[] thumbs;
        /// <summary>DC ID of thumbnail</summary>
        [IfFlag(4)] public int thumb_dc_id;
        /// <summary>Thumbnail version</summary>
        [IfFlag(4)] public int thumb_version;
        /// <summary>Document ID of custom emoji thumbnail, fetch the document using <see cref="SchemaExtensions.Messages_GetCustomEmojiDocuments">Messages_GetCustomEmojiDocuments</see></summary>
        [IfFlag(8)] public long thumb_document_id;
        /// <summary>Number of stickers in pack</summary>
        public int count;
        /// <summary>Hash</summary>
        public int hash;

        [Flags] public enum Flags : uint
        {
            /// <summary>Field <see cref="installed_date"/> has a value</summary>
            has_installed_date = 0x1,
            /// <summary>Whether this stickerset was archived (due to too many saved stickers in the current account)</summary>
            archived = 0x2,
            /// <summary>Is this stickerset official</summary>
            official = 0x4,
            /// <summary>Is this a mask stickerset</summary>
            masks = 0x8,
            /// <summary>Fields <see cref="thumbs"/>, <see cref="thumb_dc_id"/> and <see cref="thumb_version"/> have a value</summary>
            has_thumbs = 0x10,
            /// <summary>This is a custom emoji stickerset</summary>
            emojis = 0x80,
            /// <summary>Field <see cref="thumb_document_id"/> has a value</summary>
            has_thumb_document_id = 0x100,
            /// <summary>Whether the color of this TGS custom emoji stickerset should be changed to the text color when used in messages, the accent color if used as emoji status, white on chat photos, or another appropriate color based on context.</summary>
            text_color = 0x200,
            /// <summary>If set, this custom emoji stickerset can be used in <a href="https://corefork.telegram.org/api/emoji-status">channel/supergroup emoji statuses</a>.</summary>
            channel_emoji_status = 0x400,
            /// <summary>Whether we created this stickerset</summary>
            creator = 0x800,
        }
    }
    
	/// <summary>Stickerset preview		<para>See <a href="https://corefork.telegram.org/type/StickerSetCovered"/></para>		<para>Derived classes: <see cref="StickerSetCovered"/>, <see cref="StickerSetMultiCovered"/>, <see cref="StickerSetFullCovered"/>, <see cref="StickerSetNoCovered"/></para></summary>
	public abstract partial class StickerSetCoveredBase : IObject
	{
		/// <summary>Stickerset</summary>
		public virtual StickerSet Set => default;
	}
	/// <summary>Stickerset with a single sticker as preview		<para>See <a href="https://corefork.telegram.org/constructor/stickerSetCovered"/></para></summary>
	[TLDef(0x6410A5D2)]
	public sealed partial class StickerSetCovered : StickerSetCoveredBase
	{
		/// <summary>Stickerset</summary>
		public StickerSet set;
		/// <summary>Preview</summary>
		public DocumentBase cover;

		/// <summary>Stickerset</summary>
		public override StickerSet Set => set;
	}
	/// <summary>Stickerset, with multiple stickers as preview		<para>See <a href="https://corefork.telegram.org/constructor/stickerSetMultiCovered"/></para></summary>
	[TLDef(0x3407E51B)]
	public sealed partial class StickerSetMultiCovered : StickerSetCoveredBase
	{
		/// <summary>Stickerset</summary>
		public StickerSet set;
		/// <summary>Preview stickers</summary>
		public DocumentBase[] covers;

		/// <summary>Stickerset</summary>
		public override StickerSet Set => set;
	}
	/// <summary>Stickerset preview with all stickers of the stickerset included.<br/>Currently used only for <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji stickersets</a>, to avoid a further call to <see cref="SchemaExtensions.Messages_GetStickerSet">Messages_GetStickerSet</see>.		<para>See <a href="https://corefork.telegram.org/constructor/stickerSetFullCovered"/></para></summary>
	[TLDef(0x40D13C0E)]
	public sealed partial class StickerSetFullCovered : StickerSetCoveredBase
	{
		/// <summary>Stickerset</summary>
		public StickerSet set;
		/// <summary>Emoji information about every sticker in the stickerset</summary>
		public StickerPack[] packs;
		/// <summary>Keywords for some or every sticker in the stickerset.</summary>
		public StickerKeyword[] keywords;
		/// <summary>Stickers</summary>
		public DocumentBase[] documents;

		/// <summary>Stickerset</summary>
		public override StickerSet Set => set;
	}
	/// <summary>Just the stickerset information, with no previews.		<para>See <a href="https://corefork.telegram.org/constructor/stickerSetNoCovered"/></para></summary>
	[TLDef(0x77B15D1C)]
	public sealed partial class StickerSetNoCovered : StickerSetCoveredBase
	{
		/// <summary>Stickerset information.</summary>
		public StickerSet set;

		/// <summary>Stickerset information.</summary>
		public override StickerSet Set => set;
	}
	/// <summary>A suggested short name for a stickerpack		<para>See <a href="https://corefork.telegram.org/constructor/stickers.suggestedShortName"/></para></summary>
	[TLDef(0x85FEA03F)]
	public sealed partial class Stickers_SuggestedShortName : IObject
	{
		/// <summary>Suggested short name</summary>
		public string short_name;
	}
	/// <summary>Keywords for a certain sticker		<para>See <a href="https://corefork.telegram.org/constructor/stickerKeyword"/></para></summary>
	[TLDef(0xFCFEB29C)]
	public sealed partial class StickerKeyword : IObject
	{
		/// <summary>Sticker ID</summary>
		public long document_id;
		/// <summary>Keywords</summary>
		public string[] keyword;
	}

    
}