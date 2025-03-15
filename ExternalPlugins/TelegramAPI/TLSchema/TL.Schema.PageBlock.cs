using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Represents an <a href="https://instantview.telegram.org">instant view page element</a>		<para>See <a href="https://corefork.telegram.org/type/PageBlock"/></para>		<para>Derived classes: <see cref="PageBlockUnsupported"/>, <see cref="PageBlockTitle"/>, <see cref="PageBlockSubtitle"/>, <see cref="PageBlockAuthorDate"/>, <see cref="PageBlockHeader"/>, <see cref="PageBlockSubheader"/>, <see cref="PageBlockParagraph"/>, <see cref="PageBlockPreformatted"/>, <see cref="PageBlockFooter"/>, <see cref="PageBlockDivider"/>, <see cref="PageBlockAnchor"/>, <see cref="PageBlockList"/>, <see cref="PageBlockBlockquote"/>, <see cref="PageBlockPullquote"/>, <see cref="PageBlockPhoto"/>, <see cref="PageBlockVideo"/>, <see cref="PageBlockCover"/>, <see cref="PageBlockEmbed"/>, <see cref="PageBlockEmbedPost"/>, <see cref="PageBlockCollage"/>, <see cref="PageBlockSlideshow"/>, <see cref="PageBlockChannel"/>, <see cref="PageBlockAudio"/>, <see cref="PageBlockKicker"/>, <see cref="PageBlockTable"/>, <see cref="PageBlockOrderedList"/>, <see cref="PageBlockDetails"/>, <see cref="PageBlockRelatedArticles"/>, <see cref="PageBlockMap"/></para></summary>
    public abstract partial class PageBlock : IObject { }
    
    	/// <summary>Unsupported IV element		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockUnsupported"/></para></summary>
	[TLDef(0x13567E8A)]
	public sealed partial class PageBlockUnsupported : PageBlock { }
	/// <summary>Title		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockTitle"/></para></summary>
	[TLDef(0x70ABC3FD)]
	public sealed partial class PageBlockTitle : PageBlock
	{
		/// <summary>Title</summary>
		public RichText text;
	}
	/// <summary>Subtitle		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockSubtitle"/></para></summary>
	[TLDef(0x8FFA9A1F)]
	public sealed partial class PageBlockSubtitle : PageBlock
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Author and date of creation of article		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockAuthorDate"/></para></summary>
	[TLDef(0xBAAFE5E0)]
	public sealed partial class PageBlockAuthorDate : PageBlock
	{
		/// <summary>Author name</summary>
		public RichText author;
		/// <summary>Date of publication</summary>
		public DateTime published_date;
	}
	/// <summary>Page header		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockHeader"/></para></summary>
	[TLDef(0xBFD064EC)]
	public sealed partial class PageBlockHeader : PageBlock
	{
		/// <summary>Contents</summary>
		public RichText text;
	}
	/// <summary>Subheader		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockSubheader"/></para></summary>
	[TLDef(0xF12BB6E1)]
	public sealed partial class PageBlockSubheader : PageBlock
	{
		/// <summary>Subheader</summary>
		public RichText text;
	}
	/// <summary>A paragraph		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockParagraph"/></para></summary>
	[TLDef(0x467A0766)]
	public sealed partial class PageBlockParagraph : PageBlock
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Preformatted (<c>&lt;pre&gt;</c> text)		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockPreformatted"/></para></summary>
	[TLDef(0xC070D93E)]
	public sealed partial class PageBlockPreformatted : PageBlock
	{
		/// <summary>Text</summary>
		public RichText text;
		/// <summary>Programming language of preformatted text</summary>
		public string language;
	}
	/// <summary>Page footer		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockFooter"/></para></summary>
	[TLDef(0x48870999)]
	public sealed partial class PageBlockFooter : PageBlock
	{
		/// <summary>Contents</summary>
		public RichText text;
	}
	/// <summary>An empty block separating a page		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockDivider"/></para></summary>
	[TLDef(0xDB20B188)]
	public sealed partial class PageBlockDivider : PageBlock { }
	/// <summary>Link to section within the page itself (like <c>&lt;a href="#target"&gt;anchor&lt;/a&gt;</c>)		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockAnchor"/></para></summary>
	[TLDef(0xCE0D37B0)]
	public sealed partial class PageBlockAnchor : PageBlock
	{
		/// <summary>Name of target section</summary>
		public string name;
	}
	/// <summary>Unordered list of IV blocks		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockList"/></para></summary>
	[TLDef(0xE4E88011)]
	public sealed partial class PageBlockList : PageBlock
	{
		/// <summary>List of blocks in an IV page</summary>
		public PageListItem[] items;
	}
	/// <summary>Quote (equivalent to the HTML <c>&lt;blockquote&gt;</c>)		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockBlockquote"/></para></summary>
	[TLDef(0x263D7C26)]
	public sealed partial class PageBlockBlockquote : PageBlock
	{
		/// <summary>Quote contents</summary>
		public RichText text;
		/// <summary>Caption</summary>
		public RichText caption;
	}
	/// <summary>Pullquote		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockPullquote"/></para></summary>
	[TLDef(0x4F4456D3)]
	public sealed partial class PageBlockPullquote : PageBlock
	{
		/// <summary>Text</summary>
		public RichText text;
		/// <summary>Caption</summary>
		public RichText caption;
	}
	/// <summary>A photo		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockPhoto"/></para></summary>
	[TLDef(0x1759C560)]
	public sealed partial class PageBlockPhoto : PageBlock
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Photo ID</summary>
		public long photo_id;
		/// <summary>Caption</summary>
		public PageCaption caption;
		/// <summary>HTTP URL of page the photo leads to when clicked</summary>
		[IfFlag(0)] public string url;
		/// <summary>ID of preview of the page the photo leads to when clicked</summary>
		[IfFlag(0)] public long webpage_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Fields <see cref="url"/> and <see cref="webpage_id"/> have a value</summary>
			has_url = 0x1,
		}
	}
	/// <summary>Video		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockVideo"/></para></summary>
	[TLDef(0x7C8FE7B6)]
	public sealed partial class PageBlockVideo : PageBlock
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Video ID</summary>
		public long video_id;
		/// <summary>Caption</summary>
		public PageCaption caption;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the video is set to autoplay</summary>
			autoplay = 0x1,
			/// <summary>Whether the video is set to loop</summary>
			loop = 0x2,
		}
	}
	/// <summary>A page cover		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockCover"/></para></summary>
	[TLDef(0x39F23300)]
	public sealed partial class PageBlockCover : PageBlock
	{
		/// <summary>Cover</summary>
		public PageBlock cover;
	}
	/// <summary>An embedded webpage		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockEmbed"/></para></summary>
	[TLDef(0xA8718DC5)]
	public sealed partial class PageBlockEmbed : PageBlock
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Web page URL, if available</summary>
		[IfFlag(1)] public string url;
		/// <summary>HTML-markup of the embedded page</summary>
		[IfFlag(2)] public string html;
		/// <summary>Poster photo, if available</summary>
		[IfFlag(4)] public long poster_photo_id;
		/// <summary>Block width, if known</summary>
		[IfFlag(5)] public int w;
		/// <summary>Block height, if known</summary>
		[IfFlag(5)] public int h;
		/// <summary>Caption</summary>
		public PageCaption caption;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the block should be full width</summary>
			full_width = 0x1,
			/// <summary>Field <see cref="url"/> has a value</summary>
			has_url = 0x2,
			/// <summary>Field <see cref="html"/> has a value</summary>
			has_html = 0x4,
			/// <summary>Whether scrolling should be allowed</summary>
			allow_scrolling = 0x8,
			/// <summary>Field <see cref="poster_photo_id"/> has a value</summary>
			has_poster_photo_id = 0x10,
			/// <summary>Fields <see cref="w"/> and <see cref="h"/> have a value</summary>
			has_w = 0x20,
		}
	}
	/// <summary>An embedded post		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockEmbedPost"/></para></summary>
	[TLDef(0xF259A80B)]
	public sealed partial class PageBlockEmbedPost : PageBlock
	{
		/// <summary>Web page URL</summary>
		public string url;
		/// <summary>ID of generated webpage preview</summary>
		public long webpage_id;
		/// <summary>ID of the author's photo</summary>
		public long author_photo_id;
		/// <summary>Author name</summary>
		public string author;
		/// <summary>Creation date</summary>
		public DateTime date;
		/// <summary>Post contents</summary>
		public PageBlock[] blocks;
		/// <summary>Caption</summary>
		public PageCaption caption;
	}
	/// <summary>Collage of media		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockCollage"/></para></summary>
	[TLDef(0x65A0FA4D)]
	public sealed partial class PageBlockCollage : PageBlock
	{
		/// <summary>Media elements</summary>
		public PageBlock[] items;
		/// <summary>Caption</summary>
		public PageCaption caption;
	}
	/// <summary>Slideshow		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockSlideshow"/></para></summary>
	[TLDef(0x031F9590)]
	public sealed partial class PageBlockSlideshow : PageBlock
	{
		/// <summary>Slideshow items</summary>
		public PageBlock[] items;
		/// <summary>Caption</summary>
		public PageCaption caption;
	}
	/// <summary>Reference to a telegram channel		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockChannel"/></para></summary>
	[TLDef(0xEF1751B5)]
	public sealed partial class PageBlockChannel : PageBlock
	{
		/// <summary>The channel/supergroup/chat</summary>
		public ChatBase channel;
	}
	/// <summary>Audio		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockAudio"/></para></summary>
	[TLDef(0x804361EA)]
	public sealed partial class PageBlockAudio : PageBlock
	{
		/// <summary>Audio ID (to be fetched from the container <see cref="Page"/></summary>
		public long audio_id;
		/// <summary>Audio caption</summary>
		public PageCaption caption;
	}
	/// <summary>Kicker		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockKicker"/></para></summary>
	[TLDef(0x1E148390)]
	public sealed partial class PageBlockKicker : PageBlock
	{
		/// <summary>Contents</summary>
		public RichText text;
	}
	/// <summary>Table		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockTable"/></para></summary>
	[TLDef(0xBF4DEA82)]
	public sealed partial class PageBlockTable : PageBlock
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Title</summary>
		public RichText title;
		/// <summary>Table rows</summary>
		public PageTableRow[] rows;

		[Flags] public enum Flags : uint
		{
			/// <summary>Does the table have a visible border?</summary>
			bordered = 0x1,
			/// <summary>Is the table striped?</summary>
			striped = 0x2,
		}
	}
	/// <summary>Ordered list of IV blocks		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockOrderedList"/></para></summary>
	[TLDef(0x9A8AE1E1)]
	public sealed partial class PageBlockOrderedList : PageBlock
	{
		/// <summary>List items</summary>
		public PageListOrderedItem[] items;
	}
	/// <summary>A collapsible details block		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockDetails"/></para></summary>
	[TLDef(0x76768BED)]
	public sealed partial class PageBlockDetails : PageBlock
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Block contents</summary>
		public PageBlock[] blocks;
		/// <summary>Always visible heading for the block</summary>
		public RichText title;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the block is open by default</summary>
			open = 0x1,
		}
	}
	/// <summary>Related articles		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockRelatedArticles"/></para></summary>
	[TLDef(0x16115A96)]
	public sealed partial class PageBlockRelatedArticles : PageBlock
	{
		/// <summary>Title</summary>
		public RichText title;
		/// <summary>Related articles</summary>
		public PageRelatedArticle[] articles;
	}
	/// <summary>A map		<para>See <a href="https://corefork.telegram.org/constructor/pageBlockMap"/></para></summary>
	[TLDef(0xA44F3EF6)]
	public sealed partial class PageBlockMap : PageBlock
	{
		/// <summary>Location of the map center</summary>
		public GeoPoint geo;
		/// <summary>Map zoom level; 13-20</summary>
		public int zoom;
		/// <summary>Map width in pixels before applying scale; 16-102</summary>
		public int w;
		/// <summary>Map height in pixels before applying scale; 16-1024</summary>
		public int h;
		/// <summary>Caption</summary>
		public PageCaption caption;
	}

		/// <summary>Table cell		<para>See <a href="https://corefork.telegram.org/constructor/pageTableCell"/></para></summary>
	[TLDef(0x34566B6A)]
	public sealed partial class PageTableCell : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Content</summary>
		[IfFlag(7)] public RichText text;
		/// <summary>For how many columns should this cell extend</summary>
		[IfFlag(1)] public int colspan;
		/// <summary>For how many rows should this cell extend</summary>
		[IfFlag(2)] public int rowspan;

		[Flags] public enum Flags : uint
		{
			/// <summary>Is this element part of the column header</summary>
			header = 0x1,
			/// <summary>Field <see cref="colspan"/> has a value</summary>
			has_colspan = 0x2,
			/// <summary>Field <see cref="rowspan"/> has a value</summary>
			has_rowspan = 0x4,
			/// <summary>Horizontally centered block</summary>
			align_center = 0x8,
			/// <summary>Right-aligned block</summary>
			align_right = 0x10,
			/// <summary>Vertically centered block</summary>
			valign_middle = 0x20,
			/// <summary>Block vertically-aligned to the bottom</summary>
			valign_bottom = 0x40,
			/// <summary>Field <see cref="text"/> has a value</summary>
			has_text = 0x80,
		}
	}

	/// <summary>Table row		<para>See <a href="https://corefork.telegram.org/constructor/pageTableRow"/></para></summary>
	[TLDef(0xE0C0C5E5)]
	public sealed partial class PageTableRow : IObject
	{
		/// <summary>Table cells</summary>
		public PageTableCell[] cells;
	}

	/// <summary>Page caption		<para>See <a href="https://corefork.telegram.org/constructor/pageCaption"/></para></summary>
	[TLDef(0x6F747657)]
	public sealed partial class PageCaption : IObject
	{
		/// <summary>Caption</summary>
		public RichText text;
		/// <summary>Credits</summary>
		public RichText credit;
	}

	/// <summary>Item in block list		<para>See <a href="https://corefork.telegram.org/type/PageListItem"/></para>		<para>Derived classes: <see cref="PageListItemText"/>, <see cref="PageListItemBlocks"/></para></summary>
	public abstract partial class PageListItem : IObject { }
	/// <summary>List item		<para>See <a href="https://corefork.telegram.org/constructor/pageListItemText"/></para></summary>
	[TLDef(0xB92FB6CD)]
	public sealed partial class PageListItemText : PageListItem
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>List item		<para>See <a href="https://corefork.telegram.org/constructor/pageListItemBlocks"/></para></summary>
	[TLDef(0x25E073FC)]
	public sealed partial class PageListItemBlocks : PageListItem
	{
		/// <summary>Blocks</summary>
		public PageBlock[] blocks;
	}

	/// <summary>Represents an <a href="https://instantview.telegram.org">instant view ordered list</a>		<para>See <a href="https://corefork.telegram.org/type/PageListOrderedItem"/></para>		<para>Derived classes: <see cref="PageListOrderedItemText"/>, <see cref="PageListOrderedItemBlocks"/></para></summary>
	public abstract partial class PageListOrderedItem : IObject
	{
		/// <summary>Number of element within ordered list</summary>
		public string num;
	}
	/// <summary>Ordered list of text items		<para>See <a href="https://corefork.telegram.org/constructor/pageListOrderedItemText"/></para></summary>
	[TLDef(0x5E068047, inheritBefore = true)]
	public sealed partial class PageListOrderedItemText : PageListOrderedItem
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Ordered list of <a href="https://instantview.telegram.org">IV</a> blocks		<para>See <a href="https://corefork.telegram.org/constructor/pageListOrderedItemBlocks"/></para></summary>
	[TLDef(0x98DD8936, inheritBefore = true)]
	public sealed partial class PageListOrderedItemBlocks : PageListOrderedItem
	{
		/// <summary>Item contents</summary>
		public PageBlock[] blocks;
	}

	/// <summary>Related article		<para>See <a href="https://corefork.telegram.org/constructor/pageRelatedArticle"/></para></summary>
	[TLDef(0xB390DC08)]
	public sealed partial class PageRelatedArticle : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>URL of article</summary>
		public string url;
		/// <summary>Webpage ID of generated IV preview</summary>
		public long webpage_id;
		/// <summary>Title</summary>
		[IfFlag(0)] public string title;
		/// <summary>Description</summary>
		[IfFlag(1)] public string description;
		/// <summary>ID of preview photo</summary>
		[IfFlag(2)] public long photo_id;
		/// <summary>Author name</summary>
		[IfFlag(3)] public string author;
		/// <summary>Date of publication</summary>
		[IfFlag(4)] public DateTime published_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x1,
			/// <summary>Field <see cref="description"/> has a value</summary>
			has_description = 0x2,
			/// <summary>Field <see cref="photo_id"/> has a value</summary>
			has_photo_id = 0x4,
			/// <summary>Field <see cref="author"/> has a value</summary>
			has_author = 0x8,
			/// <summary>Field <see cref="published_date"/> has a value</summary>
			has_published_date = 0x10,
		}
	}

	/// <summary><a href="https://instantview.telegram.org">Instant view</a> page		<para>See <a href="https://corefork.telegram.org/constructor/page"/></para></summary>
	[TLDef(0x98657F0D)]
	public sealed partial class Page : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Original page HTTP URL</summary>
		public string url;
		/// <summary>Page elements (like with HTML elements, only as TL constructors)</summary>
		public PageBlock[] blocks;
		/// <summary>Photos in page</summary>
		public PhotoBase[] photos;
		/// <summary>Media in page</summary>
		public DocumentBase[] documents;
		/// <summary>View count</summary>
		[IfFlag(3)] public int views;

		[Flags] public enum Flags : uint
		{
			/// <summary>Indicates that not full page preview is available to the client and it will need to fetch full Instant View from the server using <see cref="SchemaExtensions.Messages_GetWebPagePreview">Messages_GetWebPagePreview</see>.</summary>
			part = 0x1,
			/// <summary>Whether the page contains RTL text</summary>
			rtl = 0x2,
			/// <summary>Whether this is an <a href="https://instantview.telegram.org/docs#what-39s-new-in-2-0">IV v2</a> page</summary>
			v2 = 0x4,
			/// <summary>Field <see cref="views"/> has a value</summary>
			has_views = 0x8,
		}
	}
}