using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Object contains info on events occurred.		<para>See <a href="https://corefork.telegram.org/type/Update"/></para>		<para>Derived classes: <see cref="UpdateNewMessage"/>, <see cref="UpdateMessageID"/>, <see cref="UpdateDeleteMessages"/>, <see cref="UpdateUserTyping"/>, <see cref="UpdateChatUserTyping"/>, <see cref="UpdateChatParticipants"/>, <see cref="UpdateUserStatus"/>, <see cref="UpdateUserName"/>, <see cref="UpdateNewAuthorization"/>, <see cref="UpdateNewEncryptedMessage"/>, <see cref="UpdateEncryptedChatTyping"/>, <see cref="UpdateEncryption"/>, <see cref="UpdateEncryptedMessagesRead"/>, <see cref="UpdateChatParticipantAdd"/>, <see cref="UpdateChatParticipantDelete"/>, <see cref="UpdateDcOptions"/>, <see cref="UpdateNotifySettings"/>, <see cref="UpdateServiceNotification"/>, <see cref="UpdatePrivacy"/>, <see cref="UpdateUserPhone"/>, <see cref="UpdateReadHistoryInbox"/>, <see cref="UpdateReadHistoryOutbox"/>, <see cref="UpdateWebPage"/>, <see cref="UpdateReadMessagesContents"/>, <see cref="UpdateChannelTooLong"/>, <see cref="UpdateChannel"/>, <see cref="UpdateNewChannelMessage"/>, <see cref="UpdateReadChannelInbox"/>, <see cref="UpdateDeleteChannelMessages"/>, <see cref="UpdateChannelMessageViews"/>, <see cref="UpdateChatParticipantAdmin"/>, <see cref="UpdateNewStickerSet"/>, <see cref="UpdateStickerSetsOrder"/>, <see cref="UpdateStickerSets"/>, <see cref="UpdateSavedGifs"/>, <see cref="UpdateBotInlineQuery"/>, <see cref="UpdateBotInlineSend"/>, <see cref="UpdateEditChannelMessage"/>, <see cref="UpdateBotCallbackQuery"/>, <see cref="UpdateEditMessage"/>, <see cref="UpdateInlineBotCallbackQuery"/>, <see cref="UpdateReadChannelOutbox"/>, <see cref="UpdateDraftMessage"/>, <see cref="UpdateReadFeaturedStickers"/>, <see cref="UpdateRecentStickers"/>, <see cref="UpdateConfig"/>, <see cref="UpdatePtsChanged"/>, <see cref="UpdateChannelWebPage"/>, <see cref="UpdateDialogPinned"/>, <see cref="UpdatePinnedDialogs"/>, <see cref="UpdateBotWebhookJSON"/>, <see cref="UpdateBotWebhookJSONQuery"/>, <see cref="UpdateBotShippingQuery"/>, <see cref="UpdateBotPrecheckoutQuery"/>, <see cref="UpdatePhoneCall"/>, <see cref="UpdateLangPackTooLong"/>, <see cref="UpdateLangPack"/>, <see cref="UpdateFavedStickers"/>, <see cref="UpdateChannelReadMessagesContents"/>, <see cref="UpdateContactsReset"/>, <see cref="UpdateChannelAvailableMessages"/>, <see cref="UpdateDialogUnreadMark"/>, <see cref="UpdateMessagePoll"/>, <see cref="UpdateChatDefaultBannedRights"/>, <see cref="UpdateFolderPeers"/>, <see cref="UpdatePeerSettings"/>, <see cref="UpdatePeerLocated"/>, <see cref="UpdateNewScheduledMessage"/>, <see cref="UpdateDeleteScheduledMessages"/>, <see cref="UpdateTheme"/>, <see cref="UpdateGeoLiveViewed"/>, <see cref="UpdateLoginToken"/>, <see cref="UpdateMessagePollVote"/>, <see cref="UpdateDialogFilter"/>, <see cref="UpdateDialogFilterOrder"/>, <see cref="UpdateDialogFilters"/>, <see cref="UpdatePhoneCallSignalingData"/>, <see cref="UpdateChannelMessageForwards"/>, <see cref="UpdateReadChannelDiscussionInbox"/>, <see cref="UpdateReadChannelDiscussionOutbox"/>, <see cref="UpdatePeerBlocked"/>, <see cref="UpdateChannelUserTyping"/>, <see cref="UpdatePinnedMessages"/>, <see cref="UpdatePinnedChannelMessages"/>, <see cref="UpdateChat"/>, <see cref="UpdateGroupCallParticipants"/>, <see cref="UpdateGroupCall"/>, <see cref="UpdatePeerHistoryTTL"/>, <see cref="UpdateChatParticipant"/>, <see cref="UpdateChannelParticipant"/>, <see cref="UpdateBotStopped"/>, <see cref="UpdateGroupCallConnection"/>, <see cref="UpdateBotCommands"/>, <see cref="UpdatePendingJoinRequests"/>, <see cref="UpdateBotChatInviteRequester"/>, <see cref="UpdateMessageReactions"/>, <see cref="UpdateAttachMenuBots"/>, <see cref="UpdateWebViewResultSent"/>, <see cref="UpdateBotMenuButton"/>, <see cref="UpdateSavedRingtones"/>, <see cref="UpdateTranscribedAudio"/>, <see cref="UpdateReadFeaturedEmojiStickers"/>, <see cref="UpdateUserEmojiStatus"/>, <see cref="UpdateRecentEmojiStatuses"/>, <see cref="UpdateRecentReactions"/>, <see cref="UpdateMoveStickerSetToTop"/>, <see cref="UpdateMessageExtendedMedia"/>, <see cref="UpdateChannelPinnedTopic"/>, <see cref="UpdateChannelPinnedTopics"/>, <see cref="UpdateUser"/>, <see cref="UpdateAutoSaveSettings"/>, <see cref="UpdateStory"/>, <see cref="UpdateReadStories"/>, <see cref="UpdateStoryID"/>, <see cref="UpdateStoriesStealthMode"/>, <see cref="UpdateSentStoryReaction"/>, <see cref="UpdateBotChatBoost"/>, <see cref="UpdateChannelViewForumAsMessages"/>, <see cref="UpdatePeerWallpaper"/>, <see cref="UpdateBotMessageReaction"/>, <see cref="UpdateBotMessageReactions"/>, <see cref="UpdateSavedDialogPinned"/>, <see cref="UpdatePinnedSavedDialogs"/>, <see cref="UpdateSavedReactionTags"/>, <see cref="UpdateSmsJob"/>, <see cref="UpdateQuickReplies"/>, <see cref="UpdateNewQuickReply"/>, <see cref="UpdateDeleteQuickReply"/>, <see cref="UpdateQuickReplyMessage"/>, <see cref="UpdateDeleteQuickReplyMessages"/>, <see cref="UpdateBotBusinessConnect"/>, <see cref="UpdateBotNewBusinessMessage"/>, <see cref="UpdateBotEditBusinessMessage"/>, <see cref="UpdateBotDeleteBusinessMessage"/>, <see cref="UpdateNewStoryReaction"/>, <see cref="UpdateBroadcastRevenueTransactions"/>, <see cref="UpdateStarsBalance"/>, <see cref="UpdateBusinessBotCallbackQuery"/>, <see cref="UpdateStarsRevenueStatus"/>, <see cref="UpdateBotPurchasedPaidMedia"/>, <see cref="UpdatePaidReactionPrivacy"/></para></summary>
    public abstract partial class Update : IObject
    {
        public virtual (long mbox_id, int pts, int pts_count) GetMBox() => default;
    }
    
    /// <summary>New message in a private chat or in a <a href="https://corefork.telegram.org/api/channel#basic-groups">basic group</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateNewMessage"/></para></summary>
	[TLDef(0x1F2B0AFD)]
	public partial class UpdateNewMessage : Update
	{
		/// <summary>Message</summary>
		public MessageBase message;
		/// <summary>New quantity of actions in a message box</summary>
		public int pts;
		/// <summary>Number of generated events</summary>
		public int pts_count;

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>Sent message with <strong>random_id</strong> client identifier was assigned an identifier.		<para>See <a href="https://corefork.telegram.org/constructor/updateMessageID"/></para></summary>
	[TLDef(0x4E90BFD6)]
	public sealed partial class UpdateMessageID : Update
	{
		/// <summary><strong>id</strong> identifier of a respective <see cref="MessageBase"/></summary>
		public int id;
		/// <summary>Previously transferred client <strong>random_id</strong> identifier</summary>
		public long random_id;
	}
	/// <summary>Messages were deleted.		<para>See <a href="https://corefork.telegram.org/constructor/updateDeleteMessages"/></para></summary>
	[TLDef(0xA20DB0E5)]
	public partial class UpdateDeleteMessages : Update
	{
		/// <summary>List of identifiers of deleted messages</summary>
		public int[] messages;
		/// <summary>New quality of actions in a message box</summary>
		public int pts;
		/// <summary>Number of generated <a href="https://corefork.telegram.org/api/updates">events</a></summary>
		public int pts_count;

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>The user is preparing a message; typing, recording, uploading, etc. This update is valid for 6 seconds. If no further updates of this kind are received after 6 seconds, it should be considered that the user stopped doing whatever they were doing		<para>See <a href="https://corefork.telegram.org/constructor/updateUserTyping"/></para></summary>
	[TLDef(0xC01E857F, inheritBefore = true)]
	public sealed partial class UpdateUserTyping : UpdateUser
	{
		/// <summary>Action type</summary>
		public SendMessageAction action;
	}
	/// <summary>The user is preparing a message in a group; typing, recording, uploading, etc. This update is valid for 6 seconds. If no further updates of this kind are received after 6 seconds, it should be considered that the user stopped doing whatever they were doing		<para>See <a href="https://corefork.telegram.org/constructor/updateChatUserTyping"/></para></summary>
	[TLDef(0x83487AF0, inheritBefore = true)]
	public sealed partial class UpdateChatUserTyping : UpdateChat
	{
		/// <summary>Peer that started typing (can be the chat itself, in case of anonymous admins).</summary>
		public Peer from_id;
		/// <summary>Type of action</summary>
		public SendMessageAction action;
	}
	/// <summary>Composition of chat participants changed.		<para>See <a href="https://corefork.telegram.org/constructor/updateChatParticipants"/></para></summary>
	[TLDef(0x07761198)]
	public sealed partial class UpdateChatParticipants : Update
	{
		/// <summary>Updated chat participants</summary>
		public ChatParticipantsBase participants;
	}
	/// <summary>Contact status update.		<para>See <a href="https://corefork.telegram.org/constructor/updateUserStatus"/></para></summary>
	[TLDef(0xE5BDF8DE, inheritBefore = true)]
	public sealed partial class UpdateUserStatus : UpdateUser
	{
		/// <summary>New status</summary>
		public UserStatus status;
	}
	/// <summary>Changes the user's first name, last name and username.		<para>See <a href="https://corefork.telegram.org/constructor/updateUserName"/></para></summary>
	[TLDef(0xA7848924, inheritBefore = true)]
	public sealed partial class UpdateUserName : UpdateUser
	{
		/// <summary>New first name. Corresponds to the new value of <strong>real_first_name</strong> field of the <see cref="UserFull"/>.</summary>
		public string first_name;
		/// <summary>New last name. Corresponds to the new value of <strong>real_last_name</strong> field of the <see cref="UserFull"/>.</summary>
		public string last_name;
		/// <summary>Usernames.</summary>
		public Username[] usernames;
	}
	/// <summary>A new session logged into the current user's account through an unknown device.		<para>See <a href="https://corefork.telegram.org/constructor/updateNewAuthorization"/></para></summary>
	[TLDef(0x8951ABEF)]
	public sealed partial class UpdateNewAuthorization : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>Authorization date</summary>
		[IfFlag(0)] public DateTime date;
		/// <summary>Name of device, for example <em>Android</em></summary>
		[IfFlag(0)] public string device;
		/// <summary>Location, for example <em>USA, NY (IP=1.2.3.4)</em></summary>
		[IfFlag(0)] public string location;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the session is <a href="https://corefork.telegram.org/api/auth#confirming-login">unconfirmed, see here »</a> for more info.</summary>
			unconfirmed = 0x1,
		}
	}
	/// <summary>New encrypted message.		<para>See <a href="https://corefork.telegram.org/constructor/updateNewEncryptedMessage"/></para></summary>
	[TLDef(0x12BCBD9A)]
	public sealed partial class UpdateNewEncryptedMessage : Update
	{
		/// <summary>Message</summary>
		public EncryptedMessageBase message;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>Interlocutor is typing a message in an encrypted chat. Update period is 6 second. If upon this time there is no repeated update, it shall be considered that the interlocutor stopped typing.		<para>See <a href="https://corefork.telegram.org/constructor/updateEncryptedChatTyping"/></para></summary>
	[TLDef(0x1710F156)]
	public sealed partial class UpdateEncryptedChatTyping : Update
	{
		/// <summary>Chat ID</summary>
		public int chat_id;
	}
	/// <summary>Change of state in an encrypted chat.		<para>See <a href="https://corefork.telegram.org/constructor/updateEncryption"/></para></summary>
	[TLDef(0xB4A2E88D)]
	public sealed partial class UpdateEncryption : Update
	{
		/// <summary>Encrypted chat</summary>
		public EncryptedChatBase chat;
		/// <summary>Date of change</summary>
		public DateTime date;
	}
	/// <summary>Communication history in an encrypted chat was marked as read.		<para>See <a href="https://corefork.telegram.org/constructor/updateEncryptedMessagesRead"/></para></summary>
	[TLDef(0x38FE25B7)]
	public sealed partial class UpdateEncryptedMessagesRead : Update
	{
		/// <summary>Chat ID</summary>
		public int chat_id;
		/// <summary>Maximum value of data for read messages</summary>
		public DateTime max_date;
		/// <summary>Time when messages were read</summary>
		public DateTime date;
	}
	/// <summary>New group member.		<para>See <a href="https://corefork.telegram.org/constructor/updateChatParticipantAdd"/></para></summary>
	[TLDef(0x3DDA5451, inheritBefore = true)]
	public sealed partial class UpdateChatParticipantAdd : UpdateChat
	{
		/// <summary>ID of the new member</summary>
		public long user_id;
		/// <summary>ID of the user, who added member to the group</summary>
		public long inviter_id;
		/// <summary>When was the participant added</summary>
		public DateTime date;
		/// <summary>Chat version number</summary>
		public int version;
	}
	/// <summary>A member has left the group.		<para>See <a href="https://corefork.telegram.org/constructor/updateChatParticipantDelete"/></para></summary>
	[TLDef(0xE32F3D77, inheritBefore = true)]
	public sealed partial class UpdateChatParticipantDelete : UpdateChat
	{
		/// <summary>ID of the user</summary>
		public long user_id;
		/// <summary>Used in basic groups to reorder updates and make sure that all of them was received.</summary>
		public int version;
	}
	/// <summary>Changes in the data center configuration options.		<para>See <a href="https://corefork.telegram.org/constructor/updateDcOptions"/></para></summary>
	[TLDef(0x8E5E9873)]
	public sealed partial class UpdateDcOptions : Update
	{
		/// <summary>New connection options</summary>
		public DcOption[] dc_options;
	}
	/// <summary>Changes in notification settings.		<para>See <a href="https://corefork.telegram.org/constructor/updateNotifySettings"/></para></summary>
	[TLDef(0xBEC268EF)]
	public sealed partial class UpdateNotifySettings : Update
	{
		/// <summary>Notification source</summary>
		public NotifyPeerBase peer;
		/// <summary>New notification settings</summary>
		public PeerNotifySettings notify_settings;
	}
	/// <summary>A service message for the user.		<para>See <a href="https://corefork.telegram.org/constructor/updateServiceNotification"/></para></summary>
	[TLDef(0xEBE46819)]
	public sealed partial class UpdateServiceNotification : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>When was the notification received<br/>The message must also be stored locally as part of the message history with the user id <c>777000</c> (Telegram Notifications).</summary>
		[IfFlag(1)] public DateTime inbox_date;
		/// <summary>String, identical in format and contents to the <a href="https://corefork.telegram.org/api/errors#error-type"><strong>type</strong></a> field in API errors. Describes type of service message. It is acceptable to ignore repeated messages of the same <strong>type</strong> within a short period of time (15 minutes).</summary>
		public string type;
		/// <summary>Message text</summary>
		public string message;
		/// <summary>Media content (optional)</summary>
		public MessageMedia media;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		public MessageEntity[] entities;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, the message must be displayed in a popup.</summary>
			popup = 0x1,
			/// <summary>Field <see cref="inbox_date"/> has a value</summary>
			has_inbox_date = 0x2,
			/// <summary>If set, any eventual webpage preview will be shown on top of the message instead of at the bottom.</summary>
			invert_media = 0x4,
		}
	}
	/// <summary>Privacy rules were changed		<para>See <a href="https://corefork.telegram.org/constructor/updatePrivacy"/></para></summary>
	[TLDef(0xEE3B272A)]
	public sealed partial class UpdatePrivacy : Update
	{
		/// <summary>Peers to which the privacy rules apply</summary>
		public PrivacyKey key;
		/// <summary>New privacy rules</summary>
		public PrivacyRule[] rules;
	}
	/// <summary>A user's phone number was changed		<para>See <a href="https://corefork.telegram.org/constructor/updateUserPhone"/></para></summary>
	[TLDef(0x05492A13, inheritBefore = true)]
	public sealed partial class UpdateUserPhone : UpdateUser
	{
		/// <summary>New phone number</summary>
		public string phone;
	}
	/// <summary>Incoming messages were read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadHistoryInbox"/></para></summary>
	[TLDef(0x9C974FDF)]
	public sealed partial class UpdateReadHistoryInbox : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(0)] public int folder_id;
		/// <summary>Peer</summary>
		public Peer peer;
		/// <summary>Maximum ID of messages read</summary>
		public int max_id;
		/// <summary>Number of messages that are still unread</summary>
		public int still_unread_count;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x1,
		}

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>Outgoing messages were read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadHistoryOutbox"/></para></summary>
	[TLDef(0x2F2F21BF)]
	public sealed partial class UpdateReadHistoryOutbox : Update
	{
		/// <summary>Peer</summary>
		public Peer peer;
		/// <summary>Maximum ID of read outgoing messages</summary>
		public int max_id;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>An <a href="https://instantview.telegram.org">instant view</a> webpage preview was generated		<para>See <a href="https://corefork.telegram.org/constructor/updateWebPage"/></para></summary>
	[TLDef(0x7F891213)]
	public partial class UpdateWebPage : Update
	{
		/// <summary>Webpage preview</summary>
		public WebPageBase webpage;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>Contents of messages in the common <a href="https://corefork.telegram.org/api/updates">message box</a> were read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadMessagesContents"/></para></summary>
	[TLDef(0xF8227181)]
	public sealed partial class UpdateReadMessagesContents : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>IDs of read messages</summary>
		public int[] messages;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;
		/// <summary>When was the last message in <c>messages</c> marked as read.</summary>
		[IfFlag(0)] public DateTime date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="date"/> has a value</summary>
			has_date = 0x1,
		}

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>There are new updates in the specified channel, the client must fetch them.<br/>If the difference is too long or if the channel isn't currently in the states, start fetching from the specified pts.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelTooLong"/></para></summary>
	[TLDef(0x108D941F)]
	public sealed partial class UpdateChannelTooLong : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The channel</summary>
		public long channel_id;
		/// <summary>The <a href="https://corefork.telegram.org/api/updates">PTS</a>.</summary>
		[IfFlag(0)] public int pts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="pts"/> has a value</summary>
			has_pts = 0x1,
		}

		public override (long, int, int) GetMBox() => (channel_id, pts, 0);
	}
	/// <summary>Channel/supergroup (<see cref="Channel"/> and/or <see cref="ChannelFull"/>) information was updated.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannel"/></para></summary>
	[TLDef(0x635B4C09)]
	public partial class UpdateChannel : Update
	{
		/// <summary>Channel ID</summary>
		public long channel_id;
	}
	/// <summary>A new message was sent in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a>		<para>See <a href="https://corefork.telegram.org/constructor/updateNewChannelMessage"/></para></summary>
	[TLDef(0x62BA04D9)]
	public sealed partial class UpdateNewChannelMessage : UpdateNewMessage
	{
		public override (long, int, int) GetMBox() => (message.Peer is PeerChannel pc ? pc.channel_id : 0, pts, pts_count);
	}
	/// <summary>Incoming messages in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> were read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadChannelInbox"/></para></summary>
	[TLDef(0x922E6E10)]
	public sealed partial class UpdateReadChannelInbox : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(0)] public int folder_id;
		/// <summary>Channel/supergroup ID</summary>
		public long channel_id;
		/// <summary>Position up to which all incoming messages are read.</summary>
		public int max_id;
		/// <summary>Count of messages weren't read yet</summary>
		public int still_unread_count;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x1,
		}

		public override (long, int, int) GetMBox() => (channel_id, pts, 0);
	}
	/// <summary>Some messages in a <a href="https://corefork.telegram.org/api/channel">supergroup/channel</a> were deleted		<para>See <a href="https://corefork.telegram.org/constructor/updateDeleteChannelMessages"/></para></summary>
	[TLDef(0xC32D5B12)]
	public sealed partial class UpdateDeleteChannelMessages : UpdateDeleteMessages
	{
		/// <summary>Channel ID</summary>
		public long channel_id;

		public override (long, int, int) GetMBox() => (channel_id, pts, pts_count);
	}
	/// <summary>The view counter of a message in a channel has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelMessageViews"/></para></summary>
	[TLDef(0xF226AC08, inheritBefore = true)]
	public sealed partial class UpdateChannelMessageViews : UpdateChannel
	{
		/// <summary>ID of the message</summary>
		public int id;
		/// <summary>New view counter</summary>
		public int views;
	}
	/// <summary>Admin permissions of a user in a <a href="https://corefork.telegram.org/api/channel#basic-groups">basic group</a> were changed		<para>See <a href="https://corefork.telegram.org/constructor/updateChatParticipantAdmin"/></para></summary>
	[TLDef(0xD7CA61A2, inheritBefore = true)]
	public sealed partial class UpdateChatParticipantAdmin : UpdateChat
	{
		/// <summary>ID of the (de)admined user</summary>
		public long user_id;
		/// <summary>Whether the user was rendered admin</summary>
		public bool is_admin;
		/// <summary>Used in basic groups to reorder updates and make sure that all of them was received.</summary>
		public int version;
	}
	/// <summary>A new stickerset was installed		<para>See <a href="https://corefork.telegram.org/constructor/updateNewStickerSet"/></para></summary>
	[TLDef(0x688A30AA)]
	public sealed partial class UpdateNewStickerSet : Update
	{
		/// <summary>The installed stickerset</summary>
		public Messages_StickerSet stickerset;
	}
	/// <summary>The order of stickersets was changed		<para>See <a href="https://corefork.telegram.org/constructor/updateStickerSetsOrder"/></para></summary>
	[TLDef(0x0BB2D201)]
	public sealed partial class UpdateStickerSetsOrder : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>New sticker order by sticker ID</summary>
		public long[] order;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the updated stickers are mask stickers</summary>
			masks = 0x1,
			/// <summary>Whether the updated stickers are custom emoji stickers</summary>
			emojis = 0x2,
		}
	}
	/// <summary>Installed stickersets have changed, the client should refetch them as <a href="https://corefork.telegram.org/api/stickers#installing-stickersets">described in the docs</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateStickerSets"/></para></summary>
	[TLDef(0x31C24808)]
	public sealed partial class UpdateStickerSets : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether mask stickersets have changed</summary>
			masks = 0x1,
			/// <summary>Whether the list of installed <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji stickersets</a> has changed</summary>
			emojis = 0x2,
		}
	}
	/// <summary>The saved gif list has changed, the client should refetch it using <see cref="SchemaExtensions.Messages_GetSavedGifs">Messages_GetSavedGifs</see>		<para>See <a href="https://corefork.telegram.org/constructor/updateSavedGifs"/></para></summary>
	[TLDef(0x9375341E)]
	public sealed partial class UpdateSavedGifs : Update { }
	/// <summary>An incoming inline query		<para>See <a href="https://corefork.telegram.org/constructor/updateBotInlineQuery"/></para></summary>
	[TLDef(0x496F379C)]
	public sealed partial class UpdateBotInlineQuery : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Query ID</summary>
		public long query_id;
		/// <summary>User that sent the query</summary>
		public long user_id;
		/// <summary>Text of query</summary>
		public string query;
		/// <summary>Attached geolocation</summary>
		[IfFlag(0)] public GeoPoint geo;
		/// <summary>Type of the chat from which the inline query was sent.</summary>
		[IfFlag(1)] public InlineQueryPeerType peer_type;
		/// <summary>Offset to navigate through results</summary>
		public string offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="geo"/> has a value</summary>
			has_geo = 0x1,
			/// <summary>Field <see cref="peer_type"/> has a value</summary>
			has_peer_type = 0x2,
		}
	}
	/// <summary>The result of an inline query that was chosen by a user and sent to their chat partner. Please see our documentation on the <a href="https://corefork.telegram.org/bots/inline#collecting-feedback">feedback collecting</a> for details on how to enable these updates for your bot.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotInlineSend"/></para></summary>
	[TLDef(0x12F12A07)]
	public sealed partial class UpdateBotInlineSend : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The user that chose the result</summary>
		public long user_id;
		/// <summary>The query that was used to obtain the result</summary>
		public string query;
		/// <summary>Optional. Sender location, only for bots that require user location</summary>
		[IfFlag(0)] public GeoPoint geo;
		/// <summary>The unique identifier for the result that was chosen</summary>
		public string id;
		/// <summary>Identifier of the sent inline message. Available only if there is an inline keyboard attached to the message. Will be also received in callback queries and can be used to edit the message.</summary>
		[IfFlag(1)] public InputBotInlineMessageIDBase msg_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="geo"/> has a value</summary>
			has_geo = 0x1,
			/// <summary>Field <see cref="msg_id"/> has a value</summary>
			has_msg_id = 0x2,
		}
	}
	/// <summary>A message was edited in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a>		<para>See <a href="https://corefork.telegram.org/constructor/updateEditChannelMessage"/></para></summary>
	[TLDef(0x1B3F4DF7)]
	public sealed partial class UpdateEditChannelMessage : UpdateEditMessage
	{
		public override (long, int, int) GetMBox() => (message.Peer is PeerChannel pc ? pc.channel_id : 0, pts, pts_count);
	}
	/// <summary>A callback button was pressed, and the button data was sent to the bot that created the button		<para>See <a href="https://corefork.telegram.org/constructor/updateBotCallbackQuery"/></para></summary>
	[TLDef(0xB9CFC48D)]
	public sealed partial class UpdateBotCallbackQuery : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Query ID</summary>
		public long query_id;
		/// <summary>ID of the user that pressed the button</summary>
		public long user_id;
		/// <summary>Chat where the inline keyboard was sent</summary>
		public Peer peer;
		/// <summary>Message ID</summary>
		public int msg_id;
		/// <summary>Global identifier, uniquely corresponding to the chat to which the message with the callback button was sent. Useful for high scores in games.</summary>
		public long chat_instance;
		/// <summary>Callback data</summary>
		[IfFlag(0)] public byte[] data;
		/// <summary>Short name of a Game to be returned, serves as the unique identifier for the game</summary>
		[IfFlag(1)] public string game_short_name;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="data"/> has a value</summary>
			has_data = 0x1,
			/// <summary>Field <see cref="game_short_name"/> has a value</summary>
			has_game_short_name = 0x2,
		}
	}
	/// <summary>A message was edited		<para>See <a href="https://corefork.telegram.org/constructor/updateEditMessage"/></para></summary>
	[TLDef(0xE40370A3)]
	public partial class UpdateEditMessage : Update
	{
		/// <summary>The new edited message</summary>
		public MessageBase message;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS count</a></summary>
		public int pts_count;

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>This notification is received by bots when a button is pressed		<para>See <a href="https://corefork.telegram.org/constructor/updateInlineBotCallbackQuery"/></para></summary>
	[TLDef(0x691E9052)]
	public sealed partial class UpdateInlineBotCallbackQuery : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Query ID</summary>
		public long query_id;
		/// <summary>ID of the user that pressed the button</summary>
		public long user_id;
		/// <summary>ID of the inline message with the button</summary>
		public InputBotInlineMessageIDBase msg_id;
		/// <summary>Global identifier, uniquely corresponding to the chat to which the message with the callback button was sent. Useful for high scores in games.</summary>
		public long chat_instance;
		/// <summary>Data associated with the callback button. Be aware that a bad client can send arbitrary data in this field.</summary>
		[IfFlag(0)] public byte[] data;
		/// <summary>Short name of a Game to be returned, serves as the unique identifier for the game</summary>
		[IfFlag(1)] public string game_short_name;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="data"/> has a value</summary>
			has_data = 0x1,
			/// <summary>Field <see cref="game_short_name"/> has a value</summary>
			has_game_short_name = 0x2,
		}
	}
	/// <summary>Outgoing messages in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> were read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadChannelOutbox"/></para></summary>
	[TLDef(0xB75F99A9)]
	public sealed partial class UpdateReadChannelOutbox : Update
	{
		/// <summary>Channel/supergroup ID</summary>
		public long channel_id;
		/// <summary>Position up to which all outgoing messages are read.</summary>
		public int max_id;
	}
	/// <summary>Notifies a change of a message <a href="https://corefork.telegram.org/api/drafts">draft</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateDraftMessage"/></para></summary>
	[TLDef(0x1B49EC6D)]
	public sealed partial class UpdateDraftMessage : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The peer to which the draft is associated</summary>
		public Peer peer;
		/// <summary>ID of the <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> to which the draft is associated</summary>
		[IfFlag(0)] public int top_msg_id;
		/// <summary>The draft</summary>
		public DraftMessageBase draft;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="top_msg_id"/> has a value</summary>
			has_top_msg_id = 0x1,
		}
	}
	/// <summary>Some featured stickers were marked as read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadFeaturedStickers"/></para></summary>
	[TLDef(0x571D2742)]
	public sealed partial class UpdateReadFeaturedStickers : Update { }
	/// <summary>The recent sticker list was updated		<para>See <a href="https://corefork.telegram.org/constructor/updateRecentStickers"/></para></summary>
	[TLDef(0x9A422C20)]
	public sealed partial class UpdateRecentStickers : Update { }
	/// <summary>The server-side configuration has changed; the client should re-fetch the config using <see cref="SchemaExtensions.Help_GetConfig">Help_GetConfig</see> and <see cref="SchemaExtensions.Help_GetAppConfig">Help_GetAppConfig</see>.		<para>See <a href="https://corefork.telegram.org/constructor/updateConfig"/></para></summary>
	[TLDef(0xA229DD06)]
	public sealed partial class UpdateConfig : Update { }
	/// <summary><a href="https://corefork.telegram.org/api/updates">Common message box sequence PTS</a> has changed, <a href="https://corefork.telegram.org/api/updates#fetching-state">state has to be refetched using updates.getState</a>		<para>See <a href="https://corefork.telegram.org/constructor/updatePtsChanged"/></para></summary>
	[TLDef(0x3354678F)]
	public sealed partial class UpdatePtsChanged : Update { }
	/// <summary>A webpage preview of a link in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> message was generated		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelWebPage"/></para></summary>
	[TLDef(0x2F2BA99F)]
	public sealed partial class UpdateChannelWebPage : UpdateWebPage
	{
		/// <summary><a href="https://corefork.telegram.org/api/channel">Channel/supergroup</a> ID</summary>
		public long channel_id;

		public override (long, int, int) GetMBox() => (channel_id, pts, pts_count);
	}
	/// <summary>A dialog was pinned/unpinned		<para>See <a href="https://corefork.telegram.org/constructor/updateDialogPinned"/></para></summary>
	[TLDef(0x6E6FE51C)]
	public sealed partial class UpdateDialogPinned : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(1)] public int folder_id;
		/// <summary>The dialog</summary>
		public DialogPeerBase peer;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the dialog was pinned</summary>
			pinned = 0x1,
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x2,
		}
	}
	/// <summary>Pinned dialogs were updated		<para>See <a href="https://corefork.telegram.org/constructor/updatePinnedDialogs"/></para></summary>
	[TLDef(0xFA0F3CA2)]
	public sealed partial class UpdatePinnedDialogs : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(1)] public int folder_id;
		/// <summary>New order of pinned dialogs</summary>
		[IfFlag(0)] public DialogPeerBase[] order;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="order"/> has a value</summary>
			has_order = 0x1,
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x2,
		}
	}
	/// <summary>A new incoming event; for bots only		<para>See <a href="https://corefork.telegram.org/constructor/updateBotWebhookJSON"/></para></summary>
	[TLDef(0x8317C0C3)]
	public sealed partial class UpdateBotWebhookJSON : Update
	{
		/// <summary>The event</summary>
		public DataJSON data;
	}
	/// <summary>A new incoming query; for bots only		<para>See <a href="https://corefork.telegram.org/constructor/updateBotWebhookJSONQuery"/></para></summary>
	[TLDef(0x9B9240A6)]
	public sealed partial class UpdateBotWebhookJSONQuery : Update
	{
		/// <summary>Query identifier</summary>
		public long query_id;
		/// <summary>Query data</summary>
		public DataJSON data;
		/// <summary>Query timeout</summary>
		public int timeout;
	}
	/// <summary>This object contains information about an incoming shipping query.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotShippingQuery"/></para></summary>
	[TLDef(0xB5AEFD7D)]
	public sealed partial class UpdateBotShippingQuery : Update
	{
		/// <summary>Unique query identifier</summary>
		public long query_id;
		/// <summary>User who sent the query</summary>
		public long user_id;
		/// <summary>Bot specified invoice payload</summary>
		public byte[] payload;
		/// <summary>User specified shipping address</summary>
		public PostAddress shipping_address;
	}
	/// <summary>This object contains information about an incoming pre-checkout query.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotPrecheckoutQuery"/></para></summary>
	[TLDef(0x8CAA9A96)]
	public sealed partial class UpdateBotPrecheckoutQuery : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Unique query identifier</summary>
		public long query_id;
		/// <summary>User who sent the query</summary>
		public long user_id;
		/// <summary>Bot specified invoice payload</summary>
		public byte[] payload;
		/// <summary>Order info provided by the user</summary>
		[IfFlag(0)] public PaymentRequestedInfo info;
		/// <summary>Identifier of the shipping option chosen by the user</summary>
		[IfFlag(1)] public string shipping_option_id;
		/// <summary>Three-letter ISO 4217 <a href="https://corefork.telegram.org/bots/payments#supported-currencies">currency</a> code, or <c>XTR</c> for <a href="https://corefork.telegram.org/api/stars">Telegram Stars</a>.</summary>
		public string currency;
		/// <summary>Total amount in the smallest units of the currency (integer, not float/double). For example, for a price of <c>US$ 1.45</c> pass <c>amount = 145</c>. See the exp parameter in <a href="https://corefork.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).</summary>
		public long total_amount;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="info"/> has a value</summary>
			has_info = 0x1,
			/// <summary>Field <see cref="shipping_option_id"/> has a value</summary>
			has_shipping_option_id = 0x2,
		}
	}
	/// <summary>An incoming phone call		<para>See <a href="https://corefork.telegram.org/constructor/updatePhoneCall"/></para></summary>
	[TLDef(0xAB0F6B1E)]
	public sealed partial class UpdatePhoneCall : Update
	{
		/// <summary>Phone call</summary>
		public PhoneCallBase phone_call;
	}
	/// <summary>A language pack has changed, the client should manually fetch the changed strings using <see cref="SchemaExtensions.Langpack_GetDifference">Langpack_GetDifference</see>		<para>See <a href="https://corefork.telegram.org/constructor/updateLangPackTooLong"/></para></summary>
	[TLDef(0x46560264)]
	public sealed partial class UpdateLangPackTooLong : Update
	{
		/// <summary>Language code</summary>
		public string lang_code;
	}
	/// <summary>Language pack updated		<para>See <a href="https://corefork.telegram.org/constructor/updateLangPack"/></para></summary>
	[TLDef(0x56022F4D)]
	public sealed partial class UpdateLangPack : Update
	{
		/// <summary>Changed strings</summary>
		public LangPackDifference difference;
	}
	/// <summary>The list of favorited stickers was changed, the client should call <see cref="SchemaExtensions.Messages_GetFavedStickers">Messages_GetFavedStickers</see> to refetch the new list		<para>See <a href="https://corefork.telegram.org/constructor/updateFavedStickers"/></para></summary>
	[TLDef(0xE511996D)]
	public sealed partial class UpdateFavedStickers : Update { }
	/// <summary>The specified <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> messages were read		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelReadMessagesContents"/></para></summary>
	[TLDef(0xEA29055D)]
	public sealed partial class UpdateChannelReadMessagesContents : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/channel">Channel/supergroup</a> ID</summary>
		public long channel_id;
		/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Forum topic ID</a>.</summary>
		[IfFlag(0)] public int top_msg_id;
		/// <summary>IDs of messages that were read</summary>
		public int[] messages;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="top_msg_id"/> has a value</summary>
			has_top_msg_id = 0x1,
		}
	}
	/// <summary>All contacts were deleted		<para>See <a href="https://corefork.telegram.org/constructor/updateContactsReset"/></para></summary>
	[TLDef(0x7084A7BE)]
	public sealed partial class UpdateContactsReset : Update { }
	/// <summary>The history of a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> was hidden.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelAvailableMessages"/></para></summary>
	[TLDef(0xB23FC698, inheritBefore = true)]
	public sealed partial class UpdateChannelAvailableMessages : UpdateChannel
	{
		/// <summary>Identifier of a maximum unavailable message in a channel due to hidden history.</summary>
		public int available_min_id;
	}
	/// <summary>The manual unread mark of a chat was changed		<para>See <a href="https://corefork.telegram.org/constructor/updateDialogUnreadMark"/></para></summary>
	[TLDef(0xE16459C3)]
	public sealed partial class UpdateDialogUnreadMark : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The dialog</summary>
		public DialogPeerBase peer;

		[Flags] public enum Flags : uint
		{
			/// <summary>Was the chat marked or unmarked as read</summary>
			unread = 0x1,
		}
	}
	/// <summary>The results of a poll have changed		<para>See <a href="https://corefork.telegram.org/constructor/updateMessagePoll"/></para></summary>
	[TLDef(0xACA1657B)]
	public sealed partial class UpdateMessagePoll : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Poll ID</summary>
		public long poll_id;
		/// <summary>If the server knows the client hasn't cached this poll yet, the poll itself</summary>
		[IfFlag(0)] public Poll poll;
		/// <summary>New poll results</summary>
		public PollResults results;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="poll"/> has a value</summary>
			has_poll = 0x1,
		}
	}
	/// <summary>Default banned rights in a <a href="https://corefork.telegram.org/api/channel">normal chat</a> were updated		<para>See <a href="https://corefork.telegram.org/constructor/updateChatDefaultBannedRights"/></para></summary>
	[TLDef(0x54C01850)]
	public sealed partial class UpdateChatDefaultBannedRights : Update
	{
		/// <summary>The chat</summary>
		public Peer peer;
		/// <summary>New default banned rights</summary>
		public ChatBannedRights default_banned_rights;
		/// <summary>Version</summary>
		public int version;
	}
	/// <summary>The peer list of a <a href="https://corefork.telegram.org/api/folders#peer-folders">peer folder</a> was updated		<para>See <a href="https://corefork.telegram.org/constructor/updateFolderPeers"/></para></summary>
	[TLDef(0x19360DC0)]
	public sealed partial class UpdateFolderPeers : Update
	{
		/// <summary>New peer list</summary>
		public FolderPeer[] folder_peers;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>Settings of a certain peer have changed		<para>See <a href="https://corefork.telegram.org/constructor/updatePeerSettings"/></para></summary>
	[TLDef(0x6A7E7366)]
	public sealed partial class UpdatePeerSettings : Update
	{
		/// <summary>The peer</summary>
		public Peer peer;
		/// <summary>Associated peer settings</summary>
		public PeerSettings settings;
	}
	/// <summary>List of peers near you was updated		<para>See <a href="https://corefork.telegram.org/constructor/updatePeerLocated"/></para></summary>
	[TLDef(0xB4AFCFB0)]
	public sealed partial class UpdatePeerLocated : Update
	{
		/// <summary>Geolocated peer list update</summary>
		public PeerLocatedBase[] peers;
	}
	/// <summary>A message was added to the <a href="https://corefork.telegram.org/api/scheduled-messages">schedule queue of a chat</a>		<para>See <a href="https://corefork.telegram.org/constructor/updateNewScheduledMessage"/></para></summary>
	[TLDef(0x39A51DFB)]
	public sealed partial class UpdateNewScheduledMessage : Update
	{
		/// <summary>Message</summary>
		public MessageBase message;
	}
	/// <summary>Some <a href="https://corefork.telegram.org/api/scheduled-messages">scheduled messages</a> were deleted (or sent) from the schedule queue of a chat		<para>See <a href="https://corefork.telegram.org/constructor/updateDeleteScheduledMessages"/></para></summary>
	[TLDef(0xF2A71983)]
	public sealed partial class UpdateDeleteScheduledMessages : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer</summary>
		public Peer peer;
		/// <summary>Deleted scheduled messages</summary>
		public int[] messages;
		/// <summary>If set, this update indicates that some scheduled messages were sent (not simply deleted from the schedule queue).  <br/>In this case, the <c>messages</c> field will contain the scheduled message IDs for the sent messages (initially returned in <see cref="UpdateNewScheduledMessage"/>), and <c>sent_messages</c> will contain the real message IDs for the sent messages.</summary>
		[IfFlag(0)] public int[] sent_messages;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="sent_messages"/> has a value</summary>
			has_sent_messages = 0x1,
		}
	}
	/// <summary>A cloud theme was updated		<para>See <a href="https://corefork.telegram.org/constructor/updateTheme"/></para></summary>
	[TLDef(0x8216FBA3)]
	public sealed partial class UpdateTheme : Update
	{
		/// <summary>Theme</summary>
		public Theme theme;
	}
	/// <summary>Live geoposition message was viewed		<para>See <a href="https://corefork.telegram.org/constructor/updateGeoLiveViewed"/></para></summary>
	[TLDef(0x871FB939)]
	public sealed partial class UpdateGeoLiveViewed : Update
	{
		/// <summary>The user that viewed the live geoposition</summary>
		public Peer peer;
		/// <summary>Message ID of geoposition message</summary>
		public int msg_id;
	}
	/// <summary>A login token (for login via QR code) was accepted.		<para>See <a href="https://corefork.telegram.org/constructor/updateLoginToken"/></para></summary>
	[TLDef(0x564FE691)]
	public sealed partial class UpdateLoginToken : Update { }
	/// <summary>A specific peer has voted in a poll		<para>See <a href="https://corefork.telegram.org/constructor/updateMessagePollVote"/></para></summary>
	[TLDef(0x24F40E77)]
	public sealed partial class UpdateMessagePollVote : Update
	{
		/// <summary>Poll ID</summary>
		public long poll_id;
		/// <summary>The peer that voted in the poll</summary>
		public Peer peer;
		/// <summary>Chosen option(s)</summary>
		public byte[][] options;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A new <a href="https://corefork.telegram.org/api/folders">folder</a> was added		<para>See <a href="https://corefork.telegram.org/constructor/updateDialogFilter"/></para></summary>
	[TLDef(0x26FFDE7D)]
	public sealed partial class UpdateDialogFilter : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> ID</summary>
		public int id;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> info</summary>
		[IfFlag(0)] public DialogFilterBase filter;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="filter"/> has a value</summary>
			has_filter = 0x1,
		}
	}
	/// <summary>New <a href="https://corefork.telegram.org/api/folders">folder</a> order		<para>See <a href="https://corefork.telegram.org/constructor/updateDialogFilterOrder"/></para></summary>
	[TLDef(0xA5D72105)]
	public sealed partial class UpdateDialogFilterOrder : Update
	{
		/// <summary>Ordered <a href="https://corefork.telegram.org/api/folders">folder IDs</a></summary>
		public int[] order;
	}
	/// <summary>Clients should update <a href="https://corefork.telegram.org/api/folders">folder</a> info		<para>See <a href="https://corefork.telegram.org/constructor/updateDialogFilters"/></para></summary>
	[TLDef(0x3504914F)]
	public sealed partial class UpdateDialogFilters : Update { }
	/// <summary>Incoming phone call signaling payload		<para>See <a href="https://corefork.telegram.org/constructor/updatePhoneCallSignalingData"/></para></summary>
	[TLDef(0x2661BF09)]
	public sealed partial class UpdatePhoneCallSignalingData : Update
	{
		/// <summary>Phone call ID</summary>
		public long phone_call_id;
		/// <summary>Signaling payload</summary>
		public byte[] data;
	}
	/// <summary>The forward counter of a message in a channel has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelMessageForwards"/></para></summary>
	[TLDef(0xD29A27F4, inheritBefore = true)]
	public sealed partial class UpdateChannelMessageForwards : UpdateChannel
	{
		/// <summary>ID of the message</summary>
		public int id;
		/// <summary>New forward counter</summary>
		public int forwards;
	}
	/// <summary>Incoming comments in a <a href="https://corefork.telegram.org/api/threads">discussion thread</a> were marked as read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadChannelDiscussionInbox"/></para></summary>
	[TLDef(0xD6B19546)]
	public sealed partial class UpdateReadChannelDiscussionInbox : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/channel">Discussion group ID</a></summary>
		public long channel_id;
		/// <summary>ID of the group message that started the <a href="https://corefork.telegram.org/api/threads">thread</a> (message in linked discussion group)</summary>
		public int top_msg_id;
		/// <summary>Message ID of latest read incoming message for this <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		public int read_max_id;
		/// <summary>If set, contains the ID of the <a href="https://corefork.telegram.org/api/channel">channel</a> that contains the post that started the <a href="https://corefork.telegram.org/api/threads">comment thread</a> in the discussion group (<c>channel_id</c>)</summary>
		[IfFlag(0)] public long broadcast_id;
		/// <summary>If set, contains the ID of the channel post that started the <a href="https://corefork.telegram.org/api/threads">comment thread</a></summary>
		[IfFlag(0)] public int broadcast_post;

		[Flags] public enum Flags : uint
		{
			/// <summary>Fields <see cref="broadcast_id"/> and <see cref="broadcast_post"/> have a value</summary>
			has_broadcast_id = 0x1,
		}
	}
	/// <summary>Outgoing comments in a <a href="https://corefork.telegram.org/api/threads">discussion thread</a> were marked as read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadChannelDiscussionOutbox"/></para></summary>
	[TLDef(0x695C9E7C)]
	public sealed partial class UpdateReadChannelDiscussionOutbox : Update
	{
		/// <summary><a href="https://corefork.telegram.org/api/channel">Supergroup ID</a></summary>
		public long channel_id;
		/// <summary>ID of the group message that started the <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		public int top_msg_id;
		/// <summary>Message ID of latest read outgoing message for this <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		public int read_max_id;
	}
	/// <summary>We blocked a peer, see <a href="https://corefork.telegram.org/api/block">here »</a> for more info on blocklists.		<para>See <a href="https://corefork.telegram.org/constructor/updatePeerBlocked"/></para></summary>
	[TLDef(0xEBE07752)]
	public sealed partial class UpdatePeerBlocked : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The (un)blocked peer</summary>
		public Peer peer_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the peer was blocked or unblocked</summary>
			blocked = 0x1,
			/// <summary>Whether the peer was added/removed to/from the story blocklist; if not set, this update affects the main blocklist, see <a href="https://corefork.telegram.org/api/block">here »</a> for more info.</summary>
			blocked_my_stories_from = 0x2,
		}
	}
	/// <summary>A user is typing in a <a href="https://corefork.telegram.org/api/channel">supergroup, channel</a> or <a href="https://corefork.telegram.org/api/threads">message thread</a>		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelUserTyping"/></para></summary>
	[TLDef(0x8C88C923)]
	public sealed partial class UpdateChannelUserTyping : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Channel ID</summary>
		public long channel_id;
		/// <summary><a href="https://corefork.telegram.org/api/threads">Thread ID</a></summary>
		[IfFlag(0)] public int top_msg_id;
		/// <summary>The peer that is typing</summary>
		public Peer from_id;
		/// <summary>Whether the user is typing, sending a media or doing something else</summary>
		public SendMessageAction action;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="top_msg_id"/> has a value</summary>
			has_top_msg_id = 0x1,
		}
	}
	/// <summary>Some messages were pinned in a chat		<para>See <a href="https://corefork.telegram.org/constructor/updatePinnedMessages"/></para></summary>
	[TLDef(0xED85EAB5)]
	public sealed partial class UpdatePinnedMessages : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer</summary>
		public Peer peer;
		/// <summary>Message IDs</summary>
		public int[] messages;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the messages were pinned or unpinned</summary>
			pinned = 0x1,
		}

		public override (long, int, int) GetMBox() => (0, pts, pts_count);
	}
	/// <summary>Messages were pinned/unpinned in a <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a>		<para>See <a href="https://corefork.telegram.org/constructor/updatePinnedChannelMessages"/></para></summary>
	[TLDef(0x5BB98608)]
	public sealed partial class UpdatePinnedChannelMessages : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Channel ID</summary>
		public long channel_id;
		/// <summary>Messages</summary>
		public int[] messages;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Number of events that were generated</a></summary>
		public int pts_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the messages were pinned or unpinned</summary>
			pinned = 0x1,
		}

		public override (long, int, int) GetMBox() => (channel_id, pts, pts_count);
	}
	/// <summary>Chat (<see cref="Chat"/> and/or <see cref="ChatFull"/>) information was updated.		<para>See <a href="https://corefork.telegram.org/constructor/updateChat"/></para></summary>
	[TLDef(0xF89A6A4E)]
	public partial class UpdateChat : Update
	{
		/// <summary>Chat ID</summary>
		public long chat_id;
	}
	/// <summary>The participant list of a certain group call has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateGroupCallParticipants"/></para></summary>
	[TLDef(0xF2EBDB4E)]
	public sealed partial class UpdateGroupCallParticipants : Update
	{
		/// <summary>Group call</summary>
		public InputGroupCall call;
		/// <summary>New participant list</summary>
		public GroupCallParticipant[] participants;
		/// <summary>Version</summary>
		public int version;
	}
	/// <summary>A new groupcall was started		<para>See <a href="https://corefork.telegram.org/constructor/updateGroupCall"/></para></summary>
	[TLDef(0x97D64341)]
	public sealed partial class UpdateGroupCall : Update
	{
		public Flags flags;
		/// <summary>The <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a> where this group call or livestream takes place</summary>
		[IfFlag(0)] public long chat_id;
		/// <summary>Info about the group call or livestream</summary>
		public GroupCallBase call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="chat_id"/> has a value</summary>
			has_chat_id = 0x1,
		}
	}
	/// <summary>The Time-To-Live for messages sent by the current user in a specific chat has changed		<para>See <a href="https://corefork.telegram.org/constructor/updatePeerHistoryTTL"/></para></summary>
	[TLDef(0xBB9BB9A5)]
	public sealed partial class UpdatePeerHistoryTTL : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The chat</summary>
		public Peer peer;
		/// <summary>The new Time-To-Live</summary>
		[IfFlag(0)] public int ttl_period;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x1,
		}
	}
	/// <summary>A user has joined or left a specific chat		<para>See <a href="https://corefork.telegram.org/constructor/updateChatParticipant"/></para></summary>
	[TLDef(0xD087663A)]
	public sealed partial class UpdateChatParticipant : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/channel">Chat</a> ID</summary>
		public long chat_id;
		/// <summary>When did this event occur</summary>
		public DateTime date;
		/// <summary>User that triggered the change (inviter, admin that kicked the user, or the even the <strong>user_id</strong> itself)</summary>
		public long actor_id;
		/// <summary>User that was affected by the change</summary>
		public long user_id;
		/// <summary>Previous participant info (empty if this participant just joined)</summary>
		[IfFlag(0)] public ChatParticipantBase prev_participant;
		/// <summary>New participant info (empty if this participant just left)</summary>
		[IfFlag(1)] public ChatParticipantBase new_participant;
		/// <summary>The invite that was used to join the group</summary>
		[IfFlag(2)] public ExportedChatInvite invite;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="prev_participant"/> has a value</summary>
			has_prev_participant = 0x1,
			/// <summary>Field <see cref="new_participant"/> has a value</summary>
			has_new_participant = 0x2,
			/// <summary>Field <see cref="invite"/> has a value</summary>
			has_invite = 0x4,
		}

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A participant has left, joined, was banned or admined in a <a href="https://corefork.telegram.org/api/channel">channel or supergroup</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelParticipant"/></para></summary>
	[TLDef(0x985D3ABB)]
	public sealed partial class UpdateChannelParticipant : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Channel ID</summary>
		public long channel_id;
		/// <summary>Date of the event</summary>
		public DateTime date;
		/// <summary>User that triggered the change (inviter, admin that kicked the user, or the even the <strong>user_id</strong> itself)</summary>
		public long actor_id;
		/// <summary>User that was affected by the change</summary>
		public long user_id;
		/// <summary>Previous participant status</summary>
		[IfFlag(0)] public ChannelParticipantBase prev_participant;
		/// <summary>New participant status</summary>
		[IfFlag(1)] public ChannelParticipantBase new_participant;
		/// <summary>Chat invite used to join the <a href="https://corefork.telegram.org/api/channel">channel/supergroup</a></summary>
		[IfFlag(2)] public ExportedChatInvite invite;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="prev_participant"/> has a value</summary>
			has_prev_participant = 0x1,
			/// <summary>Field <see cref="new_participant"/> has a value</summary>
			has_new_participant = 0x2,
			/// <summary>Field <see cref="invite"/> has a value</summary>
			has_invite = 0x4,
			/// <summary>Whether the participant joined using a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.</summary>
			via_chatlist = 0x8,
		}

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A bot was stopped or re-started.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotStopped"/></para></summary>
	[TLDef(0xC4870A49)]
	public sealed partial class UpdateBotStopped : Update
	{
		/// <summary>The user ID</summary>
		public long user_id;
		/// <summary>When did this action occur</summary>
		public DateTime date;
		/// <summary>Whether the bot was stopped or started</summary>
		public bool stopped;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>New WebRTC parameters		<para>See <a href="https://corefork.telegram.org/constructor/updateGroupCallConnection"/></para></summary>
	[TLDef(0x0B783982)]
	public sealed partial class UpdateGroupCallConnection : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>WebRTC parameters</summary>
		public DataJSON params_;

		[Flags] public enum Flags : uint
		{
			/// <summary>Are these parameters related to the screen capture session currently in progress?</summary>
			presentation = 0x1,
		}
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/bots/commands">command set</a> of a certain bot in a certain chat has changed.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotCommands"/></para></summary>
	[TLDef(0x4D712F2E)]
	public sealed partial class UpdateBotCommands : Update
	{
		/// <summary>The affected chat</summary>
		public Peer peer;
		/// <summary>ID of the bot that changed its command set</summary>
		public long bot_id;
		/// <summary>New bot commands</summary>
		public BotCommand[] commands;
	}
	/// <summary>Someone has requested to join a chat or channel		<para>See <a href="https://corefork.telegram.org/constructor/updatePendingJoinRequests"/></para></summary>
	[TLDef(0x7063C3DB)]
	public sealed partial class UpdatePendingJoinRequests : Update
	{
		/// <summary>Chat or channel</summary>
		public Peer peer;
		/// <summary>Number of pending <a href="https://corefork.telegram.org/api/invites#join-requests">join requests »</a> for the chat or channel</summary>
		public int requests_pending;
		/// <summary>IDs of users that have recently requested to join</summary>
		public long[] recent_requesters;
	}
	/// <summary>Someone has requested to join a chat or channel (bots only, users will receive an <see cref="UpdatePendingJoinRequests"/>, instead)		<para>See <a href="https://corefork.telegram.org/constructor/updateBotChatInviteRequester"/></para></summary>
	[TLDef(0x11DFA986)]
	public sealed partial class UpdateBotChatInviteRequester : Update
	{
		/// <summary>The chat or channel in question</summary>
		public Peer peer;
		/// <summary>When was the <a href="https://corefork.telegram.org/api/invites#join-requests">join request »</a> made</summary>
		public DateTime date;
		/// <summary>The user ID that is asking to join the chat or channel</summary>
		public long user_id;
		/// <summary>Bio of the user</summary>
		public string about;
		/// <summary>Chat invite link that was used by the user to send the <a href="https://corefork.telegram.org/api/invites#join-requests">join request »</a></summary>
		public ExportedChatInvite invite;
		/// <summary><a href="https://corefork.telegram.org/api/updates">QTS</a> event sequence identifier</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>New <a href="https://corefork.telegram.org/api/reactions">message reactions »</a> are available		<para>See <a href="https://corefork.telegram.org/constructor/updateMessageReactions"/></para></summary>
	[TLDef(0x5E1B3CB8)]
	public sealed partial class UpdateMessageReactions : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer</summary>
		public Peer peer;
		/// <summary>Message ID</summary>
		public int msg_id;
		/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Forum topic ID</a></summary>
		[IfFlag(0)] public int top_msg_id;
		/// <summary>Reactions</summary>
		public MessageReactions reactions;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="top_msg_id"/> has a value</summary>
			has_top_msg_id = 0x1,
		}
	}
	/// <summary>The list of installed <a href="https://corefork.telegram.org/api/bots/attach">attachment menu entries »</a> has changed, use <see cref="SchemaExtensions.Messages_GetAttachMenuBots">Messages_GetAttachMenuBots</see> to fetch the updated list.		<para>See <a href="https://corefork.telegram.org/constructor/updateAttachMenuBots"/></para></summary>
	[TLDef(0x17B7A20B)]
	public sealed partial class UpdateAttachMenuBots : Update { }
	/// <summary>Indicates to a bot that a webview was closed and an inline message was sent on behalf of the user using <see cref="SchemaExtensions.Messages_SendWebViewResultMessage">Messages_SendWebViewResultMessage</see>		<para>See <a href="https://corefork.telegram.org/constructor/updateWebViewResultSent"/></para></summary>
	[TLDef(0x1592B79D)]
	public sealed partial class UpdateWebViewResultSent : Update
	{
		/// <summary>Web app interaction ID</summary>
		public long query_id;
	}
	/// <summary>The menu button behavior for the specified bot has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateBotMenuButton"/></para></summary>
	[TLDef(0x14B85813)]
	public sealed partial class UpdateBotMenuButton : Update
	{
		/// <summary>Bot ID</summary>
		public long bot_id;
		/// <summary>New menu button</summary>
		public BotMenuButtonBase button;
	}
	/// <summary>The list of saved notification sounds has changed, use <see cref="SchemaExtensions.Account_GetSavedRingtones">Account_GetSavedRingtones</see> to fetch the new list.		<para>See <a href="https://corefork.telegram.org/constructor/updateSavedRingtones"/></para></summary>
	[TLDef(0x74D8BE99)]
	public sealed partial class UpdateSavedRingtones : Update { }
	/// <summary>A pending <a href="https://corefork.telegram.org/api/transcribe">voice message transcription »</a> initiated with <see cref="SchemaExtensions.Messages_TranscribeAudio">Messages_TranscribeAudio</see> was updated.		<para>See <a href="https://corefork.telegram.org/constructor/updateTranscribedAudio"/></para></summary>
	[TLDef(0x0084CD5A)]
	public sealed partial class UpdateTranscribedAudio : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer of the transcribed message</summary>
		public Peer peer;
		/// <summary>Transcribed message ID</summary>
		public int msg_id;
		/// <summary>Transcription ID</summary>
		public long transcription_id;
		/// <summary>Transcribed text</summary>
		public string text;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this transcription is still pending and further <see cref="UpdateTranscribedAudio"/> about it will be sent in the future.</summary>
			pending = 0x1,
		}
	}
	/// <summary>Some featured <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji stickers</a> were marked as read		<para>See <a href="https://corefork.telegram.org/constructor/updateReadFeaturedEmojiStickers"/></para></summary>
	[TLDef(0xFB4C496C)]
	public sealed partial class UpdateReadFeaturedEmojiStickers : Update { }
	/// <summary>The <a href="https://corefork.telegram.org/api/emoji-status">emoji status</a> of a certain user has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateUserEmojiStatus"/></para></summary>
	[TLDef(0x28373599, inheritBefore = true)]
	public sealed partial class UpdateUserEmojiStatus : UpdateUser
	{
		/// <summary>New <a href="https://corefork.telegram.org/api/emoji-status">emoji status</a></summary>
		public EmojiStatusBase emoji_status;
	}
	/// <summary>The list of recent <a href="https://corefork.telegram.org/api/emoji-status">emoji statuses</a> has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateRecentEmojiStatuses"/></para></summary>
	[TLDef(0x30F443DB)]
	public sealed partial class UpdateRecentEmojiStatuses : Update { }
	/// <summary>The list of recent <a href="https://corefork.telegram.org/api/reactions">message reactions</a> has changed		<para>See <a href="https://corefork.telegram.org/constructor/updateRecentReactions"/></para></summary>
	[TLDef(0x6F7863F4)]
	public sealed partial class UpdateRecentReactions : Update { }
	/// <summary>A stickerset was just moved to top, <a href="https://corefork.telegram.org/api/stickers#recent-stickersets">see here for more info »</a>		<para>See <a href="https://corefork.telegram.org/constructor/updateMoveStickerSetToTop"/></para></summary>
	[TLDef(0x86FCCF85)]
	public sealed partial class UpdateMoveStickerSetToTop : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/stickers">Stickerset</a> ID</summary>
		public long stickerset;

		[Flags] public enum Flags : uint
		{
			/// <summary>This update is referring to a <a href="https://corefork.telegram.org/api/stickers#mask-stickers">mask stickerset</a></summary>
			masks = 0x1,
			/// <summary>This update is referring to a <a href="https://corefork.telegram.org/api/custom-emoji">custom emoji stickerset</a></summary>
			emojis = 0x2,
		}
	}
	/// <summary>You <a href="https://corefork.telegram.org/api/paid-media">bought a paid media »</a>: this update contains the revealed media.		<para>See <a href="https://corefork.telegram.org/constructor/updateMessageExtendedMedia"/></para></summary>
	[TLDef(0xD5A41724)]
	public sealed partial class UpdateMessageExtendedMedia : Update
	{
		/// <summary>Peer where the paid media was posted</summary>
		public Peer peer;
		/// <summary>ID of the message containing the paid media</summary>
		public int msg_id;
		/// <summary>Revealed media, contains only <see cref="MessageExtendedMedia"/>s.</summary>
		public MessageExtendedMediaBase[] extended_media;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic »</a> was pinned or unpinned.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelPinnedTopic"/></para></summary>
	[TLDef(0x192EFBE3)]
	public sealed partial class UpdateChannelPinnedTopic : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The forum ID</summary>
		public long channel_id;
		/// <summary>The topic ID</summary>
		public int topic_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the topic was pinned or unpinned</summary>
			pinned = 0x1,
		}
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/forum#forum-topics">pinned topics</a> of a forum have changed.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelPinnedTopics"/></para></summary>
	[TLDef(0xFE198602)]
	public sealed partial class UpdateChannelPinnedTopics : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Forum ID.</summary>
		public long channel_id;
		/// <summary>Ordered list containing the IDs of all pinned topics.</summary>
		[IfFlag(0)] public int[] order;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="order"/> has a value</summary>
			has_order = 0x1,
		}
	}
	/// <summary>User (<see cref="User"/> and/or <see cref="UserFull"/>) information was updated.		<para>See <a href="https://corefork.telegram.org/constructor/updateUser"/></para></summary>
	[TLDef(0x20529438)]
	public partial class UpdateUser : Update
	{
		/// <summary>User ID</summary>
		public long user_id;
	}
	/// <summary>Media autosave settings have changed and must be refetched using <see cref="SchemaExtensions.Account_GetAutoSaveSettings">Account_GetAutoSaveSettings</see>.		<para>See <a href="https://corefork.telegram.org/constructor/updateAutoSaveSettings"/></para></summary>
	[TLDef(0xEC05B097)]
	public sealed partial class UpdateAutoSaveSettings : Update { }
	/// <summary>A new story was posted.		<para>See <a href="https://corefork.telegram.org/constructor/updateStory"/></para></summary>
	[TLDef(0x75B3B798)]
	public sealed partial class UpdateStory : Update
	{
		/// <summary>ID of the poster.</summary>
		public Peer peer;
		/// <summary>The story that was posted.</summary>
		public StoryItemBase story;
	}
	/// <summary>Stories of a specific peer were marked as read.		<para>See <a href="https://corefork.telegram.org/constructor/updateReadStories"/></para></summary>
	[TLDef(0xF74E932B)]
	public sealed partial class UpdateReadStories : Update
	{
		/// <summary>The peer</summary>
		public Peer peer;
		/// <summary>ID of the last story that was marked as read</summary>
		public int max_id;
	}
	/// <summary>A story was successfully uploaded.		<para>See <a href="https://corefork.telegram.org/constructor/updateStoryID"/></para></summary>
	[TLDef(0x1BF335B9)]
	public sealed partial class UpdateStoryID : Update
	{
		/// <summary>The <c>id</c> that was attributed to the story.</summary>
		public int id;
		/// <summary>The <c>random_id</c> that was passed to <see cref="SchemaExtensions.Stories_SendStory">Stories_SendStory</see>.</summary>
		public long random_id;
	}
	/// <summary>Indicates that <a href="https://corefork.telegram.org/api/stories#stealth-mode">stories stealth mode</a> was activated.		<para>See <a href="https://corefork.telegram.org/constructor/updateStoriesStealthMode"/></para></summary>
	[TLDef(0x2C084DC1)]
	public sealed partial class UpdateStoriesStealthMode : Update
	{
		/// <summary>Information about the current <a href="https://corefork.telegram.org/api/stories#stealth-mode">stealth mode</a> session.</summary>
		public StoriesStealthMode stealth_mode;
	}
	/// <summary>Indicates we <a href="https://corefork.telegram.org/api/stories#reactions">reacted to a story »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateSentStoryReaction"/></para></summary>
	[TLDef(0x7D627683)]
	public sealed partial class UpdateSentStoryReaction : Update
	{
		/// <summary>The peer that sent the story</summary>
		public Peer peer;
		/// <summary>ID of the story we reacted to</summary>
		public int story_id;
		/// <summary>The reaction that was sent</summary>
		public Reaction reaction;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/boost">channel/supergroup boost</a> has changed (bots only)		<para>See <a href="https://corefork.telegram.org/constructor/updateBotChatBoost"/></para></summary>
	[TLDef(0x904DD49C)]
	public sealed partial class UpdateBotChatBoost : Update
	{
		/// <summary>Channel</summary>
		public Peer peer;
		/// <summary>New boost information</summary>
		public Boost boost;
		/// <summary><a href="https://corefork.telegram.org/api/updates">QTS</a> event sequence identifier</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>Users may also choose to display messages from all topics as if they were sent to a normal group, using a "View as messages" setting in the local client.<br/>This setting only affects the current account, and is synced to other logged in sessions using the <see cref="SchemaExtensions.Channels_ToggleViewForumAsMessages">Channels_ToggleViewForumAsMessages</see> method; invoking this method will update the value of the <c>view_forum_as_messages</c> flag of <see cref="ChannelFull"/> or <see cref="Dialog"/> and emit an <see cref="UpdateChannelViewForumAsMessages"/>.		<para>See <a href="https://corefork.telegram.org/constructor/updateChannelViewForumAsMessages"/></para></summary>
	[TLDef(0x07B68920, inheritBefore = true)]
	public sealed partial class UpdateChannelViewForumAsMessages : UpdateChannel
	{
		/// <summary>The new value of the toggle.</summary>
		public bool enabled;
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/wallpapers">wallpaper »</a> of a given peer has changed.		<para>See <a href="https://corefork.telegram.org/constructor/updatePeerWallpaper"/></para></summary>
	[TLDef(0xAE3F101D)]
	public sealed partial class UpdatePeerWallpaper : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The peer where the wallpaper has changed.</summary>
		public Peer peer;
		/// <summary>The new wallpaper, if none the wallpaper was removed and the default wallpaper should be used.</summary>
		[IfFlag(0)] public WallPaperBase wallpaper;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="wallpaper"/> has a value</summary>
			has_wallpaper = 0x1,
			/// <summary>Whether the other user has chosen a custom wallpaper for us using <see cref="SchemaExtensions.Messages_SetChatWallPaper">Messages_SetChatWallPaper</see> and the <c>for_both</c> flag, see <a href="https://corefork.telegram.org/api/wallpapers#installing-wallpapers-in-a-specific-chat-or-channel">here »</a> for more info.</summary>
			wallpaper_overridden = 0x2,
		}
	}
	/// <summary>Bots only: a user has changed their reactions on a message with public reactions.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotMessageReaction"/></para></summary>
	[TLDef(0xAC21D3CE)]
	public sealed partial class UpdateBotMessageReaction : Update
	{
		/// <summary>Peer of the reacted-to message.</summary>
		public Peer peer;
		/// <summary>ID of the reacted-to message.</summary>
		public int msg_id;
		/// <summary>Date of the change.</summary>
		public DateTime date;
		/// <summary>The user that (un)reacted to the message.</summary>
		public Peer actor;
		/// <summary>Old reactions</summary>
		public Reaction[] old_reactions;
		/// <summary>New reactions</summary>
		public Reaction[] new_reactions;
		/// <summary><a href="https://corefork.telegram.org/api/updates">QTS</a> event sequence identifier</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>Bots only: the number of reactions on a message with anonymous reactions has changed.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotMessageReactions"/></para></summary>
	[TLDef(0x09CB7759)]
	public sealed partial class UpdateBotMessageReactions : Update
	{
		/// <summary>Peer of the reacted-to message.</summary>
		public Peer peer;
		/// <summary>ID of the reacted-to message.</summary>
		public int msg_id;
		/// <summary>Date of the change.</summary>
		public DateTime date;
		/// <summary>New reaction counters.</summary>
		public ReactionCount[] reactions;
		/// <summary><a href="https://corefork.telegram.org/api/updates">QTS</a> event sequence identifier</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/saved-messages">saved message dialog</a> was pinned/unpinned		<para>See <a href="https://corefork.telegram.org/constructor/updateSavedDialogPinned"/></para></summary>
	[TLDef(0xAEAF9E74)]
	public sealed partial class UpdateSavedDialogPinned : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The dialog</summary>
		public DialogPeerBase peer;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the dialog was pinned</summary>
			pinned = 0x1,
		}
	}
	/// <summary><a href="https://corefork.telegram.org/api/saved-messages">Pinned saved dialogs »</a> were updated		<para>See <a href="https://corefork.telegram.org/constructor/updatePinnedSavedDialogs"/></para></summary>
	[TLDef(0x686C85A6)]
	public sealed partial class UpdatePinnedSavedDialogs : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>New order of pinned saved dialogs</summary>
		[IfFlag(0)] public DialogPeerBase[] order;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="order"/> has a value</summary>
			has_order = 0x1,
		}
	}
	/// <summary>The list of <a href="https://corefork.telegram.org/api/saved-messages#tags">reaction tag »</a> names assigned by the user has changed and should be refetched using <see cref="SchemaExtensions.Messages_GetSavedReactionTags">Messages_GetSavedReactionTags</see>.		<para>See <a href="https://corefork.telegram.org/constructor/updateSavedReactionTags"/></para></summary>
	[TLDef(0x39C67432)]
	public sealed partial class UpdateSavedReactionTags : Update { }
	/// <summary>A new SMS job was received		<para>See <a href="https://corefork.telegram.org/constructor/updateSmsJob"/></para></summary>
	[TLDef(0xF16269D4)]
	public sealed partial class UpdateSmsJob : Update
	{
		/// <summary>SMS job ID</summary>
		public string job_id;
	}
	/// <summary>Info about or the order of <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcuts »</a> was changed.		<para>See <a href="https://corefork.telegram.org/constructor/updateQuickReplies"/></para></summary>
	[TLDef(0xF9470AB2)]
	public sealed partial class UpdateQuickReplies : Update
	{
		/// <summary>New quick reply shortcut order and information.</summary>
		public QuickReply[] quick_replies;
	}
	/// <summary>A new <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut »</a> was created.		<para>See <a href="https://corefork.telegram.org/constructor/updateNewQuickReply"/></para></summary>
	[TLDef(0xF53DA717)]
	public sealed partial class UpdateNewQuickReply : Update
	{
		/// <summary>Quick reply shortcut.</summary>
		public QuickReply quick_reply;
	}
	/// <summary>A <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut »</a> was deleted. This will <strong>not</strong> emit <see cref="UpdateDeleteQuickReplyMessages"/> updates, even if all the messages in the shortcut are also deleted by this update.		<para>See <a href="https://corefork.telegram.org/constructor/updateDeleteQuickReply"/></para></summary>
	[TLDef(0x53E6F1EC)]
	public partial class UpdateDeleteQuickReply : Update
	{
		/// <summary>ID of the quick reply shortcut that was deleted.</summary>
		public int shortcut_id;
	}
	/// <summary>A new message was added to a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateQuickReplyMessage"/></para></summary>
	[TLDef(0x3E050D0F)]
	public sealed partial class UpdateQuickReplyMessage : Update
	{
		/// <summary>The message that was added (the <see cref="Message"/>.<c>quick_reply_shortcut_id</c> field will contain the shortcut ID).</summary>
		public MessageBase message;
	}
	/// <summary>One or more messages in a <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcut »</a> were deleted.		<para>See <a href="https://corefork.telegram.org/constructor/updateDeleteQuickReplyMessages"/></para></summary>
	[TLDef(0x566FE7CD, inheritBefore = true)]
	public sealed partial class UpdateDeleteQuickReplyMessages : UpdateDeleteQuickReply
	{
		/// <summary>IDs of the deleted messages.</summary>
		public int[] messages;
	}
	/// <summary>Connecting or disconnecting a <a href="https://corefork.telegram.org/api/business#connected-bots">business bot</a> or changing the connection settings will emit an <see cref="UpdateBotBusinessConnect"/> update to the bot, with the new settings and a <c>connection_id</c> that will be used by the bot to handle updates from and send messages as the user.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotBusinessConnect"/></para></summary>
	[TLDef(0x8AE5C97A)]
	public sealed partial class UpdateBotBusinessConnect : Update
	{
		/// <summary>Business connection settings</summary>
		public BotBusinessConnection connection;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A message was received via a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business chat »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotNewBusinessMessage"/></para></summary>
	[TLDef(0x9DDB347C)]
	public sealed partial class UpdateBotNewBusinessMessage : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Connection ID.</summary>
		public string connection_id;
		/// <summary>New message.</summary>
		public MessageBase message;
		/// <summary>The message that <c>message</c> is replying to.</summary>
		[IfFlag(0)] public MessageBase reply_to_message;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_to_message"/> has a value</summary>
			has_reply_to_message = 0x1,
		}

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A message was edited in a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business chat »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotEditBusinessMessage"/></para></summary>
	[TLDef(0x07DF587C)]
	public sealed partial class UpdateBotEditBusinessMessage : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Business connection ID</summary>
		public string connection_id;
		/// <summary>New message.</summary>
		public MessageBase message;
		/// <summary>The message that <c>message</c> is replying to.</summary>
		[IfFlag(0)] public MessageBase reply_to_message;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_to_message"/> has a value</summary>
			has_reply_to_message = 0x1,
		}

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>A message was deleted in a <a href="https://corefork.telegram.org/api/business#connected-bots">connected business chat »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotDeleteBusinessMessage"/></para></summary>
	[TLDef(0xA02A982E)]
	public sealed partial class UpdateBotDeleteBusinessMessage : Update
	{
		/// <summary>Business connection ID.</summary>
		public string connection_id;
		/// <summary><a href="https://corefork.telegram.org/api/peers">Peer</a> where the messages were deleted.</summary>
		public Peer peer;
		/// <summary>IDs of the messages that were deleted.</summary>
		public int[] messages;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>Represents a new <a href="https://corefork.telegram.org/api/reactions#notifications-about-reactions">reaction to a story</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateNewStoryReaction"/></para></summary>
	[TLDef(0x1824E40B)]
	public sealed partial class UpdateNewStoryReaction : Update
	{
		/// <summary><a href="https://corefork.telegram.org/api/stories">Story ID</a>.</summary>
		public int story_id;
		/// <summary>The peer where the story was posted.</summary>
		public Peer peer;
		/// <summary>The <a href="https://corefork.telegram.org/api/reactions">reaction</a>.</summary>
		public Reaction reaction;
	}
	/// <summary>A new <a href="https://corefork.telegram.org/api/revenue#revenue-statistics">channel ad revenue transaction was made, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/updateBroadcastRevenueTransactions"/></para></summary>
	[TLDef(0xDFD961F5)]
	public sealed partial class UpdateBroadcastRevenueTransactions : Update
	{
		/// <summary>Channel</summary>
		public Peer peer;
		/// <summary>New ad revenue balance.</summary>
		public BroadcastRevenueBalances balances;
	}
	/// <summary>The current account's <a href="https://corefork.telegram.org/api/stars">Telegram Stars balance »</a> has changed.		<para>See <a href="https://corefork.telegram.org/constructor/updateStarsBalance"/></para></summary>
	[TLDef(0x4E80A379)]
	public sealed partial class UpdateStarsBalance : Update
	{
		/// <summary>New balance.</summary>
		public StarsAmount balance;
	}
	/// <summary>A callback button sent via a <a href="https://corefork.telegram.org/api/business#connected-bots">business connection</a> was pressed, and the button data was sent to the bot that created the button.		<para>See <a href="https://corefork.telegram.org/constructor/updateBusinessBotCallbackQuery"/></para></summary>
	[TLDef(0x1EA2FDA7)]
	public sealed partial class UpdateBusinessBotCallbackQuery : Update
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Query ID</summary>
		public long query_id;
		/// <summary>ID of the user that pressed the button</summary>
		public long user_id;
		/// <summary><a href="https://corefork.telegram.org/api/business#connected-bots">Business connection ID</a></summary>
		public string connection_id;
		/// <summary>Message that contains the keyboard (also contains info about the chat where the message was sent).</summary>
		public MessageBase message;
		/// <summary>The message that <c>message</c> is replying to.</summary>
		[IfFlag(2)] public MessageBase reply_to_message;
		/// <summary>Global identifier, uniquely corresponding to the chat to which the message with the callback button was sent. Useful for high scores in games.</summary>
		public long chat_instance;
		/// <summary>Callback data</summary>
		[IfFlag(0)] public byte[] data;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="data"/> has a value</summary>
			has_data = 0x1,
			/// <summary>Field <see cref="reply_to_message"/> has a value</summary>
			has_reply_to_message = 0x4,
		}
	}
	/// <summary>The <a href="https://corefork.telegram.org/api/stars#revenue-statistics">Telegram Star balance of a channel/bot we own has changed »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateStarsRevenueStatus"/></para></summary>
	[TLDef(0xA584B019)]
	public sealed partial class UpdateStarsRevenueStatus : Update
	{
		/// <summary>Channel/bot</summary>
		public Peer peer;
		/// <summary>New Telegram Star balance.</summary>
		public StarsRevenueStatus status;
	}
	/// <summary>Bots only: a user has purchased a <a href="https://corefork.telegram.org/api/paid-media">paid media</a>.		<para>See <a href="https://corefork.telegram.org/constructor/updateBotPurchasedPaidMedia"/></para></summary>
	[TLDef(0x283BD312)]
	public sealed partial class UpdateBotPurchasedPaidMedia : Update
	{
		/// <summary>The user that bought the media</summary>
		public long user_id;
		/// <summary>Payload passed by the bot in <see cref="InputMediaPaidMedia"/>.<c>payload</c></summary>
		public string payload;
		/// <summary>New <strong>qts</strong> value, see <a href="https://corefork.telegram.org/api/updates">updates »</a> for more info.</summary>
		public int qts;

		public override (long, int, int) GetMBox() => (-1, qts, 1);
	}
	/// <summary>Contains the current <a href="https://corefork.telegram.org/api/reactions#paid-reactions">default paid reaction privacy, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/updatePaidReactionPrivacy"/></para></summary>
	[TLDef(0x8B725FCE)]
	public sealed partial class UpdatePaidReactionPrivacy : Update
	{
		/// <summary>Whether paid reaction privacy is enabled or disabled.</summary>
		public PaidReactionPrivacy private_;
	}

	/// <summary>Updates state.		<para>See <a href="https://corefork.telegram.org/constructor/updates.state"/></para></summary>
	[TLDef(0xA56C2A3E)]
	public sealed partial class Updates_State : IObject
	{
		/// <summary>Number of events occurred in a text box</summary>
		public int pts;
		/// <summary>Position in a sequence of updates in secret chats. For further details refer to article <a href="https://corefork.telegram.org/api/end-to-end">secret chats</a></summary>
		public int qts;
		/// <summary>Date of condition</summary>
		public DateTime date;
		/// <summary>Number of sent updates</summary>
		public int seq;
		/// <summary>Number of unread messages</summary>
		public int unread_count;
	}

	/// <summary>Occurred changes.		<para>See <a href="https://corefork.telegram.org/type/updates.Difference"/></para>		<para>Derived classes: <see cref="Updates_DifferenceEmpty"/>, <see cref="Updates_Difference"/>, <see cref="Updates_DifferenceSlice"/>, <see cref="Updates_DifferenceTooLong"/></para></summary>
	public abstract partial class Updates_DifferenceBase : IObject, IPeerResolver
	{
		/// <summary>List of new messages</summary>
		public virtual MessageBase[] NewMessages => default;
		/// <summary>List of new encrypted secret chat messages</summary>
		public virtual EncryptedMessageBase[] NewEncryptedMessages => default;
		/// <summary>List of updates</summary>
		public virtual Update[] OtherUpdates => default;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public abstract IPeerInfo UserOrChat(Peer peer);
	}
	/// <summary>No events.		<para>See <a href="https://corefork.telegram.org/constructor/updates.differenceEmpty"/></para></summary>
	[TLDef(0x5D75A138)]
	public sealed partial class Updates_DifferenceEmpty : Updates_DifferenceBase, IPeerResolver
	{
		/// <summary>Current date</summary>
		public DateTime date;
		/// <summary>Number of sent updates</summary>
		public int seq;

		public override MessageBase[] NewMessages => Array.Empty<MessageBase>();
		public override EncryptedMessageBase[] NewEncryptedMessages => Array.Empty<EncryptedMessageBase>();
		public override Update[] OtherUpdates => Array.Empty<Update>();
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	/// <summary>Full list of occurred events.		<para>See <a href="https://corefork.telegram.org/constructor/updates.difference"/></para></summary>
	[TLDef(0x00F49CA0)]
	public sealed partial class Updates_Difference : Updates_DifferenceBase, IPeerResolver
	{
		/// <summary>List of new messages</summary>
		public MessageBase[] new_messages;
		/// <summary>List of new encrypted secret chat messages</summary>
		public EncryptedMessageBase[] new_encrypted_messages;
		/// <summary>List of updates</summary>
		public Update[] other_updates;
		/// <summary>List of chats mentioned in events</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>List of users mentioned in events</summary>
		public Dictionary<long, User> users;
		/// <summary>Current state</summary>
		public Updates_State state;

		/// <summary>List of new messages</summary>
		public override MessageBase[] NewMessages => new_messages;
		/// <summary>List of new encrypted secret chat messages</summary>
		public override EncryptedMessageBase[] NewEncryptedMessages => new_encrypted_messages;
		/// <summary>List of updates</summary>
		public override Update[] OtherUpdates => other_updates;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Incomplete list of occurred events.		<para>See <a href="https://corefork.telegram.org/constructor/updates.differenceSlice"/></para></summary>
	[TLDef(0xA8FB1981)]
	public sealed partial class Updates_DifferenceSlice : Updates_DifferenceBase, IPeerResolver
	{
		/// <summary>List of new messages</summary>
		public MessageBase[] new_messages;
		/// <summary>New messages from the <a href="https://corefork.telegram.org/api/updates">encrypted event sequence</a></summary>
		public EncryptedMessageBase[] new_encrypted_messages;
		/// <summary>List of updates</summary>
		public Update[] other_updates;
		/// <summary>List of chats mentioned in events</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>List of users mentioned in events</summary>
		public Dictionary<long, User> users;
		/// <summary>Intermediary state</summary>
		public Updates_State intermediate_state;

		/// <summary>List of new messages</summary>
		public override MessageBase[] NewMessages => new_messages;
		/// <summary>New messages from the <a href="https://corefork.telegram.org/api/updates">encrypted event sequence</a></summary>
		public override EncryptedMessageBase[] NewEncryptedMessages => new_encrypted_messages;
		/// <summary>List of updates</summary>
		public override Update[] OtherUpdates => other_updates;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>The difference is <a href="https://corefork.telegram.org/api/updates#recovering-gaps">too long</a>, and the specified state must be used to refetch updates.		<para>See <a href="https://corefork.telegram.org/constructor/updates.differenceTooLong"/></para></summary>
	[TLDef(0x4AFE8F6D)]
	public sealed partial class Updates_DifferenceTooLong : Updates_DifferenceBase, IPeerResolver
	{
		/// <summary>The new state to use.</summary>
		public int pts;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}

	/// <summary>Object which is perceived by the client without a call on its part when an event occurs.		<para>See <a href="https://corefork.telegram.org/type/Updates"/></para>		<para>Derived classes: <see cref="UpdatesTooLong"/>, <see cref="UpdateShortMessage"/>, <see cref="UpdateShortChatMessage"/>, <see cref="UpdateShort"/>, <see cref="UpdatesCombined"/>, <see cref="Updates"/>, <see cref="UpdateShortSentMessage"/></para></summary>
	public abstract partial class UpdatesBase : IObject, IPeerResolver
	{
		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public virtual DateTime Date => default;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public abstract IPeerInfo UserOrChat(Peer peer);
	}
	/// <summary>Too many updates, it is necessary to execute <see cref="SchemaExtensions.Updates_GetDifference">Updates_GetDifference</see>.		<para>See <a href="https://corefork.telegram.org/constructor/updatesTooLong"/></para></summary>
	[TLDef(0xE317AF7E)]
	public sealed partial class UpdatesTooLong : UpdatesBase, IPeerResolver
	{
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	/// <summary>Info about a message sent to (received from) another user		<para>See <a href="https://corefork.telegram.org/constructor/updateShortMessage"/></para></summary>
	[TLDef(0x313BC7F8)]
	public sealed partial class UpdateShortMessage : UpdatesBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The message ID</summary>
		public int id;
		/// <summary>The ID of the sender (if <c>outgoing</c> will be the ID of the destination) of the message</summary>
		public long user_id;
		/// <summary>The message</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS count</a></summary>
		public int pts_count;
		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public DateTime date;
		/// <summary>Info about a forwarded message</summary>
		[IfFlag(2)] public MessageFwdHeader fwd_from;
		/// <summary>Info about the inline bot used to generate this message</summary>
		[IfFlag(11)] public long via_bot_id;
		/// <summary>Reply and <a href="https://corefork.telegram.org/api/threads">thread</a> information</summary>
		[IfFlag(3)] public MessageReplyHeaderBase reply_to;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Entities</a> for styled text</summary>
		[IfFlag(7)] public MessageEntity[] entities;
		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		[IfFlag(25)] public int ttl_period;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the message is outgoing</summary>
			out_ = 0x2,
			/// <summary>Field <see cref="fwd_from"/> has a value</summary>
			has_fwd_from = 0x4,
			/// <summary>Field <see cref="reply_to"/> has a value</summary>
			has_reply_to = 0x8,
			/// <summary>Whether we were mentioned in the message</summary>
			mentioned = 0x10,
			/// <summary>Whether there are some <strong>unread</strong> mentions in this message</summary>
			media_unread = 0x20,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x80,
			/// <summary>Field <see cref="via_bot_id"/> has a value</summary>
			has_via_bot_id = 0x800,
			/// <summary>If true, the message is a silent message, no notifications should be triggered</summary>
			silent = 0x2000,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x2000000,
		}

		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public override DateTime Date => date;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	/// <summary>Shortened constructor containing info on one new incoming text message from a chat		<para>See <a href="https://corefork.telegram.org/constructor/updateShortChatMessage"/></para></summary>
	[TLDef(0x4D6DEEA5)]
	public sealed partial class UpdateShortChatMessage : UpdatesBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the message</summary>
		public int id;
		/// <summary>ID of the sender of the message</summary>
		public long from_id;
		/// <summary>ID of the chat where the message was sent</summary>
		public long chat_id;
		/// <summary>Message</summary>
		public string message;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS count</a></summary>
		public int pts_count;
		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public DateTime date;
		/// <summary>Info about a forwarded message</summary>
		[IfFlag(2)] public MessageFwdHeader fwd_from;
		/// <summary>Info about the inline bot used to generate this message</summary>
		[IfFlag(11)] public long via_bot_id;
		/// <summary>Reply (thread) information</summary>
		[IfFlag(3)] public MessageReplyHeaderBase reply_to;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Entities</a> for styled text</summary>
		[IfFlag(7)] public MessageEntity[] entities;
		/// <summary>Time To Live of the message, once updateShortChatMessage.date+updateShortChatMessage.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		[IfFlag(25)] public int ttl_period;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the message is outgoing</summary>
			out_ = 0x2,
			/// <summary>Field <see cref="fwd_from"/> has a value</summary>
			has_fwd_from = 0x4,
			/// <summary>Field <see cref="reply_to"/> has a value</summary>
			has_reply_to = 0x8,
			/// <summary>Whether we were mentioned in this message</summary>
			mentioned = 0x10,
			/// <summary>Whether the message contains some <strong>unread</strong> mentions</summary>
			media_unread = 0x20,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x80,
			/// <summary>Field <see cref="via_bot_id"/> has a value</summary>
			has_via_bot_id = 0x800,
			/// <summary>If true, the message is a silent message, no notifications should be triggered</summary>
			silent = 0x2000,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x2000000,
		}

		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public override DateTime Date => date;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	/// <summary>Shortened constructor containing info on one update not requiring auxiliary data		<para>See <a href="https://corefork.telegram.org/constructor/updateShort"/></para></summary>
	[TLDef(0x78D4DEC1)]
	public sealed partial class UpdateShort : UpdatesBase, IPeerResolver
	{
		/// <summary>Update</summary>
		public Update update;
		/// <summary>Date of event</summary>
		public DateTime date;

		/// <summary>Date of event</summary>
		public override DateTime Date => date;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	/// <summary>Constructor for a group of updates.		<para>See <a href="https://corefork.telegram.org/constructor/updatesCombined"/></para></summary>
	[TLDef(0x725B04C3)]
	public sealed partial class UpdatesCombined : UpdatesBase, IPeerResolver
	{
		/// <summary>List of updates</summary>
		public Update[] updates;
		/// <summary>List of users mentioned in updates</summary>
		public Dictionary<long, User> users;
		/// <summary>List of chats mentioned in updates</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Current date</summary>
		public DateTime date;
		/// <summary>Value <strong>seq</strong> for the earliest update in a group</summary>
		public int seq_start;
		/// <summary>Value <strong>seq</strong> for the latest update in a group</summary>
		public int seq;

		/// <summary>Current date</summary>
		public override DateTime Date => date;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Full constructor of updates		<para>See <a href="https://corefork.telegram.org/constructor/updates"/></para></summary>
	[TLDef(0x74AE4240)]
	public sealed partial class Updates : UpdatesBase, IPeerResolver
	{
		/// <summary>List of updates</summary>
		public Update[] updates;
		/// <summary>List of users mentioned in updates</summary>
		public Dictionary<long, User> users;
		/// <summary>List of chats mentioned in updates</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Current date</summary>
		public DateTime date;
		/// <summary>Total number of sent updates</summary>
		public int seq;

		/// <summary>Current date</summary>
		public override DateTime Date => date;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Shortened constructor containing info on one outgoing message to a contact (the destination chat has to be extracted from the method call that returned this object).		<para>See <a href="https://corefork.telegram.org/constructor/updateShortSentMessage"/></para></summary>
	[TLDef(0x9015E101)]
	public sealed partial class UpdateShortSentMessage : UpdatesBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the sent message</summary>
		public int id;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS</a></summary>
		public int pts;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS count</a></summary>
		public int pts_count;
		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public DateTime date;
		/// <summary>Attached media</summary>
		[IfFlag(9)] public MessageMedia media;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Entities</a> for styled text</summary>
		[IfFlag(7)] public MessageEntity[] entities;
		/// <summary>Time To Live of the message, once message.date+message.ttl_period === time(), the message will be deleted on the server, and must be deleted locally as well.</summary>
		[IfFlag(25)] public int ttl_period;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the message is outgoing</summary>
			out_ = 0x2,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x80,
			/// <summary>Field <see cref="media"/> has a value</summary>
			has_media = 0x200,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x2000000,
		}

		/// <summary><a href="https://corefork.telegram.org/api/updates">date</a></summary>
		public override DateTime Date => date;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	
		/// <summary>Contains the difference (new messages) between our local channel state and the remote state		<para>See <a href="https://corefork.telegram.org/type/updates.ChannelDifference"/></para>		<para>Derived classes: <see cref="Updates_ChannelDifferenceEmpty"/>, <see cref="Updates_ChannelDifferenceTooLong"/>, <see cref="Updates_ChannelDifference"/></para></summary>
	public abstract partial class Updates_ChannelDifferenceBase : IObject, IPeerResolver
	{
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public abstract IPeerInfo UserOrChat(Peer peer);
	}
	/// <summary>There are no new updates		<para>See <a href="https://corefork.telegram.org/constructor/updates.channelDifferenceEmpty"/></para></summary>
	[TLDef(0x3E11AFFB)]
	public sealed partial class Updates_ChannelDifferenceEmpty : Updates_ChannelDifferenceBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The latest <a href="https://corefork.telegram.org/api/updates">PTS</a></summary>
		public int pts;
		/// <summary>Clients are supposed to refetch the channel difference after timeout seconds have elapsed, if the user is <a href="https://corefork.telegram.org/api/updates#subscribing-to-updates-of-channels-supergroups">currently viewing the chat, see here »</a> for more info.</summary>
		[IfFlag(1)] public int timeout;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether there are more updates that must be fetched (always false)</summary>
			final = 0x1,
			/// <summary>Field <see cref="timeout"/> has a value</summary>
			has_timeout = 0x2,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}
	/// <summary>The provided <c>pts + limit &lt; remote pts</c>. Simply, there are too many updates to be fetched (more than <c>limit</c>), the client has to resolve the update gap in one of the following ways (assuming the existence of a persistent database to locally store messages):		<para>See <a href="https://corefork.telegram.org/constructor/updates.channelDifferenceTooLong"/></para></summary>
	[TLDef(0xA4BCC6FE)]
	public sealed partial class Updates_ChannelDifferenceTooLong : Updates_ChannelDifferenceBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Clients are supposed to refetch the channel difference after timeout seconds have elapsed</summary>
		[IfFlag(1)] public int timeout;
		/// <summary>Dialog containing the latest <a href="https://corefork.telegram.org/api/updates">PTS</a> that can be used to reset the channel state</summary>
		public DialogBase dialog;
		/// <summary>The latest messages</summary>
		public MessageBase[] messages;
		/// <summary>Chats from messages</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users from messages</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether there are more updates that must be fetched (always false)</summary>
			final = 0x1,
			/// <summary>Field <see cref="timeout"/> has a value</summary>
			has_timeout = 0x2,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>The new updates		<para>See <a href="https://corefork.telegram.org/constructor/updates.channelDifference"/></para></summary>
	[TLDef(0x2064674E)]
	public sealed partial class Updates_ChannelDifference : Updates_ChannelDifferenceBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The <a href="https://corefork.telegram.org/api/updates">PTS</a> from which to start getting updates the next time</summary>
		public int pts;
		/// <summary>Clients are supposed to refetch the channel difference after timeout seconds have elapsed, if the user is <a href="https://corefork.telegram.org/api/updates#subscribing-to-updates-of-channels-supergroups">currently viewing the chat, see here »</a> for more info.</summary>
		[IfFlag(1)] public int timeout;
		/// <summary>New messages</summary>
		public MessageBase[] new_messages;
		/// <summary>Other updates</summary>
		public Update[] other_updates;
		/// <summary>Chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether there are more updates to be fetched using getDifference, starting from the provided <c>pts</c></summary>
			final = 0x1,
			/// <summary>Field <see cref="timeout"/> has a value</summary>
			has_timeout = 0x2,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
}