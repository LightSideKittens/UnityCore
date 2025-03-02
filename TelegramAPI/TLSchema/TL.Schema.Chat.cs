using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Object defines a group.		<para>See <a href="https://corefork.telegram.org/type/Chat"/></para>		<para>Derived classes: <see cref="ChatEmpty"/>, <see cref="Chat"/>, <see cref="ChatForbidden"/>, <see cref="Channel"/>, <see cref="ChannelForbidden"/></para></summary>
    public abstract partial class ChatBase : IObject
    {
        /// <summary>ID of the group, see <a href="https://corefork.telegram.org/api/peers#peer-id">here »</a> for more info</summary>
        public virtual long ID => default;
        /// <summary>Title</summary>
        public virtual string Title => default;
    }
    
    /// <summary>Empty constructor, group doesn't exist		<para>See <a href="https://corefork.telegram.org/constructor/chatEmpty"/></para></summary>
	[TLDef(0x29562865)]
	public sealed partial class ChatEmpty : ChatBase
	{
		/// <summary>Group identifier</summary>
		public long id;

		/// <summary>Group identifier</summary>
		public override long ID => id;
	}
	/// <summary>Info about a group.		<para>See <a href="https://corefork.telegram.org/constructor/chat"/></para></summary>
	[TLDef(0x41CBF256)]
	public sealed partial class Chat : ChatBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the group, see <a href="https://corefork.telegram.org/api/peers#peer-id">here »</a> for more info</summary>
		public long id;
		/// <summary>Title</summary>
		public string title;
		/// <summary>Chat photo</summary>
		public ChatPhoto photo;
		/// <summary>Participant count</summary>
		public int participants_count;
		/// <summary>Date of creation of the group</summary>
		public DateTime date;
		/// <summary>Used in basic groups to reorder updates and make sure that all of them were received.</summary>
		public int version;
		/// <summary>Means this chat was <a href="https://corefork.telegram.org/api/channel">upgraded</a> to a supergroup</summary>
		[IfFlag(6)] public InputChannelBase migrated_to;
		/// <summary><a href="https://corefork.telegram.org/api/rights">Admin rights</a> of the user in the group</summary>
		[IfFlag(14)] public ChatAdminRights admin_rights;
		/// <summary><a href="https://corefork.telegram.org/api/rights">Default banned rights</a> of all users in the group</summary>
		[IfFlag(18)] public ChatBannedRights default_banned_rights;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the current user is the creator of the group</summary>
			creator = 0x1,
			/// <summary>Whether the current user has left the group</summary>
			left = 0x4,
			/// <summary>Whether the group was <a href="https://corefork.telegram.org/api/channel">migrated</a></summary>
			deactivated = 0x20,
			/// <summary>Field <see cref="migrated_to"/> has a value</summary>
			has_migrated_to = 0x40,
			/// <summary>Field <see cref="admin_rights"/> has a value</summary>
			has_admin_rights = 0x4000,
			/// <summary>Field <see cref="default_banned_rights"/> has a value</summary>
			has_default_banned_rights = 0x40000,
			/// <summary>Whether a group call is currently active</summary>
			call_active = 0x800000,
			/// <summary>Whether there's anyone in the group call</summary>
			call_not_empty = 0x1000000,
			/// <summary>Whether this group is <a href="https://telegram.org/blog/protected-content-delete-by-date-and-more">protected</a>, thus does not allow forwarding messages from it</summary>
			noforwards = 0x2000000,
		}

		/// <summary>ID of the group, see <a href="https://corefork.telegram.org/api/peers#peer-id">here »</a> for more info</summary>
		public override long ID => id;
		/// <summary>Title</summary>
		public override string Title => title;
	}
	/// <summary>A group to which the user has no access. E.g., because the user was kicked from the group.		<para>See <a href="https://corefork.telegram.org/constructor/chatForbidden"/></para></summary>
	[TLDef(0x6592A1A7)]
	public sealed partial class ChatForbidden : ChatBase
	{
		/// <summary>User identifier</summary>
		public long id;
		/// <summary>Group name</summary>
		public string title;

		/// <summary>User identifier</summary>
		public override long ID => id;
		/// <summary>Group name</summary>
		public override string Title => title;
	}
	/// <summary>Channel/supergroup info		<para>See <a href="https://corefork.telegram.org/constructor/channel"/></para></summary>
	[TLDef(0xE00998B7)]
	public sealed partial class Channel : ChatBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Extra bits of information, use <c>flags2.HasFlag(...)</c> to test for those</summary>
		public Flags2 flags2;
		/// <summary>ID of the channel, see <a href="https://corefork.telegram.org/api/peers#peer-id">here »</a> for more info</summary>
		public long id;
		/// <summary>Access hash, see <a href="https://corefork.telegram.org/api/peers#access-hash">here »</a> for more info</summary>
		[IfFlag(13)] public long access_hash;
		/// <summary>Title</summary>
		public string title;
		/// <summary>Main active username.</summary>
		[IfFlag(6)] public string username;
		/// <summary>Profile photo</summary>
		public ChatPhoto photo;
		/// <summary>Date when the user joined the supergroup/channel, or if the user isn't a member, its creation date</summary>
		public DateTime date;
		/// <summary>Contains the reason why access to this channel must be restricted. <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
		[IfFlag(9)] public RestrictionReason[] restriction_reason;
		/// <summary>Admin rights of the user in this channel (see <a href="https://corefork.telegram.org/api/rights">rights</a>)</summary>
		[IfFlag(14)] public ChatAdminRights admin_rights;
		/// <summary>Banned rights of the user in this channel (see <a href="https://corefork.telegram.org/api/rights">rights</a>)</summary>
		[IfFlag(15)] public ChatBannedRights banned_rights;
		/// <summary>Default chat rights (see <a href="https://corefork.telegram.org/api/rights">rights</a>)</summary>
		[IfFlag(18)] public ChatBannedRights default_banned_rights;
		/// <summary>Participant count</summary>
		[IfFlag(17)] public int participants_count;
		/// <summary>Additional usernames</summary>
		[IfFlag(32)] public Username[] usernames;
		/// <summary>ID of the maximum read <a href="https://corefork.telegram.org/api/stories">story</a>.</summary>
		[IfFlag(36)] public int stories_max_id;
		/// <summary>The channel's <a href="https://corefork.telegram.org/api/colors">accent color</a>.</summary>
		[IfFlag(39)] public PeerColor color;
		/// <summary>The channel's <a href="https://corefork.telegram.org/api/colors">profile color</a>.</summary>
		[IfFlag(40)] public PeerColor profile_color;
		/// <summary><a href="https://corefork.telegram.org/api/emoji-status">Emoji status</a></summary>
		[IfFlag(41)] public EmojiStatusBase emoji_status;
		/// <summary><a href="https://corefork.telegram.org/api/boost">Boost level</a>. <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
		[IfFlag(42)] public int level;
		/// <summary>Expiration date of the <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription »</a> the current user has bought to gain access to this channel.</summary>
		[IfFlag(43)] public DateTime subscription_until_date;
		[IfFlag(45)] public long bot_verification_icon;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the current user is the creator of this channel</summary>
			creator = 0x1,
			/// <summary>Whether the current user has left or is not a member of this channel</summary>
			left = 0x4,
			/// <summary>Is this a channel?</summary>
			broadcast = 0x20,
			/// <summary>Field <see cref="username"/> has a value</summary>
			has_username = 0x40,
			/// <summary>Is this channel verified by telegram?</summary>
			verified = 0x80,
			/// <summary>Is this a supergroup? <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			megagroup = 0x100,
			/// <summary>Whether viewing/writing in this channel for a reason (see <c>restriction_reason</c>)</summary>
			restricted = 0x200,
			/// <summary>Whether signatures are enabled (channels)</summary>
			signatures = 0x800,
			/// <summary>See <a href="https://corefork.telegram.org/api/min">min</a></summary>
			min = 0x1000,
			/// <summary>Field <see cref="access_hash"/> has a value</summary>
			has_access_hash = 0x2000,
			/// <summary>Field <see cref="admin_rights"/> has a value</summary>
			has_admin_rights = 0x4000,
			/// <summary>Field <see cref="banned_rights"/> has a value</summary>
			has_banned_rights = 0x8000,
			/// <summary>Field <see cref="participants_count"/> has a value</summary>
			has_participants_count = 0x20000,
			/// <summary>Field <see cref="default_banned_rights"/> has a value</summary>
			has_default_banned_rights = 0x40000,
			/// <summary>This channel/supergroup is probably a scam <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			scam = 0x80000,
			/// <summary>Whether this channel has a linked <a href="https://corefork.telegram.org/api/discussion">discussion group »</a> (or this supergroup is a channel's discussion group). The actual ID of the linked channel/supergroup is contained in <see cref="ChannelFull"/>.<c>linked_chat_id</c>. <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			has_link = 0x100000,
			/// <summary>Whether this chanel has a geoposition</summary>
			has_geo = 0x200000,
			/// <summary>Whether slow mode is enabled for groups to prevent flood in chat. <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			slowmode_enabled = 0x400000,
			/// <summary>Whether a group call or livestream is currently active</summary>
			call_active = 0x800000,
			/// <summary>Whether there's anyone in the group call or livestream</summary>
			call_not_empty = 0x1000000,
			/// <summary>If set, this <a href="https://corefork.telegram.org/api/channel">supergroup/channel</a> was reported by many users as a fake or scam: be careful when interacting with it. <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			fake = 0x2000000,
			/// <summary>Whether this <a href="https://corefork.telegram.org/api/channel">supergroup</a> is a gigagroup<br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			gigagroup = 0x4000000,
			/// <summary>Whether this channel or group is <a href="https://telegram.org/blog/protected-content-delete-by-date-and-more">protected</a>, thus does not allow forwarding messages from it</summary>
			noforwards = 0x8000000,
			/// <summary>Whether a user needs to join the supergroup before they can send messages: can be false only for <a href="https://corefork.telegram.org/api/discussion">discussion groups »</a>, toggle using <see cref="SchemaExtensions.Channels_ToggleJoinToSend">Channels_ToggleJoinToSend</see><br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			join_to_send = 0x10000000,
			/// <summary>Whether a user's join request will have to be <a href="https://corefork.telegram.org/api/invites#join-requests">approved by administrators</a>, toggle using <see cref="SchemaExtensions.Channels_ToggleJoinRequest">Channels_ToggleJoinRequest</see><br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			join_request = 0x20000000,
			/// <summary>Whether this supergroup is a <a href="https://corefork.telegram.org/api/forum">forum</a>. <br/>Changes to this flag should invalidate the local <see cref="ChannelFull"/> cache for this channel/supergroup ID, see <a href="https://corefork.telegram.org/api/peers#full-info-database">here »</a> for more info.</summary>
			forum = 0x40000000,
		}

		[Flags] public enum Flags2 : uint
		{
			/// <summary>Field <see cref="usernames"/> has a value</summary>
			has_usernames = 0x1,
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/stories#hiding-stories-of-other-users">hidden all stories posted by this channel »</a>.</summary>
			stories_hidden = 0x2,
			/// <summary>If set, indicates that the <c>stories_hidden</c> flag was not populated, and its value must cannot be relied on; use the previously cached value, or re-fetch the constructor using <see cref="SchemaExtensions.Channels_GetChannels">Channels_GetChannels</see> to obtain the latest value of the <c>stories_hidden</c> flag.</summary>
			stories_hidden_min = 0x4,
			/// <summary>No stories from the channel are visible.</summary>
			stories_unavailable = 0x8,
			/// <summary>Field <see cref="stories_max_id"/> has a value</summary>
			has_stories_max_id = 0x10,
			/// <summary>Field <see cref="color"/> has a value</summary>
			has_color = 0x80,
			/// <summary>Field <see cref="profile_color"/> has a value</summary>
			has_profile_color = 0x100,
			/// <summary>Field <see cref="emoji_status"/> has a value</summary>
			has_emoji_status = 0x200,
			/// <summary>Field <see cref="level"/> has a value</summary>
			has_level = 0x400,
			/// <summary>Field <see cref="subscription_until_date"/> has a value</summary>
			has_subscription_until_date = 0x800,
			/// <summary>If set, messages sent by admins to this channel will link to the admin's profile (just like with groups).</summary>
			signature_profiles = 0x1000,
			/// <summary>Field <see cref="bot_verification_icon"/> has a value</summary>
			has_bot_verification_icon = 0x2000,
		}

		/// <summary>ID of the channel, see <a href="https://corefork.telegram.org/api/peers#peer-id">here »</a> for more info</summary>
		public override long ID => id;
		/// <summary>Title</summary>
		public override string Title => title;
	}
	/// <summary>Indicates a channel/supergroup we can't access because we were banned, or for some other reason.		<para>See <a href="https://corefork.telegram.org/constructor/channelForbidden"/></para></summary>
	[TLDef(0x17D493D5)]
	public sealed partial class ChannelForbidden : ChatBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Channel ID</summary>
		public long id;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>Title</summary>
		public string title;
		/// <summary>The ban is valid until the specified date</summary>
		[IfFlag(16)] public DateTime until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Is this a channel</summary>
			broadcast = 0x20,
			/// <summary>Is this a supergroup</summary>
			megagroup = 0x100,
			/// <summary>Field <see cref="until_date"/> has a value</summary>
			has_until_date = 0x10000,
		}

		/// <summary>Channel ID</summary>
		public override long ID => id;
		/// <summary>Title</summary>
		public override string Title => title;
	}

	/// <summary>Full info about a <a href="https://corefork.telegram.org/api/channel#channels">channel</a>, <a href="https://corefork.telegram.org/api/channel#supergroups">supergroup</a>, <a href="https://corefork.telegram.org/api/channel#gigagroups">gigagroup</a> or <a href="https://corefork.telegram.org/api/channel#basic-groups">basic group</a>.		<para>See <a href="https://corefork.telegram.org/type/ChatFull"/></para>		<para>Derived classes: <see cref="ChatFull"/>, <see cref="ChannelFull"/></para></summary>
	public abstract partial class ChatFullBase : IObject
	{
		/// <summary>ID of the chat</summary>
		public virtual long ID => default;
		/// <summary>About string for this chat</summary>
		public virtual string About => default;
		/// <summary>Chat photo</summary>
		public virtual PhotoBase ChatPhoto => default;
		/// <summary>Notification settings</summary>
		public virtual PeerNotifySettings NotifySettings => default;
		/// <summary>Chat invite</summary>
		public virtual ExportedChatInvite ExportedInvite => default;
		/// <summary>Info about bots that are in this chat</summary>
		public virtual BotInfo[] BotInfo => default;
		/// <summary>Message ID of the last <a href="https://corefork.telegram.org/api/pin">pinned message</a></summary>
		public virtual int PinnedMsg => default;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public virtual int Folder => default;
		/// <summary>Group call information</summary>
		public virtual InputGroupCall Call => default;
		/// <summary>Time-To-Live of messages sent by the current user to this chat</summary>
		public virtual int TtlPeriod => default;
		/// <summary>When using <see cref="SchemaExtensions.Phone_GetGroupCallJoinAs">Phone_GetGroupCallJoinAs</see> to get a list of peers that can be used to join a group call, this field indicates the peer that should be selected by default.</summary>
		public virtual Peer GroupcallDefaultJoinAs => default;
		/// <summary>Emoji representing a specific chat theme</summary>
		public virtual string ThemeEmoticon => default;
		/// <summary>Pending <a href="https://corefork.telegram.org/api/invites#join-requests">join requests »</a></summary>
		public virtual int RequestsPending => default;
		/// <summary>IDs of users who requested to join recently</summary>
		public virtual long[] RecentRequesters => default;
		/// <summary>Allowed <a href="https://corefork.telegram.org/api/reactions">message reactions »</a></summary>
		public virtual ChatReactions AvailableReactions => default;
		/// <summary>This flag may be used to impose a custom limit of unique reactions (i.e. a customizable version of <a href="https://corefork.telegram.org/api/config#reactions-uniq-max">appConfig.reactions_uniq_max</a>).</summary>
		public virtual int ReactionsLimit => default;
	}
	/// <summary>Full info about a <a href="https://corefork.telegram.org/api/channel#basic-groups">basic group</a>.		<para>See <a href="https://corefork.telegram.org/constructor/chatFull"/></para></summary>
	[TLDef(0x2633421B)]
	public sealed partial class ChatFull : ChatFullBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the chat</summary>
		public long id;
		/// <summary>About string for this chat</summary>
		public string about;
		/// <summary>Participant list</summary>
		public ChatParticipantsBase participants;
		/// <summary>Chat photo</summary>
		[IfFlag(2)] public PhotoBase chat_photo;
		/// <summary>Notification settings</summary>
		public PeerNotifySettings notify_settings;
		/// <summary>Chat invite</summary>
		[IfFlag(13)] public ExportedChatInvite exported_invite;
		/// <summary>Info about bots that are in this chat</summary>
		[IfFlag(3)] public BotInfo[] bot_info;
		/// <summary>Message ID of the last <a href="https://corefork.telegram.org/api/pin">pinned message</a></summary>
		[IfFlag(6)] public int pinned_msg_id;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(11)] public int folder_id;
		/// <summary>Group call information</summary>
		[IfFlag(12)] public InputGroupCall call;
		/// <summary>Time-To-Live of messages sent by the current user to this chat</summary>
		[IfFlag(14)] public int ttl_period;
		/// <summary>When using <see cref="SchemaExtensions.Phone_GetGroupCallJoinAs">Phone_GetGroupCallJoinAs</see> to get a list of peers that can be used to join a group call, this field indicates the peer that should be selected by default.</summary>
		[IfFlag(15)] public Peer groupcall_default_join_as;
		/// <summary>Emoji representing a specific chat theme</summary>
		[IfFlag(16)] public string theme_emoticon;
		/// <summary>Pending <a href="https://corefork.telegram.org/api/invites#join-requests">join requests »</a></summary>
		[IfFlag(17)] public int requests_pending;
		/// <summary>IDs of users who requested to join recently</summary>
		[IfFlag(17)] public long[] recent_requesters;
		/// <summary>Allowed <a href="https://corefork.telegram.org/api/reactions">message reactions »</a></summary>
		[IfFlag(18)] public ChatReactions available_reactions;
		/// <summary>This flag may be used to impose a custom limit of unique reactions (i.e. a customizable version of <a href="https://corefork.telegram.org/api/config#reactions-uniq-max">appConfig.reactions_uniq_max</a>).</summary>
		[IfFlag(20)] public int reactions_limit;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="chat_photo"/> has a value</summary>
			has_chat_photo = 0x4,
			/// <summary>Field <see cref="bot_info"/> has a value</summary>
			has_bot_info = 0x8,
			/// <summary>Field <see cref="pinned_msg_id"/> has a value</summary>
			has_pinned_msg_id = 0x40,
			/// <summary>Can we change the username of this chat</summary>
			can_set_username = 0x80,
			/// <summary>Whether <a href="https://corefork.telegram.org/api/scheduled-messages">scheduled messages</a> are available</summary>
			has_scheduled = 0x100,
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x800,
			/// <summary>Field <see cref="call"/> has a value</summary>
			has_call = 0x1000,
			/// <summary>Field <see cref="exported_invite"/> has a value</summary>
			has_exported_invite = 0x2000,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x4000,
			/// <summary>Field <see cref="groupcall_default_join_as"/> has a value</summary>
			has_groupcall_default_join_as = 0x8000,
			/// <summary>Field <see cref="theme_emoticon"/> has a value</summary>
			has_theme_emoticon = 0x10000,
			/// <summary>Fields <see cref="requests_pending"/> and <see cref="recent_requesters"/> have a value</summary>
			has_requests_pending = 0x20000,
			/// <summary>Field <see cref="available_reactions"/> has a value</summary>
			has_available_reactions = 0x40000,
			/// <summary>Whether the <a href="https://corefork.telegram.org/api/translation">real-time chat translation popup</a> should be hidden.</summary>
			translations_disabled = 0x80000,
			/// <summary>Field <see cref="reactions_limit"/> has a value</summary>
			has_reactions_limit = 0x100000,
		}

		/// <summary>ID of the chat</summary>
		public override long ID => id;
		/// <summary>About string for this chat</summary>
		public override string About => about;
		/// <summary>Chat photo</summary>
		public override PhotoBase ChatPhoto => chat_photo;
		/// <summary>Notification settings</summary>
		public override PeerNotifySettings NotifySettings => notify_settings;
		/// <summary>Chat invite</summary>
		public override ExportedChatInvite ExportedInvite => exported_invite;
		/// <summary>Info about bots that are in this chat</summary>
		public override BotInfo[] BotInfo => bot_info;
		/// <summary>Message ID of the last <a href="https://corefork.telegram.org/api/pin">pinned message</a></summary>
		public override int PinnedMsg => pinned_msg_id;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public override int Folder => folder_id;
		/// <summary>Group call information</summary>
		public override InputGroupCall Call => call;
		/// <summary>Time-To-Live of messages sent by the current user to this chat</summary>
		public override int TtlPeriod => ttl_period;
		/// <summary>When using <see cref="SchemaExtensions.Phone_GetGroupCallJoinAs">Phone_GetGroupCallJoinAs</see> to get a list of peers that can be used to join a group call, this field indicates the peer that should be selected by default.</summary>
		public override Peer GroupcallDefaultJoinAs => groupcall_default_join_as;
		/// <summary>Emoji representing a specific chat theme</summary>
		public override string ThemeEmoticon => theme_emoticon;
		/// <summary>Pending <a href="https://corefork.telegram.org/api/invites#join-requests">join requests »</a></summary>
		public override int RequestsPending => requests_pending;
		/// <summary>IDs of users who requested to join recently</summary>
		public override long[] RecentRequesters => recent_requesters;
		/// <summary>Allowed <a href="https://corefork.telegram.org/api/reactions">message reactions »</a></summary>
		public override ChatReactions AvailableReactions => available_reactions;
		/// <summary>This flag may be used to impose a custom limit of unique reactions (i.e. a customizable version of <a href="https://corefork.telegram.org/api/config#reactions-uniq-max">appConfig.reactions_uniq_max</a>).</summary>
		public override int ReactionsLimit => reactions_limit;
	}
	/// <summary>Full info about a <a href="https://corefork.telegram.org/api/channel#channels">channel</a>, <a href="https://corefork.telegram.org/api/channel#supergroups">supergroup</a> or <a href="https://corefork.telegram.org/api/channel#gigagroups">gigagroup</a>.		<para>See <a href="https://corefork.telegram.org/constructor/channelFull"/></para></summary>
	[TLDef(0x52D6806B)]
	public sealed partial class ChannelFull : ChatFullBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Extra bits of information, use <c>flags2.HasFlag(...)</c> to test for those</summary>
		public Flags2 flags2;
		/// <summary>ID of the channel</summary>
		public long id;
		/// <summary>Info about the channel</summary>
		public string about;
		/// <summary>Number of participants of the channel</summary>
		[IfFlag(0)] public int participants_count;
		/// <summary>Number of channel admins</summary>
		[IfFlag(1)] public int admins_count;
		/// <summary>Number of users <a href="https://corefork.telegram.org/api/rights">kicked</a> from the channel</summary>
		[IfFlag(2)] public int kicked_count;
		/// <summary>Number of users <a href="https://corefork.telegram.org/api/rights">banned</a> from the channel</summary>
		[IfFlag(2)] public int banned_count;
		/// <summary>Number of users currently online</summary>
		[IfFlag(13)] public int online_count;
		/// <summary>Position up to which all incoming messages are read.</summary>
		public int read_inbox_max_id;
		/// <summary>Position up to which all outgoing messages are read.</summary>
		public int read_outbox_max_id;
		/// <summary>Count of unread messages</summary>
		public int unread_count;
		/// <summary>Channel picture</summary>
		public PhotoBase chat_photo;
		/// <summary>Notification settings</summary>
		public PeerNotifySettings notify_settings;
		/// <summary>Invite link</summary>
		[IfFlag(23)] public ExportedChatInvite exported_invite;
		/// <summary>Info about bots in the channel/supergroup</summary>
		public BotInfo[] bot_info;
		/// <summary>The chat ID from which this group was <a href="https://corefork.telegram.org/api/channel">migrated</a></summary>
		[IfFlag(4)] public long migrated_from_chat_id;
		/// <summary>The message ID in the original chat at which this group was <a href="https://corefork.telegram.org/api/channel">migrated</a></summary>
		[IfFlag(4)] public int migrated_from_max_id;
		/// <summary>Message ID of the last <a href="https://corefork.telegram.org/api/pin">pinned message</a></summary>
		[IfFlag(5)] public int pinned_msg_id;
		/// <summary>Associated stickerset</summary>
		[IfFlag(8)] public StickerSet stickerset;
		/// <summary>Identifier of a maximum unavailable message in a channel due to hidden history.</summary>
		[IfFlag(9)] public int available_min_id;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(11)] public int folder_id;
		/// <summary>ID of the linked <a href="https://corefork.telegram.org/api/discussion">discussion chat</a> for channels (and vice versa, the ID of the linked channel for discussion chats).</summary>
		[IfFlag(14)] public long linked_chat_id;
		/// <summary>Location of the geogroup</summary>
		[IfFlag(15)] public ChannelLocation location;
		/// <summary>If specified, users in supergroups will only be able to send one message every <c>slowmode_seconds</c> seconds</summary>
		[IfFlag(17)] public int slowmode_seconds;
		/// <summary>Indicates when the user will be allowed to send another message in the supergroup (unixtime)</summary>
		[IfFlag(18)] public DateTime slowmode_next_send_date;
		/// <summary>If set, specifies the DC to use for fetching channel statistics</summary>
		[IfFlag(12)] public int stats_dc;
		/// <summary>Latest <a href="https://corefork.telegram.org/api/updates">PTS</a> for this channel</summary>
		public int pts;
		/// <summary>Livestream or group call information</summary>
		[IfFlag(21)] public InputGroupCall call;
		/// <summary>Time-To-Live of messages in this channel or supergroup</summary>
		[IfFlag(24)] public int ttl_period;
		/// <summary>A list of <a href="https://corefork.telegram.org/api/config#suggestions">suggested actions</a> for the supergroup admin, <a href="https://corefork.telegram.org/api/config#suggestions">see here for more info »</a>.</summary>
		[IfFlag(25)] public string[] pending_suggestions;
		/// <summary>When using <see cref="SchemaExtensions.Phone_GetGroupCallJoinAs">Phone_GetGroupCallJoinAs</see> to get a list of peers that can be used to join a group call, this field indicates the peer that should be selected by default.</summary>
		[IfFlag(26)] public Peer groupcall_default_join_as;
		/// <summary>Emoji representing a specific chat theme</summary>
		[IfFlag(27)] public string theme_emoticon;
		/// <summary>Pending <a href="https://corefork.telegram.org/api/invites#join-requests">join requests »</a></summary>
		[IfFlag(28)] public int requests_pending;
		/// <summary>IDs of users who requested to join recently</summary>
		[IfFlag(28)] public long[] recent_requesters;
		/// <summary>Default peer used for sending messages to this channel</summary>
		[IfFlag(29)] public Peer default_send_as;
		/// <summary>Allowed <a href="https://corefork.telegram.org/api/reactions">message reactions »</a></summary>
		[IfFlag(30)] public ChatReactions available_reactions;
		/// <summary>This flag may be used to impose a custom limit of unique reactions (i.e. a customizable version of <a href="https://corefork.telegram.org/api/config#reactions-uniq-max">appConfig.reactions_uniq_max</a>).</summary>
		[IfFlag(45)] public int reactions_limit;
		/// <summary>Channel <a href="https://corefork.telegram.org/api/stories">stories</a></summary>
		[IfFlag(36)] public PeerStories stories;
		/// <summary><a href="https://corefork.telegram.org/api/wallpapers">Wallpaper</a></summary>
		[IfFlag(39)] public WallPaperBase wallpaper;
		/// <summary>The number of <a href="https://corefork.telegram.org/api/boost">boosts</a> the current user has applied to the current <em>supergroup</em>.</summary>
		[IfFlag(40)] public int boosts_applied;
		/// <summary>The number of <a href="https://corefork.telegram.org/api/boost">boosts</a> this <em>supergroup</em> requires to bypass slowmode and other restrictions, see <a href="https://corefork.telegram.org/api/boost#bypass-slowmode-and-chat-restrictions">here »</a> for more info.</summary>
		[IfFlag(41)] public int boosts_unrestrict;
		/// <summary><a href="https://corefork.telegram.org/api/custom-emoji">Custom emoji stickerset</a> associated to the current <em>supergroup</em>, set using <see cref="SchemaExtensions.Channels_SetEmojiStickers">Channels_SetEmojiStickers</see> after reaching the appropriate boost level, see <a href="https://corefork.telegram.org/api/boost#setting-a-custom-emoji-stickerset-for-supergroups">here »</a> for more info.</summary>
		[IfFlag(42)] public StickerSet emojiset;
		[IfFlag(49)] public BotVerification bot_verification;
		[IfFlag(50)] public int stargifts_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="participants_count"/> has a value</summary>
			has_participants_count = 0x1,
			/// <summary>Field <see cref="admins_count"/> has a value</summary>
			has_admins_count = 0x2,
			/// <summary>Fields <see cref="kicked_count"/> and <see cref="banned_count"/> have a value</summary>
			has_kicked_count = 0x4,
			/// <summary>Can we view the participant list?</summary>
			can_view_participants = 0x8,
			/// <summary>Fields <see cref="migrated_from_chat_id"/> and <see cref="migrated_from_max_id"/> have a value</summary>
			has_migrated_from_chat_id = 0x10,
			/// <summary>Field <see cref="pinned_msg_id"/> has a value</summary>
			has_pinned_msg_id = 0x20,
			/// <summary>Can we set the channel's username?</summary>
			can_set_username = 0x40,
			/// <summary>Can we <see cref="SchemaExtensions.Channels_SetStickers">Channels_SetStickers</see> a stickerpack to the supergroup?</summary>
			can_set_stickers = 0x80,
			/// <summary>Field <see cref="stickerset"/> has a value</summary>
			has_stickerset = 0x100,
			/// <summary>Field <see cref="available_min_id"/> has a value</summary>
			has_available_min_id = 0x200,
			/// <summary>Is the history before we joined hidden to us?</summary>
			hidden_prehistory = 0x400,
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x800,
			/// <summary>Field <see cref="stats_dc"/> has a value</summary>
			has_stats_dc = 0x1000,
			/// <summary>Field <see cref="online_count"/> has a value</summary>
			has_online_count = 0x2000,
			/// <summary>Field <see cref="linked_chat_id"/> has a value</summary>
			has_linked_chat_id = 0x4000,
			/// <summary>Field <see cref="location"/> has a value</summary>
			has_location = 0x8000,
			/// <summary>Can we set the geolocation of this group (for geogroups)</summary>
			can_set_location = 0x10000,
			/// <summary>Field <see cref="slowmode_seconds"/> has a value</summary>
			has_slowmode_seconds = 0x20000,
			/// <summary>Field <see cref="slowmode_next_send_date"/> has a value</summary>
			has_slowmode_next_send_date = 0x40000,
			/// <summary>Whether scheduled messages are available</summary>
			has_scheduled = 0x80000,
			/// <summary>Can the user view <a href="https://corefork.telegram.org/api/stats">channel/supergroup statistics</a></summary>
			can_view_stats = 0x100000,
			/// <summary>Field <see cref="call"/> has a value</summary>
			has_call = 0x200000,
			/// <summary>Whether any anonymous admin of this supergroup was blocked: if set, you won't receive messages from anonymous group admins in <a href="https://corefork.telegram.org/api/discussion">discussion replies via @replies</a></summary>
			blocked = 0x400000,
			/// <summary>Field <see cref="exported_invite"/> has a value</summary>
			has_exported_invite = 0x800000,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x1000000,
			/// <summary>Field <see cref="pending_suggestions"/> has a value</summary>
			has_pending_suggestions = 0x2000000,
			/// <summary>Field <see cref="groupcall_default_join_as"/> has a value</summary>
			has_groupcall_default_join_as = 0x4000000,
			/// <summary>Field <see cref="theme_emoticon"/> has a value</summary>
			has_theme_emoticon = 0x8000000,
			/// <summary>Fields <see cref="requests_pending"/> and <see cref="recent_requesters"/> have a value</summary>
			has_requests_pending = 0x10000000,
			/// <summary>Field <see cref="default_send_as"/> has a value</summary>
			has_default_send_as = 0x20000000,
			/// <summary>Field <see cref="available_reactions"/> has a value</summary>
			has_available_reactions = 0x40000000,
		}

		[Flags] public enum Flags2 : uint
		{
			/// <summary>Can we delete this channel?</summary>
			can_delete_channel = 0x1,
			/// <summary>Whether <a href="https://corefork.telegram.org/api/antispam">native antispam</a> functionality is enabled in this supergroup.</summary>
			antispam = 0x2,
			/// <summary>Whether the participant list is hidden.</summary>
			participants_hidden = 0x4,
			/// <summary>Whether the <a href="https://corefork.telegram.org/api/translation">real-time chat translation popup</a> should be hidden.</summary>
			translations_disabled = 0x8,
			/// <summary>Field <see cref="stories"/> has a value</summary>
			has_stories = 0x10,
			/// <summary>Whether this user has some <a href="https://corefork.telegram.org/api/stories#pinned-or-archived-stories">pinned stories</a>.</summary>
			stories_pinned_available = 0x20,
			/// <summary>Users may also choose to display messages from all topics of a <a href="https://corefork.telegram.org/api/forum">forum</a> as if they were sent to a normal group, using a "View as messages" setting in the local client.  <br/>This setting only affects the current account, and is synced to other logged in sessions using the <see cref="SchemaExtensions.Channels_ToggleViewForumAsMessages">Channels_ToggleViewForumAsMessages</see> method; invoking this method will update the value of this flag.</summary>
			view_forum_as_messages = 0x40,
			/// <summary>Field <see cref="wallpaper"/> has a value</summary>
			has_wallpaper = 0x80,
			/// <summary>Field <see cref="boosts_applied"/> has a value</summary>
			has_boosts_applied = 0x100,
			/// <summary>Field <see cref="boosts_unrestrict"/> has a value</summary>
			has_boosts_unrestrict = 0x200,
			/// <summary>Field <see cref="emojiset"/> has a value</summary>
			has_emojiset = 0x400,
			/// <summary>Whether ads on this channel were <a href="https://corefork.telegram.org/api/boost#disable-ads-on-the-channel">disabled as specified here »</a> (this flag is only visible to the owner of the channel).</summary>
			restricted_sponsored = 0x800,
			/// <summary>If set, this user can view <a href="https://corefork.telegram.org/api/revenue#revenue-statistics">ad revenue statistics »</a> for this channel.</summary>
			can_view_revenue = 0x1000,
			/// <summary>Field <see cref="reactions_limit"/> has a value</summary>
			has_reactions_limit = 0x2000,
			/// <summary>Whether the current user can send or forward <a href="https://corefork.telegram.org/api/paid-media">paid media »</a> to this channel.</summary>
			paid_media_allowed = 0x4000,
			/// <summary>If set, this user can view <a href="https://corefork.telegram.org/api/stars#revenue-statistics">Telegram Star revenue statistics »</a> for this channel.</summary>
			can_view_stars_revenue = 0x8000,
			/// <summary>If set, users may send <a href="https://corefork.telegram.org/api/reactions#paid-reactions">paid Telegram Star reactions »</a> to messages of this channel.</summary>
			paid_reactions_available = 0x10000,
			/// <summary>Field <see cref="bot_verification"/> has a value</summary>
			has_bot_verification = 0x20000,
			/// <summary>Field <see cref="stargifts_count"/> has a value</summary>
			has_stargifts_count = 0x40000,
			stargifts_available = 0x80000,
		}

		/// <summary>ID of the channel</summary>
		public override long ID => id;
		/// <summary>Info about the channel</summary>
		public override string About => about;
		/// <summary>Channel picture</summary>
		public override PhotoBase ChatPhoto => chat_photo;
		/// <summary>Notification settings</summary>
		public override PeerNotifySettings NotifySettings => notify_settings;
		/// <summary>Invite link</summary>
		public override ExportedChatInvite ExportedInvite => exported_invite;
		/// <summary>Info about bots in the channel/supergroup</summary>
		public override BotInfo[] BotInfo => bot_info;
		/// <summary>Message ID of the last <a href="https://corefork.telegram.org/api/pin">pinned message</a></summary>
		public override int PinnedMsg => pinned_msg_id;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public override int Folder => folder_id;
		/// <summary>Livestream or group call information</summary>
		public override InputGroupCall Call => call;
		/// <summary>Time-To-Live of messages in this channel or supergroup</summary>
		public override int TtlPeriod => ttl_period;
		/// <summary>When using <see cref="SchemaExtensions.Phone_GetGroupCallJoinAs">Phone_GetGroupCallJoinAs</see> to get a list of peers that can be used to join a group call, this field indicates the peer that should be selected by default.</summary>
		public override Peer GroupcallDefaultJoinAs => groupcall_default_join_as;
		/// <summary>Emoji representing a specific chat theme</summary>
		public override string ThemeEmoticon => theme_emoticon;
		/// <summary>Pending <a href="https://corefork.telegram.org/api/invites#join-requests">join requests »</a></summary>
		public override int RequestsPending => requests_pending;
		/// <summary>IDs of users who requested to join recently</summary>
		public override long[] RecentRequesters => recent_requesters;
		/// <summary>Allowed <a href="https://corefork.telegram.org/api/reactions">message reactions »</a></summary>
		public override ChatReactions AvailableReactions => available_reactions;
		/// <summary>This flag may be used to impose a custom limit of unique reactions (i.e. a customizable version of <a href="https://corefork.telegram.org/api/config#reactions-uniq-max">appConfig.reactions_uniq_max</a>).</summary>
		public override int ReactionsLimit => reactions_limit;
	}

	/// <summary>Details of a group member.		<para>See <a href="https://corefork.telegram.org/type/ChatParticipant"/></para>		<para>Derived classes: <see cref="ChatParticipant"/>, <see cref="ChatParticipantCreator"/>, <see cref="ChatParticipantAdmin"/></para></summary>
	public abstract partial class ChatParticipantBase : IObject
	{
		/// <summary>Member user ID</summary>
		public virtual long UserId => default;
	}
	/// <summary>Group member.		<para>See <a href="https://corefork.telegram.org/constructor/chatParticipant"/></para></summary>
	[TLDef(0xC02D4007)]
	public partial class ChatParticipant : ChatParticipantBase
	{
		/// <summary>Member user ID</summary>
		public long user_id;
		/// <summary>ID of the user that added the member to the group</summary>
		public long inviter_id;
		/// <summary>Date added to the group</summary>
		public DateTime date;

		/// <summary>Member user ID</summary>
		public override long UserId => user_id;
	}
	/// <summary>Represents the creator of the group		<para>See <a href="https://corefork.telegram.org/constructor/chatParticipantCreator"/></para></summary>
	[TLDef(0xE46BCEE4)]
	public sealed partial class ChatParticipantCreator : ChatParticipantBase
	{
		/// <summary>ID of the user that created the group</summary>
		public long user_id;

		/// <summary>ID of the user that created the group</summary>
		public override long UserId => user_id;
	}
	/// <summary>Chat admin		<para>See <a href="https://corefork.telegram.org/constructor/chatParticipantAdmin"/></para></summary>
	[TLDef(0xA0933F5B)]
	public sealed partial class ChatParticipantAdmin : ChatParticipant
	{
	}

	/// <summary>Object contains info on group members.		<para>See <a href="https://corefork.telegram.org/type/ChatParticipants"/></para>		<para>Derived classes: <see cref="ChatParticipantsForbidden"/>, <see cref="ChatParticipants"/></para></summary>
	public abstract partial class ChatParticipantsBase : IObject
	{
		/// <summary>Group ID</summary>
		public virtual long ChatId => default;
	}
	/// <summary>Info on members is unavailable		<para>See <a href="https://corefork.telegram.org/constructor/chatParticipantsForbidden"/></para></summary>
	[TLDef(0x8763D3E1)]
	public sealed partial class ChatParticipantsForbidden : ChatParticipantsBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Group ID</summary>
		public long chat_id;
		/// <summary>Info about the group membership of the current user</summary>
		[IfFlag(0)] public ChatParticipantBase self_participant;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="self_participant"/> has a value</summary>
			has_self_participant = 0x1,
		}

		/// <summary>Group ID</summary>
		public override long ChatId => chat_id;
	}
	/// <summary>Group members.		<para>See <a href="https://corefork.telegram.org/constructor/chatParticipants"/></para></summary>
	[TLDef(0x3CBC93F8)]
	public sealed partial class ChatParticipants : ChatParticipantsBase
	{
		/// <summary>Group identifier</summary>
		public long chat_id;
		/// <summary>List of group members</summary>
		public ChatParticipantBase[] participants;
		/// <summary>Group version number</summary>
		public int version;

		/// <summary>Group identifier</summary>
		public override long ChatId => chat_id;
	}

	/// <summary>Group profile photo.		<para>See <a href="https://corefork.telegram.org/constructor/chatPhoto"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/chatPhotoEmpty">chatPhotoEmpty</a></remarks>
	[TLDef(0x1C6E1C11)]
	public sealed partial class ChatPhoto : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Photo ID</summary>
		public long photo_id;
		/// <summary><a href="https://corefork.telegram.org/api/files#stripped-thumbnails">Stripped thumbnail</a></summary>
		[IfFlag(1)] public byte[] stripped_thumb;
		/// <summary>DC where this photo is stored</summary>
		public int dc_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the user has an animated profile picture</summary>
			has_video = 0x1,
			/// <summary>Field <see cref="stripped_thumb"/> has a value</summary>
			has_stripped_thumb = 0x2,
		}
	}
	
		/// <summary>Exported chat invite		<para>See <a href="https://corefork.telegram.org/constructor/chatInviteExported"/></para></summary>
	[TLDef(0xA22CBD96)]
	public sealed partial class ChatInviteExported : ExportedChatInvite
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Chat invitation link</summary>
		public string link;
		/// <summary>ID of the admin that created this chat invite</summary>
		public long admin_id;
		/// <summary>When was this chat invite created</summary>
		public DateTime date;
		/// <summary>When was this chat invite last modified</summary>
		[IfFlag(4)] public DateTime start_date;
		/// <summary>When does this chat invite expire</summary>
		[IfFlag(1)] public DateTime expire_date;
		/// <summary>Maximum number of users that can join using this link</summary>
		[IfFlag(2)] public int usage_limit;
		/// <summary>How many users joined using this link</summary>
		[IfFlag(3)] public int usage;
		/// <summary>Number of users that have already used this link to join</summary>
		[IfFlag(7)] public int requested;
		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscriptions »</a>, contains the number of chat members which have already joined the chat using the link, but have already left due to expiration of their subscription.</summary>
		[IfFlag(10)] public int subscription_expired;
		/// <summary>Custom description for the invite link, visible only to admins</summary>
		[IfFlag(8)] public string title;
		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscriptions »</a>, contains the pricing of the subscription the user must activate to join the private channel.</summary>
		[IfFlag(9)] public StarsSubscriptionPricing subscription_pricing;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this chat invite was revoked</summary>
			revoked = 0x1,
			/// <summary>Field <see cref="expire_date"/> has a value</summary>
			has_expire_date = 0x2,
			/// <summary>Field <see cref="usage_limit"/> has a value</summary>
			has_usage_limit = 0x4,
			/// <summary>Field <see cref="usage"/> has a value</summary>
			has_usage = 0x8,
			/// <summary>Field <see cref="start_date"/> has a value</summary>
			has_start_date = 0x10,
			/// <summary>Whether this chat invite has no expiration</summary>
			permanent = 0x20,
			/// <summary>Whether users importing this invite link will have to be approved to join the channel or group</summary>
			request_needed = 0x40,
			/// <summary>Field <see cref="requested"/> has a value</summary>
			has_requested = 0x80,
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x100,
			/// <summary>Field <see cref="subscription_pricing"/> has a value</summary>
			has_subscription_pricing = 0x200,
			/// <summary>Field <see cref="subscription_expired"/> has a value</summary>
			has_subscription_expired = 0x400,
		}
	}
	/// <summary>Used in updates and in the channel log to indicate when a user is requesting to join or has joined a <a href="https://corefork.telegram.org/api/discussion#requiring-users-to-join-the-group">discussion group</a>		<para>See <a href="https://corefork.telegram.org/constructor/chatInvitePublicJoinRequests"/></para></summary>
	[TLDef(0xED107AB7)]
	public sealed partial class ChatInvitePublicJoinRequests : ExportedChatInvite { }

	/// <summary>Chat invite		<para>See <a href="https://corefork.telegram.org/type/ChatInvite"/></para>		<para>Derived classes: <see cref="ChatInviteAlready"/>, <see cref="ChatInvite"/>, <see cref="ChatInvitePeek"/></para></summary>
	public abstract partial class ChatInviteBase : IObject { }
	/// <summary>The user has already joined this chat		<para>See <a href="https://corefork.telegram.org/constructor/chatInviteAlready"/></para></summary>
	[TLDef(0x5A686D7C)]
	public sealed partial class ChatInviteAlready : ChatInviteBase
	{
		/// <summary>The chat connected to the invite</summary>
		public ChatBase chat;
	}
	/// <summary>Chat invite info		<para>See <a href="https://corefork.telegram.org/constructor/chatInvite"/></para></summary>
	[TLDef(0x5C9D3702)]
	public sealed partial class ChatInvite : ChatInviteBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Chat/supergroup/channel title</summary>
		public string title;
		/// <summary>Description of the group of channel</summary>
		[IfFlag(5)] public string about;
		/// <summary>Chat/supergroup/channel photo</summary>
		public PhotoBase photo;
		/// <summary>Participant count</summary>
		public int participants_count;
		/// <summary>A few of the participants that are in the group</summary>
		[IfFlag(4)] public UserBase[] participants;
		/// <summary><a href="https://corefork.telegram.org/api/colors">Profile color palette ID</a></summary>
		public int color;
		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscriptions »</a>, contains the pricing of the subscription the user must activate to join the private channel.</summary>
		[IfFlag(10)] public StarsSubscriptionPricing subscription_pricing;
		/// <summary>For <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscriptions »</a>, the ID of the payment form for the subscription.</summary>
		[IfFlag(12)] public long subscription_form_id;
		[IfFlag(13)] public BotVerification bot_verification;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> or a <a href="https://corefork.telegram.org/api/channel">normal group</a></summary>
			channel = 0x1,
			/// <summary>Whether this is a <a href="https://corefork.telegram.org/api/channel">channel</a></summary>
			broadcast = 0x2,
			/// <summary>Whether this is a public <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
			public_ = 0x4,
			/// <summary>Whether this is a <a href="https://corefork.telegram.org/api/channel">supergroup</a></summary>
			megagroup = 0x8,
			/// <summary>Field <see cref="participants"/> has a value</summary>
			has_participants = 0x10,
			/// <summary>Field <see cref="about"/> has a value</summary>
			has_about = 0x20,
			/// <summary>Whether the <a href="https://corefork.telegram.org/api/invites#join-requests">join request »</a> must be first approved by an administrator</summary>
			request_needed = 0x40,
			/// <summary>Is this chat or channel verified by Telegram?</summary>
			verified = 0x80,
			/// <summary>This chat is probably a scam</summary>
			scam = 0x100,
			/// <summary>If set, this chat was reported by many users as a fake or scam: be careful when interacting with it.</summary>
			fake = 0x200,
			/// <summary>Field <see cref="subscription_pricing"/> has a value</summary>
			has_subscription_pricing = 0x400,
			/// <summary>If set, indicates that the user has already paid for the associated <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscriptions »</a> and it hasn't expired yet, so they may re-join the channel using <see cref="SchemaExtensions.Messages_ImportChatInvite">Messages_ImportChatInvite</see> without repeating the payment.</summary>
			can_refulfill_subscription = 0x800,
			/// <summary>Field <see cref="subscription_form_id"/> has a value</summary>
			has_subscription_form_id = 0x1000,
			/// <summary>Field <see cref="bot_verification"/> has a value</summary>
			has_bot_verification = 0x2000,
		}
	}
	/// <summary>A chat invitation that also allows peeking into the group to read messages without joining it.		<para>See <a href="https://corefork.telegram.org/constructor/chatInvitePeek"/></para></summary>
	[TLDef(0x61695CB0)]
	public sealed partial class ChatInvitePeek : ChatInviteBase
	{
		/// <summary>Chat information</summary>
		public ChatBase chat;
		/// <summary>Read-only anonymous access to this group will be revoked at this date</summary>
		public DateTime expires;
	}
	
	/// <summary>Number of online users in a chat		<para>See <a href="https://corefork.telegram.org/constructor/chatOnlines"/></para></summary>
	[TLDef(0xF041E250)]
	public sealed partial class ChatOnlines : IObject
	{
		/// <summary>Number of online users</summary>
		public int onlines;
	}
	/// <summary>Represents the rights of an admin in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a>.		<para>See <a href="https://corefork.telegram.org/constructor/chatAdminRights"/></para></summary>
	[TLDef(0x5FB224D5)]
	public sealed partial class ChatAdminRights : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, allows the admin to modify the description of the <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
			change_info = 0x1,
			/// <summary>If set, allows the admin to post messages in the <a href="https://corefork.telegram.org/api/channel">channel</a></summary>
			post_messages = 0x2,
			/// <summary>If set, allows the admin to also edit messages from other admins in the <a href="https://corefork.telegram.org/api/channel">channel</a></summary>
			edit_messages = 0x4,
			/// <summary>If set, allows the admin to also delete messages from other admins in the <a href="https://corefork.telegram.org/api/channel">channel</a></summary>
			delete_messages = 0x8,
			/// <summary>If set, allows the admin to ban users from the <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
			ban_users = 0x10,
			/// <summary>If set, allows the admin to invite users in the <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
			invite_users = 0x20,
			/// <summary>If set, allows the admin to pin messages in the <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
			pin_messages = 0x80,
			/// <summary>If set, allows the admin to add other admins with the same (or more limited) permissions in the <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
			add_admins = 0x200,
			/// <summary>Whether this admin is anonymous</summary>
			anonymous = 0x400,
			/// <summary>If set, allows the admin to change group call/livestream settings</summary>
			manage_call = 0x800,
			/// <summary>Set this flag if none of the other flags are set, but you still want the user to be an admin: if this or any of the other flags are set, the admin can get the chat <a href="https://corefork.telegram.org/api/recent-actions">admin log</a>, get <a href="https://corefork.telegram.org/api/stats">chat statistics</a>, get <a href="https://corefork.telegram.org/api/stats">message statistics in channels</a>, get channel members, see anonymous administrators in supergroups and ignore slow mode.</summary>
			other = 0x1000,
			/// <summary>If set, allows the admin to create, delete or modify <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topics »</a>.</summary>
			manage_topics = 0x2000,
			/// <summary>If set, allows the admin to post <a href="https://corefork.telegram.org/api/stories">stories</a> as the <a href="https://corefork.telegram.org/api/channel">channel</a>.</summary>
			post_stories = 0x4000,
			/// <summary>If set, allows the admin to edit <a href="https://corefork.telegram.org/api/stories">stories</a> posted by the other admins of the <a href="https://corefork.telegram.org/api/channel">channel</a>.</summary>
			edit_stories = 0x8000,
			/// <summary>If set, allows the admin to delete <a href="https://corefork.telegram.org/api/stories">stories</a> posted by the other admins of the <a href="https://corefork.telegram.org/api/channel">channel</a>.</summary>
			delete_stories = 0x10000,
		}
	}

	/// <summary>Represents the rights of a normal user in a <a href="https://corefork.telegram.org/api/channel">supergroup/channel/chat</a>. In this case, the flags are inverted: if set, a flag <strong>does not allow</strong> a user to do X.		<para>See <a href="https://corefork.telegram.org/constructor/chatBannedRights"/></para></summary>
	[TLDef(0x9F120418)]
	public sealed partial class ChatBannedRights : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Validity of said permissions (it is considered forever any value less then 30 seconds or more then 366 days).</summary>
		public DateTime until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, does not allow a user to view messages in a <a href="https://corefork.telegram.org/api/channel">supergroup/channel/chat</a></summary>
			view_messages = 0x1,
			/// <summary>If set, does not allow a user to send messages in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			send_messages = 0x2,
			/// <summary>If set, does not allow a user to send any media in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			send_media = 0x4,
			/// <summary>If set, does not allow a user to send stickers in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			send_stickers = 0x8,
			/// <summary>If set, does not allow a user to send gifs in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			send_gifs = 0x10,
			/// <summary>If set, does not allow a user to send games in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			send_games = 0x20,
			/// <summary>If set, does not allow a user to use inline bots in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_inline = 0x40,
			/// <summary>If set, does not allow a user to embed links in the messages of a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			embed_links = 0x80,
			/// <summary>If set, does not allow a user to send polls in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			send_polls = 0x100,
			/// <summary>If set, does not allow any user to change the description of a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			change_info = 0x400,
			/// <summary>If set, does not allow any user to invite users in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			invite_users = 0x8000,
			/// <summary>If set, does not allow any user to pin messages in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a></summary>
			pin_messages = 0x20000,
			/// <summary>If set, does not allow any user to create, delete or modify <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topics »</a>.</summary>
			manage_topics = 0x40000,
			/// <summary>If set, does not allow a user to send photos in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_photos = 0x80000,
			/// <summary>If set, does not allow a user to send videos in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_videos = 0x100000,
			/// <summary>If set, does not allow a user to send round videos in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_roundvideos = 0x200000,
			/// <summary>If set, does not allow a user to send audio files in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_audios = 0x400000,
			/// <summary>If set, does not allow a user to send voice messages in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_voices = 0x800000,
			/// <summary>If set, does not allow a user to send documents in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_docs = 0x1000000,
			/// <summary>If set, does not allow a user to send text messages in a <a href="https://corefork.telegram.org/api/channel">supergroup/chat</a>.</summary>
			send_plain = 0x2000000,
		}
	}
	/// <summary>When and which user joined the chat using a chat invite		<para>See <a href="https://corefork.telegram.org/constructor/chatInviteImporter"/></para></summary>
	[TLDef(0x8C5ADFD9)]
	public sealed partial class ChatInviteImporter : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The user</summary>
		public long user_id;
		/// <summary>When did the user join</summary>
		public DateTime date;
		/// <summary>For users with pending requests, contains bio of the user that requested to join</summary>
		[IfFlag(2)] public string about;
		/// <summary>The administrator that approved the <a href="https://corefork.telegram.org/api/invites#join-requests">join request »</a> of the user</summary>
		[IfFlag(1)] public long approved_by;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this user currently has a pending <a href="https://corefork.telegram.org/api/invites#join-requests">join request »</a></summary>
			requested = 0x1,
			/// <summary>Field <see cref="approved_by"/> has a value</summary>
			has_approved_by = 0x2,
			/// <summary>Field <see cref="about"/> has a value</summary>
			has_about = 0x4,
			/// <summary>The participant joined by importing a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.</summary>
			via_chatlist = 0x8,
		}
	}



	/// <summary>Info about chat invites generated by admins.		<para>See <a href="https://corefork.telegram.org/constructor/chatAdminWithInvites"/></para></summary>
	[TLDef(0xF2ECEF23)]
	public sealed partial class ChatAdminWithInvites : IObject
	{
		/// <summary>The admin</summary>
		public long admin_id;
		/// <summary>Number of invites generated by the admin</summary>
		public int invites_count;
		/// <summary>Number of revoked invites</summary>
		public int revoked_invites_count;
	}
	/// <summary>Available chat reactions		<para>See <a href="https://corefork.telegram.org/type/ChatReactions"/></para>		<para>Derived classes: <see cref="ChatReactionsAll"/>, <see cref="ChatReactionsSome"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/chatReactionsNone">chatReactionsNone</a></remarks>
	public abstract partial class ChatReactions : IObject { }
	/// <summary>All reactions or all non-custom reactions are allowed		<para>See <a href="https://corefork.telegram.org/constructor/chatReactionsAll"/></para></summary>
	[TLDef(0x52928BCA)]
	public sealed partial class ChatReactionsAll : ChatReactions
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to allow custom reactions</summary>
			allow_custom = 0x1,
		}
	}
	/// <summary>Some reactions are allowed		<para>See <a href="https://corefork.telegram.org/constructor/chatReactionsSome"/></para></summary>
	[TLDef(0x661D4037)]
	public sealed partial class ChatReactionsSome : ChatReactions
	{
		/// <summary>Allowed set of reactions: the <a href="https://corefork.telegram.org/api/config#reactions-in-chat-max">reactions_in_chat_max</a> configuration field indicates the maximum number of reactions that can be specified in this field.</summary>
		public Reaction[] reactions;
	}

	/// <summary>Info about an exported <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/chatlists.exportedChatlistInvite"/></para></summary>
	[TLDef(0x10E6E3A6)]
	public sealed partial class Chatlists_ExportedChatlistInvite : IObject
	{
		/// <summary>Folder ID</summary>
		public DialogFilterBase filter;
		/// <summary>The exported <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.</summary>
		public ExportedChatlistInvite invite;
	}

	/// <summary>Info about multiple <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep links »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/chatlists.exportedInvites"/></para></summary>
	[TLDef(0x10AB6DC7)]
	public sealed partial class Chatlists_ExportedInvites : IObject, IPeerResolver
	{
		/// <summary>The <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep links »</a>.</summary>
		public ExportedChatlistInvite[] invites;
		/// <summary>Related chat information</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Related user information</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Info about a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.		<para>See <a href="https://corefork.telegram.org/type/chatlists.ChatlistInvite"/></para>		<para>Derived classes: <see cref="Chatlists_ChatlistInviteAlready"/>, <see cref="Chatlists_ChatlistInvite"/></para></summary>
	public abstract partial class Chatlists_ChatlistInviteBase : IObject
	{
		/// <summary>Related chat information</summary>
		public virtual Dictionary<long, ChatBase> Chats => default;
		/// <summary>Related user information</summary>
		public virtual Dictionary<long, User> Users => default;
	}
	/// <summary>Updated info about a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a> we already imported.		<para>See <a href="https://corefork.telegram.org/constructor/chatlists.chatlistInviteAlready"/></para></summary>
	[TLDef(0xFA87F659)]
	public sealed partial class Chatlists_ChatlistInviteAlready : Chatlists_ChatlistInviteBase, IPeerResolver
	{
		/// <summary>ID of the imported folder</summary>
		public int filter_id;
		/// <summary>New peers to be imported</summary>
		public Peer[] missing_peers;
		/// <summary>Peers that were already imported</summary>
		public Peer[] already_peers;
		/// <summary>Related chat information</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Related user information</summary>
		public Dictionary<long, User> users;

		/// <summary>Related chat information</summary>
		public override Dictionary<long, ChatBase> Chats => chats;
		/// <summary>Related user information</summary>
		public override Dictionary<long, User> Users => users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Info about a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/chatlists.chatlistInvite"/></para></summary>
	[TLDef(0xF10ECE2F)]
	public sealed partial class Chatlists_ChatlistInvite : Chatlists_ChatlistInviteBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Name of the link</summary>
		public TextWithEntities title;
		/// <summary>Emoji to use as icon for the folder.</summary>
		[IfFlag(0)] public string emoticon;
		/// <summary>Supergroups and channels to join</summary>
		public Peer[] peers;
		/// <summary>Related chat information</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Related user information</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="emoticon"/> has a value</summary>
			has_emoticon = 0x1,
			title_noanimate = 0x2,
		}

		/// <summary>Related chat information</summary>
		public override Dictionary<long, ChatBase> Chats => chats;
		/// <summary>Related user information</summary>
		public override Dictionary<long, User> Users => users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Updated information about a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/chatlists.chatlistUpdates"/></para></summary>
	[TLDef(0x93BD878D)]
	public sealed partial class Chatlists_ChatlistUpdates : IObject, IPeerResolver
	{
		/// <summary>New peers to join</summary>
		public Peer[] missing_peers;
		/// <summary>Related chat information</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Related user information</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	
}