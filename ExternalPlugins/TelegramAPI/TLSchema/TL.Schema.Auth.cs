using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Contains info on a confirmation code message sent via SMS, phone call or Telegram.		<para>See <a href="https://corefork.telegram.org/type/auth.SentCode"/></para>		<para>Derived classes: <see cref="Auth_SentCode"/>, <see cref="Auth_SentCodeSuccess"/></para></summary>
    public abstract partial class Auth_SentCodeBase : IObject { }
    
    /// <summary>Contains info about a sent verification code.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCode"/></para></summary>
	[TLDef(0x5E002502)]
	public sealed partial class Auth_SentCode : Auth_SentCodeBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Phone code type</summary>
		public Auth_SentCodeType type;
		/// <summary>Phone code hash, to be stored and later re-used with <see cref="SchemaExtensions.Auth_SignIn">Auth_SignIn</see></summary>
		public string phone_code_hash;
		/// <summary>Phone code type that will be sent next, if the phone code is not received within <c>timeout</c> seconds: to send it use <see cref="SchemaExtensions.Auth_ResendCode">Auth_ResendCode</see></summary>
		[IfFlag(1)] public Auth_CodeType next_type;
		/// <summary>Timeout for reception of the phone code</summary>
		[IfFlag(2)] public int timeout;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_type"/> has a value</summary>
			has_next_type = 0x2,
			/// <summary>Field <see cref="timeout"/> has a value</summary>
			has_timeout = 0x4,
		}
	}
	/// <summary>The user successfully authorized using <a href="https://corefork.telegram.org/api/auth#future-auth-tokens">future auth tokens</a>		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeSuccess"/></para></summary>
	[TLDef(0x2390FE44)]
	public sealed partial class Auth_SentCodeSuccess : Auth_SentCodeBase
	{
		/// <summary>Authorization info</summary>
		public Auth_AuthorizationBase authorization;
	}

	/// <summary>Object contains info on user authorization.		<para>See <a href="https://corefork.telegram.org/type/auth.Authorization"/></para>		<para>Derived classes: <see cref="Auth_Authorization"/>, <see cref="Auth_AuthorizationSignUpRequired"/></para></summary>
	public abstract partial class Auth_AuthorizationBase : IObject { }
	/// <summary>Contains user authorization info.		<para>See <a href="https://corefork.telegram.org/constructor/auth.authorization"/></para></summary>
	[TLDef(0x2EA2C0D4)]
	public sealed partial class Auth_Authorization : Auth_AuthorizationBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Iff setup_password_required is set and the user declines to set a 2-step verification password, they will be able to log into their account via SMS again only after this many days pass.</summary>
		[IfFlag(1)] public int otherwise_relogin_days;
		/// <summary>Temporary <a href="https://corefork.telegram.org/passport">passport</a> sessions</summary>
		[IfFlag(0)] public int tmp_sessions;
		/// <summary>A <a href="https://corefork.telegram.org/api/auth#future-auth-tokens">future auth token</a></summary>
		[IfFlag(2)] public byte[] future_auth_token;
		/// <summary>Info on authorized user</summary>
		public UserBase user;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="tmp_sessions"/> has a value</summary>
			has_tmp_sessions = 0x1,
			/// <summary>Suggests the user to set up a 2-step verification password to be able to log in again</summary>
			setup_password_required = 0x2,
			/// <summary>Field <see cref="future_auth_token"/> has a value</summary>
			has_future_auth_token = 0x4,
		}
	}
	/// <summary>An account with this phone number doesn't exist on telegram: the user has to <a href="https://corefork.telegram.org/api/auth">enter basic information and sign up</a>		<para>See <a href="https://corefork.telegram.org/constructor/auth.authorizationSignUpRequired"/></para></summary>
	[TLDef(0x44747E9A)]
	public sealed partial class Auth_AuthorizationSignUpRequired : Auth_AuthorizationBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Telegram's terms of service: the user must read and accept the terms of service before signing up to telegram</summary>
		[IfFlag(0)] public Help_TermsOfService terms_of_service;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="terms_of_service"/> has a value</summary>
			has_terms_of_service = 0x1,
		}
	}

	/// <summary>Data for copying of authorization between data centers.		<para>See <a href="https://corefork.telegram.org/constructor/auth.exportedAuthorization"/></para></summary>
	[TLDef(0xB434E2B8)]
	public sealed partial class Auth_ExportedAuthorization : IObject
	{
		/// <summary>current user identifier</summary>
		public long id;
		/// <summary>authorizes key</summary>
		public byte[] bytes;
	}
	
		/// <summary>Logged-in session		<para>See <a href="https://corefork.telegram.org/constructor/authorization"/></para></summary>
	[TLDef(0xAD01D61D)]
	public sealed partial class Authorization : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Identifier</summary>
		public long hash;
		/// <summary>Device model</summary>
		public string device_model;
		/// <summary>Platform</summary>
		public string platform;
		/// <summary>System version</summary>
		public string system_version;
		/// <summary><a href="https://corefork.telegram.org/api/obtaining_api_id">API ID</a></summary>
		public int api_id;
		/// <summary>App name</summary>
		public string app_name;
		/// <summary>App version</summary>
		public string app_version;
		/// <summary>When was the session created</summary>
		public DateTime date_created;
		/// <summary>When was the session last active</summary>
		public DateTime date_active;
		/// <summary>Last known IP</summary>
		public string ip;
		/// <summary>Country determined from IP</summary>
		public string country;
		/// <summary>Region determined from IP</summary>
		public string region;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is the current session</summary>
			current = 0x1,
			/// <summary>Whether the session is from an official app</summary>
			official_app = 0x2,
			/// <summary>Whether the session is still waiting for a 2FA password</summary>
			password_pending = 0x4,
			/// <summary>Whether this session will accept encrypted chats</summary>
			encrypted_requests_disabled = 0x8,
			/// <summary>Whether this session will accept phone calls</summary>
			call_requests_disabled = 0x10,
			/// <summary>Whether the session is <a href="https://corefork.telegram.org/api/auth#confirming-login">unconfirmed, see here »</a> for more info.</summary>
			unconfirmed = 0x20,
		}
	}
	
	/// <summary>Recovery info of a <a href="https://corefork.telegram.org/api/srp">2FA password</a>, only for accounts with a <a href="https://corefork.telegram.org/api/srp#email-verification">recovery email configured</a>.		<para>See <a href="https://corefork.telegram.org/constructor/auth.passwordRecovery"/></para></summary>
	[TLDef(0x137948A5)]
	public sealed partial class Auth_PasswordRecovery : IObject
	{
		/// <summary>The email to which the recovery code was sent must match this <a href="https://corefork.telegram.org/api/pattern">pattern</a>.</summary>
		public string email_pattern;
	}
	
		/// <summary>Type of verification code that will be sent next if you call the resendCode method		<para>See <a href="https://corefork.telegram.org/type/auth.CodeType"/></para></summary>
	public enum Auth_CodeType : uint
	{
		///<summary>The next time, the authentication code will be delivered via an immediately canceled incoming call.</summary>
		Sms = 0x72A3158C,
		///<summary>The next time, the authentication code is to be delivered via an outgoing phone call.</summary>
		Call = 0x741CD3E3,
		///<summary>The next time, the authentication code will be delivered via an immediately canceled incoming call.</summary>
		FlashCall = 0x226CCEFB,
		///<summary>The next time, the authentication code will be delivered via an immediately canceled incoming call, handled manually by the user.</summary>
		MissedCall = 0xD61AD6EE,
		///<summary>The next time, the authentication code will be delivered via <a href="https://fragment.com">fragment.com</a></summary>
		FragmentSms = 0x06ED998C,
	}

	/// <summary>Type of the verification code that was sent		<para>See <a href="https://corefork.telegram.org/type/auth.SentCodeType"/></para>		<para>Derived classes: <see cref="Auth_SentCodeTypeApp"/>, <see cref="Auth_SentCodeTypeSms"/>, <see cref="Auth_SentCodeTypeCall"/>, <see cref="Auth_SentCodeTypeFlashCall"/>, <see cref="Auth_SentCodeTypeMissedCall"/>, <see cref="Auth_SentCodeTypeEmailCode"/>, <see cref="Auth_SentCodeTypeSetUpEmailRequired"/>, <see cref="Auth_SentCodeTypeFragmentSms"/>, <see cref="Auth_SentCodeTypeFirebaseSms"/>, <see cref="Auth_SentCodeTypeSmsWord"/>, <see cref="Auth_SentCodeTypeSmsPhrase"/></para></summary>
	public abstract partial class Auth_SentCodeType : IObject { }
	/// <summary>The code was sent through the telegram app		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeApp"/></para></summary>
	[TLDef(0x3DBB5986)]
	public sealed partial class Auth_SentCodeTypeApp : Auth_SentCodeType
	{
		/// <summary>Length of the code in bytes</summary>
		public int length;
	}
	/// <summary>The code was sent via SMS		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeSms"/></para></summary>
	[TLDef(0xC000BBA2)]
	public partial class Auth_SentCodeTypeSms : Auth_SentCodeType
	{
		/// <summary>Length of the code in bytes</summary>
		public int length;
	}
	/// <summary>The code will be sent via a phone call: a synthesized voice will tell the user which verification code to input.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeCall"/></para></summary>
	[TLDef(0x5353E5A7)]
	public partial class Auth_SentCodeTypeCall : Auth_SentCodeType
	{
		/// <summary>Length of the verification code</summary>
		public int length;
	}
	/// <summary>The code will be sent via a flash phone call, that will be closed immediately. The phone code will then be the phone number itself, just make sure that the phone number matches the specified pattern.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeFlashCall"/></para></summary>
	[TLDef(0xAB03C6D9)]
	public sealed partial class Auth_SentCodeTypeFlashCall : Auth_SentCodeType
	{
		/// <summary><a href="https://corefork.telegram.org/api/pattern">pattern</a> to match</summary>
		public string pattern;
	}
	/// <summary>The code will be sent via a flash phone call, that will be closed immediately. The last digits of the phone number that calls are the code that must be entered manually by the user.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeMissedCall"/></para></summary>
	[TLDef(0x82006484)]
	public sealed partial class Auth_SentCodeTypeMissedCall : Auth_SentCodeTypeCall
	{
		/// <summary>Prefix of the phone number from which the call will be made</summary>
		public string prefix;
	}
	/// <summary>The code was sent via the <a href="https://corefork.telegram.org/api/auth#email-verification">previously configured login email »</a>		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeEmailCode"/></para></summary>
	[TLDef(0xF450F59B)]
	public sealed partial class Auth_SentCodeTypeEmailCode : Auth_SentCodeType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/pattern">Pattern</a> of the email</summary>
		public string email_pattern;
		/// <summary>Length of the sent verification code</summary>
		public int length;
		/// <summary>Clients should wait for the specified amount of seconds before allowing the user to invoke <see cref="SchemaExtensions.Auth_ResetLoginEmail">Auth_ResetLoginEmail</see> (will be 0 for <a href="https://corefork.telegram.org/api/premium">Premium</a> users).</summary>
		[IfFlag(3)] public int reset_available_period;
		/// <summary>An email reset was already requested, and will occur at the specified date.</summary>
		[IfFlag(4)] public DateTime reset_pending_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether authorization through Apple ID is allowed</summary>
			apple_signin_allowed = 0x1,
			/// <summary>Whether authorization through Google ID is allowed</summary>
			google_signin_allowed = 0x2,
			/// <summary>Field <see cref="reset_available_period"/> has a value</summary>
			has_reset_available_period = 0x8,
			/// <summary>Field <see cref="reset_pending_date"/> has a value</summary>
			has_reset_pending_date = 0x10,
		}
	}
	/// <summary>The user should add and verify an email address in order to login as described <a href="https://corefork.telegram.org/api/auth#email-verification">here »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeSetUpEmailRequired"/></para></summary>
	[TLDef(0xA5491DEA)]
	public sealed partial class Auth_SentCodeTypeSetUpEmailRequired : Auth_SentCodeType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether authorization through Apple ID is allowed</summary>
			apple_signin_allowed = 0x1,
			/// <summary>Whether authorization through Google ID is allowed</summary>
			google_signin_allowed = 0x2,
		}
	}
	/// <summary>The code was delivered via <a href="https://fragment.com">fragment.com</a>.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeFragmentSms"/></para></summary>
	[TLDef(0xD9565C39)]
	public sealed partial class Auth_SentCodeTypeFragmentSms : Auth_SentCodeTypeSms
	{
		/// <summary>Open the specified URL to log into <a href="https://fragment.com">fragment.com</a> with the wallet that owns the specified phone number and view the code.</summary>
		public string url;
	}
	/// <summary>An authentication code should be delivered via SMS after Firebase attestation, as described in the <a href="https://corefork.telegram.org/api/auth">auth documentation »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeFirebaseSms"/></para></summary>
	[TLDef(0x009FD736)]
	public sealed partial class Auth_SentCodeTypeFirebaseSms : Auth_SentCodeTypeSms
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>On Android, the nonce to be used as described in the <a href="https://corefork.telegram.org/api/auth">auth documentation »</a></summary>
		[IfFlag(0)] public byte[] nonce;
		/// <summary>Google Play Integrity project ID</summary>
		[IfFlag(2)] public long play_integrity_project_id;
		/// <summary>Play Integrity API nonce</summary>
		[IfFlag(2)] public byte[] play_integrity_nonce;
		/// <summary>On iOS, must be compared with the <c>receipt</c> extracted from the received push notification.</summary>
		[IfFlag(1)] public string receipt;
		/// <summary>On iOS: if a push notification with the <c>ios_push_secret</c> isn't received within <c>push_timeout</c> seconds, the <c>next_type</c> authentication method must be used, with <see cref="SchemaExtensions.Auth_ResendCode">Auth_ResendCode</see>.</summary>
		[IfFlag(1)] public int push_timeout;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="nonce"/> has a value</summary>
			has_nonce = 0x1,
			/// <summary>Fields <see cref="receipt"/> and <see cref="push_timeout"/> have a value</summary>
			has_receipt = 0x2,
			/// <summary>Fields <see cref="play_integrity_project_id"/> and <see cref="play_integrity_nonce"/> have a value</summary>
			has_play_integrity_project_id = 0x4,
		}
	}
	/// <summary>The code was sent via SMS as a secret word, starting with the letter specified in <c>beginning</c>		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeSmsWord"/></para></summary>
	[TLDef(0xA416AC81)]
	public sealed partial class Auth_SentCodeTypeSmsWord : Auth_SentCodeType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, the secret word in the sent SMS (which may contain multiple words) starts with this letter.</summary>
		[IfFlag(0)] public string beginning;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="beginning"/> has a value</summary>
			has_beginning = 0x1,
		}
	}
	/// <summary>The code was sent via SMS as a secret phrase starting with the word specified in <c>beginning</c>		<para>See <a href="https://corefork.telegram.org/constructor/auth.sentCodeTypeSmsPhrase"/></para></summary>
	[TLDef(0xB37794AF)]
	public sealed partial class Auth_SentCodeTypeSmsPhrase : Auth_SentCodeType
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, the secret phrase (and the SMS) starts with this word.</summary>
		[IfFlag(0)] public string beginning;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="beginning"/> has a value</summary>
			has_beginning = 0x1,
		}
	}
	
	/// <summary>Login token (for QR code login)		<para>See <a href="https://corefork.telegram.org/type/auth.LoginToken"/></para>		<para>Derived classes: <see cref="Auth_LoginToken"/>, <see cref="Auth_LoginTokenMigrateTo"/>, <see cref="Auth_LoginTokenSuccess"/></para></summary>
	public abstract partial class Auth_LoginTokenBase : IObject { }
	/// <summary>Login token (for <a href="https://corefork.telegram.org/api/qr-login">QR code login</a>)		<para>See <a href="https://corefork.telegram.org/constructor/auth.loginToken"/></para></summary>
	[TLDef(0x629F1980)]
	public sealed partial class Auth_LoginToken : Auth_LoginTokenBase
	{
		/// <summary>Expiration date of QR code</summary>
		public DateTime expires;
		/// <summary>Token to render in QR code</summary>
		public byte[] token;
	}
	/// <summary>Repeat the query to the specified DC		<para>See <a href="https://corefork.telegram.org/constructor/auth.loginTokenMigrateTo"/></para></summary>
	[TLDef(0x068E9916)]
	public sealed partial class Auth_LoginTokenMigrateTo : Auth_LoginTokenBase
	{
		/// <summary>DC ID</summary>
		public int dc_id;
		/// <summary>Token to use for login</summary>
		public byte[] token;
	}
	/// <summary>Login via token (QR code) succeeded!		<para>See <a href="https://corefork.telegram.org/constructor/auth.loginTokenSuccess"/></para></summary>
	[TLDef(0x390D5C5E)]
	public sealed partial class Auth_LoginTokenSuccess : Auth_LoginTokenBase
	{
		/// <summary>Authorization info</summary>
		public Auth_AuthorizationBase authorization;
	}

	/// <summary><a href="https://corefork.telegram.org/api/auth#future-auth-tokens">Future auth token »</a> to be used on subsequent authorizations		<para>See <a href="https://corefork.telegram.org/constructor/auth.loggedOut"/></para></summary>
	[TLDef(0xC3A2835F)]
	public sealed partial class Auth_LoggedOut : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/auth#future-auth-tokens">Future auth token »</a> to be used on subsequent authorizations</summary>
		[IfFlag(0)] public byte[] future_auth_token;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="future_auth_token"/> has a value</summary>
			has_future_auth_token = 0x1,
		}
	}
}