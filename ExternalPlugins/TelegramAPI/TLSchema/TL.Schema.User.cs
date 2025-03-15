using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
	/// <summary>Object defines a user.		<para>See <a href="https://corefork.telegram.org/type/User"/></para>		<para>Derived classes: <see cref="UserEmpty"/>, <see cref="User"/></para></summary>
	public abstract partial class UserBase : IObject
	{
	}

	/// <summary>Empty constructor, non-existent user.		<para>See <a href="https://corefork.telegram.org/constructor/userEmpty"/></para></summary>
	[TLDef(0xD3BC4B7A)]
	public sealed partial class UserEmpty : UserBase
	{
		/// <summary>User identifier or <c>0</c></summary>
		public long id;
	}

	/// <summary>Indicates info about a certain user.		<para>See <a href="https://corefork.telegram.org/constructor/user"/></para></summary>
	[TLDef(0x4B46C37E)]
	public sealed partial class User : UserBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Extra bits of information, use <c>flags2.HasFlag(...)</c> to test for those</summary>
		public Flags2 flags2;

		/// <summary>ID of the user, see <a href="https://corefork.telegram.org/api/peers#peer-id">here »</a> for more info.</summary>
		public long id;

		/// <summary>Access hash of the user, see <a href="https://corefork.telegram.org/api/peers#access-hash">here »</a> for more info. <br/>If this flag is set, when updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, generate a virtual flag called <c>min_access_hash</c>, which is: <br/>- Set to <c>true</c> if <c>min</c> is set AND <br/>-- The <c>phone</c> flag is <strong>not</strong> set OR <br/>-- The <c>phone</c> flag is set and the associated phone number string is non-empty <br/>- Set to <c>false</c> otherwise. <br/><br/>Then, apply both <c>access_hash</c> and <c>min_access_hash</c> to the local database if: <br/>- <c>min_access_hash</c> is false OR <br/>- <c>min_access_hash</c> is true AND <br/>-- There is no locally cached object for this user OR <br/>-- There is no <c>access_hash</c> in the local cache OR <br/>-- The cached object's <c>min_access_hash</c> is also true <br/><br/>If the final merged object stored to the database has the <c>min_access_hash</c> field set to true, the related <c>access_hash</c> is <strong>only</strong> suitable to use in <see cref="InputPeerPhotoFileLocation"><c>inputPeerPhotoFileLocation</c> »</see>, to directly <a href="https://corefork.telegram.org/api/files">download the profile pictures</a> of users, everywhere else a <c>inputPeer*FromMessage</c> constructor will have to be generated as specified <a href="https://corefork.telegram.org/api/min">here »</a>. <br/>Bots can also use min access hashes in some conditions, by passing <c>0</c> instead of the min access hash.</summary>
		[IfFlag(0)] public long access_hash;

		/// <summary>First name. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>min</c> flag of the locally cached user entry is set.</summary>
		[IfFlag(1)] public string first_name;

		/// <summary>Last name. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>min</c> flag of the locally cached user entry is set.</summary>
		[IfFlag(2)] public string last_name;

		/// <summary>Main active username. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>min</c> flag of the locally cached user entry is set. <br/>Changes to this flag should invalidate the local <see cref="UserFull"/> cache for this user ID if the above conditions are respected and the <c>bot_can_edit</c> flag is also set.</summary>
		[IfFlag(3)] public string username;

		/// <summary>Phone number. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>min</c> flag of the locally cached user entry is set.</summary>
		[IfFlag(4)] public string phone;

		/// <summary>Profile picture of user. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>apply_min_photo</c> flag is set OR <br/>-- The <c>min</c> flag of the locally cached user entry is set.</summary>
		[IfFlag(5)] public UserProfilePhoto photo;

		/// <summary>Online status of user. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>min</c> flag of the locally cached user entry is set OR <br/>-- The locally cached user entry is equal to <see langword="null"/>.</summary>
		[IfFlag(6)] public UserStatus status;

		/// <summary>Version of the <see cref="UserFull">bot_info field in userFull</see>, incremented every time it changes. <br/>Changes to this flag should invalidate the local <see cref="UserFull"/> cache for this user ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
		[IfFlag(14)] public int bot_info_version;

		/// <summary>Contains the reason why access to this user must be restricted.</summary>
		[IfFlag(18)] public RestrictionReason[] restriction_reason;

		/// <summary>Inline placeholder for this inline bot</summary>
		[IfFlag(19)] public string bot_inline_placeholder;

		/// <summary>Language code of the user</summary>
		[IfFlag(22)] public string lang_code;

		/// <summary><a href="https://corefork.telegram.org/api/emoji-status">Emoji status</a></summary>
		[IfFlag(30)] public EmojiStatusBase emoji_status;

		/// <summary>Additional usernames. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, apply changes to this field only if: <br/>- The <c>min</c> flag is not set OR <br/>- The <c>min</c> flag is set AND <br/>-- The <c>min</c> flag of the locally cached user entry is set. <br/>Changes to this flag (if the above conditions are respected) should invalidate the local <see cref="UserFull"/> cache for this user ID.</summary>
		[IfFlag(32)] public Username[] usernames;

		/// <summary>ID of the maximum read <a href="https://corefork.telegram.org/api/stories">story</a>.  <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag of the incoming constructor is set.</summary>
		[IfFlag(37)] public int stories_max_id;

		/// <summary>The user's <a href="https://corefork.telegram.org/api/colors">accent color</a>.</summary>
		[IfFlag(40)] public PeerColor color;

		/// <summary>The user's <a href="https://corefork.telegram.org/api/colors">profile color</a>.</summary>
		[IfFlag(41)] public PeerColor profile_color;

		/// <summary>Monthly Active Users (MAU) of this bot (may be absent for small bots).</summary>
		[IfFlag(44)] public int bot_active_users;

		[IfFlag(46)] public long bot_verification_icon;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Field <see cref="access_hash"/> has a value</summary>
			has_access_hash = 0x1,

			/// <summary>Field <see cref="first_name"/> has a value</summary>
			has_first_name = 0x2,

			/// <summary>Field <see cref="last_name"/> has a value</summary>
			has_last_name = 0x4,

			/// <summary>Field <see cref="username"/> has a value</summary>
			has_username = 0x8,

			/// <summary>Field <see cref="phone"/> has a value</summary>
			has_phone = 0x10,

			/// <summary>Field <see cref="photo"/> has a value</summary>
			has_photo = 0x20,

			/// <summary>Field <see cref="status"/> has a value</summary>
			has_status = 0x40,

			/// <summary>Whether this user indicates the currently logged in user</summary>
			self = 0x400,

			/// <summary>Whether this user is a contact <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag is set.</summary>
			contact = 0x800,

			/// <summary>Whether this user is a mutual contact. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag is set.</summary>
			mutual_contact = 0x1000,

			/// <summary>Whether the account of this user was deleted. <br/>Changes to this flag should invalidate the local <see cref="UserFull"/> cache for this user ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			deleted = 0x2000,

			/// <summary>Is this user a bot? <br/>Changes to this flag should invalidate the local <see cref="UserFull"/> cache for this user ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			bot = 0x4000,

			/// <summary>Can the bot see all messages in groups?</summary>
			bot_chat_history = 0x8000,

			/// <summary>Can the bot be added to groups?</summary>
			bot_nochats = 0x10000,

			/// <summary>Whether this user is verified</summary>
			verified = 0x20000,

			/// <summary>Access to this user must be restricted for the reason specified in <c>restriction_reason</c></summary>
			restricted = 0x40000,

			/// <summary>Field <see cref="bot_inline_placeholder"/> has a value</summary>
			has_bot_inline_placeholder = 0x80000,

			/// <summary>See <a href="https://corefork.telegram.org/api/min">min</a></summary>
			min = 0x100000,

			/// <summary>Whether the bot can request our geolocation in inline mode</summary>
			bot_inline_geo = 0x200000,

			/// <summary>Field <see cref="lang_code"/> has a value</summary>
			has_lang_code = 0x400000,

			/// <summary>Whether this is an official support user</summary>
			support = 0x800000,

			/// <summary>This may be a scam user</summary>
			scam = 0x1000000,

			/// <summary>If set and <c>min</c> is set, the value of <c>photo</c> can be used to update the local database, see the documentation of that flag for more info.</summary>
			apply_min_photo = 0x2000000,

			/// <summary>If set, this user was reported by many users as a fake or scam user: be careful when interacting with them.</summary>
			fake = 0x4000000,

			/// <summary>Whether this bot offers an <a href="https://corefork.telegram.org/api/bots/attach">attachment menu web app</a></summary>
			bot_attach_menu = 0x8000000,

			/// <summary>Whether this user is a Telegram Premium user <br/>Changes to this flag should invalidate the local <see cref="UserFull"/> cache for this user ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info. <br/>Changes to this flag if the <c>self</c> flag is set should also trigger the following calls, to refresh the respective caches: <br/>- The <see cref="SchemaExtensions.Help_GetConfig">Help_GetConfig</see> cache <br/>- The <see cref="SchemaExtensions.Messages_GetTopReactions">Messages_GetTopReactions</see> cache if the <c>bot</c> flag is <strong>not</strong> set</summary>
			premium = 0x10000000,

			/// <summary>Whether we installed the <a href="https://corefork.telegram.org/api/bots/attach">attachment menu web app</a> offered by this bot. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag is set.</summary>
			attach_menu_enabled = 0x20000000,

			/// <summary>Field <see cref="emoji_status"/> has a value</summary>
			has_emoji_status = 0x40000000,
		}

		[Flags]
		public enum Flags2 : uint
		{
			/// <summary>Field <see cref="usernames"/> has a value</summary>
			has_usernames = 0x1,

			/// <summary>Whether we can edit the profile picture, name, about text and description of this bot because we own it. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag is set. <br/>Changes to this flag (if <c>min</c> is not set) should invalidate the local <see cref="UserFull"/> cache for this user ID.</summary>
			bot_can_edit = 0x2,

			/// <summary>Whether we marked this user as a <a href="https://corefork.telegram.org/api/privacy">close friend, see here » for more info</a>. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag is set.</summary>
			close_friend = 0x4,

			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/stories#hiding-stories-of-other-users">hidden »</a> all active stories of this user. <br/>When updating the <a href="https://corefork.telegram.org/api/peers">local peer database</a>, do not apply changes to this field if the <c>min</c> flag is set.</summary>
			stories_hidden = 0x8,

			/// <summary>No stories from this user are visible.</summary>
			stories_unavailable = 0x10,

			/// <summary>Field <see cref="stories_max_id"/> has a value</summary>
			has_stories_max_id = 0x20,

			/// <summary>Field <see cref="color"/> has a value</summary>
			has_color = 0x100,

			/// <summary>Field <see cref="profile_color"/> has a value</summary>
			has_profile_color = 0x200,

			/// <summary>If set, we can only write to this user if they have already sent some messages to us, if we are subscribed to <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a>, or if they're a mutual contact (<see cref="User"/>.<c>mutual_contact</c>).  <br/>All the secondary conditions listed above must be checked separately to verify whether we can still write to the user, even if this flag is set (i.e. a mutual contact will have this flag set even if we can still write to them, and so on...); to avoid doing these extra checks if we haven't yet cached all the required information (for example while displaying the chat list in the sharing UI) the <see cref="SchemaExtensions.Users_GetIsPremiumRequiredToContact">Users_GetIsPremiumRequiredToContact</see> method may be invoked instead, passing the list of users currently visible in the UI, returning a list of booleans that directly specify whether we can or cannot write to each user; alternatively, the <see cref="UserFull"/>.<c>contact_require_premium</c> flag contains the same (fully checked, i.e. it's not just a copy of this flag) info returned by <see cref="SchemaExtensions.Users_GetIsPremiumRequiredToContact">Users_GetIsPremiumRequiredToContact</see>. <br/>To set this flag for ourselves invoke <see cref="SchemaExtensions.Account_SetGlobalPrivacySettings">Account_SetGlobalPrivacySettings</see>, setting the <c>settings.new_noncontact_peers_require_premium</c> flag.</summary>
			contact_require_premium = 0x400,

			/// <summary>Whether this bot can be <a href="https://corefork.telegram.org/api/business#connected-bots">connected to a user as specified here »</a>.</summary>
			bot_business = 0x800,

			/// <summary>Field <see cref="bot_active_users"/> has a value</summary>
			has_bot_active_users = 0x1000,

			/// <summary>If set, this bot has configured a <a href="https://corefork.telegram.org/api/bots/webapps#main-mini-apps">Main Mini App »</a>.</summary>
			bot_has_main_app = 0x2000,

			/// <summary>Field <see cref="bot_verification_icon"/> has a value</summary>
			has_bot_verification_icon = 0x4000,
		}
	}

	/// <summary>User profile photo.		<para>See <a href="https://corefork.telegram.org/constructor/userProfilePhoto"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/userProfilePhotoEmpty">userProfilePhotoEmpty</a></remarks>
	[TLDef(0x82D1F706)]
	public sealed partial class UserProfilePhoto : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		/// <summary>Identifier of the respective photo</summary>
		public long photo_id;

		/// <summary><a href="https://corefork.telegram.org/api/files#stripped-thumbnails">Stripped thumbnail</a></summary>
		[IfFlag(1)] public byte[] stripped_thumb;

		/// <summary>DC ID where the photo is stored</summary>
		public int dc_id;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>Whether an <a href="https://corefork.telegram.org/api/files#animated-profile-pictures">animated profile picture</a> is available for this user</summary>
			has_video = 0x1,

			/// <summary>Field <see cref="stripped_thumb"/> has a value</summary>
			has_stripped_thumb = 0x2,

			/// <summary>Whether this profile photo is only visible to us (i.e. it was set using <see cref="SchemaExtensions.Photos_UploadContactProfilePhoto">Photos_UploadContactProfilePhoto</see>).</summary>
			personal = 0x4,
		}
	}

	/// <summary>User online status		<para>See <a href="https://corefork.telegram.org/type/UserStatus"/></para>		<para>Derived classes: <see cref="UserStatusOnline"/>, <see cref="UserStatusOffline"/>, <see cref="UserStatusRecently"/>, <see cref="UserStatusLastWeek"/>, <see cref="UserStatusLastMonth"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/userStatusEmpty">userStatusEmpty</a></remarks>
	public abstract partial class UserStatus : IObject
	{
	}

	/// <summary>Online status of the user.		<para>See <a href="https://corefork.telegram.org/constructor/userStatusOnline"/></para></summary>
	[TLDef(0xEDB93949)]
	public sealed partial class UserStatusOnline : UserStatus
	{
		/// <summary>Time to expiration of the current online status</summary>
		public DateTime expires;
	}

	/// <summary>The user's offline status.		<para>See <a href="https://corefork.telegram.org/constructor/userStatusOffline"/></para></summary>
	[TLDef(0x008C703F)]
	public sealed partial class UserStatusOffline : UserStatus
	{
		/// <summary>Time the user was last seen online</summary>
		public int was_online;
	}

	/// <summary>Online status: last seen recently		<para>See <a href="https://corefork.telegram.org/constructor/userStatusRecently"/></para></summary>
	[TLDef(0x7B197DC8)]
	public sealed partial class UserStatusRecently : UserStatus
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, the exact user status of this user is actually available to us, but to view it we must first purchase a <a href="https://corefork.telegram.org/api/premium">Premium</a> subscription, or allow this user to see <em>our</em> exact last online status. See <see cref="PrivacyKey.StatusTimestamp">here »</see> for more info.</summary>
			by_me = 0x1,
		}
	}

	/// <summary>Online status: last seen last week		<para>See <a href="https://corefork.telegram.org/constructor/userStatusLastWeek"/></para></summary>
	[TLDef(0x541A1D1A)]
	public sealed partial class UserStatusLastWeek : UserStatus
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, the exact user status of this user is actually available to us, but to view it we must first purchase a <a href="https://corefork.telegram.org/api/premium">Premium</a> subscription, or allow this user to see <em>our</em> exact last online status. See <see cref="PrivacyKey.StatusTimestamp">here »</see> for more info.</summary>
			by_me = 0x1,
		}
	}

	/// <summary>Online status: last seen last month		<para>See <a href="https://corefork.telegram.org/constructor/userStatusLastMonth"/></para></summary>
	[TLDef(0x65899777)]
	public sealed partial class UserStatusLastMonth : UserStatus
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags]
		public enum Flags : uint
		{
			/// <summary>If set, the exact user status of this user is actually available to us, but to view it we must first purchase a <a href="https://corefork.telegram.org/api/premium">Premium</a> subscription, or allow this user to see <em>our</em> exact last online status. See <see cref="PrivacyKey.StatusTimestamp">here »</see> for more info.</summary>
			by_me = 0x1,
		}
	}
	
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/users.users"/></para></summary>
	[TLDef(0x62D706B8)]
	public partial class Users_Users : IObject
	{
		public Dictionary<long, User> users;
	}
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/users.usersSlice"/></para></summary>
	[TLDef(0x315A4974)]
	public sealed partial class Users_UsersSlice : Users_Users
	{
		public int count;
	}
	/// <summary>Contains information about a username.		<para>See <a href="https://corefork.telegram.org/constructor/username"/></para></summary>
    [TLDef(0xB4073647)]
    public sealed partial class Username : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary>The username.</summary>
    	public string username;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Whether the username is editable, meaning it wasn't bought on <a href="https://fragment.com">fragment</a>.</summary>
    		editable = 0x1,
    		/// <summary>Whether the username is active.</summary>
    		active = 0x2,
    	}
    }
	/// <summary>Extended user info		<para>See <a href="https://corefork.telegram.org/constructor/userFull"/></para></summary>
	[TLDef(0x4D975BBC)]
	public sealed partial class UserFull : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Extra bits of information, use <c>flags2.HasFlag(...)</c> to test for those</summary>
		public Flags2 flags2;
		/// <summary>User ID</summary>
		public long id;
		/// <summary>Bio of the user</summary>
		[IfFlag(1)] public string about;
		/// <summary>Peer settings</summary>
		public PeerSettings settings;
		/// <summary>Personal profile photo, to be shown instead of <c>profile_photo</c>.</summary>
		[IfFlag(21)] public PhotoBase personal_photo;
		/// <summary>Profile photo</summary>
		[IfFlag(2)] public PhotoBase profile_photo;
		/// <summary>Fallback profile photo, displayed if no photo is present in <c>profile_photo</c> or <c>personal_photo</c>, due to privacy settings.</summary>
		[IfFlag(22)] public PhotoBase fallback_photo;
		/// <summary>Notification settings</summary>
		public PeerNotifySettings notify_settings;
		/// <summary>For bots, info about the bot (bot commands, etc)</summary>
		[IfFlag(3)] public BotInfo bot_info;
		/// <summary>Message ID of the last <a href="https://corefork.telegram.org/api/pin">pinned message</a></summary>
		[IfFlag(6)] public int pinned_msg_id;
		/// <summary>Chats in common with this user</summary>
		public int common_chats_count;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(11)] public int folder_id;
		/// <summary>Time To Live of all messages in this chat; once a message is this many seconds old, it must be deleted.</summary>
		[IfFlag(14)] public int ttl_period;
		/// <summary>Emoji associated with chat theme</summary>
		[IfFlag(15)] public string theme_emoticon;
		/// <summary>Anonymized text to be shown instead of the user's name on forwarded messages</summary>
		[IfFlag(16)] public string private_forward_name;
		/// <summary>A <a href="https://corefork.telegram.org/api/rights#suggested-bot-rights">suggested set of administrator rights</a> for the bot, to be shown when adding the bot as admin to a group, see <a href="https://corefork.telegram.org/api/rights#suggested-bot-rights">here for more info on how to handle them »</a>.</summary>
		[IfFlag(17)] public ChatAdminRights bot_group_admin_rights;
		/// <summary>A <a href="https://corefork.telegram.org/api/rights#suggested-bot-rights">suggested set of administrator rights</a> for the bot, to be shown when adding the bot as admin to a channel, see <a href="https://corefork.telegram.org/api/rights#suggested-bot-rights">here for more info on how to handle them »</a>.</summary>
		[IfFlag(18)] public ChatAdminRights bot_broadcast_admin_rights;
		/// <summary>Telegram Premium subscriptions gift options</summary>
		[IfFlag(19)] public PremiumGiftOption[] premium_gifts;
		/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a> to use in the private chat with the user.</summary>
		[IfFlag(24)] public WallPaperBase wallpaper;
		/// <summary>Active <a href="https://corefork.telegram.org/api/stories">stories »</a></summary>
		[IfFlag(25)] public PeerStories stories;
		/// <summary><a href="https://corefork.telegram.org/api/business#opening-hours">Telegram Business working hours »</a>.</summary>
		[IfFlag(32)] public BusinessWorkHours business_work_hours;
		/// <summary><a href="https://corefork.telegram.org/api/business#location">Telegram Business location »</a>.</summary>
		[IfFlag(33)] public BusinessLocation business_location;
		/// <summary><a href="https://corefork.telegram.org/api/business#greeting-messages">Telegram Business greeting message »</a>.</summary>
		[IfFlag(34)] public BusinessGreetingMessage business_greeting_message;
		/// <summary><a href="https://corefork.telegram.org/api/business#away-messages">Telegram Business away message »</a>.</summary>
		[IfFlag(35)] public BusinessAwayMessage business_away_message;
		/// <summary>Specifies a custom <a href="https://corefork.telegram.org/api/business#business-introduction">Telegram Business profile introduction »</a>.</summary>
		[IfFlag(36)] public BusinessIntro business_intro;
		/// <summary>Contains info about the user's <a href="https://corefork.telegram.org/api/profile#birthday">birthday »</a>.</summary>
		[IfFlag(37)] public Birthday birthday;
		/// <summary>ID of the associated personal <a href="https://corefork.telegram.org/api/channel">channel »</a>, that should be shown in the <a href="https://corefork.telegram.org/api/profile#personal-channel">profile page</a>.</summary>
		[IfFlag(38)] public long personal_channel_id;
		/// <summary>ID of the latest message of the associated personal <a href="https://corefork.telegram.org/api/channel">channel »</a>, that should be previewed in the <a href="https://corefork.telegram.org/api/profile#personal-channel">profile page</a>.</summary>
		[IfFlag(38)] public int personal_channel_message;
		/// <summary>Number of <a href="https://corefork.telegram.org/api/gifts">gifts</a> the user has chosen to display on their profile</summary>
		[IfFlag(40)] public int stargifts_count;
		/// <summary>This bot has an active <a href="https://corefork.telegram.org/api/bots/referrals">referral program »</a></summary>
		[IfFlag(43)] public StarRefProgram starref_program;
		[IfFlag(44)] public BotVerification bot_verification;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether you have blocked this user</summary>
			blocked = 0x1,
			/// <summary>Field <see cref="about"/> has a value</summary>
			has_about = 0x2,
			/// <summary>Field <see cref="profile_photo"/> has a value</summary>
			has_profile_photo = 0x4,
			/// <summary>Field <see cref="bot_info"/> has a value</summary>
			has_bot_info = 0x8,
			/// <summary>Whether this user can make VoIP calls</summary>
			phone_calls_available = 0x10,
			/// <summary>Whether this user's privacy settings allow you to call them</summary>
			phone_calls_private = 0x20,
			/// <summary>Field <see cref="pinned_msg_id"/> has a value</summary>
			has_pinned_msg_id = 0x40,
			/// <summary>Whether you can pin messages in the chat with this user, you can do this only for a chat with yourself</summary>
			can_pin_message = 0x80,
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x800,
			/// <summary>Whether <a href="https://corefork.telegram.org/api/scheduled-messages">scheduled messages</a> are available</summary>
			has_scheduled = 0x1000,
			/// <summary>Whether the user can receive video calls</summary>
			video_calls_available = 0x2000,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x4000,
			/// <summary>Field <see cref="theme_emoticon"/> has a value</summary>
			has_theme_emoticon = 0x8000,
			/// <summary>Field <see cref="private_forward_name"/> has a value</summary>
			has_private_forward_name = 0x10000,
			/// <summary>Field <see cref="bot_group_admin_rights"/> has a value</summary>
			has_bot_group_admin_rights = 0x20000,
			/// <summary>Field <see cref="bot_broadcast_admin_rights"/> has a value</summary>
			has_bot_broadcast_admin_rights = 0x40000,
			/// <summary>Field <see cref="premium_gifts"/> has a value</summary>
			has_premium_gifts = 0x80000,
			/// <summary>Whether this user doesn't allow sending voice messages in a private chat with them</summary>
			voice_messages_forbidden = 0x100000,
			/// <summary>Field <see cref="personal_photo"/> has a value</summary>
			has_personal_photo = 0x200000,
			/// <summary>Field <see cref="fallback_photo"/> has a value</summary>
			has_fallback_photo = 0x400000,
			/// <summary>Whether the <a href="https://corefork.telegram.org/api/translation">real-time chat translation popup</a> should be hidden.</summary>
			translations_disabled = 0x800000,
			/// <summary>Field <see cref="wallpaper"/> has a value</summary>
			has_wallpaper = 0x1000000,
			/// <summary>Field <see cref="stories"/> has a value</summary>
			has_stories = 0x2000000,
			/// <summary>Whether this user has some <a href="https://corefork.telegram.org/api/stories#pinned-or-archived-stories">pinned stories</a>.</summary>
			stories_pinned_available = 0x4000000,
			/// <summary>Whether we've <a href="https://corefork.telegram.org/api/block">blocked this user, preventing them from seeing our stories »</a>.</summary>
			blocked_my_stories_from = 0x8000000,
			/// <summary>Whether the other user has chosen a custom wallpaper for us using <see cref="SchemaExtensions.Messages_SetChatWallPaper">Messages_SetChatWallPaper</see> and the <c>for_both</c> flag, see <a href="https://corefork.telegram.org/api/wallpapers#installing-wallpapers-in-a-specific-chat-or-channel">here »</a> for more info.</summary>
			wallpaper_overridden = 0x10000000,
			/// <summary>If set, we cannot write to this user: subscribe to <a href="https://corefork.telegram.org/api/premium">Telegram Premium</a> to get permission to write to this user. <br/>To set this flag for ourselves invoke <see cref="SchemaExtensions.Account_SetGlobalPrivacySettings">Account_SetGlobalPrivacySettings</see>, setting the <c>settings.new_noncontact_peers_require_premium</c> flag, see <a href="https://corefork.telegram.org/api/privacy#require-premium-for-new-non-contact-users">here »</a> for more info.</summary>
			contact_require_premium = 0x20000000,
			/// <summary>If set, we cannot fetch the exact read date of messages we send to this user using <see cref="SchemaExtensions.Messages_GetOutboxReadDate">Messages_GetOutboxReadDate</see>.  <br/>The exact read date of messages might still be unavailable for other reasons, see <see cref="SchemaExtensions.Messages_GetOutboxReadDate">Messages_GetOutboxReadDate</see> for more info.  <br/>To set this flag for ourselves invoke <see cref="SchemaExtensions.Account_SetGlobalPrivacySettings">Account_SetGlobalPrivacySettings</see>, setting the <c>settings.hide_read_marks</c> flag.</summary>
			read_dates_private = 0x40000000,
		}

		[Flags] public enum Flags2 : uint
		{
			/// <summary>Field <see cref="business_work_hours"/> has a value</summary>
			has_business_work_hours = 0x1,
			/// <summary>Field <see cref="business_location"/> has a value</summary>
			has_business_location = 0x2,
			/// <summary>Field <see cref="business_greeting_message"/> has a value</summary>
			has_business_greeting_message = 0x4,
			/// <summary>Field <see cref="business_away_message"/> has a value</summary>
			has_business_away_message = 0x8,
			/// <summary>Field <see cref="business_intro"/> has a value</summary>
			has_business_intro = 0x10,
			/// <summary>Field <see cref="birthday"/> has a value</summary>
			has_birthday = 0x20,
			/// <summary>Fields <see cref="personal_channel_id"/> and <see cref="personal_channel_message"/> have a value</summary>
			has_personal_channel_id = 0x40,
			/// <summary>Whether ads were re-enabled for the current account (only accessible to the currently logged-in user), see <a href="https://corefork.telegram.org/api/business#re-enable-ads">here »</a> for more info.</summary>
			sponsored_enabled = 0x80,
			/// <summary>Field <see cref="stargifts_count"/> has a value</summary>
			has_stargifts_count = 0x100,
			/// <summary>If set, this user can view <a href="https://corefork.telegram.org/api/revenue#revenue-statistics">ad revenue statistics »</a> for this bot.</summary>
			can_view_revenue = 0x200,
			/// <summary>If set, this is a bot that can <a href="https://corefork.telegram.org/api/emoji-status#setting-an-emoji-status-from-a-bot">change our emoji status »</a></summary>
			bot_can_manage_emoji_status = 0x400,
			/// <summary>Field <see cref="starref_program"/> has a value</summary>
			has_starref_program = 0x800,
			/// <summary>Field <see cref="bot_verification"/> has a value</summary>
			has_bot_verification = 0x1000,
		}
	}
	
	/// <summary>Full user information		<para>See <a href="https://corefork.telegram.org/constructor/users.userFull"/></para></summary>
    [TLDef(0x3B6D152E)]
    public sealed partial class Users_UserFull : IObject, IPeerResolver
    {
    	/// <summary>Full user information</summary>
    	public UserFull full_user;
    	/// <summary>Mentioned chats</summary>
    	public Dictionary<long, ChatBase> chats;
    	/// <summary>Mentioned users</summary>
    	public Dictionary<long, User> users;
    	/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
    	public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
    }
    

}