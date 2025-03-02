using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Filter for getting only certain types of channel messages		<para>See <a href="https://corefork.telegram.org/constructor/channelMessagesFilter"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/channelMessagesFilterEmpty">channelMessagesFilterEmpty</a></remarks>
    [TLDef(0xCD77D957)]
    public sealed partial class ChannelMessagesFilter : IObject
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>A range of messages to fetch</summary>
        public MessageRange[] ranges;

        [Flags] public enum Flags : uint
        {
            /// <summary>Whether to exclude new messages from the search</summary>
            exclude_new_messages = 0x2,
        }
    }
    
    	/// <summary>Channel participant		<para>See <a href="https://corefork.telegram.org/type/ChannelParticipant"/></para>		<para>Derived classes: <see cref="ChannelParticipant"/>, <see cref="ChannelParticipantSelf"/>, <see cref="ChannelParticipantCreator"/>, <see cref="ChannelParticipantAdmin"/>, <see cref="ChannelParticipantBanned"/>, <see cref="ChannelParticipantLeft"/></para></summary>
	public abstract partial class ChannelParticipantBase : IObject { }
	/// <summary>Channel/supergroup participant		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipant"/></para></summary>
	[TLDef(0xCB397619)]
	public sealed partial class ChannelParticipant : ChannelParticipantBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Participant user ID</summary>
		public long user_id;
		/// <summary>Date joined</summary>
		public DateTime date;
		/// <summary>If set, contains the expiration date of the current <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription period »</a> for the specified participant.</summary>
		[IfFlag(0)] public DateTime subscription_until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="subscription_until_date"/> has a value</summary>
			has_subscription_until_date = 0x1,
		}
	}
	/// <summary>Myself		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantSelf"/></para></summary>
	[TLDef(0x4F607BEF)]
	public sealed partial class ChannelParticipantSelf : ChannelParticipantBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>User that invited me to the channel/supergroup</summary>
		public long inviter_id;
		/// <summary>When did I join the channel/supergroup</summary>
		public DateTime date;
		/// <summary>If set, contains the expiration date of the current <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription period »</a> for the specified participant.</summary>
		[IfFlag(1)] public DateTime subscription_until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether I joined upon specific approval of an admin</summary>
			via_request = 0x1,
			/// <summary>Field <see cref="subscription_until_date"/> has a value</summary>
			has_subscription_until_date = 0x2,
		}
	}
	/// <summary>Channel/supergroup creator		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantCreator"/></para></summary>
	[TLDef(0x2FE601D3)]
	public sealed partial class ChannelParticipantCreator : ChannelParticipantBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>Creator admin rights</summary>
		public ChatAdminRights admin_rights;
		/// <summary>The role (rank) of the group creator in the group: just an arbitrary string, <c>admin</c> by default</summary>
		[IfFlag(0)] public string rank;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="rank"/> has a value</summary>
			has_rank = 0x1,
		}
	}
	/// <summary>Admin		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantAdmin"/></para></summary>
	[TLDef(0x34C3BB53)]
	public sealed partial class ChannelParticipantAdmin : ChannelParticipantBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Admin user ID</summary>
		public long user_id;
		/// <summary>User that invited the admin to the channel/group</summary>
		[IfFlag(1)] public long inviter_id;
		/// <summary>User that promoted the user to admin</summary>
		public long promoted_by;
		/// <summary>When did the user join</summary>
		public DateTime date;
		/// <summary>Admin <a href="https://corefork.telegram.org/api/rights">rights</a></summary>
		public ChatAdminRights admin_rights;
		/// <summary>The role (rank) of the admin in the group: just an arbitrary string, <c>admin</c> by default</summary>
		[IfFlag(2)] public string rank;

		[Flags] public enum Flags : uint
		{
			/// <summary>Can this admin promote other admins with the same permissions?</summary>
			can_edit = 0x1,
			/// <summary>Is this the current user</summary>
			self = 0x2,
			/// <summary>Field <see cref="rank"/> has a value</summary>
			has_rank = 0x4,
		}
	}
	/// <summary>Banned/kicked user		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantBanned"/></para></summary>
	[TLDef(0x6DF8014E)]
	public sealed partial class ChannelParticipantBanned : ChannelParticipantBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The banned peer</summary>
		public Peer peer;
		/// <summary>User was kicked by the specified admin</summary>
		public long kicked_by;
		/// <summary>When did the user join the group</summary>
		public DateTime date;
		/// <summary>Banned <a href="https://corefork.telegram.org/api/rights">rights</a></summary>
		public ChatBannedRights banned_rights;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the user has left the group</summary>
			left = 0x1,
		}
	}
	/// <summary>A participant that left the channel/supergroup		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantLeft"/></para></summary>
	[TLDef(0x1B03F006)]
	public sealed partial class ChannelParticipantLeft : ChannelParticipantBase
	{
		/// <summary>The peer that left</summary>
		public Peer peer;
	}

	/// <summary>Filter for fetching channel participants		<para>See <a href="https://corefork.telegram.org/type/ChannelParticipantsFilter"/></para>		<para>Derived classes: <see cref="ChannelParticipantsRecent"/>, <see cref="ChannelParticipantsAdmins"/>, <see cref="ChannelParticipantsKicked"/>, <see cref="ChannelParticipantsBots"/>, <see cref="ChannelParticipantsBanned"/>, <see cref="ChannelParticipantsSearch"/>, <see cref="ChannelParticipantsContacts"/>, <see cref="ChannelParticipantsMentions"/></para></summary>
	public abstract partial class ChannelParticipantsFilter : IObject { }
	/// <summary>Fetch only recent participants		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsRecent"/></para></summary>
	[TLDef(0xDE3F3C79)]
	public sealed partial class ChannelParticipantsRecent : ChannelParticipantsFilter { }
	/// <summary>Fetch only admin participants		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsAdmins"/></para></summary>
	[TLDef(0xB4608969)]
	public sealed partial class ChannelParticipantsAdmins : ChannelParticipantsFilter { }
	/// <summary>Fetch only kicked participants		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsKicked"/></para></summary>
	[TLDef(0xA3B54985)]
	public sealed partial class ChannelParticipantsKicked : ChannelParticipantsFilter
	{
		/// <summary>Optional filter for searching kicked participants by name (otherwise empty)</summary>
		public string q;
	}
	/// <summary>Fetch only bot participants		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsBots"/></para></summary>
	[TLDef(0xB0D1865B)]
	public sealed partial class ChannelParticipantsBots : ChannelParticipantsFilter { }
	/// <summary>Fetch only banned participants		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsBanned"/></para></summary>
	[TLDef(0x1427A5E1)]
	public sealed partial class ChannelParticipantsBanned : ChannelParticipantsFilter
	{
		/// <summary>Optional filter for searching banned participants by name (otherwise empty)</summary>
		public string q;
	}
	/// <summary>Query participants by name		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsSearch"/></para></summary>
	[TLDef(0x0656AC4B)]
	public sealed partial class ChannelParticipantsSearch : ChannelParticipantsFilter
	{
		/// <summary>Search query</summary>
		public string q;
	}
	/// <summary>Fetch only participants that are also contacts		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsContacts"/></para></summary>
	[TLDef(0xBB6AE88D)]
	public sealed partial class ChannelParticipantsContacts : ChannelParticipantsFilter
	{
		/// <summary>Optional search query for searching contact participants by name</summary>
		public string q;
	}
	/// <summary>This filter is used when looking for supergroup members to mention.<br/>This filter will automatically remove anonymous admins, and return even non-participant users that replied to a specific <a href="https://corefork.telegram.org/api/threads">thread</a> through the <a href="https://corefork.telegram.org/api/threads#channel-comments">comment section</a> of a channel.		<para>See <a href="https://corefork.telegram.org/constructor/channelParticipantsMentions"/></para></summary>
	[TLDef(0xE04B5CEB)]
	public sealed partial class ChannelParticipantsMentions : ChannelParticipantsFilter
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Filter by user name or username</summary>
		[IfFlag(0)] public string q;
		/// <summary>Look only for users that posted in this <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		[IfFlag(1)] public int top_msg_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="q"/> has a value</summary>
			has_q = 0x1,
			/// <summary>Field <see cref="top_msg_id"/> has a value</summary>
			has_top_msg_id = 0x2,
		}
	}

	/// <summary>Represents multiple channel participants		<para>See <a href="https://corefork.telegram.org/constructor/channels.channelParticipants"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/channels.channelParticipantsNotModified">channels.channelParticipantsNotModified</a></remarks>
	[TLDef(0x9AB0FEAF)]
	public sealed partial class Channels_ChannelParticipants : IObject, IPeerResolver
	{
		/// <summary>Total number of participants that correspond to the given query</summary>
		public int count;
		/// <summary>Participants</summary>
		public ChannelParticipantBase[] participants;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in participant info</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Represents a channel participant		<para>See <a href="https://corefork.telegram.org/constructor/channels.channelParticipant"/></para></summary>
	[TLDef(0xDFB80317)]
	public sealed partial class Channels_ChannelParticipant : IObject, IPeerResolver
	{
		/// <summary>The channel participant</summary>
		public ChannelParticipantBase participant;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
		/// <summary>Channel admin log event		<para>See <a href="https://corefork.telegram.org/type/ChannelAdminLogEventAction"/></para>		<para>Derived classes: <see cref="ChannelAdminLogEventActionChangeTitle"/>, <see cref="ChannelAdminLogEventActionChangeAbout"/>, <see cref="ChannelAdminLogEventActionChangeUsername"/>, <see cref="ChannelAdminLogEventActionChangePhoto"/>, <see cref="ChannelAdminLogEventActionToggleInvites"/>, <see cref="ChannelAdminLogEventActionToggleSignatures"/>, <see cref="ChannelAdminLogEventActionUpdatePinned"/>, <see cref="ChannelAdminLogEventActionEditMessage"/>, <see cref="ChannelAdminLogEventActionDeleteMessage"/>, <see cref="ChannelAdminLogEventActionParticipantJoin"/>, <see cref="ChannelAdminLogEventActionParticipantLeave"/>, <see cref="ChannelAdminLogEventActionParticipantInvite"/>, <see cref="ChannelAdminLogEventActionParticipantToggleBan"/>, <see cref="ChannelAdminLogEventActionParticipantToggleAdmin"/>, <see cref="ChannelAdminLogEventActionChangeStickerSet"/>, <see cref="ChannelAdminLogEventActionTogglePreHistoryHidden"/>, <see cref="ChannelAdminLogEventActionDefaultBannedRights"/>, <see cref="ChannelAdminLogEventActionStopPoll"/>, <see cref="ChannelAdminLogEventActionChangeLinkedChat"/>, <see cref="ChannelAdminLogEventActionChangeLocation"/>, <see cref="ChannelAdminLogEventActionToggleSlowMode"/>, <see cref="ChannelAdminLogEventActionStartGroupCall"/>, <see cref="ChannelAdminLogEventActionDiscardGroupCall"/>, <see cref="ChannelAdminLogEventActionParticipantMute"/>, <see cref="ChannelAdminLogEventActionParticipantUnmute"/>, <see cref="ChannelAdminLogEventActionToggleGroupCallSetting"/>, <see cref="ChannelAdminLogEventActionParticipantJoinByInvite"/>, <see cref="ChannelAdminLogEventActionExportedInviteDelete"/>, <see cref="ChannelAdminLogEventActionExportedInviteRevoke"/>, <see cref="ChannelAdminLogEventActionExportedInviteEdit"/>, <see cref="ChannelAdminLogEventActionParticipantVolume"/>, <see cref="ChannelAdminLogEventActionChangeHistoryTTL"/>, <see cref="ChannelAdminLogEventActionParticipantJoinByRequest"/>, <see cref="ChannelAdminLogEventActionToggleNoForwards"/>, <see cref="ChannelAdminLogEventActionSendMessage"/>, <see cref="ChannelAdminLogEventActionChangeAvailableReactions"/>, <see cref="ChannelAdminLogEventActionChangeUsernames"/>, <see cref="ChannelAdminLogEventActionToggleForum"/>, <see cref="ChannelAdminLogEventActionCreateTopic"/>, <see cref="ChannelAdminLogEventActionEditTopic"/>, <see cref="ChannelAdminLogEventActionDeleteTopic"/>, <see cref="ChannelAdminLogEventActionPinTopic"/>, <see cref="ChannelAdminLogEventActionToggleAntiSpam"/>, <see cref="ChannelAdminLogEventActionChangePeerColor"/>, <see cref="ChannelAdminLogEventActionChangeProfilePeerColor"/>, <see cref="ChannelAdminLogEventActionChangeWallpaper"/>, <see cref="ChannelAdminLogEventActionChangeEmojiStatus"/>, <see cref="ChannelAdminLogEventActionChangeEmojiStickerSet"/>, <see cref="ChannelAdminLogEventActionToggleSignatureProfiles"/>, <see cref="ChannelAdminLogEventActionParticipantSubExtend"/></para></summary>
	public abstract partial class ChannelAdminLogEventAction : IObject { }
	/// <summary>Channel/supergroup title was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeTitle"/></para></summary>
	[TLDef(0xE6DFB825)]
	public sealed partial class ChannelAdminLogEventActionChangeTitle : ChannelAdminLogEventAction
	{
		/// <summary>Previous title</summary>
		public string prev_value;
		/// <summary>New title</summary>
		public string new_value;
	}
	/// <summary>The description was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeAbout"/></para></summary>
	[TLDef(0x55188A2E)]
	public sealed partial class ChannelAdminLogEventActionChangeAbout : ChannelAdminLogEventAction
	{
		/// <summary>Previous description</summary>
		public string prev_value;
		/// <summary>New description</summary>
		public string new_value;
	}
	/// <summary>Channel/supergroup username was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeUsername"/></para></summary>
	[TLDef(0x6A4AFC38)]
	public sealed partial class ChannelAdminLogEventActionChangeUsername : ChannelAdminLogEventAction
	{
		/// <summary>Old username</summary>
		public string prev_value;
		/// <summary>New username</summary>
		public string new_value;
	}
	/// <summary>The channel/supergroup's picture was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangePhoto"/></para></summary>
	[TLDef(0x434BD2AF)]
	public sealed partial class ChannelAdminLogEventActionChangePhoto : ChannelAdminLogEventAction
	{
		/// <summary>Previous picture</summary>
		public PhotoBase prev_photo;
		/// <summary>New picture</summary>
		public PhotoBase new_photo;
	}
	/// <summary>Invites were enabled/disabled		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleInvites"/></para></summary>
	[TLDef(0x1B7907AE)]
	public sealed partial class ChannelAdminLogEventActionToggleInvites : ChannelAdminLogEventAction
	{
		/// <summary>New value</summary>
		public bool new_value;
	}
	/// <summary>Channel signatures were enabled/disabled		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleSignatures"/></para></summary>
	[TLDef(0x26AE0971)]
	public partial class ChannelAdminLogEventActionToggleSignatures : ChannelAdminLogEventAction
	{
		/// <summary>New value</summary>
		public bool new_value;
	}
	/// <summary>A message was pinned		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionUpdatePinned"/></para></summary>
	[TLDef(0xE9E82C18)]
	public sealed partial class ChannelAdminLogEventActionUpdatePinned : ChannelAdminLogEventAction
	{
		/// <summary>The message that was pinned</summary>
		public MessageBase message;
	}
	/// <summary>A message was edited		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionEditMessage"/></para></summary>
	[TLDef(0x709B2405)]
	public sealed partial class ChannelAdminLogEventActionEditMessage : ChannelAdminLogEventAction
	{
		/// <summary>Old message</summary>
		public MessageBase prev_message;
		/// <summary>New message</summary>
		public MessageBase new_message;
	}
	/// <summary>A message was deleted		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionDeleteMessage"/></para></summary>
	[TLDef(0x42E047BB)]
	public sealed partial class ChannelAdminLogEventActionDeleteMessage : ChannelAdminLogEventAction
	{
		/// <summary>The message that was deleted</summary>
		public MessageBase message;
	}
	/// <summary>A user has joined the group (in the case of big groups, info of the user that has joined isn't shown)		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantJoin"/></para></summary>
	[TLDef(0x183040D3)]
	public sealed partial class ChannelAdminLogEventActionParticipantJoin : ChannelAdminLogEventAction { }
	/// <summary>A user left the channel/supergroup (in the case of big groups, info of the user that has joined isn't shown)		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantLeave"/></para></summary>
	[TLDef(0xF89777F2)]
	public sealed partial class ChannelAdminLogEventActionParticipantLeave : ChannelAdminLogEventAction { }
	/// <summary>A user was invited to the group		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantInvite"/></para></summary>
	[TLDef(0xE31C34D8)]
	public sealed partial class ChannelAdminLogEventActionParticipantInvite : ChannelAdminLogEventAction
	{
		/// <summary>The user that was invited</summary>
		public ChannelParticipantBase participant;
	}
	/// <summary>The banned <a href="https://corefork.telegram.org/api/rights">rights</a> of a user were changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantToggleBan"/></para></summary>
	[TLDef(0xE6D83D7E)]
	public sealed partial class ChannelAdminLogEventActionParticipantToggleBan : ChannelAdminLogEventAction
	{
		/// <summary>Old banned rights of user</summary>
		public ChannelParticipantBase prev_participant;
		/// <summary>New banned rights of user</summary>
		public ChannelParticipantBase new_participant;
	}
	/// <summary>The admin <a href="https://corefork.telegram.org/api/rights">rights</a> of a user were changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantToggleAdmin"/></para></summary>
	[TLDef(0xD5676710)]
	public sealed partial class ChannelAdminLogEventActionParticipantToggleAdmin : ChannelAdminLogEventAction
	{
		/// <summary>Previous admin rights</summary>
		public ChannelParticipantBase prev_participant;
		/// <summary>New admin rights</summary>
		public ChannelParticipantBase new_participant;
	}
	/// <summary>The supergroup's stickerset was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeStickerSet"/></para></summary>
	[TLDef(0xB1C3CAA7)]
	public partial class ChannelAdminLogEventActionChangeStickerSet : ChannelAdminLogEventAction
	{
		/// <summary>Previous stickerset</summary>
		public InputStickerSet prev_stickerset;
		/// <summary>New stickerset</summary>
		public InputStickerSet new_stickerset;
	}
	/// <summary>The hidden prehistory setting was <see cref="SchemaExtensions.Channels_TogglePreHistoryHidden">Channels_TogglePreHistoryHidden</see>		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionTogglePreHistoryHidden"/></para></summary>
	[TLDef(0x5F5C95F1)]
	public sealed partial class ChannelAdminLogEventActionTogglePreHistoryHidden : ChannelAdminLogEventAction
	{
		/// <summary>New value</summary>
		public bool new_value;
	}
	/// <summary>The default banned rights were modified		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionDefaultBannedRights"/></para></summary>
	[TLDef(0x2DF5FC0A)]
	public sealed partial class ChannelAdminLogEventActionDefaultBannedRights : ChannelAdminLogEventAction
	{
		/// <summary>Previous global <a href="https://corefork.telegram.org/api/rights">banned rights</a></summary>
		public ChatBannedRights prev_banned_rights;
		/// <summary>New global <a href="https://corefork.telegram.org/api/rights">banned rights</a>.</summary>
		public ChatBannedRights new_banned_rights;
	}
	/// <summary>A poll was stopped		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionStopPoll"/></para></summary>
	[TLDef(0x8F079643)]
	public sealed partial class ChannelAdminLogEventActionStopPoll : ChannelAdminLogEventAction
	{
		/// <summary>The poll that was stopped</summary>
		public MessageBase message;
	}
	/// <summary>The linked chat was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeLinkedChat"/></para></summary>
	[TLDef(0x050C7AC8)]
	public sealed partial class ChannelAdminLogEventActionChangeLinkedChat : ChannelAdminLogEventAction
	{
		/// <summary>Previous linked chat</summary>
		public long prev_value;
		/// <summary>New linked chat</summary>
		public long new_value;
	}
	/// <summary>The geogroup location was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeLocation"/></para></summary>
	[TLDef(0x0E6B76AE)]
	public sealed partial class ChannelAdminLogEventActionChangeLocation : ChannelAdminLogEventAction
	{
		/// <summary>Previous location</summary>
		public ChannelLocation prev_value;
		/// <summary>New location</summary>
		public ChannelLocation new_value;
	}
	/// <summary><see cref="SchemaExtensions.Channels_ToggleSlowMode">Channels_ToggleSlowMode</see>		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleSlowMode"/></para></summary>
	[TLDef(0x53909779)]
	public sealed partial class ChannelAdminLogEventActionToggleSlowMode : ChannelAdminLogEventAction
	{
		/// <summary>Previous slow mode value</summary>
		public int prev_value;
		/// <summary>New slow mode value</summary>
		public int new_value;
	}
	/// <summary>A group call was started		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionStartGroupCall"/></para></summary>
	[TLDef(0x23209745)]
	public sealed partial class ChannelAdminLogEventActionStartGroupCall : ChannelAdminLogEventAction
	{
		/// <summary>Group call</summary>
		public InputGroupCall call;
	}
	/// <summary>A group call was terminated		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionDiscardGroupCall"/></para></summary>
	[TLDef(0xDB9F9140)]
	public sealed partial class ChannelAdminLogEventActionDiscardGroupCall : ChannelAdminLogEventAction
	{
		/// <summary>The group call that was terminated</summary>
		public InputGroupCall call;
	}
	/// <summary>A group call participant was muted		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantMute"/></para></summary>
	[TLDef(0xF92424D2)]
	public sealed partial class ChannelAdminLogEventActionParticipantMute : ChannelAdminLogEventAction
	{
		/// <summary>The participant that was muted</summary>
		public GroupCallParticipant participant;
	}
	/// <summary>A group call participant was unmuted		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantUnmute"/></para></summary>
	[TLDef(0xE64429C0)]
	public sealed partial class ChannelAdminLogEventActionParticipantUnmute : ChannelAdminLogEventAction
	{
		/// <summary>The participant that was unmuted</summary>
		public GroupCallParticipant participant;
	}
	/// <summary>Group call settings were changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleGroupCallSetting"/></para></summary>
	[TLDef(0x56D6A247)]
	public sealed partial class ChannelAdminLogEventActionToggleGroupCallSetting : ChannelAdminLogEventAction
	{
		/// <summary>Whether all users are muted by default upon joining</summary>
		public bool join_muted;
	}
	/// <summary>A user joined the <a href="https://corefork.telegram.org/api/channel">supergroup/channel</a> using a specific invite link		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantJoinByInvite"/></para></summary>
	[TLDef(0xFE9FC158)]
	public sealed partial class ChannelAdminLogEventActionParticipantJoinByInvite : ChannelAdminLogEventAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The invite link used to join the <a href="https://corefork.telegram.org/api/channel">supergroup/channel</a></summary>
		public ExportedChatInvite invite;

		[Flags] public enum Flags : uint
		{
			/// <summary>The participant joined by importing a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.</summary>
			via_chatlist = 0x1,
		}
	}
	/// <summary>A chat invite was deleted		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionExportedInviteDelete"/></para></summary>
	[TLDef(0x5A50FCA4)]
	public sealed partial class ChannelAdminLogEventActionExportedInviteDelete : ChannelAdminLogEventAction
	{
		/// <summary>The deleted chat invite</summary>
		public ExportedChatInvite invite;
	}
	/// <summary>A specific invite link was revoked		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionExportedInviteRevoke"/></para></summary>
	[TLDef(0x410A134E)]
	public sealed partial class ChannelAdminLogEventActionExportedInviteRevoke : ChannelAdminLogEventAction
	{
		/// <summary>The invite link that was revoked</summary>
		public ExportedChatInvite invite;
	}
	/// <summary>A chat invite was edited		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionExportedInviteEdit"/></para></summary>
	[TLDef(0xE90EBB59)]
	public sealed partial class ChannelAdminLogEventActionExportedInviteEdit : ChannelAdminLogEventAction
	{
		/// <summary>Previous chat invite information</summary>
		public ExportedChatInvite prev_invite;
		/// <summary>New chat invite information</summary>
		public ExportedChatInvite new_invite;
	}
	/// <summary>channelAdminLogEvent.user_id has set the volume of participant.peer to participant.volume		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantVolume"/></para></summary>
	[TLDef(0x3E7F6847)]
	public sealed partial class ChannelAdminLogEventActionParticipantVolume : ChannelAdminLogEventAction
	{
		/// <summary>The participant whose volume was changed</summary>
		public GroupCallParticipant participant;
	}
	/// <summary>The Time-To-Live of messages in this chat was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeHistoryTTL"/></para></summary>
	[TLDef(0x6E941A38)]
	public sealed partial class ChannelAdminLogEventActionChangeHistoryTTL : ChannelAdminLogEventAction
	{
		/// <summary>Previous value</summary>
		public int prev_value;
		/// <summary>New value</summary>
		public int new_value;
	}
	/// <summary>A new member was accepted to the chat by an admin		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantJoinByRequest"/></para></summary>
	[TLDef(0xAFB6144A)]
	public sealed partial class ChannelAdminLogEventActionParticipantJoinByRequest : ChannelAdminLogEventAction
	{
		/// <summary>The invite link that was used to join the chat</summary>
		public ExportedChatInvite invite;
		/// <summary>ID of the admin that approved the invite</summary>
		public long approved_by;
	}
	/// <summary>Forwards were enabled or disabled		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleNoForwards"/></para></summary>
	[TLDef(0xCB2AC766)]
	public sealed partial class ChannelAdminLogEventActionToggleNoForwards : ChannelAdminLogEventAction
	{
		/// <summary>Old value</summary>
		public bool new_value;
	}
	/// <summary>A message was posted in a channel		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionSendMessage"/></para></summary>
	[TLDef(0x278F2868)]
	public sealed partial class ChannelAdminLogEventActionSendMessage : ChannelAdminLogEventAction
	{
		/// <summary>The message that was sent</summary>
		public MessageBase message;
	}
	/// <summary>The set of allowed <a href="https://corefork.telegram.org/api/reactions">message reactions »</a> for this channel has changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeAvailableReactions"/></para></summary>
	[TLDef(0xBE4E0EF8)]
	public sealed partial class ChannelAdminLogEventActionChangeAvailableReactions : ChannelAdminLogEventAction
	{
		/// <summary>Previously allowed reaction emojis</summary>
		public ChatReactions prev_value;
		/// <summary>New allowed reaction emojis</summary>
		public ChatReactions new_value;
	}
	/// <summary>The list of usernames associated with the channel was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeUsernames"/></para></summary>
	[TLDef(0xF04FB3A9)]
	public sealed partial class ChannelAdminLogEventActionChangeUsernames : ChannelAdminLogEventAction
	{
		/// <summary>Previous set of usernames</summary>
		public string[] prev_value;
		/// <summary>New set of usernames</summary>
		public string[] new_value;
	}
	/// <summary><a href="https://corefork.telegram.org/api/forum">Forum</a> functionality was enabled or disabled.		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleForum"/></para></summary>
	[TLDef(0x02CC6383)]
	public sealed partial class ChannelAdminLogEventActionToggleForum : ChannelAdminLogEventAction
	{
		/// <summary>Whether <a href="https://corefork.telegram.org/api/forum">forum</a> functionality was enabled or disabled.</summary>
		public bool new_value;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> was created		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionCreateTopic"/></para></summary>
	[TLDef(0x58707D28)]
	public sealed partial class ChannelAdminLogEventActionCreateTopic : ChannelAdminLogEventAction
	{
		/// <summary>The <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> that was created</summary>
		public ForumTopicBase topic;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> was edited		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionEditTopic"/></para></summary>
	[TLDef(0xF06FE208)]
	public sealed partial class ChannelAdminLogEventActionEditTopic : ChannelAdminLogEventAction
	{
		/// <summary>Previous topic information</summary>
		public ForumTopicBase prev_topic;
		/// <summary>New topic information</summary>
		public ForumTopicBase new_topic;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> was deleted		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionDeleteTopic"/></para></summary>
	[TLDef(0xAE168909)]
	public sealed partial class ChannelAdminLogEventActionDeleteTopic : ChannelAdminLogEventAction
	{
		/// <summary>The <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> that was deleted</summary>
		public ForumTopicBase topic;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> was pinned or unpinned		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionPinTopic"/></para></summary>
	[TLDef(0x5D8D353B)]
	public sealed partial class ChannelAdminLogEventActionPinTopic : ChannelAdminLogEventAction
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Previous topic information</summary>
		[IfFlag(0)] public ForumTopicBase prev_topic;
		/// <summary>New topic information</summary>
		[IfFlag(1)] public ForumTopicBase new_topic;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="prev_topic"/> has a value</summary>
			has_prev_topic = 0x1,
			/// <summary>Field <see cref="new_topic"/> has a value</summary>
			has_new_topic = 0x2,
		}
	}
	/// <summary><a href="https://corefork.telegram.org/api/antispam">Native antispam</a> functionality was enabled or disabled.		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleAntiSpam"/></para></summary>
	[TLDef(0x64F36DFC)]
	public sealed partial class ChannelAdminLogEventActionToggleAntiSpam : ChannelAdminLogEventAction
	{
		/// <summary>Whether antispam functionality was enabled or disabled.</summary>
		public bool new_value;
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/colors">message accent color</a> was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangePeerColor"/></para></summary>
	[TLDef(0x5796E780)]
	public partial class ChannelAdminLogEventActionChangePeerColor : ChannelAdminLogEventAction
	{
		/// <summary>Previous accent palette</summary>
		public PeerColor prev_value;
		/// <summary>New accent palette</summary>
		public PeerColor new_value;
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/colors">profile accent color</a> was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeProfilePeerColor"/></para></summary>
	[TLDef(0x5E477B25)]
	public sealed partial class ChannelAdminLogEventActionChangeProfilePeerColor : ChannelAdminLogEventActionChangePeerColor { }
	/// <summary>The <a href="https://corefork.telegram.org/api/wallpapers">wallpaper</a> was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeWallpaper"/></para></summary>
	[TLDef(0x31BB5D52)]
	public sealed partial class ChannelAdminLogEventActionChangeWallpaper : ChannelAdminLogEventAction
	{
		/// <summary>Previous wallpaper</summary>
		public WallPaperBase prev_value;
		/// <summary>New wallpaper</summary>
		public WallPaperBase new_value;
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/emoji-status">emoji status</a> was changed		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeEmojiStatus"/></para></summary>
	[TLDef(0x3EA9FEB1)]
	public sealed partial class ChannelAdminLogEventActionChangeEmojiStatus : ChannelAdminLogEventAction
	{
		/// <summary>Previous emoji status</summary>
		public EmojiStatusBase prev_value;
		/// <summary>New emoji status</summary>
		public EmojiStatusBase new_value;
	}
	/// <summary>The supergroup's <a href="https://corefork.telegram.org/api/boost#setting-a-custom-emoji-stickerset-for-supergroups">custom emoji stickerset</a> was changed.		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionChangeEmojiStickerSet"/></para></summary>
	[TLDef(0x46D840AB)]
	public sealed partial class ChannelAdminLogEventActionChangeEmojiStickerSet : ChannelAdminLogEventActionChangeStickerSet { }
	/// <summary>Channel signature profiles were enabled/disabled.		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionToggleSignatureProfiles"/></para></summary>
	[TLDef(0x60A79C79)]
	public sealed partial class ChannelAdminLogEventActionToggleSignatureProfiles : ChannelAdminLogEventActionToggleSignatures { }
	/// <summary>A paid subscriber has extended their <a href="https://corefork.telegram.org/api/stars#star-subscriptions">Telegram Star subscription »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventActionParticipantSubExtend"/></para></summary>
	[TLDef(0x64642DB3)]
	public sealed partial class ChannelAdminLogEventActionParticipantSubExtend : ChannelAdminLogEventAction
	{
		/// <summary>Same as <c>new_participant</c>.</summary>
		public ChannelParticipantBase prev_participant;
		/// <summary>The subscriber that extended the subscription.</summary>
		public ChannelParticipantBase new_participant;
	}

	/// <summary>Admin log event		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEvent"/></para></summary>
	[TLDef(0x1FAD68CD)]
	public sealed partial class ChannelAdminLogEvent : IObject
	{
		/// <summary>Event ID</summary>
		public long id;
		/// <summary>Date</summary>
		public DateTime date;
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>Action</summary>
		public ChannelAdminLogEventAction action;
	}

	/// <summary>Admin log events		<para>See <a href="https://corefork.telegram.org/constructor/channels.adminLogResults"/></para></summary>
	[TLDef(0xED8AF74D)]
	public sealed partial class Channels_AdminLogResults : IObject, IPeerResolver
	{
		/// <summary>Admin log events</summary>
		public ChannelAdminLogEvent[] events;
		/// <summary>Chats mentioned in events</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in events</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Filter only certain admin log events		<para>See <a href="https://corefork.telegram.org/constructor/channelAdminLogEventsFilter"/></para></summary>
	[TLDef(0xEA107AE4)]
	public sealed partial class ChannelAdminLogEventsFilter : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary><see cref="ChannelAdminLogEventActionParticipantJoin">Join events</see>, including <see cref="ChannelAdminLogEventActionParticipantJoinByInvite">joins using invite links</see> and <see cref="ChannelAdminLogEventActionParticipantJoinByRequest">join requests</see>.</summary>
			join = 0x1,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantLeave">Leave events</see></summary>
			leave = 0x2,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantInvite">Invite events</see></summary>
			invite = 0x4,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantToggleBan">Ban events</see></summary>
			ban = 0x8,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantToggleBan">Unban events</see></summary>
			unban = 0x10,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantToggleBan">Kick events</see></summary>
			kick = 0x20,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantToggleBan">Unkick events</see></summary>
			unkick = 0x40,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantToggleAdmin">Admin promotion events</see></summary>
			promote = 0x80,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantToggleAdmin">Admin demotion events</see></summary>
			demote = 0x100,
			/// <summary>Info change events (when <see cref="ChannelAdminLogEventActionChangeAbout">about</see>, <see cref="ChannelAdminLogEventActionChangeLinkedChat">linked chat</see>, <see cref="ChannelAdminLogEventActionChangeLocation">location</see>, <see cref="ChannelAdminLogEventActionChangePhoto">photo</see>, <see cref="ChannelAdminLogEventActionChangeStickerSet">stickerset</see>, <see cref="ChannelAdminLogEventActionChangeTitle">title</see> or <see cref="ChannelAdminLogEventActionChangeUsername">username</see>, <see cref="ChannelAdminLogEventActionToggleSlowMode">slowmode</see>, <see cref="ChannelAdminLogEventActionChangeHistoryTTL">history TTL</see> settings of a channel gets modified)</summary>
			info = 0x200,
			/// <summary>Settings change events (<see cref="ChannelAdminLogEventActionToggleInvites">invites</see>, <see cref="ChannelAdminLogEventActionTogglePreHistoryHidden">hidden prehistory</see>, <see cref="ChannelAdminLogEventActionToggleSignatures">signatures</see>, <see cref="ChannelAdminLogEventActionDefaultBannedRights">default banned rights</see>, <see cref="ChannelAdminLogEventActionToggleForum">forum toggle events</see>)</summary>
			settings = 0x400,
			/// <summary><see cref="ChannelAdminLogEventActionUpdatePinned">Message pin events</see></summary>
			pinned = 0x800,
			/// <summary><see cref="ChannelAdminLogEventActionEditMessage">Message edit events</see></summary>
			edit = 0x1000,
			/// <summary><see cref="ChannelAdminLogEventActionDeleteMessage">Message deletion events</see></summary>
			delete = 0x2000,
			/// <summary>Group call events</summary>
			group_call = 0x4000,
			/// <summary>Invite events</summary>
			invites = 0x8000,
			/// <summary>A message was posted in a channel</summary>
			send = 0x10000,
			/// <summary><a href="https://corefork.telegram.org/api/forum">Forum</a>-related events</summary>
			forums = 0x20000,
			/// <summary><see cref="ChannelAdminLogEventActionParticipantSubExtend">Telegram Star subscription extension events »</see></summary>
			sub_extend = 0x40000,
		}
	}
	
	/// <summary>Geographical location of supergroup (geogroups)		<para>See <a href="https://corefork.telegram.org/constructor/channelLocation"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/channelLocationEmpty">channelLocationEmpty</a></remarks>
	[TLDef(0x209B82DB)]
	public sealed partial class ChannelLocation : IObject
	{
		/// <summary>Geographical location of supergroup</summary>
		public GeoPoint geo_point;
		/// <summary>Textual description of the address</summary>
		public string address;
	}
	/// <summary>A list of peers that can be used to send messages in a specific group		<para>See <a href="https://corefork.telegram.org/constructor/channels.sendAsPeers"/></para></summary>
	[TLDef(0xF496B0C6)]
	public sealed partial class Channels_SendAsPeers : IObject, IPeerResolver
	{
		/// <summary>Peers that can be used to send messages to the group</summary>
		public SendAsPeer[] peers;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Status of the method call used to report a <a href="https://corefork.telegram.org/api/sponsored-messages">sponsored message »</a>.		<para>See <a href="https://corefork.telegram.org/type/channels.SponsoredMessageReportResult"/></para>		<para>Derived classes: <see cref="Channels_SponsoredMessageReportResultChooseOption"/>, <see cref="Channels_SponsoredMessageReportResultAdsHidden"/>, <see cref="Channels_SponsoredMessageReportResultReported"/></para></summary>
	public abstract partial class Channels_SponsoredMessageReportResult : IObject { }
	/// <summary>The user must choose a report option from the localized options available in <c>options</c>, and after selection, <see cref="SchemaExtensions.Channels_ReportSponsoredMessage">Channels_ReportSponsoredMessage</see> must be invoked again, passing the option's <c>option</c> field to the <c>option</c> param of the method.		<para>See <a href="https://corefork.telegram.org/constructor/channels.sponsoredMessageReportResultChooseOption"/></para></summary>
	[TLDef(0x846F9E42)]
	public sealed partial class Channels_SponsoredMessageReportResultChooseOption : Channels_SponsoredMessageReportResult
	{
		/// <summary>Title of the option selection popup.</summary>
		public string title;
		/// <summary>Localized list of options.</summary>
		public SponsoredMessageReportOption[] options;
	}
	/// <summary>Sponsored messages were hidden for the user in all chats.		<para>See <a href="https://corefork.telegram.org/constructor/channels.sponsoredMessageReportResultAdsHidden"/></para></summary>
	[TLDef(0x3E3BCF2F)]
	public sealed partial class Channels_SponsoredMessageReportResultAdsHidden : Channels_SponsoredMessageReportResult { }
	/// <summary>The sponsored message was reported successfully.		<para>See <a href="https://corefork.telegram.org/constructor/channels.sponsoredMessageReportResultReported"/></para></summary>
	[TLDef(0xAD798849)]
	public sealed partial class Channels_SponsoredMessageReportResultReported : Channels_SponsoredMessageReportResult { }
	
}