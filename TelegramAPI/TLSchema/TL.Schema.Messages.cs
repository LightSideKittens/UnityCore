using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Object contains a list of chats with messages and auxiliary data.		<para>See <a href="https://corefork.telegram.org/type/messages.Dialogs"/></para>		<para>Derived classes: <see cref="Messages_Dialogs"/>, <see cref="Messages_DialogsSlice"/>, <see cref="Messages_DialogsNotModified"/></para></summary>
    public abstract partial class Messages_DialogsBase : IObject, IPeerResolver
    {
        /// <summary>List of chats</summary>
        public virtual DialogBase[] Dialogs => default;
        /// <summary>List of last messages from each chat</summary>
        public virtual MessageBase[] Messages => default;
        /// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
        public abstract IPeerInfo UserOrChat(Peer peer);
    }
    
    /// <summary>Full list of chats with messages and auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/messages.dialogs"/></para></summary>
	[TLDef(0x15BA6C40)]
	public partial class Messages_Dialogs : Messages_DialogsBase, IPeerResolver
	{
		/// <summary>List of chats</summary>
		public DialogBase[] dialogs;
		/// <summary>List of last messages from each chat</summary>
		public MessageBase[] messages;
		/// <summary>List of groups mentioned in the chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>List of users mentioned in messages and groups</summary>
		public Dictionary<long, User> users;

		/// <summary>List of chats</summary>
		public override DialogBase[] Dialogs => dialogs;
		/// <summary>List of last messages from each chat</summary>
		public override MessageBase[] Messages => messages;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Incomplete list of dialogs with messages and auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/messages.dialogsSlice"/></para></summary>
	[TLDef(0x71E094F3)]
	public sealed partial class Messages_DialogsSlice : Messages_Dialogs, IPeerResolver
	{
		/// <summary>Total number of dialogs</summary>
		public int count;
	}
	/// <summary>Dialogs haven't changed		<para>See <a href="https://corefork.telegram.org/constructor/messages.dialogsNotModified"/></para></summary>
	[TLDef(0xF0E3E596)]
	public sealed partial class Messages_DialogsNotModified : Messages_DialogsBase, IPeerResolver
	{
		/// <summary>Number of dialogs found server-side by the query</summary>
		public int count;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}

	/// <summary>Object contains information on list of messages with auxiliary data.		<para>See <a href="https://corefork.telegram.org/type/messages.Messages"/></para>		<para>Derived classes: <see cref="Messages_Messages"/>, <see cref="Messages_MessagesSlice"/>, <see cref="Messages_ChannelMessages"/>, <see cref="Messages_MessagesNotModified"/></para></summary>
	public abstract partial class Messages_MessagesBase : IObject, IPeerResolver
	{
		/// <summary>List of messages</summary>
		public virtual MessageBase[] Messages => default;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public abstract IPeerInfo UserOrChat(Peer peer);
	}
	/// <summary>Full list of messages with auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/messages.messages"/></para></summary>
	[TLDef(0x8C718E87)]
	public partial class Messages_Messages : Messages_MessagesBase, IPeerResolver
	{
		/// <summary>List of messages</summary>
		public MessageBase[] messages;
		/// <summary>List of chats mentioned in dialogs</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>List of users mentioned in messages and chats</summary>
		public Dictionary<long, User> users;

		/// <summary>List of messages</summary>
		public override MessageBase[] Messages => messages;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Incomplete list of messages and auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/messages.messagesSlice"/></para></summary>
	[TLDef(0x3A54685E)]
	public sealed partial class Messages_MessagesSlice : Messages_Messages, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of messages in the list</summary>
		public int count;
		/// <summary>Rate to use in the <c>offset_rate</c> parameter in the next call to <see cref="SchemaExtensions.Messages_SearchGlobal">Messages_SearchGlobal</see></summary>
		[IfFlag(0)] public int next_rate;
		/// <summary>Indicates the absolute position of <c>messages[0]</c> within the total result set with count <c>count</c>. <br/>This is useful, for example, if the result was fetched using <c>offset_id</c>, and we need to display a <c>progress/total</c> counter (like <c>photo 134 of 200</c>, for all media in a chat, we could simply use <c>photo ${offset_id_offset} of ${count}</c>.</summary>
		[IfFlag(2)] public int offset_id_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_rate"/> has a value</summary>
			has_next_rate = 0x1,
			/// <summary>If set, indicates that the results may be inexact</summary>
			inexact = 0x2,
			/// <summary>Field <see cref="offset_id_offset"/> has a value</summary>
			has_offset_id_offset = 0x4,
		}
	}
	/// <summary>Channel messages		<para>See <a href="https://corefork.telegram.org/constructor/messages.channelMessages"/></para></summary>
	[TLDef(0xC776BA4E)]
	public sealed partial class Messages_ChannelMessages : Messages_MessagesBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;
		/// <summary>Total number of results were found server-side (may not be all included here)</summary>
		public int count;
		/// <summary>Indicates the absolute position of <c>messages[0]</c> within the total result set with count <c>count</c>. <br/>This is useful, for example, if the result was fetched using <c>offset_id</c>, and we need to display a <c>progress/total</c> counter (like <c>photo 134 of 200</c>, for all media in a chat, we could simply use <c>photo ${offset_id_offset} of ${count}</c>.</summary>
		[IfFlag(2)] public int offset_id_offset;
		/// <summary>Found messages</summary>
		public MessageBase[] messages;
		/// <summary><a href="https://corefork.telegram.org/api/forum#forum-topics">Forum topic</a> information</summary>
		public ForumTopicBase[] topics;
		/// <summary>Chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, returned results may be inexact</summary>
			inexact = 0x2,
			/// <summary>Field <see cref="offset_id_offset"/> has a value</summary>
			has_offset_id_offset = 0x4,
		}

		/// <summary>Found messages</summary>
		public override MessageBase[] Messages => messages;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>No new messages matching the query were found		<para>See <a href="https://corefork.telegram.org/constructor/messages.messagesNotModified"/></para></summary>
	[TLDef(0x74535F21)]
	public sealed partial class Messages_MessagesNotModified : Messages_MessagesBase, IPeerResolver
	{
		/// <summary>Number of results found server-side by the given query</summary>
		public int count;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public override IPeerInfo UserOrChat(Peer peer) => null;
	}

	/// <summary>List of chats with auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/messages.chats"/></para></summary>
	[TLDef(0x64FF9FD5)]
	public partial class Messages_Chats : IObject
	{
		/// <summary>List of chats</summary>
		public Dictionary<long, ChatBase> chats;
	}
	/// <summary>Partial list of chats, more would have to be fetched with <a href="https://corefork.telegram.org/api/offsets">pagination</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.chatsSlice"/></para></summary>
	[TLDef(0x9CD81144)]
	public sealed partial class Messages_ChatsSlice : Messages_Chats
	{
		/// <summary>Total number of results that were found server-side (not all are included in <c>chats</c>)</summary>
		public int count;
	}

	/// <summary>Full info about a <a href="https://corefork.telegram.org/api/channel#channels">channel</a>, <a href="https://corefork.telegram.org/api/channel#supergroups">supergroup</a>, <a href="https://corefork.telegram.org/api/channel#gigagroups">gigagroup</a> or <a href="https://corefork.telegram.org/api/channel#basic-groups">basic group</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.chatFull"/></para></summary>
	[TLDef(0xE5D7D19C)]
	public sealed partial class Messages_ChatFull : IObject, IPeerResolver
	{
		/// <summary>Full info</summary>
		public ChatFullBase full_chat;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Affected part of communication history with the user or in a chat.		<para>See <a href="https://corefork.telegram.org/constructor/messages.affectedHistory"/></para></summary>
	[TLDef(0xB45C69D1, inheritBefore = true)]
	public partial class Messages_AffectedHistory : Messages_AffectedMessages
	{
		/// <summary>If a parameter contains positive value, it is necessary to repeat the method call using the given value; during the proceeding of all the history the value itself shall gradually decrease</summary>
		public int offset;
	}

	/// <summary>Object describes message filter.		<para>See <a href="https://corefork.telegram.org/type/MessagesFilter"/></para>		<para>Derived classes: <see cref="InputMessagesFilterPhotos"/>, <see cref="InputMessagesFilterVideo"/>, <see cref="InputMessagesFilterPhotoVideo"/>, <see cref="InputMessagesFilterDocument"/>, <see cref="InputMessagesFilterUrl"/>, <see cref="InputMessagesFilterGif"/>, <see cref="InputMessagesFilterVoice"/>, <see cref="InputMessagesFilterMusic"/>, <see cref="InputMessagesFilterChatPhotos"/>, <see cref="InputMessagesFilterPhoneCalls"/>, <see cref="InputMessagesFilterRoundVoice"/>, <see cref="InputMessagesFilterRoundVideo"/>, <see cref="InputMessagesFilterMyMentions"/>, <see cref="InputMessagesFilterGeo"/>, <see cref="InputMessagesFilterContacts"/>, <see cref="InputMessagesFilterPinned"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputMessagesFilterEmpty">inputMessagesFilterEmpty</a></remarks>
	public abstract partial class MessagesFilter : IObject { }
	/// <summary>Filter for messages containing photos.		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterPhotos"/></para></summary>
	[TLDef(0x9609A51C)]
	public sealed partial class InputMessagesFilterPhotos : MessagesFilter { }
	/// <summary>Filter for messages containing videos.		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterVideo"/></para></summary>
	[TLDef(0x9FC00E65)]
	public sealed partial class InputMessagesFilterVideo : MessagesFilter { }
	/// <summary>Filter for messages containing photos or videos.		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterPhotoVideo"/></para></summary>
	[TLDef(0x56E9F0E4)]
	public sealed partial class InputMessagesFilterPhotoVideo : MessagesFilter { }
	/// <summary>Filter for messages containing documents.		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterDocument"/></para></summary>
	[TLDef(0x9EDDF188)]
	public sealed partial class InputMessagesFilterDocument : MessagesFilter { }
	/// <summary>Return only messages containing URLs		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterUrl"/></para></summary>
	[TLDef(0x7EF0DD87)]
	public sealed partial class InputMessagesFilterUrl : MessagesFilter { }
	/// <summary>Return only messages containing gifs		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterGif"/></para></summary>
	[TLDef(0xFFC86587)]
	public sealed partial class InputMessagesFilterGif : MessagesFilter { }
	/// <summary>Return only messages containing voice notes		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterVoice"/></para></summary>
	[TLDef(0x50F5C392)]
	public sealed partial class InputMessagesFilterVoice : MessagesFilter { }
	/// <summary>Return only messages containing audio files		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterMusic"/></para></summary>
	[TLDef(0x3751B49E)]
	public sealed partial class InputMessagesFilterMusic : MessagesFilter { }
	/// <summary>Return only chat photo changes		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterChatPhotos"/></para></summary>
	[TLDef(0x3A20ECB8)]
	public sealed partial class InputMessagesFilterChatPhotos : MessagesFilter { }
	/// <summary>Return only phone calls		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterPhoneCalls"/></para></summary>
	[TLDef(0x80C99768)]
	public sealed partial class InputMessagesFilterPhoneCalls : MessagesFilter
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;

		[Flags] public enum Flags : uint
		{
			/// <summary>Return only missed phone calls</summary>
			missed = 0x1,
		}
	}
	/// <summary>Return only round videos and voice notes		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterRoundVoice"/></para></summary>
	[TLDef(0x7A7C17A4)]
	public sealed partial class InputMessagesFilterRoundVoice : MessagesFilter { }
	/// <summary>Return only round videos		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterRoundVideo"/></para></summary>
	[TLDef(0xB549DA53)]
	public sealed partial class InputMessagesFilterRoundVideo : MessagesFilter { }
	/// <summary>Return only messages where the current user was <a href="https://corefork.telegram.org/api/mentions">mentioned</a>.		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterMyMentions"/></para></summary>
	[TLDef(0xC1F8E69A)]
	public sealed partial class InputMessagesFilterMyMentions : MessagesFilter { }
	/// <summary>Return only messages containing geolocations		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterGeo"/></para></summary>
	[TLDef(0xE7026D0D)]
	public sealed partial class InputMessagesFilterGeo : MessagesFilter { }
	/// <summary>Return only messages containing contacts		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterContacts"/></para></summary>
	[TLDef(0xE062DB83)]
	public sealed partial class InputMessagesFilterContacts : MessagesFilter { }
	/// <summary>Fetch only pinned messages		<para>See <a href="https://corefork.telegram.org/constructor/inputMessagesFilterPinned"/></para></summary>
	[TLDef(0x1BB00451)]
	public sealed partial class InputMessagesFilterPinned : MessagesFilter { }
	
	/// <summary>Result of a query to an inline bot		<para>See <a href="https://corefork.telegram.org/constructor/messages.botResults"/></para></summary>
    [TLDef(0xE021F2F6)]
    public sealed partial class Messages_BotResults : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary>Query ID</summary>
    	public long query_id;
    	/// <summary>The next offset to use when navigating through results</summary>
    	[IfFlag(1)] public string next_offset;
    	/// <summary>Shown as a button on top of the remaining inline result list; if clicked, redirects the user to a private chat with the bot with the specified start parameter.</summary>
    	[IfFlag(2)] public InlineBotSwitchPM switch_pm;
    	/// <summary>Shown as a button on top of the remaining inline result list; if clicked, opens the specified <a href="https://corefork.telegram.org/api/bots/webapps#inline-mode-mini-apps">inline mode mini app</a>.</summary>
    	[IfFlag(3)] public InlineBotWebView switch_webview;
    	/// <summary>The results</summary>
    	public BotInlineResultBase[] results;
    	/// <summary>Caching validity of the results</summary>
    	public int cache_time;
    	/// <summary>Users mentioned in the results</summary>
    	public Dictionary<long, User> users;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Whether the result is a picture gallery</summary>
    		gallery = 0x1,
    		/// <summary>Field <see cref="next_offset"/> has a value</summary>
    		has_next_offset = 0x2,
    		/// <summary>Field <see cref="switch_pm"/> has a value</summary>
    		has_switch_pm = 0x4,
    		/// <summary>Field <see cref="switch_webview"/> has a value</summary>
    		has_switch_webview = 0x8,
    	}
    }
	
	/// <summary>Callback answer sent by the bot in response to a button press		<para>See <a href="https://corefork.telegram.org/constructor/messages.botCallbackAnswer"/></para></summary>
    [TLDef(0x36585EA4)]
    public sealed partial class Messages_BotCallbackAnswer : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary>Alert to show</summary>
    	[IfFlag(0)] public string message;
    	/// <summary>URL to open</summary>
    	[IfFlag(2)] public string url;
    	/// <summary>For how long should this answer be cached</summary>
    	public int cache_time;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Field <see cref="message"/> has a value</summary>
    		has_message = 0x1,
    		/// <summary>Whether an alert should be shown to the user instead of a toast notification</summary>
    		alert = 0x2,
    		/// <summary>Field <see cref="url"/> has a value</summary>
    		has_url_field = 0x4,
    		/// <summary>Whether an URL is present</summary>
    		has_url = 0x8,
    		/// <summary>Whether to show games in WebView or in native UI.</summary>
    		native_ui = 0x10,
    	}
    }

    /// <summary>Message edit data for media		<para>See <a href="https://corefork.telegram.org/constructor/messages.messageEditData"/></para></summary>
    [TLDef(0x26B5DDE6)]
    public sealed partial class Messages_MessageEditData : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Media caption, if the specified media's caption can be edited</summary>
    		caption = 0x1,
    	}
    }
    
    /// <summary>Dialog info of multiple peers		<para>See <a href="https://corefork.telegram.org/constructor/messages.peerDialogs"/></para></summary>
    [TLDef(0x3371C354)]
    public sealed partial class Messages_PeerDialogs : IObject, IPeerResolver
    {
    	/// <summary>Dialog info</summary>
    	public DialogBase[] dialogs;
    	/// <summary>Messages mentioned in dialog info</summary>
    	public MessageBase[] messages;
    	/// <summary>Chats</summary>
    	public Dictionary<long, ChatBase> chats;
    	/// <summary>Users</summary>
    	public Dictionary<long, User> users;
    	/// <summary>Current <a href="https://corefork.telegram.org/api/updates">update state of dialog</a></summary>
    	public Updates_State state;
    	/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
    	public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
    }
    
    /// <summary>Featured stickers		<para>See <a href="https://corefork.telegram.org/type/messages.FeaturedStickers"/></para>		<para>Derived classes: <see cref="Messages_FeaturedStickersNotModified"/>, <see cref="Messages_FeaturedStickers"/></para></summary>
    public abstract partial class Messages_FeaturedStickersBase : IObject { }
    /// <summary>Featured stickers haven't changed		<para>See <a href="https://corefork.telegram.org/constructor/messages.featuredStickersNotModified"/></para></summary>
    [TLDef(0xC6DC0C66)]
    public sealed partial class Messages_FeaturedStickersNotModified : Messages_FeaturedStickersBase
    {
    	/// <summary>Total number of featured stickers</summary>
    	public int count;
    }
    /// <summary>Featured stickersets		<para>See <a href="https://corefork.telegram.org/constructor/messages.featuredStickers"/></para></summary>
    [TLDef(0xBE382906)]
    public sealed partial class Messages_FeaturedStickers : Messages_FeaturedStickersBase
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public long hash;
    	/// <summary>Total number of featured stickers</summary>
    	public int count;
    	/// <summary>Featured stickersets</summary>
    	public StickerSetCoveredBase[] sets;
    	/// <summary>IDs of new featured stickersets</summary>
    	public long[] unread;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Whether this is a premium stickerset</summary>
    		premium = 0x1,
    	}
    }

    /// <summary>Recently used stickers		<para>See <a href="https://corefork.telegram.org/constructor/messages.recentStickers"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.recentStickersNotModified">messages.recentStickersNotModified</a></remarks>
    [TLDef(0x88D37C56)]
    public sealed partial class Messages_RecentStickers : IObject
    {
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public long hash;
    	/// <summary>Emojis associated to stickers</summary>
    	public StickerPack[] packs;
    	/// <summary>Recent stickers</summary>
    	public DocumentBase[] stickers;
    	/// <summary>When was each sticker last used</summary>
    	public int[] dates;
    }

    /// <summary>Archived stickersets		<para>See <a href="https://corefork.telegram.org/constructor/messages.archivedStickers"/></para></summary>
    [TLDef(0x4FCBA9C8)]
    public sealed partial class Messages_ArchivedStickers : IObject
    {
    	/// <summary>Number of archived stickers</summary>
    	public int count;
    	/// <summary>Archived stickersets</summary>
    	public StickerSetCoveredBase[] sets;
    }

    /// <summary>Result of stickerset installation process		<para>See <a href="https://corefork.telegram.org/type/messages.StickerSetInstallResult"/></para>		<para>Derived classes: <see cref="Messages_StickerSetInstallResultSuccess"/>, <see cref="Messages_StickerSetInstallResultArchive"/></para></summary>
    public abstract partial class Messages_StickerSetInstallResult : IObject { }
    /// <summary>The stickerset was installed successfully		<para>See <a href="https://corefork.telegram.org/constructor/messages.stickerSetInstallResultSuccess"/></para></summary>
    [TLDef(0x38641628)]
    public sealed partial class Messages_StickerSetInstallResultSuccess : Messages_StickerSetInstallResult { }
    /// <summary>The stickerset was installed, but since there are too many stickersets some were archived		<para>See <a href="https://corefork.telegram.org/constructor/messages.stickerSetInstallResultArchive"/></para></summary>
    [TLDef(0x35E410A8)]
    public sealed partial class Messages_StickerSetInstallResultArchive : Messages_StickerSetInstallResult
    {
    	/// <summary>Archived stickersets</summary>
    	public StickerSetCoveredBase[] sets;
    }
    
    /// <summary>Highscores in a game		<para>See <a href="https://corefork.telegram.org/constructor/messages.highScores"/></para></summary>
    [TLDef(0x9A3BFD99)]
    public sealed partial class Messages_HighScores : IObject
    {
    	/// <summary>Highscores</summary>
    	public HighScore[] scores;
    	/// <summary>Users, associated to the highscores</summary>
    	public Dictionary<long, User> users;
    }
    
    /// <summary>Favorited stickers		<para>See <a href="https://corefork.telegram.org/constructor/messages.favedStickers"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.favedStickersNotModified">messages.favedStickersNotModified</a></remarks>
    [TLDef(0x2CB51097)]
    public sealed partial class Messages_FavedStickers : IObject
    {
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public long hash;
    	/// <summary>Emojis associated to stickers</summary>
    	public StickerPack[] packs;
    	/// <summary>Favorited stickers</summary>
    	public DocumentBase[] stickers;
    }
    
    /// <summary>Found stickersets		<para>See <a href="https://corefork.telegram.org/constructor/messages.foundStickerSets"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.foundStickerSetsNotModified">messages.foundStickerSetsNotModified</a></remarks>
    [TLDef(0x8AF09DD2)]
    public sealed partial class Messages_FoundStickerSets : IObject
    {
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public long hash;
    	/// <summary>Found stickersets</summary>
    	public StickerSetCoveredBase[] sets;
    }
    
    /// <summary>Indicates how many results would be found by a <see cref="SchemaExtensions.Messages_Search">Messages_Search</see> call with the same parameters		<para>See <a href="https://corefork.telegram.org/constructor/messages.searchCounter"/></para></summary>
    [TLDef(0xE844EBFF)]
    public sealed partial class Messages_SearchCounter : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary>Provided message filter</summary>
    	public MessagesFilter filter;
    	/// <summary>Number of results that were found server-side</summary>
    	public int count;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>If set, the results may be inexact</summary>
    		inexact = 0x2,
    	}
    }
    
	/// <summary>Inactive chat list		<para>See <a href="https://corefork.telegram.org/constructor/messages.inactiveChats"/></para></summary>
	[TLDef(0xA927FEC5)]
	public sealed partial class Messages_InactiveChats : IObject, IPeerResolver
	{
		/// <summary>When was the chat last active</summary>
		public int[] dates;
		/// <summary>Chat list</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in the chat list</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>How users voted in a poll		<para>See <a href="https://corefork.telegram.org/constructor/messages.votesList"/></para></summary>
	[TLDef(0x4899484E)]
	public sealed partial class Messages_VotesList : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of votes for all options (or only for the chosen <c>option</c>, if provided to <see cref="SchemaExtensions.Messages_GetPollVotes">Messages_GetPollVotes</see>)</summary>
		public int count;
		/// <summary>Vote info for each user</summary>
		public MessagePeerVoteBase[] votes;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Info about users that voted in the poll</summary>
		public Dictionary<long, User> users;
		/// <summary>Offset to use with the next <see cref="SchemaExtensions.Messages_GetPollVotes">Messages_GetPollVotes</see> request, empty string if no more results are available.</summary>
		[IfFlag(0)] public string next_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>View, forward counter + info about replies of a specific message		<para>See <a href="https://corefork.telegram.org/constructor/messageViews"/></para></summary>
	[TLDef(0x455B853D)]
	public sealed partial class MessageViews : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>View count of message</summary>
		[IfFlag(0)] public int views;
		/// <summary>Forward count of message</summary>
		[IfFlag(1)] public int forwards;
		/// <summary>Reply and <a href="https://corefork.telegram.org/api/threads">thread</a> information of message</summary>
		[IfFlag(2)] public MessageReplies replies;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="views"/> has a value</summary>
			has_views = 0x1,
			/// <summary>Field <see cref="forwards"/> has a value</summary>
			has_forwards = 0x2,
			/// <summary>Field <see cref="replies"/> has a value</summary>
			has_replies = 0x4,
		}
	}

	/// <summary>View, forward counter + info about replies		<para>See <a href="https://corefork.telegram.org/constructor/messages.messageViews"/></para></summary>
	[TLDef(0xB6C4F543)]
	public sealed partial class Messages_MessageViews : IObject, IPeerResolver
	{
		/// <summary>View, forward counter + info about replies</summary>
		public MessageViews[] views;
		/// <summary>Chats mentioned in constructor</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in constructor</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Information about a <a href="https://corefork.telegram.org/api/threads">message thread</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.discussionMessage"/></para></summary>
	[TLDef(0xA6341782)]
	public sealed partial class Messages_DiscussionMessage : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The messages from which the thread starts. The messages are returned in reverse chronological order (i.e., in order of decreasing message ID).</summary>
		public MessageBase[] messages;
		/// <summary>Message ID of latest reply in this <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		[IfFlag(0)] public int max_id;
		/// <summary>Message ID of latest read incoming message in this <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		[IfFlag(1)] public int read_inbox_max_id;
		/// <summary>Message ID of latest read outgoing message in this <a href="https://corefork.telegram.org/api/threads">thread</a></summary>
		[IfFlag(2)] public int read_outbox_max_id;
		/// <summary>Number of unread messages</summary>
		public int unread_count;
		/// <summary>Chats mentioned in constructor</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in constructor</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="max_id"/> has a value</summary>
			has_max_id = 0x1,
			/// <summary>Field <see cref="read_inbox_max_id"/> has a value</summary>
			has_read_inbox_max_id = 0x2,
			/// <summary>Field <see cref="read_outbox_max_id"/> has a value</summary>
			has_read_outbox_max_id = 0x4,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Reply information		<para>See <a href="https://corefork.telegram.org/type/MessageReplyHeader"/></para>		<para>Derived classes: <see cref="MessageReplyHeader"/>, <see cref="MessageReplyStoryHeader"/></para></summary>
	public abstract partial class MessageReplyHeaderBase : IObject { }
	/// <summary>Message replies and <a href="https://corefork.telegram.org/api/threads">thread</a> information		<para>See <a href="https://corefork.telegram.org/constructor/messageReplyHeader"/></para></summary>
	[TLDef(0xAFBC09DB)]
	public sealed partial class MessageReplyHeader : MessageReplyHeaderBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of message to which this message is replying</summary>
		[IfFlag(4)] public int reply_to_msg_id;
		/// <summary>For replies sent in <a href="https://corefork.telegram.org/api/threads">channel discussion threads</a> of which the current user is not a member, the discussion group ID</summary>
		[IfFlag(0)] public Peer reply_to_peer_id;
		/// <summary>When replying to a message sent by a certain peer to another chat, contains info about the peer that originally sent the message to that other chat.</summary>
		[IfFlag(5)] public MessageFwdHeader reply_from;
		/// <summary>When replying to a media sent by a certain peer to another chat, contains the media of the replied-to message.</summary>
		[IfFlag(8)] public MessageMedia reply_media;
		/// <summary>ID of the message that started this <a href="https://corefork.telegram.org/api/threads">message thread</a></summary>
		[IfFlag(1)] public int reply_to_top_id;
		/// <summary>Used to quote-reply to only a certain section (specified here) of the original message.</summary>
		[IfFlag(6)] public string quote_text;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a> from the <c>quote_text</c> field.</summary>
		[IfFlag(7)] public MessageEntity[] quote_entities;
		/// <summary>Offset of the message <c>quote_text</c> within the original message (in <a href="https://corefork.telegram.org/api/entities#entity-length">UTF-16 code units</a>).</summary>
		[IfFlag(10)] public int quote_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="reply_to_peer_id"/> has a value</summary>
			has_reply_to_peer_id = 0x1,
			/// <summary>Field <see cref="reply_to_top_id"/> has a value</summary>
			has_reply_to_top_id = 0x2,
			/// <summary>This is a reply to a scheduled message.</summary>
			reply_to_scheduled = 0x4,
			/// <summary>Whether this message was sent in a <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topic</a> (except for the General topic).</summary>
			forum_topic = 0x8,
			/// <summary>Field <see cref="reply_to_msg_id"/> has a value</summary>
			has_reply_to_msg_id = 0x10,
			/// <summary>Field <see cref="reply_from"/> has a value</summary>
			has_reply_from = 0x20,
			/// <summary>Field <see cref="quote_text"/> has a value</summary>
			has_quote_text = 0x40,
			/// <summary>Field <see cref="quote_entities"/> has a value</summary>
			has_quote_entities = 0x80,
			/// <summary>Field <see cref="reply_media"/> has a value</summary>
			has_reply_media = 0x100,
			/// <summary>Whether this message is quoting a part of another message.</summary>
			quote = 0x200,
			/// <summary>Field <see cref="quote_offset"/> has a value</summary>
			has_quote_offset = 0x400,
		}
	}
	/// <summary>Represents a reply to a <a href="https://corefork.telegram.org/api/stories">story</a>		<para>See <a href="https://corefork.telegram.org/constructor/messageReplyStoryHeader"/></para></summary>
	[TLDef(0x0E5AF939)]
	public sealed partial class MessageReplyStoryHeader : MessageReplyHeaderBase
	{
		/// <summary>Sender of the story.</summary>
		public Peer peer;
		/// <summary>Story ID</summary>
		public int story_id;
	}

	/// <summary>Info about <a href="https://corefork.telegram.org/api/threads">the comment section of a channel post, or a simple message thread</a>		<para>See <a href="https://corefork.telegram.org/constructor/messageReplies"/></para></summary>
	[TLDef(0x83D60FC2)]
	public sealed partial class MessageReplies : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Contains the total number of replies in this thread or comment section.</summary>
		public int replies;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS</a> of the message that started this thread.</summary>
		public int replies_pts;
		/// <summary>For channel post comments, contains information about the last few comment posters for a specific thread, to show a small list of commenter profile pictures in client previews.</summary>
		[IfFlag(1)] public Peer[] recent_repliers;
		/// <summary>For channel post comments, contains the ID of the associated <a href="https://corefork.telegram.org/api/discussion">discussion supergroup</a></summary>
		[IfFlag(0)] public long channel_id;
		/// <summary>ID of the latest message in this thread or comment section.</summary>
		[IfFlag(2)] public int max_id;
		/// <summary>Contains the ID of the latest read message in this thread or comment section.</summary>
		[IfFlag(3)] public int read_max_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this constructor contains information about the <a href="https://corefork.telegram.org/api/threads">comment section of a channel post, or a simple message thread</a></summary>
			comments = 0x1,
			/// <summary>Field <see cref="recent_repliers"/> has a value</summary>
			has_recent_repliers = 0x2,
			/// <summary>Field <see cref="max_id"/> has a value</summary>
			has_max_id = 0x4,
			/// <summary>Field <see cref="read_max_id"/> has a value</summary>
			has_read_max_id = 0x8,
		}
	}
	
	/// <summary>ID of a specific <a href="https://corefork.telegram.org/api/import">chat import session, click here for more info »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.historyImport"/></para></summary>
	[TLDef(0x1662AF0B)]
	public sealed partial class Messages_HistoryImport : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/import">History import ID</a></summary>
		public long id;
	}

	/// <summary>Contains information about a chat export file <a href="https://corefork.telegram.org/api/import">generated by a foreign chat app, click here for more info</a>.<br/>If neither the <c>pm</c> or <c>group</c> flags are set, the specified chat export was generated from a chat of unknown type.		<para>See <a href="https://corefork.telegram.org/constructor/messages.historyImportParsed"/></para></summary>
	[TLDef(0x5E0FB7B9)]
	public sealed partial class Messages_HistoryImportParsed : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Title of the chat.</summary>
		[IfFlag(2)] public string title;

		[Flags] public enum Flags : uint
		{
			/// <summary>The chat export file was generated from a private chat.</summary>
			pm = 0x1,
			/// <summary>The chat export file was generated from a group chat.</summary>
			group = 0x2,
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x4,
		}
	}

	/// <summary>Messages found and affected by changes		<para>See <a href="https://corefork.telegram.org/constructor/messages.affectedFoundMessages"/></para></summary>
	[TLDef(0xEF8D3E6C, inheritBefore = true)]
	public sealed partial class Messages_AffectedFoundMessages : Messages_AffectedHistory
	{
		/// <summary>Affected message IDs</summary>
		public int[] messages;
	}
	
	/// <summary>Info about chat invites exported by a certain admin.		<para>See <a href="https://corefork.telegram.org/constructor/messages.exportedChatInvites"/></para></summary>
	[TLDef(0xBDC62DCC)]
	public sealed partial class Messages_ExportedChatInvites : IObject
	{
		/// <summary>Number of invites exported by the admin</summary>
		public int count;
		/// <summary>Exported invites</summary>
		public ExportedChatInvite[] invites;
		/// <summary>Info about the admin</summary>
		public Dictionary<long, User> users;
	}

	/// <summary>Contains info about a chat invite, and eventually a pointer to the newest chat invite.		<para>See <a href="https://corefork.telegram.org/type/messages.ExportedChatInvite"/></para>		<para>Derived classes: <see cref="Messages_ExportedChatInvite"/>, <see cref="Messages_ExportedChatInviteReplaced"/></para></summary>
	public abstract partial class Messages_ExportedChatInviteBase : IObject
	{
		/// <summary>Info about the chat invite</summary>
		public virtual ExportedChatInvite Invite => default;
		/// <summary>Mentioned users</summary>
		public virtual Dictionary<long, User> Users => default;
	}
	/// <summary>Info about a chat invite		<para>See <a href="https://corefork.telegram.org/constructor/messages.exportedChatInvite"/></para></summary>
	[TLDef(0x1871BE50)]
	public sealed partial class Messages_ExportedChatInvite : Messages_ExportedChatInviteBase
	{
		/// <summary>Info about the chat invite</summary>
		public ExportedChatInvite invite;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		/// <summary>Info about the chat invite</summary>
		public override ExportedChatInvite Invite => invite;
		/// <summary>Mentioned users</summary>
		public override Dictionary<long, User> Users => users;
	}
	/// <summary>The specified chat invite was replaced with another one		<para>See <a href="https://corefork.telegram.org/constructor/messages.exportedChatInviteReplaced"/></para></summary>
	[TLDef(0x222600EF)]
	public sealed partial class Messages_ExportedChatInviteReplaced : Messages_ExportedChatInviteBase
	{
		/// <summary>The replaced chat invite</summary>
		public ExportedChatInvite invite;
		/// <summary>The invite that replaces the previous invite</summary>
		public ExportedChatInvite new_invite;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		/// <summary>The replaced chat invite</summary>
		public override ExportedChatInvite Invite => invite;
		/// <summary>Mentioned users</summary>
		public override Dictionary<long, User> Users => users;
	}

	/// <summary>Info about the users that joined the chat using a specific chat invite		<para>See <a href="https://corefork.telegram.org/constructor/messages.chatInviteImporters"/></para></summary>
	[TLDef(0x81B6B00A)]
	public sealed partial class Messages_ChatInviteImporters : IObject
	{
		/// <summary>Number of users that joined</summary>
		public int count;
		/// <summary>The users that joined</summary>
		public ChatInviteImporter[] importers;
		/// <summary>The users that joined</summary>
		public Dictionary<long, User> users;
	}
	
	/// <summary>Indicates a range of chat messages		<para>See <a href="https://corefork.telegram.org/constructor/messageRange"/></para></summary>
	[TLDef(0x0AE30253)]
	public sealed partial class MessageRange : IObject
	{
		/// <summary>Start of range (message ID)</summary>
		public int min_id;
		/// <summary>End of range (message ID)</summary>
		public int max_id;
	}
	
	/// <summary>Info about chat invites generated by admins.		<para>See <a href="https://corefork.telegram.org/constructor/messages.chatAdminsWithInvites"/></para></summary>
	[TLDef(0xB69B72D7)]
	public sealed partial class Messages_ChatAdminsWithInvites : IObject
	{
		/// <summary>Info about chat invites generated by admins.</summary>
		public ChatAdminWithInvites[] admins;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
	}

	/// <summary>Contains a confirmation text to be shown to the user, upon <a href="https://corefork.telegram.org/api/import">importing chat history, click here for more info »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.checkedHistoryImportPeer"/></para></summary>
	[TLDef(0xA24DE717)]
	public sealed partial class Messages_CheckedHistoryImportPeer : IObject
	{
		/// <summary>A confirmation text to be shown to the user, upon <a href="https://corefork.telegram.org/api/import">importing chat history »</a>.</summary>
		public string confirm_text;
	}
	
	/// <summary>A set of sponsored messages associated to a channel		<para>See <a href="https://corefork.telegram.org/constructor/messages.sponsoredMessages"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.sponsoredMessagesEmpty">messages.sponsoredMessagesEmpty</a></remarks>
	[TLDef(0xC9EE1D87)]
	public sealed partial class Messages_SponsoredMessages : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>If set, specifies the minimum number of messages between shown sponsored messages; otherwise, only one sponsored message must be shown after all ordinary messages.</summary>
		[IfFlag(0)] public int posts_between;
		/// <summary>Sponsored messages</summary>
		public SponsoredMessage[] messages;
		/// <summary>Chats mentioned in the sponsored messages</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in the sponsored messages</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="posts_between"/> has a value</summary>
			has_posts_between = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>Information about found messages sent on a specific day		<para>See <a href="https://corefork.telegram.org/constructor/messages.searchResultsCalendar"/></para></summary>
	[TLDef(0x147EE23C)]
	public sealed partial class Messages_SearchResultsCalendar : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of results matching query</summary>
		public int count;
		/// <summary>Starting timestamp of attached messages</summary>
		public DateTime min_date;
		/// <summary>Ending timestamp of attached messages</summary>
		public int min_msg_id;
		/// <summary>Indicates the absolute position of <c>messages[0]</c> within the total result set with count <c>count</c>. <br/>This is useful, for example, if we need to display a <c>progress/total</c> counter (like <c>photo 134 of 200</c>, for all media in a chat, we could simply use <c>photo ${offset_id_offset} of ${count}</c>.</summary>
		[IfFlag(1)] public int offset_id_offset;
		/// <summary>Used to split the <c>messages</c> by days: multiple <see cref="SearchResultsCalendarPeriod"/> constructors are returned, each containing information about the first, last and total number of messages matching the filter that were sent on a specific day.  <br/>This information can be easily used to split the returned <c>messages</c> by day.</summary>
		public SearchResultsCalendarPeriod[] periods;
		/// <summary>Messages</summary>
		public MessageBase[] messages;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>If set, indicates that the results may be inexact</summary>
			inexact = 0x1,
			/// <summary>Field <see cref="offset_id_offset"/> has a value</summary>
			has_offset_id_offset = 0x2,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>Information about sparse positions of messages		<para>See <a href="https://corefork.telegram.org/constructor/messages.searchResultsPositions"/></para></summary>
	[TLDef(0x53B22BAF)]
	public sealed partial class Messages_SearchResultsPositions : IObject
	{
		/// <summary>Total number of found messages</summary>
		public int count;
		/// <summary>List of message positions</summary>
		public SearchResultsPosition[] positions;
	}
	
	/// <summary>Peer settings		<para>See <a href="https://corefork.telegram.org/constructor/messages.peerSettings"/></para></summary>
	[TLDef(0x6880B94D)]
	public sealed partial class Messages_PeerSettings : IObject, IPeerResolver
	{
		/// <summary>Peer settings</summary>
		public PeerSettings settings;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>List of peers that reacted to a specific message		<para>See <a href="https://corefork.telegram.org/constructor/messages.messageReactionsList"/></para></summary>
	[TLDef(0x31BD492D)]
	public sealed partial class Messages_MessageReactionsList : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of reactions matching query</summary>
		public int count;
		/// <summary>List of peers that reacted to a specific message</summary>
		public MessagePeerReaction[] reactions;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>If set, indicates the next offset to use to load more results by invoking <see cref="SchemaExtensions.Messages_GetMessageReactionsList">Messages_GetMessageReactionsList</see>.</summary>
		[IfFlag(0)] public string next_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>Animations and metadata associated with <a href="https://corefork.telegram.org/api/reactions">message reactions »</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.availableReactions"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.availableReactionsNotModified">messages.availableReactionsNotModified</a></remarks>
    [TLDef(0x768E3AAD)]
    public sealed partial class Messages_AvailableReactions : IObject
    {
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public int hash;
    	/// <summary>Animations and metadata associated with <a href="https://corefork.telegram.org/api/reactions">message reactions »</a></summary>
    	public AvailableReaction[] reactions;
    }
	
	/// <summary><a href="https://corefork.telegram.org/api/transcribe">Transcribed text from a voice message »</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.transcribedAudio"/></para></summary>
	[TLDef(0xCFB9D957)]
	public sealed partial class Messages_TranscribedAudio : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Transcription ID</summary>
		public long transcription_id;
		/// <summary>Transcripted text</summary>
		public string text;
		/// <summary>For non-<a href="https://corefork.telegram.org/api/premium">Premium</a> users, this flag will be set, indicating the remaining transcriptions in the free trial period.</summary>
		[IfFlag(1)] public int trial_remains_num;
		/// <summary>For non-<a href="https://corefork.telegram.org/api/premium">Premium</a> users, this flag will be set, indicating the date when the <c>trial_remains_num</c> counter will be reset to the maximum value of <a href="https://corefork.telegram.org/api/config#transcribe-audio-trial-weekly-number">transcribe_audio_trial_weekly_number</a>.</summary>
		[IfFlag(1)] public DateTime trial_remains_until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the transcription is partial because audio transcription is still in progress, if set the user may receive further <see cref="UpdateTranscribedAudio"/> updates with the updated transcription.</summary>
			pending = 0x1,
			/// <summary>Fields <see cref="trial_remains_num"/> and <see cref="trial_remains_until_date"/> have a value</summary>
			has_trial_remains_num = 0x2,
		}
	}
	
	/// <summary>List of <a href="https://corefork.telegram.org/api/reactions">message reactions</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.reactions"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.reactionsNotModified">messages.reactionsNotModified</a></remarks>
	[TLDef(0xEAFDF716)]
	public sealed partial class Messages_Reactions : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>Reactions</summary>
		public Reaction[] reactions;
	}
	
	/// <summary>Contains information about multiple <a href="https://corefork.telegram.org/api/forum#forum-topics">forum topics</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.forumTopics"/></para></summary>
	[TLDef(0x367617D3)]
	public sealed partial class Messages_ForumTopics : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of topics matching query; may be more than the topics contained in <c>topics</c>, in which case <a href="https://corefork.telegram.org/api/offsets">pagination</a> is required.</summary>
		public int count;
		/// <summary>Forum topics</summary>
		public ForumTopicBase[] topics;
		/// <summary>Related messages (contains the messages mentioned by <see cref="ForumTopic"/>.<c>top_message</c>).</summary>
		public MessageBase[] messages;
		/// <summary>Related chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Related users</summary>
		public Dictionary<long, User> users;
		/// <summary><a href="https://corefork.telegram.org/api/updates">Event count after generation</a></summary>
		public int pts;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the returned topics are ordered by creation date; if set, pagination by <c>offset_date</c> should use <see cref="ForumTopic"/>.<c>date</c>; otherwise topics are ordered by the last message date, so paginate by the <c>date</c> of the <see cref="MessageBase"/> referenced by <see cref="ForumTopic"/>.<c>top_message</c>.</summary>
			order_by_create_date = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>Represents a list of <a href="https://corefork.telegram.org/api/emoji-categories">emoji categories</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.emojiGroups"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.emojiGroupsNotModified">messages.emojiGroupsNotModified</a></remarks>
	[TLDef(0x881FB94B)]
	public sealed partial class Messages_EmojiGroups : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary>A list of <a href="https://corefork.telegram.org/api/emoji-categories">emoji categories</a>.</summary>
		public EmojiGroupBase[] groups;
	}
	
	/// <summary>Translated text with <a href="https://corefork.telegram.org/api/entities">entities</a>.		<para>See <a href="https://corefork.telegram.org/type/messages.TranslatedText"/></para>		<para>Derived classes: <see cref="Messages_TranslateResult"/></para></summary>
	public abstract partial class Messages_TranslatedText : IObject { }
	/// <summary>Translated text with <a href="https://corefork.telegram.org/api/entities">entities</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.translateResult"/></para></summary>
	[TLDef(0x33DB32F8)]
	public sealed partial class Messages_TranslateResult : Messages_TranslatedText
	{
		/// <summary>Text+<a href="https://corefork.telegram.org/api/entities">entities</a>, for each input message.</summary>
		public TextWithEntities[] result;
	}
	
	/// <summary>Contains information about a <a href="https://corefork.telegram.org/api/bots/webapps#direct-link-mini-apps">direct link Mini App</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.botApp"/></para></summary>
	[TLDef(0xEB50ADF5)]
	public sealed partial class Messages_BotApp : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Bot app information</summary>
		public BotApp app;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the web app was never used by the user, and confirmation must be asked from the user before opening it.</summary>
			inactive = 0x1,
			/// <summary>The bot is asking permission to send messages to the user: if the user agrees, set the <c>write_allowed</c> flag when invoking <see cref="SchemaExtensions.Messages_RequestAppWebView">Messages_RequestAppWebView</see>.</summary>
			request_write_access = 0x2,
			/// <summary>Deprecated flag, can be ignored.</summary>
			has_settings = 0x4,
		}
	}
	
	/// <summary>Represents an Instant View webpage.		<para>See <a href="https://corefork.telegram.org/constructor/messages.webPage"/></para></summary>
	[TLDef(0xFD5E12BD)]
	public sealed partial class Messages_WebPage : IObject, IPeerResolver
	{
		/// <summary>The instant view webpage.</summary>
		public WebPageBase webpage;
		/// <summary>Chats mentioned in the webpage.</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in the webpage.</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>Represents some <a href="https://corefork.telegram.org/api/saved-messages">saved message dialogs »</a>.		<para>See <a href="https://corefork.telegram.org/type/messages.SavedDialogs"/></para>		<para>Derived classes: <see cref="Messages_SavedDialogs"/>, <see cref="Messages_SavedDialogsSlice"/>, <see cref="Messages_SavedDialogsNotModified"/></para></summary>
	public abstract partial class Messages_SavedDialogsBase : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/saved-messages">Saved message dialogs »</a>.</summary>
		public virtual SavedDialog[] Dialogs => default;
		/// <summary>List of last messages from each saved dialog</summary>
		public virtual MessageBase[] Messages => default;
		/// <summary>Mentioned chats</summary>
		public virtual Dictionary<long, ChatBase> Chats => default;
		/// <summary>Mentioned users</summary>
		public virtual Dictionary<long, User> Users => default;
	}
	/// <summary>Represents some <a href="https://corefork.telegram.org/api/saved-messages">saved message dialogs »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.savedDialogs"/></para></summary>
	[TLDef(0xF83AE221)]
	public partial class Messages_SavedDialogs : Messages_SavedDialogsBase, IPeerResolver
	{
		/// <summary><a href="https://corefork.telegram.org/api/saved-messages">Saved message dialogs »</a>.</summary>
		public SavedDialog[] dialogs;
		/// <summary>List of last messages from each saved dialog</summary>
		public MessageBase[] messages;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		/// <summary><a href="https://corefork.telegram.org/api/saved-messages">Saved message dialogs »</a>.</summary>
		public override SavedDialog[] Dialogs => dialogs;
		/// <summary>List of last messages from each saved dialog</summary>
		public override MessageBase[] Messages => messages;
		/// <summary>Mentioned chats</summary>
		public override Dictionary<long, ChatBase> Chats => chats;
		/// <summary>Mentioned users</summary>
		public override Dictionary<long, User> Users => users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Incomplete list of <a href="https://corefork.telegram.org/api/saved-messages">saved message dialogs »</a> with messages and auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/messages.savedDialogsSlice"/></para></summary>
	[TLDef(0x44BA9DD9)]
	public sealed partial class Messages_SavedDialogsSlice : Messages_SavedDialogs
	{
		/// <summary>Total number of saved message dialogs</summary>
		public int count;
	}
	/// <summary>The saved dialogs haven't changed		<para>See <a href="https://corefork.telegram.org/constructor/messages.savedDialogsNotModified"/></para></summary>
	[TLDef(0xC01F6FE8)]
	public sealed partial class Messages_SavedDialogsNotModified : Messages_SavedDialogsBase
	{
		/// <summary>Number of <a href="https://corefork.telegram.org/api/saved-messages">saved dialogs</a> found server-side by the query</summary>
		public int count;
	}
	
	/// <summary>List of <a href="https://corefork.telegram.org/api/saved-messages#tags">reaction tag »</a> names assigned by the user.		<para>See <a href="https://corefork.telegram.org/constructor/messages.savedReactionTags"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.savedReactionTagsNotModified">messages.savedReactionTagsNotModified</a></remarks>
    [TLDef(0x3259950A)]
    public sealed partial class Messages_SavedReactionTags : IObject
    {
    	/// <summary>Saved reaction tags.</summary>
    	public SavedReactionTag[] tags;
    	/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
    	public long hash;
    }
	
	/// <summary>Info about <a href="https://corefork.telegram.org/api/business#quick-reply-shortcuts">quick reply shortcuts »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.quickReplies"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.quickRepliesNotModified">messages.quickRepliesNotModified</a></remarks>
	[TLDef(0xC68D6695)]
	public sealed partial class Messages_QuickReplies : IObject, IPeerResolver
	{
		/// <summary>Quick reply shortcuts.</summary>
		public QuickReply[] quick_replies;
		/// <summary>Messages mentioned in <c>quick_replies</c>.</summary>
		public MessageBase[] messages;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary><a href="https://corefork.telegram.org/api/folders">Folder and folder tags</a> information		<para>See <a href="https://corefork.telegram.org/constructor/messages.dialogFilters"/></para></summary>
    [TLDef(0x2AD93719)]
    public sealed partial class Messages_DialogFilters : IObject
    {
    	/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
    	public Flags flags;
    	/// <summary>Folders.</summary>
    	public DialogFilterBase[] filters;

    	[Flags] public enum Flags : uint
    	{
    		/// <summary>Whether <a href="https://corefork.telegram.org/api/folders#folder-tags">folder tags</a> are enabled.</summary>
    		tags_enabled = 0x1,
    	}
    }
	
	/// <summary>The list of <a href="https://corefork.telegram.org/api/stickers">stickersets owned by the current account »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.myStickers"/></para></summary>
	[TLDef(0xFAFF629D)]
	public sealed partial class Messages_MyStickers : IObject
	{
		/// <summary>Total number of owned stickersets.</summary>
		public int count;
		/// <summary>Stickersets</summary>
		public StickerSetCoveredBase[] sets;
	}
	
	/// <summary>Contains info about successfully or unsuccessfully <a href="https://corefork.telegram.org/api/invites#direct-invites">invited »</a> users.		<para>See <a href="https://corefork.telegram.org/constructor/messages.invitedUsers"/></para></summary>
	[TLDef(0x7F5DEFA6)]
	public sealed partial class Messages_InvitedUsers : IObject
	{
		/// <summary>List of updates about successfully invited users (and eventually info about the created group)</summary>
		public UpdatesBase updates;
		/// <summary>A list of users that could not be invited, along with the reason why they couldn't be invited.</summary>
		public MissingInvitee[] missing_invitees;
	}
	
	/// <summary>The full list of usable <a href="https://corefork.telegram.org/api/effects">animated message effects »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/messages.availableEffects"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/messages.availableEffectsNotModified">messages.availableEffectsNotModified</a></remarks>
	[TLDef(0xBDDB616E)]
	public sealed partial class Messages_AvailableEffects : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public int hash;
		/// <summary>Message effects</summary>
		public AvailableEffect[] effects;
		/// <summary>Documents specified in the <c>effects</c> constructors.</summary>
		public DocumentBase[] documents;
	}
	
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/bots/inline#21-using-a-prepared-inline-message">prepared inline message saved by a bot, to be sent to the user via a web app »</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.botPreparedInlineMessage"/></para></summary>
	[TLDef(0x8ECF0511)]
	public sealed partial class Messages_BotPreparedInlineMessage : IObject
	{
		/// <summary>The ID of the saved message, to be passed to the <c>id</c> field of the <a href="https://corefork.telegram.org/api/web-events#web-app-send-prepared-message">web_app_send_prepared_message event »</a></summary>
		public string id;
		/// <summary>Expiration date of the message</summary>
		public DateTime expire_date;
	}

	/// <summary>Represents a <a href="https://corefork.telegram.org/api/bots/inline#21-using-a-prepared-inline-message">prepared inline message received via a bot's mini app, that can be sent to some chats »</a>		<para>See <a href="https://corefork.telegram.org/constructor/messages.preparedInlineMessage"/></para></summary>
	[TLDef(0xFF57708D)]
	public sealed partial class Messages_PreparedInlineMessage : IObject
	{
		/// <summary>The <c>query_id</c> to pass to <see cref="SchemaExtensions.Messages_SendInlineBotResult">Messages_SendInlineBotResult</see></summary>
		public long query_id;
		/// <summary>The contents of the message, to be shown in a preview</summary>
		public BotInlineResultBase result;
		/// <summary>Types of chats where this message can be sent</summary>
		public InlineQueryPeerType[] peer_types;
		/// <summary>Caching validity of the results</summary>
		public int cache_time;
		/// <summary>Users mentioned in the results</summary>
		public Dictionary<long, User> users;
	}
	
	/// <summary>Found <a href="https://corefork.telegram.org/api/stickers">stickers</a>		<para>See <a href="https://corefork.telegram.org/type/messages.FoundStickers"/></para>		<para>Derived classes: <see cref="Messages_FoundStickersNotModified"/>, <see cref="Messages_FoundStickers"/></para></summary>
	public abstract partial class Messages_FoundStickersBase : IObject { }
	/// <summary>No new stickers were found for the specified query		<para>See <a href="https://corefork.telegram.org/constructor/messages.foundStickersNotModified"/></para></summary>
	[TLDef(0x6010C534)]
	public sealed partial class Messages_FoundStickersNotModified : Messages_FoundStickersBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Offset for <a href="https://corefork.telegram.org/api/offsets">pagination</a></summary>
		[IfFlag(0)] public int next_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
	}
	/// <summary>Found stickers		<para>See <a href="https://corefork.telegram.org/constructor/messages.foundStickers"/></para></summary>
	[TLDef(0x82C9E290)]
	public sealed partial class Messages_FoundStickers : Messages_FoundStickersBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Offset for <a href="https://corefork.telegram.org/api/offsets">pagination</a></summary>
		[IfFlag(0)] public int next_offset;
		/// <summary><a href="https://corefork.telegram.org/api/offsets#hash-generation">Hash used for caching, for more info click here</a></summary>
		public long hash;
		/// <summary>Found stickers</summary>
		public DocumentBase[] stickers;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
	}
	
	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/messages.webPagePreview"/></para></summary>
	[TLDef(0xB53E8B21)]
	public sealed partial class Messages_WebPagePreview : IObject
	{
		public MessageMedia media;
		public Dictionary<long, User> users;
	}
}