using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Chat info.		<para>See <a href="https://corefork.telegram.org/type/Dialog"/></para>		<para>Derived classes: <see cref="Dialog"/>, <see cref="DialogFolder"/></para></summary>
    public abstract partial class DialogBase : IObject
    {
        /// <summary>The chat</summary>
        public virtual Peer Peer => default;
        /// <summary>The latest message ID</summary>
        public virtual int TopMessage => default;
    }
    
    /// <summary>Chat		<para>See <a href="https://corefork.telegram.org/constructor/dialog"/></para></summary>
	[TLDef(0xD58A08C6)]
	public sealed partial class Dialog : DialogBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The chat</summary>
		public Peer peer;
		/// <summary>The latest message ID</summary>
		public int top_message;
		/// <summary>Position up to which all incoming messages are read.</summary>
		public int read_inbox_max_id;
		/// <summary>Position up to which all outgoing messages are read.</summary>
		public int read_outbox_max_id;
		/// <summary>Number of unread messages</summary>
		public int unread_count;
		/// <summary>Number of <a href="https://corefork.telegram.org/api/mentions">unread mentions</a></summary>
		public int unread_mentions_count;
		/// <summary>Number of unread reactions to messages you sent</summary>
		public int unread_reactions_count;
		/// <summary>Notification settings</summary>
		public PeerNotifySettings notify_settings;
		/// <summary><a href="https://corefork.telegram.org/api/updates">PTS</a></summary>
		[IfFlag(0)] public int pts;
		/// <summary>Message <a href="https://corefork.telegram.org/api/drafts">draft</a></summary>
		[IfFlag(1)] public DraftMessageBase draft;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(4)] public int folder_id;
		/// <summary>Time-to-live of all messages sent in this dialog</summary>
		[IfFlag(5)] public int ttl_period;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="pts"/> has a value</summary>
			has_pts = 0x1,
			/// <summary>Field <see cref="draft"/> has a value</summary>
			has_draft = 0x2,
			/// <summary>Is the dialog pinned</summary>
			pinned = 0x4,
			/// <summary>Whether the chat was manually marked as unread</summary>
			unread_mark = 0x8,
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x10,
			/// <summary>Field <see cref="ttl_period"/> has a value</summary>
			has_ttl_period = 0x20,
			/// <summary>Users may also choose to display messages from all topics of a <a href="https://corefork.telegram.org/api/forum">forum</a> as if they were sent to a normal group, using a "View as messages" setting in the local client.  <br/>This setting only affects the current account, and is synced to other logged in sessions using the <see cref="SchemaExtensions.Channels_ToggleViewForumAsMessages">Channels_ToggleViewForumAsMessages</see> method; invoking this method will update the value of this flag.</summary>
			view_forum_as_messages = 0x40,
		}

		/// <summary>The chat</summary>
		public override Peer Peer => peer;
		/// <summary>The latest message ID</summary>
		public override int TopMessage => top_message;
	}
	/// <summary>Dialog in folder		<para>See <a href="https://corefork.telegram.org/constructor/dialogFolder"/></para></summary>
	[TLDef(0x71BD134C)]
	public sealed partial class DialogFolder : DialogBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The folder</summary>
		public Folder folder;
		/// <summary>Peer in folder</summary>
		public Peer peer;
		/// <summary>Latest message ID of dialog</summary>
		public int top_message;
		/// <summary>Number of unread muted peers in folder</summary>
		public int unread_muted_peers_count;
		/// <summary>Number of unread unmuted peers in folder</summary>
		public int unread_unmuted_peers_count;
		/// <summary>Number of unread messages from muted peers in folder</summary>
		public int unread_muted_messages_count;
		/// <summary>Number of unread messages from unmuted peers in folder</summary>
		public int unread_unmuted_messages_count;

		[Flags] public enum Flags : uint
		{
			/// <summary>Is this folder pinned</summary>
			pinned = 0x4,
		}

		/// <summary>Peer in folder</summary>
		public override Peer Peer => peer;
		/// <summary>Latest message ID of dialog</summary>
		public override int TopMessage => top_message;
	}
	
	/// <summary>Dialog filter (<a href="https://corefork.telegram.org/api/folders">folder »</a>)		<para>See <a href="https://corefork.telegram.org/type/DialogFilter"/></para>		<para>Derived classes: <see cref="DialogFilter"/>, <see cref="DialogFilterChatlist"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/dialogFilterDefault">dialogFilterDefault</a></remarks>
	public abstract partial class DialogFilterBase : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> ID</summary>
		public virtual int ID => default;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> name (max 12 UTF-8 chars)</summary>
		public virtual TextWithEntities Title => default;
		/// <summary>Emoji to use as icon for the folder.</summary>
		public virtual string Emoticon => default;
		/// <summary>A color ID for the <a href="https://corefork.telegram.org/api/folders#folder-tags">folder tag associated to this folder, see here »</a> for more info.</summary>
		public virtual int Color => default;
		/// <summary>Pinned chats, <a href="https://corefork.telegram.org/api/folders">folders</a> can have unlimited pinned chats</summary>
		public virtual InputPeer[] PinnedPeers => default;
		/// <summary>Include the following chats in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
		public virtual InputPeer[] IncludePeers => default;
	}
	/// <summary>Dialog filter AKA <a href="https://corefork.telegram.org/api/folders">folder</a>		<para>See <a href="https://corefork.telegram.org/constructor/dialogFilter"/></para></summary>
	[TLDef(0xAA472651)]
	public sealed partial class DialogFilter : DialogFilterBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> ID</summary>
		public int id;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> name (max 12 UTF-8 chars)</summary>
		public TextWithEntities title;
		/// <summary>Emoji to use as icon for the folder.</summary>
		[IfFlag(25)] public string emoticon;
		/// <summary>A color ID for the <a href="https://corefork.telegram.org/api/folders#folder-tags">folder tag associated to this folder, see here »</a> for more info.</summary>
		[IfFlag(27)] public int color;
		/// <summary>Pinned chats, <a href="https://corefork.telegram.org/api/folders">folders</a> can have unlimited pinned chats</summary>
		public InputPeer[] pinned_peers;
		/// <summary>Include the following chats in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
		public InputPeer[] include_peers;
		/// <summary>Exclude the following chats from this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
		public InputPeer[] exclude_peers;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to include all contacts in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			contacts = 0x1,
			/// <summary>Whether to include all non-contacts in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			non_contacts = 0x2,
			/// <summary>Whether to include all groups in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			groups = 0x4,
			/// <summary>Whether to include all channels in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			broadcasts = 0x8,
			/// <summary>Whether to include all bots in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			bots = 0x10,
			/// <summary>Whether to exclude muted chats from this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			exclude_muted = 0x800,
			/// <summary>Whether to exclude read chats from this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			exclude_read = 0x1000,
			/// <summary>Whether to exclude archived chats from this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
			exclude_archived = 0x2000,
			/// <summary>Field <see cref="emoticon"/> has a value</summary>
			has_emoticon = 0x2000000,
			/// <summary>Field <see cref="color"/> has a value</summary>
			has_color = 0x8000000,
			title_noanimate = 0x10000000,
		}

		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> ID</summary>
		public override int ID => id;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> name (max 12 UTF-8 chars)</summary>
		public override TextWithEntities Title => title;
		/// <summary>Emoji to use as icon for the folder.</summary>
		public override string Emoticon => emoticon;
		/// <summary>A color ID for the <a href="https://corefork.telegram.org/api/folders#folder-tags">folder tag associated to this folder, see here »</a> for more info.</summary>
		public override int Color => color;
		/// <summary>Pinned chats, <a href="https://corefork.telegram.org/api/folders">folders</a> can have unlimited pinned chats</summary>
		public override InputPeer[] PinnedPeers => pinned_peers;
		/// <summary>Include the following chats in this <a href="https://corefork.telegram.org/api/folders">folder</a></summary>
		public override InputPeer[] IncludePeers => include_peers;
	}
	/// <summary>A folder imported using a <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep link »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/dialogFilterChatlist"/></para></summary>
	[TLDef(0x96537BD7)]
	public sealed partial class DialogFilterChatlist : DialogFilterBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the folder</summary>
		public int id;
		/// <summary>Name of the folder (max 12 UTF-8 chars)</summary>
		public TextWithEntities title;
		/// <summary>Emoji to use as icon for the folder.</summary>
		[IfFlag(25)] public string emoticon;
		/// <summary>A color ID for the <a href="https://corefork.telegram.org/api/folders#folder-tags">folder tag associated to this folder, see here »</a> for more info.</summary>
		[IfFlag(27)] public int color;
		/// <summary>Pinned chats, <a href="https://corefork.telegram.org/api/folders">folders</a> can have unlimited pinned chats</summary>
		public InputPeer[] pinned_peers;
		/// <summary>Chats to include in the folder</summary>
		public InputPeer[] include_peers;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="emoticon"/> has a value</summary>
			has_emoticon = 0x2000000,
			/// <summary>Whether the current user has created some <a href="https://corefork.telegram.org/api/links#chat-folder-links">chat folder deep links »</a> to share the folder as well.</summary>
			has_my_invites = 0x4000000,
			/// <summary>Field <see cref="color"/> has a value</summary>
			has_color = 0x8000000,
			title_noanimate = 0x10000000,
		}

		/// <summary>ID of the folder</summary>
		public override int ID => id;
		/// <summary>Name of the folder (max 12 UTF-8 chars)</summary>
		public override TextWithEntities Title => title;
		/// <summary>Emoji to use as icon for the folder.</summary>
		public override string Emoticon => emoticon;
		/// <summary>A color ID for the <a href="https://corefork.telegram.org/api/folders#folder-tags">folder tag associated to this folder, see here »</a> for more info.</summary>
		public override int Color => color;
		/// <summary>Pinned chats, <a href="https://corefork.telegram.org/api/folders">folders</a> can have unlimited pinned chats</summary>
		public override InputPeer[] PinnedPeers => pinned_peers;
		/// <summary>Chats to include in the folder</summary>
		public override InputPeer[] IncludePeers => include_peers;
	}

	/// <summary>Suggested <a href="https://corefork.telegram.org/api/folders">folders</a>		<para>See <a href="https://corefork.telegram.org/constructor/dialogFilterSuggested"/></para></summary>
	[TLDef(0x77744D4A)]
	public sealed partial class DialogFilterSuggested : IObject
	{
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder info</a></summary>
		public DialogFilterBase filter;
		/// <summary><a href="https://corefork.telegram.org/api/folders">Folder</a> description</summary>
		public string description;
	}

	/// <summary>Peer, or all peers in a folder		<para>See <a href="https://corefork.telegram.org/type/DialogPeer"/></para>		<para>Derived classes: <see cref="DialogPeer"/>, <see cref="DialogPeerFolder"/></para></summary>
	public abstract partial class DialogPeerBase : IObject { }
	/// <summary>Peer		<para>See <a href="https://corefork.telegram.org/constructor/dialogPeer"/></para></summary>
	[TLDef(0xE56DBF05)]
	public sealed partial class DialogPeer : DialogPeerBase
	{
		/// <summary>Peer</summary>
		public Peer peer;
	}
	/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder</a>		<para>See <a href="https://corefork.telegram.org/constructor/dialogPeerFolder"/></para></summary>
	[TLDef(0x514519E2)]
	public sealed partial class DialogPeerFolder : DialogPeerBase
	{
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		public int folder_id;
	}
	
}