using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Logged-in sessions		<para>See <a href="https://corefork.telegram.org/constructor/account.authorizations"/></para></summary>
    [TLDef(0x4BFF8EA0)]
    public sealed partial class Account_Authorizations : IObject
    {
        /// <summary>Time-to-live of session</summary>
        public int authorization_ttl_days;
        /// <summary>Logged-in sessions</summary>
        public Authorization[] authorizations;
    }
    
    	/// <summary>Configuration for two-factor authorization		<para>See <a href="https://corefork.telegram.org/constructor/account.password"/></para></summary>
	[TLDef(0x957B50FB)]
	public sealed partial class Account_Password : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The <a href="https://corefork.telegram.org/api/srp">KDF algorithm for SRP two-factor authentication</a> of the current password</summary>
		[IfFlag(2)] public PasswordKdfAlgo current_algo;
		/// <summary>Srp B param for <a href="https://corefork.telegram.org/api/srp">SRP authorization</a></summary>
		[IfFlag(2)] public byte[] srp_B;
		/// <summary>Srp ID param for <a href="https://corefork.telegram.org/api/srp">SRP authorization</a></summary>
		[IfFlag(2)] public long srp_id;
		/// <summary>Text hint for the password</summary>
		[IfFlag(3)] public string hint;
		/// <summary>A <a href="https://corefork.telegram.org/api/srp#email-verification">password recovery email</a> with the specified <a href="https://corefork.telegram.org/api/pattern">pattern</a> is still awaiting verification</summary>
		[IfFlag(4)] public string email_unconfirmed_pattern;
		/// <summary>The <a href="https://corefork.telegram.org/api/srp">KDF algorithm for SRP two-factor authentication</a> to use when creating new passwords</summary>
		public PasswordKdfAlgo new_algo;
		/// <summary>The KDF algorithm for telegram <a href="https://corefork.telegram.org/passport">passport</a></summary>
		public SecurePasswordKdfAlgo new_secure_algo;
		/// <summary>Secure random string</summary>
		public byte[] secure_random;
		/// <summary>The 2FA password will be automatically removed at this date, unless the user cancels the operation</summary>
		[IfFlag(5)] public DateTime pending_reset_date;
		/// <summary>A verified login email with the specified <a href="https://corefork.telegram.org/api/pattern">pattern</a> is configured</summary>
		[IfFlag(6)] public string login_email_pattern;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the user has a recovery method configured</summary>
			has_recovery = 0x1,
			/// <summary>Whether telegram <a href="https://corefork.telegram.org/passport">passport</a> is enabled</summary>
			has_secure_values = 0x2,
			/// <summary>Whether the user has a password</summary>
			has_password = 0x4,
			/// <summary>Field <see cref="hint"/> has a value</summary>
			has_hint = 0x8,
			/// <summary>Field <see cref="email_unconfirmed_pattern"/> has a value</summary>
			has_email_unconfirmed_pattern = 0x10,
			/// <summary>Field <see cref="pending_reset_date"/> has a value</summary>
			has_pending_reset_date = 0x20,
			/// <summary>Field <see cref="login_email_pattern"/> has a value</summary>
			has_login_email_pattern = 0x40,
		}
	}

	/// <summary>Private info associated to the password info (recovery email, telegram <a href="https://corefork.telegram.org/passport">passport</a> info &amp; so on)		<para>See <a href="https://corefork.telegram.org/constructor/account.passwordSettings"/></para></summary>
	[TLDef(0x9A5C33E5)]
	public sealed partial class Account_PasswordSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/srp#email-verification">2FA Recovery email</a></summary>
		[IfFlag(0)] public string email;
		/// <summary>Telegram <a href="https://corefork.telegram.org/passport">passport</a> settings</summary>
		[IfFlag(1)] public SecureSecretSettings secure_settings;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="email"/> has a value</summary>
			has_email = 0x1,
			/// <summary>Field <see cref="secure_settings"/> has a value</summary>
			has_secure_settings = 0x2,
		}
	}

	/// <summary>Settings for setting up a new password		<para>See <a href="https://corefork.telegram.org/constructor/account.passwordInputSettings"/></para></summary>
	[TLDef(0xC23727C9)]
	public sealed partial class Account_PasswordInputSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The <a href="https://corefork.telegram.org/api/srp">SRP algorithm</a> to use</summary>
		[IfFlag(0)] public PasswordKdfAlgo new_algo;
		/// <summary>The <a href="https://corefork.telegram.org/api/srp">computed password hash</a></summary>
		[IfFlag(0)] public byte[] new_password_hash;
		/// <summary>Text hint for the password</summary>
		[IfFlag(0)] public string hint;
		/// <summary>Password recovery email</summary>
		[IfFlag(1)] public string email;
		/// <summary>Telegram <a href="https://corefork.telegram.org/passport">passport</a> settings</summary>
		[IfFlag(2)] public SecureSecretSettings new_secure_settings;

		[Flags] public enum Flags : uint
		{
			/// <summary>Fields <see cref="new_algo"/>, <see cref="new_password_hash"/> and <see cref="hint"/> have a value</summary>
			has_new_algo = 0x1,
			/// <summary>Field <see cref="email"/> has a value</summary>
			has_email = 0x2,
			/// <summary>Field <see cref="new_secure_settings"/> has a value</summary>
			has_new_secure_settings = 0x4,
		}
	}
	
		
	/// <summary><a href="https://corefork.telegram.org/passport">Telegram Passport</a> authorization form		<para>See <a href="https://corefork.telegram.org/constructor/account.authorizationForm"/></para></summary>
	[TLDef(0xAD2E1CD8)]
	public sealed partial class Account_AuthorizationForm : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Required <a href="https://corefork.telegram.org/passport">Telegram Passport</a> documents</summary>
		public SecureRequiredTypeBase[] required_types;
		/// <summary>Already submitted <a href="https://corefork.telegram.org/passport">Telegram Passport</a> documents</summary>
		public SecureValue[] values;
		/// <summary><a href="https://corefork.telegram.org/passport">Telegram Passport</a> errors</summary>
		public SecureValueErrorBase[] errors;
		/// <summary>Info about the bot to which the form will be submitted</summary>
		public Dictionary<long, User> users;
		/// <summary>URL of the service's privacy policy</summary>
		[IfFlag(0)] public string privacy_policy_url;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="privacy_policy_url"/> has a value</summary>
			has_privacy_policy_url = 0x1,
		}
	}

	/// <summary>The sent email code		<para>See <a href="https://corefork.telegram.org/constructor/account.sentEmailCode"/></para></summary>
	[TLDef(0x811F854F)]
	public sealed partial class Account_SentEmailCode : IObject
	{
		/// <summary>The email (to which the code was sent) must match this <a href="https://corefork.telegram.org/api/pattern">pattern</a></summary>
		public string email_pattern;
		/// <summary>The length of the verification code</summary>
		public int length;
	}
	
	
	/// <summary>Temporary payment password		<para>See <a href="https://corefork.telegram.org/constructor/account.tmpPassword"/></para></summary>
	[TLDef(0xDB64FD34)]
	public sealed partial class Account_TmpPassword : IObject
	{
		/// <summary>Temporary password</summary>
		public byte[] tmp_password;
		/// <summary>Validity period</summary>
		public DateTime valid_until;
	}
	
	/// <summary>Web authorizations		<para>See <a href="https://corefork.telegram.org/constructor/account.webAuthorizations"/></para></summary>
	[TLDef(0xED56C9FC)]
	public sealed partial class Account_WebAuthorizations : IObject
	{
		/// <summary>Web authorization list</summary>
		public WebAuthorization[] authorizations;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;
	}
	/// <summary>Takeout info		<para>See <a href="https://corefork.telegram.org/constructor/account.takeout"/></para></summary>
	[TLDef(0x4DBA4501)]
	public sealed partial class Account_Takeout : IObject
	{
		/// <summary>Takeout ID</summary>
		public long id;
	}
	/// <summary>Installed <a href="https://corefork.telegram.org/api/wallpapers">wallpapers</a>		<para>See <a href="https://corefork.telegram.org/constructor/account.wallPapers"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/account.wallPapersNotModified">account.wallPapersNotModified</a></remarks>
	[TLDef(0xCDC3858C)]
	public sealed partial class Account_WallPapers : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpapers</a></summary>
		public WallPaperBase[] wallpapers;
	}
	/// <summary>Media autodownload settings		<para>See <a href="https://corefork.telegram.org/constructor/account.autoDownloadSettings"/></para></summary>
	[TLDef(0x63CACF26)]
	public sealed partial class Account_AutoDownloadSettings : IObject
	{
		/// <summary>Low data usage preset</summary>
		public AutoDownloadSettings low;
		/// <summary>Medium data usage preset</summary>
		public AutoDownloadSettings medium;
		/// <summary>High data usage preset</summary>
		public AutoDownloadSettings high;
	}
	/// <summary>Installed themes		<para>See <a href="https://corefork.telegram.org/constructor/account.themes"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/account.themesNotModified">account.themesNotModified</a></remarks>
	[TLDef(0x9A3D8C6D)]
	public sealed partial class Account_Themes : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>Themes</summary>
		public Theme[] themes;
	}
	/// <summary>Sensitive content settings		<para>See <a href="https://corefork.telegram.org/constructor/account.contentSettings"/></para></summary>
	[TLDef(0x57E28221)]
	public sealed partial class Account_ContentSettings : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether viewing of sensitive (NSFW) content is enabled</summary>
			sensitive_enabled = 0x1,
			/// <summary>Whether the current client can change the sensitive content settings to view NSFW content</summary>
			sensitive_can_change = 0x2,
		}
	}
	/// <summary>Result of an <see cref="SchemaExtensions.Account_ResetPassword">Account_ResetPassword</see> request.		<para>See <a href="https://corefork.telegram.org/type/account.ResetPasswordResult"/></para>		<para>Derived classes: <see cref="Account_ResetPasswordFailedWait"/>, <see cref="Account_ResetPasswordRequestedWait"/>, <see cref="Account_ResetPasswordOk"/></para></summary>
	public abstract partial class Account_ResetPasswordResult : IObject { }
	/// <summary>You recently requested a password reset that was canceled, please wait until the specified date before requesting another reset.		<para>See <a href="https://corefork.telegram.org/constructor/account.resetPasswordFailedWait"/></para></summary>
	[TLDef(0xE3779861)]
	public sealed partial class Account_ResetPasswordFailedWait : Account_ResetPasswordResult
	{
		/// <summary>Wait until this date before requesting another reset.</summary>
		public DateTime retry_date;
	}
	/// <summary>You successfully requested a password reset, please wait until the specified date before finalizing the reset.		<para>See <a href="https://corefork.telegram.org/constructor/account.resetPasswordRequestedWait"/></para></summary>
	[TLDef(0xE9EFFC7D)]
	public sealed partial class Account_ResetPasswordRequestedWait : Account_ResetPasswordResult
	{
		/// <summary>Wait until this date before finalizing the reset.</summary>
		public DateTime until_date;
	}
	/// <summary>The 2FA password was reset successfully.		<para>See <a href="https://corefork.telegram.org/constructor/account.resetPasswordOk"/></para></summary>
	[TLDef(0xE926D63E)]
	public sealed partial class Account_ResetPasswordOk : Account_ResetPasswordResult { }

	/// <summary>A list of saved notification sounds		<para>See <a href="https://corefork.telegram.org/constructor/account.savedRingtones"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/account.savedRingtonesNotModified">account.savedRingtonesNotModified</a></remarks>
	[TLDef(0xC1E92CC5)]
	public sealed partial class Account_SavedRingtones : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>Saved notification sounds</summary>
		public DocumentBase[] ringtones;
	}

	/// <summary>The notification sound was already in MP3 format and was saved without any modification		<para>See <a href="https://corefork.telegram.org/constructor/account.savedRingtone"/></para></summary>
	[TLDef(0xB7263F6D)]
	public partial class Account_SavedRingtone : IObject { }
	/// <summary>The notification sound was not in MP3 format and was successfully converted and saved, use the returned <see cref="DocumentBase"/> to refer to the notification sound from now on		<para>See <a href="https://corefork.telegram.org/constructor/account.savedRingtoneConverted"/></para></summary>
	[TLDef(0x1F307EB7)]
	public sealed partial class Account_SavedRingtoneConverted : Account_SavedRingtone
	{
		/// <summary>The converted notification sound</summary>
		public DocumentBase document;
	}
	/// <summary>A list of <a href="https://corefork.telegram.org/api/emoji-status">emoji statuses</a>		<para>See <a href="https://corefork.telegram.org/constructor/account.emojiStatuses"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/account.emojiStatusesNotModified">account.emojiStatusesNotModified</a></remarks>
	[TLDef(0x90C467D1)]
	public sealed partial class Account_EmojiStatuses : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary><a href="https://corefork.telegram.org/api/emoji-status">Emoji statuses</a></summary>
		public EmojiStatusBase[] statuses;
	}
	/// <summary>The email was verified correctly.		<para>See <a href="https://corefork.telegram.org/constructor/account.emailVerified"/></para></summary>
	[TLDef(0x2B96CD1B)]
	public partial class Account_EmailVerified : IObject
	{
		/// <summary>The verified email address.</summary>
		public string email;
	}
	/// <summary>The email was verified correctly, and a login code was just sent to it.		<para>See <a href="https://corefork.telegram.org/constructor/account.emailVerifiedLogin"/></para></summary>
	[TLDef(0xE1BB0D61, inheritBefore = true)]
	public sealed partial class Account_EmailVerifiedLogin : Account_EmailVerified
	{
		/// <summary>Info about the sent <a href="https://corefork.telegram.org/api/auth">login code</a></summary>
		public Auth_SentCodeBase sent_code;
	}

	/// <summary>Contains media autosave settings		<para>See <a href="https://corefork.telegram.org/constructor/account.autoSaveSettings"/></para></summary>
    [TLDef(0x4C3E069D)]
    public sealed partial class Account_AutoSaveSettings : IObject, IPeerResolver
    {
    	/// <summary>Default media autosave settings for private chats</summary>
    	public AutoSaveSettings users_settings;
    	/// <summary>Default media autosave settings for <a href="https://corefork.telegram.org/api/channel">groups and supergroups</a></summary>
    	public AutoSaveSettings chats_settings;
    	/// <summary>Default media autosave settings for <a href="https://corefork.telegram.org/api/channel">channels</a></summary>
    	public AutoSaveSettings broadcasts_settings;
    	/// <summary>Peer-specific granular autosave settings</summary>
    	public AutoSaveException[] exceptions;
    	/// <summary>Chats mentioned in the peer-specific granular autosave settings</summary>
    	public Dictionary<long, ChatBase> chats;
    	/// <summary>Users mentioned in the peer-specific granular autosave settings</summary>
    	public Dictionary<long, User> users;
    	/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
    	public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
    }
	/// <summary>Info about currently connected <a href="https://corefork.telegram.org/api/business#connected-bots">business bots</a>.		<para>See <a href="https://corefork.telegram.org/constructor/account.connectedBots"/></para></summary>
	[TLDef(0x17D7F87B)]
	public sealed partial class Account_ConnectedBots : IObject
	{
		/// <summary>Info about the connected bots</summary>
		public ConnectedBot[] connected_bots;
		/// <summary>Bot information</summary>
		public Dictionary<long, User> users;
	}
	/// <summary>Contains info about <a href="https://corefork.telegram.org/api/business#business-chat-links">business chat deep links »</a> created by the current account.		<para>See <a href="https://corefork.telegram.org/constructor/account.businessChatLinks"/></para></summary>
	[TLDef(0xEC43A2D1)]
	public sealed partial class Account_BusinessChatLinks : IObject, IPeerResolver
	{
		/// <summary>Links</summary>
		public BusinessChatLink[] links;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Contains info about a single resolved <a href="https://corefork.telegram.org/api/business#business-chat-links">business chat deep link »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/account.resolvedBusinessChatLinks"/></para></summary>
	[TLDef(0x9A23AF21)]
	public sealed partial class Account_ResolvedBusinessChatLinks : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Destination peer</summary>
		public Peer peer;
		/// <summary>Message to pre-fill in the message input field.</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(0)] public MessageEntity[] entities;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the result</summary>
		public IPeerInfo UserOrChat => peer?.UserOrChat(users, chats);
	}
	/// <summary>Time to live in days of the current account		<para>See <a href="https://corefork.telegram.org/constructor/accountDaysTTL"/></para></summary>
	[TLDef(0xB8D0AFDF)]
	public sealed partial class AccountDaysTTL : IObject
	{
		/// <summary>This account will self-destruct in the specified number of days</summary>
		public int days;
	}

}