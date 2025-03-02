using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Bot or inline keyboard buttons		<para>See <a href="https://corefork.telegram.org/type/KeyboardButton"/></para>		<para>Derived classes: <see cref="KeyboardButton"/>, <see cref="KeyboardButtonUrl"/>, <see cref="KeyboardButtonCallback"/>, <see cref="KeyboardButtonRequestPhone"/>, <see cref="KeyboardButtonRequestGeoLocation"/>, <see cref="KeyboardButtonSwitchInline"/>, <see cref="KeyboardButtonGame"/>, <see cref="KeyboardButtonBuy"/>, <see cref="KeyboardButtonUrlAuth"/>, <see cref="InputKeyboardButtonUrlAuth"/>, <see cref="KeyboardButtonRequestPoll"/>, <see cref="InputKeyboardButtonUserProfile"/>, <see cref="KeyboardButtonUserProfile"/>, <see cref="KeyboardButtonWebView"/>, <see cref="KeyboardButtonSimpleWebView"/>, <see cref="KeyboardButtonRequestPeer"/>, <see cref="InputKeyboardButtonRequestPeer"/>, <see cref="KeyboardButtonCopy"/></para></summary>
    public abstract partial class KeyboardButtonBase : IObject
    {
        /// <summary>Button text</summary>
        public virtual string Text => default;
    }
    
    	/// <summary>Bot keyboard button		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButton"/></para></summary>
	[TLDef(0xA2FA4880)]
	public partial class KeyboardButton : KeyboardButtonBase
	{
		/// <summary>Button text</summary>
		public string text;

		/// <summary>Button text</summary>
		public override string Text => text;
	}
	/// <summary>URL button		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonUrl"/></para></summary>
	[TLDef(0x258AFF05, inheritBefore = true)]
	public sealed partial class KeyboardButtonUrl : KeyboardButton
	{
		/// <summary>URL</summary>
		public string url;
	}
	/// <summary>Callback button		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonCallback"/></para></summary>
	[TLDef(0x35BBDB6B)]
	public sealed partial class KeyboardButtonCallback : KeyboardButtonBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Button text</summary>
		public string text;
		/// <summary>Callback data</summary>
		public byte[] data;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the user should verify his identity by entering his <a href="https://corefork.telegram.org/api/srp">2FA SRP parameters</a> to the <see cref="SchemaExtensions.Messages_GetBotCallbackAnswer">Messages_GetBotCallbackAnswer</see> method. NOTE: telegram and the bot WILL NOT have access to the plaintext password, thanks to <a href="https://corefork.telegram.org/api/srp">SRP</a>. This button is mainly used by the official <a href="https://t.me/botfather">@botfather</a> bot, for verifying the user's identity before transferring ownership of a bot to another user.</summary>
			requires_password = 0x1,
		}

		/// <summary>Button text</summary>
		public override string Text => text;
	}
	/// <summary>Button to request a user's phone number		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonRequestPhone"/></para></summary>
	[TLDef(0xB16A6C29)]
	public sealed partial class KeyboardButtonRequestPhone : KeyboardButton
	{
	}
	/// <summary>Button to request a user's geolocation		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonRequestGeoLocation"/></para></summary>
	[TLDef(0xFC796B3F)]
	public sealed partial class KeyboardButtonRequestGeoLocation : KeyboardButton
	{
	}
	/// <summary>Button to force a user to switch to inline mode: pressing the button will prompt the user to select one of their chats, open that chat and insert the bot's username and the specified inline query in the input field.		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonSwitchInline"/></para></summary>
	[TLDef(0x93B9FBB5)]
	public sealed partial class KeyboardButtonSwitchInline : KeyboardButtonBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Button label</summary>
		public string text;
		/// <summary>The inline query to use</summary>
		public string query;
		/// <summary>Filter to use when selecting chats.</summary>
		[IfFlag(1)] public InlineQueryPeerType[] peer_types;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, pressing the button will insert the bot's username and the specified inline <c>query</c> in the current chat's input field.</summary>
			same_peer = 0x1,
			/// <summary>Field <see cref="peer_types"/> has a value</summary>
			has_peer_types = 0x2,
		}

		/// <summary>Button label</summary>
		public override string Text => text;
	}
	/// <summary>Button to start a game		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonGame"/></para></summary>
	[TLDef(0x50F41CCF)]
	public sealed partial class KeyboardButtonGame : KeyboardButton
	{
	}
	/// <summary>Button to buy a product		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonBuy"/></para></summary>
	[TLDef(0xAFD93FBB)]
	public sealed partial class KeyboardButtonBuy : KeyboardButton
	{
	}
	/// <summary>Button to request a user to authorize via URL using <a href="https://telegram.org/blog/privacy-discussions-web-bots#meet-seamless-web-bots">Seamless Telegram Login</a>. When the user clicks on such a button, <see cref="SchemaExtensions.Messages_RequestUrlAuth">Messages_RequestUrlAuth</see> should be called, providing the <c>button_id</c> and the ID of the container message. The returned <see cref="UrlAuthResultRequest"/> object will contain more details about the authorization request (<c>request_write_access</c> if the bot would like to send messages to the user along with the username of the bot which will be used for user authorization). Finally, the user can choose to call <see cref="SchemaExtensions.Messages_AcceptUrlAuth">Messages_AcceptUrlAuth</see> to get a <see cref="UrlAuthResultAccepted"/> with the URL to open instead of the <c>url</c> of this constructor, or a <see langword="null"/>, in which case the <c>url</c> of this constructor must be opened, instead. If the user refuses the authorization request but still wants to open the link, the <c>url</c> of this constructor must be used.		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonUrlAuth"/></para></summary>
	[TLDef(0x10B78D29)]
	public sealed partial class KeyboardButtonUrlAuth : KeyboardButtonBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Button label</summary>
		public string text;
		/// <summary>New text of the button in forwarded messages.</summary>
		[IfFlag(0)] public string fwd_text;
		/// <summary>An HTTP URL to be opened with user authorization data added to the query string when the button is pressed. If the user refuses to provide authorization data, the original URL without information about the user will be opened. The data added is the same as described in <a href="https://corefork.telegram.org/widgets/login#receiving-authorization-data">Receiving authorization data</a>.<br/><br/><strong>NOTE</strong>: Services must <strong>always</strong> check the hash of the received data to verify the authentication and the integrity of the data as described in <a href="https://corefork.telegram.org/widgets/login#checking-authorization">Checking authorization</a>.</summary>
		public string url;
		/// <summary>ID of the button to pass to <see cref="SchemaExtensions.Messages_RequestUrlAuth">Messages_RequestUrlAuth</see></summary>
		public int button_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="fwd_text"/> has a value</summary>
			has_fwd_text = 0x1,
		}

		/// <summary>Button label</summary>
		public override string Text => text;
	}
	/// <summary>A button that allows the user to create and send a poll when pressed; available only in private		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonRequestPoll"/></para></summary>
	[TLDef(0xBBC7515D)]
	public sealed partial class KeyboardButtonRequestPoll : KeyboardButton
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, only quiz polls can be sent</summary>
		[IfFlag(0)] public bool quiz;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="quiz"/> has a value</summary>
			has_quiz = 0x1,
		}
	}
	/// <summary>Button that links directly to a user profile		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonUserProfile"/></para></summary>
	[TLDef(0x308660C1, inheritBefore = true)]
	public sealed partial class KeyboardButtonUserProfile : KeyboardButton
	{
		/// <summary>User ID</summary>
		public long user_id;
	}
	/// <summary>Button to open a <a href="https://corefork.telegram.org/api/bots/webapps">bot mini app</a> using <see cref="SchemaExtensions.Messages_RequestWebView">Messages_RequestWebView</see>, sending over user information after user confirmation.		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonWebView"/></para></summary>
	[TLDef(0x13767230, inheritBefore = true)]
	public partial class KeyboardButtonWebView : KeyboardButton
	{
		/// <summary><a href="https://corefork.telegram.org/api/bots/webapps">Web app url</a></summary>
		public string url;
	}
	/// <summary>Button to open a <a href="https://corefork.telegram.org/api/bots/webapps">bot mini app</a> using <see cref="SchemaExtensions.Messages_RequestSimpleWebView">Messages_RequestSimpleWebView</see>, without sending user information to the web app.		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonSimpleWebView"/></para></summary>
	[TLDef(0xA0C0505C)]
	public sealed partial class KeyboardButtonSimpleWebView : KeyboardButtonWebView
	{
	}
	/// <summary>Prompts the user to select and share one or more peers with the bot using <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonRequestPeer"/></para></summary>
	[TLDef(0x53D7BFD8, inheritBefore = true)]
	public sealed partial class KeyboardButtonRequestPeer : KeyboardButton
	{
		/// <summary>Button ID, to be passed to <see cref="SchemaExtensions.Messages_SendBotRequestedPeer">Messages_SendBotRequestedPeer</see>.</summary>
		public int button_id;
		/// <summary>Filtering criteria to use for the peer selection list shown to the user. <br/>The list should display all existing peers of the specified type, and should also offer an option for the user to create and immediately use one or more (up to <c>max_quantity</c>) peers of the specified type, if needed.</summary>
		public RequestPeerType peer_type;
		/// <summary>Maximum number of peers that can be chosen.</summary>
		public int max_quantity;
	}
	/// <summary>Clipboard button: when clicked, the attached text must be copied to the clipboard.		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonCopy"/></para></summary>
	[TLDef(0x75D2698E, inheritBefore = true)]
	public sealed partial class KeyboardButtonCopy : KeyboardButton
	{
		/// <summary>The text that will be copied to the clipboard</summary>
		public string copy_text;
	}

	/// <summary>Inline keyboard row		<para>See <a href="https://corefork.telegram.org/constructor/keyboardButtonRow"/></para></summary>
	[TLDef(0x77608B83)]
	public sealed partial class KeyboardButtonRow : IObject
	{
		/// <summary>Bot or inline keyboard buttons</summary>
		public KeyboardButtonBase[] buttons;
	}

}