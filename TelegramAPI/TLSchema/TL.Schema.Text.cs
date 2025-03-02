namespace TL
{
#pragma warning disable CS1574
    /// <summary>Rich text		<para>See <a href="https://corefork.telegram.org/type/RichText"/></para>		<para>Derived classes: <see cref="TextPlain"/>, <see cref="TextBold"/>, <see cref="TextItalic"/>, <see cref="TextUnderline"/>, <see cref="TextStrike"/>, <see cref="TextFixed"/>, <see cref="TextUrl"/>, <see cref="TextEmail"/>, <see cref="TextConcat"/>, <see cref="TextSubscript"/>, <see cref="TextSuperscript"/>, <see cref="TextMarked"/>, <see cref="TextPhone"/>, <see cref="TextImage"/>, <see cref="TextAnchor"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/textEmpty">textEmpty</a></remarks>
    public abstract partial class RichText : IObject { }
    
    	/// <summary>Plain text		<para>See <a href="https://corefork.telegram.org/constructor/textPlain"/></para></summary>
	[TLDef(0x744694E0)]
	public sealed partial class TextPlain : RichText
	{
		/// <summary>Text</summary>
		public string text;
	}
	/// <summary><strong>Bold</strong> text		<para>See <a href="https://corefork.telegram.org/constructor/textBold"/></para></summary>
	[TLDef(0x6724ABC4)]
	public sealed partial class TextBold : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary><em>Italic</em> text		<para>See <a href="https://corefork.telegram.org/constructor/textItalic"/></para></summary>
	[TLDef(0xD912A59C)]
	public sealed partial class TextItalic : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Underlined text		<para>See <a href="https://corefork.telegram.org/constructor/textUnderline"/></para></summary>
	[TLDef(0xC12622C4)]
	public sealed partial class TextUnderline : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary><del>Strikethrough</del> text		<para>See <a href="https://corefork.telegram.org/constructor/textStrike"/></para></summary>
	[TLDef(0x9BF8BB95)]
	public sealed partial class TextStrike : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary><c>fixed-width</c> rich text		<para>See <a href="https://corefork.telegram.org/constructor/textFixed"/></para></summary>
	[TLDef(0x6C3F19B9)]
	public sealed partial class TextFixed : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Link		<para>See <a href="https://corefork.telegram.org/constructor/textUrl"/></para></summary>
	[TLDef(0x3C2884C1)]
	public sealed partial class TextUrl : RichText
	{
		/// <summary>Text of link</summary>
		public RichText text;
		/// <summary>Webpage HTTP URL</summary>
		public string url;
		/// <summary>If a preview was already generated for the page, the page ID</summary>
		public long webpage_id;
	}
	/// <summary>Rich text email link		<para>See <a href="https://corefork.telegram.org/constructor/textEmail"/></para></summary>
	[TLDef(0xDE5A0DD6)]
	public sealed partial class TextEmail : RichText
	{
		/// <summary>Link text</summary>
		public RichText text;
		/// <summary>Email address</summary>
		public string email;
	}
	/// <summary>Concatenation of rich texts		<para>See <a href="https://corefork.telegram.org/constructor/textConcat"/></para></summary>
	[TLDef(0x7E6260D7)]
	public sealed partial class TextConcat : RichText
	{
		/// <summary>Concatenated rich texts</summary>
		public RichText[] texts;
	}
	/// <summary>Subscript text		<para>See <a href="https://corefork.telegram.org/constructor/textSubscript"/></para></summary>
	[TLDef(0xED6A8504)]
	public sealed partial class TextSubscript : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Superscript text		<para>See <a href="https://corefork.telegram.org/constructor/textSuperscript"/></para></summary>
	[TLDef(0xC7FB5E01)]
	public sealed partial class TextSuperscript : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Highlighted text		<para>See <a href="https://corefork.telegram.org/constructor/textMarked"/></para></summary>
	[TLDef(0x034B8621)]
	public sealed partial class TextMarked : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
	}
	/// <summary>Rich text linked to a phone number		<para>See <a href="https://corefork.telegram.org/constructor/textPhone"/></para></summary>
	[TLDef(0x1CCB966A)]
	public sealed partial class TextPhone : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
		/// <summary>Phone number</summary>
		public string phone;
	}
	/// <summary>Inline image		<para>See <a href="https://corefork.telegram.org/constructor/textImage"/></para></summary>
	[TLDef(0x081CCF4F)]
	public sealed partial class TextImage : RichText
	{
		/// <summary>Document ID</summary>
		public long document_id;
		/// <summary>Width</summary>
		public int w;
		/// <summary>Height</summary>
		public int h;
	}
	/// <summary>Text linking to another section of the page		<para>See <a href="https://corefork.telegram.org/constructor/textAnchor"/></para></summary>
	[TLDef(0x35553762)]
	public sealed partial class TextAnchor : RichText
	{
		/// <summary>Text</summary>
		public RichText text;
		/// <summary>Section name</summary>
		public string name;
	}


}