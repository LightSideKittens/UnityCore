using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Privacy <strong>keys</strong> together with <a href="https://corefork.telegram.org/api/privacy#privacy-rules">privacy rules »</a> indicate <em>what</em> can or can't someone do and are specified by a <see cref="PrivacyKey"/> constructor, and its input counterpart <see cref="InputPrivacyKey"/>.		<para>See <a href="https://corefork.telegram.org/type/InputPrivacyKey"/></para></summary>
    public enum InputPrivacyKey : uint
    {
        ///<summary>Whether people will be able to see our exact last online timestamp.<br/><br/>Note that if <em>we</em> decide to hide our exact last online timestamp to someone (i.e., users A, B, C, or all users) <strong>and</strong> we do not have a <a href="https://corefork.telegram.org/api/premium">Premium</a> subscription, we won't be able to see the exact last online timestamp of those users (A, B, C, or all users), even if those users <em>do</em> share it with us.<br/><br/>If those users <em>do</em> share their exact online status with us, but we can't see it due to the reason mentioned above, the <c>by_me</c> flag of <see cref="UserStatusRecently"/>, <see cref="UserStatusLastWeek"/>, <see cref="UserStatusLastMonth"/> will be set.</summary>
        StatusTimestamp = 0x4F96CB18,
        ///<summary>Whether people will be able to invite you to chats</summary>
        ChatInvite = 0xBDFB0426,
        ///<summary>Whether you will accept phone calls</summary>
        PhoneCall = 0xFABADC5F,
        ///<summary>Whether to allow P2P communication during VoIP calls</summary>
        PhoneP2P = 0xDB9E70D2,
        ///<summary>Whether messages forwarded from you will be <a href="https://telegram.org/blog/unsend-privacy-emoji#anonymous-forwarding">anonymous</a></summary>
        Forwards = 0xA4DD4C08,
        ///<summary>Whether people will be able to see your profile picture</summary>
        ProfilePhoto = 0x5719BACC,
        ///<summary>Whether people will be able to see your phone number</summary>
        PhoneNumber = 0x0352DAFA,
        ///<summary>Whether people can add you to their contact list by your phone number</summary>
        AddedByPhone = 0xD1219BDD,
        ///<summary>Whether people can send you voice messages or round videos (<a href="https://corefork.telegram.org/api/premium">Premium</a> users only).</summary>
        VoiceMessages = 0xAEE69D68,
        ///<summary>Whether people can see your bio</summary>
        About = 0x3823CC40,
        ///<summary>Whether the user can see our birthday.</summary>
        Birthday = 0xD65A11CC,
        ///<summary>Whether received <a href="https://corefork.telegram.org/api/gifts">gifts</a> will be automatically displayed on our profile</summary>
        StarGiftsAutoSave = 0xE1732341,
    }
    
    
	/// <summary>Privacy <strong>keys</strong> together with <a href="https://corefork.telegram.org/api/privacy#privacy-rules">privacy rules »</a> indicate <em>what</em> can or can't someone do and are specified by a <see cref="PrivacyKey"/> constructor, and its input counterpart <see cref="InputPrivacyKey"/>.		<para>See <a href="https://corefork.telegram.org/type/PrivacyKey"/></para></summary>
	public enum PrivacyKey : uint
	{
		///<summary>Whether we can see the last online timestamp of this user.<br/><br/>Note that if <em>we</em> decide to hide our exact last online timestamp to someone (i.e., users A, B, C, or all users) <strong>and</strong> we do not have a <a href="https://corefork.telegram.org/api/premium">Premium</a> subscription, we won't be able to see the exact last online timestamp of those users (A, B, C, or all users), even if those users <em>do</em> share it with us.<br/><br/>If those users <em>do</em> share their exact online status with us, but we can't see it due to the reason mentioned above, the <c>by_me</c> flag of <see cref="UserStatusRecently"/>, <see cref="UserStatusLastWeek"/>, <see cref="UserStatusLastMonth"/> will be set.</summary>
		StatusTimestamp = 0xBC2EAB30,
		///<summary>Whether the user can be invited to chats</summary>
		ChatInvite = 0x500E6DFA,
		///<summary>Whether the user accepts phone calls</summary>
		PhoneCall = 0x3D662B7B,
		///<summary>Whether P2P connections in phone calls with this user are allowed</summary>
		PhoneP2P = 0x39491CC8,
		///<summary>Whether messages forwarded from the user will be <a href="https://telegram.org/blog/unsend-privacy-emoji#anonymous-forwarding">anonymously forwarded</a></summary>
		Forwards = 0x69EC56A3,
		///<summary>Whether the profile picture of the user is visible</summary>
		ProfilePhoto = 0x96151FED,
		///<summary>Whether the user allows us to see his phone number</summary>
		PhoneNumber = 0xD19AE46D,
		///<summary>Whether this user can be added to our contact list by their phone number</summary>
		AddedByPhone = 0x42FFD42B,
		///<summary>Whether the user accepts voice messages</summary>
		VoiceMessages = 0x0697F414,
		///<summary>Whether people can see your bio</summary>
		About = 0xA486B761,
		///<summary>Whether the user can see our birthday.</summary>
		Birthday = 0x2000A518,
		///<summary>Whether received <a href="https://corefork.telegram.org/api/gifts">gifts</a> will be automatically displayed on our profile</summary>
		StarGiftsAutoSave = 0x2CA4FDF8,
	}


	/// <summary>Privacy <strong>rules</strong> together with <a href="https://corefork.telegram.org/api/privacy#privacy-keys">privacy keys</a> indicate <em>what</em> can or can't someone do and are specified by a <see cref="PrivacyRule"/> constructor, and its input counterpart <see cref="InputPrivacyRule"/>.		<para>See <a href="https://corefork.telegram.org/type/PrivacyRule"/></para>		<para>Derived classes: <see cref="PrivacyValueAllowContacts"/>, <see cref="PrivacyValueAllowAll"/>, <see cref="PrivacyValueAllowUsers"/>, <see cref="PrivacyValueDisallowContacts"/>, <see cref="PrivacyValueDisallowAll"/>, <see cref="PrivacyValueDisallowUsers"/>, <see cref="PrivacyValueAllowChatParticipants"/>, <see cref="PrivacyValueDisallowChatParticipants"/>, <see cref="PrivacyValueAllowCloseFriends"/>, <see cref="PrivacyValueAllowPremium"/>, <see cref="PrivacyValueAllowBots"/>, <see cref="PrivacyValueDisallowBots"/></para></summary>
	public abstract partial class PrivacyRule : IObject { }
	/// <summary>Allow all contacts		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowContacts"/></para></summary>
	[TLDef(0xFFFE1BAC)]
	public sealed partial class PrivacyValueAllowContacts : PrivacyRule { }
	/// <summary>Allow all users		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowAll"/></para></summary>
	[TLDef(0x65427B82)]
	public sealed partial class PrivacyValueAllowAll : PrivacyRule { }
	/// <summary>Allow only certain users		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowUsers"/></para></summary>
	[TLDef(0xB8905FB2)]
	public sealed partial class PrivacyValueAllowUsers : PrivacyRule
	{
		/// <summary>Allowed users</summary>
		public long[] users;
	}
	/// <summary>Disallow only contacts		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueDisallowContacts"/></para></summary>
	[TLDef(0xF888FA1A)]
	public sealed partial class PrivacyValueDisallowContacts : PrivacyRule { }
	/// <summary>Disallow all users		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueDisallowAll"/></para></summary>
	[TLDef(0x8B73E763)]
	public sealed partial class PrivacyValueDisallowAll : PrivacyRule { }
	/// <summary>Disallow only certain users		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueDisallowUsers"/></para></summary>
	[TLDef(0xE4621141)]
	public sealed partial class PrivacyValueDisallowUsers : PrivacyRule
	{
		/// <summary>Disallowed users</summary>
		public long[] users;
	}
	/// <summary>Allow all participants of certain chats		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowChatParticipants"/></para></summary>
	[TLDef(0x6B134E8E)]
	public sealed partial class PrivacyValueAllowChatParticipants : PrivacyRule
	{
		/// <summary>Allowed chats</summary>
		public long[] chats;
	}
	/// <summary>Disallow only participants of certain chats		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueDisallowChatParticipants"/></para></summary>
	[TLDef(0x41C87565)]
	public sealed partial class PrivacyValueDisallowChatParticipants : PrivacyRule
	{
		/// <summary>Disallowed chats</summary>
		public long[] chats;
	}
	/// <summary>Allow only <a href="https://corefork.telegram.org/api/privacy">close friends »</a>		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowCloseFriends"/></para></summary>
	[TLDef(0xF7E8D89B)]
	public sealed partial class PrivacyValueAllowCloseFriends : PrivacyRule { }
	/// <summary>Allow only users with a <a href="https://corefork.telegram.org/api/premium">Premium subscription »</a>, currently only usable for <see cref="InputPrivacyKey.ChatInvite"/>.		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowPremium"/></para></summary>
	[TLDef(0xECE9814B)]
	public sealed partial class PrivacyValueAllowPremium : PrivacyRule { }
	/// <summary>Allow bots and mini apps		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueAllowBots"/></para></summary>
	[TLDef(0x21461B5D)]
	public sealed partial class PrivacyValueAllowBots : PrivacyRule { }
	/// <summary>Disallow bots and mini apps		<para>See <a href="https://corefork.telegram.org/constructor/privacyValueDisallowBots"/></para></summary>
	[TLDef(0xF6A5F82F)]
	public sealed partial class PrivacyValueDisallowBots : PrivacyRule { }

	/// <summary>Privacy rules		<para>See <a href="https://corefork.telegram.org/constructor/account.privacyRules"/></para></summary>
	[TLDef(0x50A04E45)]
	public sealed partial class Account_PrivacyRules : IObject, IPeerResolver
	{
		/// <summary>Privacy rules</summary>
		public PrivacyRule[] rules;
		/// <summary>Chats to which the rules apply</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users to which the rules apply</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

}