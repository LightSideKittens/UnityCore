using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Aggregated view and reaction information of a <a href="https://corefork.telegram.org/api/stories">story</a>.		<para>See <a href="https://corefork.telegram.org/constructor/storyViews"/></para></summary>
    [TLDef(0x8D595CD6)]
    public sealed partial class StoryViews : IObject
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>View counter of the story</summary>
        public int views_count;
        /// <summary>Forward counter of the story</summary>
        [IfFlag(2)] public int forwards_count;
        /// <summary>All reactions sent to this story</summary>
        [IfFlag(3)] public ReactionCount[] reactions;
        /// <summary>Number of reactions added to the story</summary>
        [IfFlag(4)] public int reactions_count;
        /// <summary>User IDs of some recent viewers of the story</summary>
        [IfFlag(0)] public long[] recent_viewers;

        [Flags] public enum Flags : uint
        {
            /// <summary>Field <see cref="recent_viewers"/> has a value</summary>
            has_recent_viewers = 0x1,
            /// <summary>If set, indicates that the viewers list is currently viewable, and was not yet deleted because the story has expired while the user didn't have a <a href="https://corefork.telegram.org/api/premium">Premium</a> account.</summary>
            has_viewers = 0x2,
            /// <summary>Field <see cref="forwards_count"/> has a value</summary>
            has_forwards_count = 0x4,
            /// <summary>Field <see cref="reactions"/> has a value</summary>
            has_reactions = 0x8,
            /// <summary>Field <see cref="reactions_count"/> has a value</summary>
            has_reactions_count = 0x10,
        }
    }
    
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories">Telegram Story</a>		<para>See <a href="https://corefork.telegram.org/type/StoryItem"/></para>		<para>Derived classes: <see cref="StoryItemDeleted"/>, <see cref="StoryItemSkipped"/>, <see cref="StoryItem"/></para></summary>
	public abstract partial class StoryItemBase : IObject
	{
		/// <summary>Story ID</summary>
		public virtual int ID => default;
	}
	/// <summary>Represents a previously active story, that was deleted		<para>See <a href="https://corefork.telegram.org/constructor/storyItemDeleted"/></para></summary>
	[TLDef(0x51E6EE4F)]
	public sealed partial class StoryItemDeleted : StoryItemBase
	{
		/// <summary>Story ID</summary>
		public int id;

		/// <summary>Story ID</summary>
		public override int ID => id;
	}
	/// <summary>Represents an active story, whose full information was omitted for space and performance reasons; use <see cref="SchemaExtensions.Stories_GetStoriesByID">Stories_GetStoriesByID</see> to fetch full info about the skipped story when and if needed.		<para>See <a href="https://corefork.telegram.org/constructor/storyItemSkipped"/></para></summary>
	[TLDef(0xFFADC913)]
	public sealed partial class StoryItemSkipped : StoryItemBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Story ID</summary>
		public int id;
		/// <summary>When was the story posted.</summary>
		public DateTime date;
		/// <summary>When does the story expire.</summary>
		public DateTime expire_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this story can only be viewed by <a href="https://corefork.telegram.org/api/privacy">our close friends, see here »</a> for more info</summary>
			close_friends = 0x100,
		}

		/// <summary>Story ID</summary>
		public override int ID => id;
	}
	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories">story</a>.		<para>See <a href="https://corefork.telegram.org/constructor/storyItem"/></para></summary>
	[TLDef(0x79B26A24)]
	public sealed partial class StoryItem : StoryItemBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of the story.</summary>
		public int id;
		/// <summary>When was the story posted.</summary>
		public DateTime date;
		/// <summary>Sender of the story.</summary>
		[IfFlag(18)] public Peer from_id;
		/// <summary>For <a href="https://corefork.telegram.org/api/stories#reposting-stories">reposted stories »</a>, contains info about the original story.</summary>
		[IfFlag(17)] public StoryFwdHeader fwd_from;
		/// <summary>When does the story expire.</summary>
		public DateTime expire_date;
		/// <summary>Story caption.</summary>
		[IfFlag(0)] public string caption;
		/// <summary><a href="https://corefork.telegram.org/api/entities">Message entities for styled text</a></summary>
		[IfFlag(1)] public MessageEntity[] entities;
		/// <summary>Story media.</summary>
		public MessageMedia media;
		/// <summary>List of media areas, see <a href="https://corefork.telegram.org/api/stories#media-areas">here »</a> for more info on media areas.</summary>
		[IfFlag(14)] public MediaArea[] media_areas;
		/// <summary><a href="https://corefork.telegram.org/api/privacy">Privacy rules</a> indicating who can and can't view this story</summary>
		[IfFlag(2)] public PrivacyRule[] privacy;
		/// <summary>View date and reaction information</summary>
		[IfFlag(3)] public StoryViews views;
		/// <summary>The reaction we sent.</summary>
		[IfFlag(15)] public Reaction sent_reaction;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="caption"/> has a value</summary>
			has_caption = 0x1,
			/// <summary>Field <see cref="entities"/> has a value</summary>
			has_entities = 0x2,
			/// <summary>Field <see cref="privacy"/> has a value</summary>
			has_privacy = 0x4,
			/// <summary>Field <see cref="views"/> has a value</summary>
			has_views = 0x8,
			/// <summary>Whether this story is pinned on the user's profile</summary>
			pinned = 0x20,
			/// <summary>Whether this story is public and can be viewed by everyone</summary>
			public_ = 0x80,
			/// <summary>Whether this story can only be viewed by <a href="https://corefork.telegram.org/api/privacy">our close friends, see here »</a> for more info</summary>
			close_friends = 0x100,
			/// <summary>Full information about this story was omitted for space and performance reasons; use <see cref="SchemaExtensions.Stories_GetStoriesByID">Stories_GetStoriesByID</see> to fetch full info about this story when and if needed.</summary>
			min = 0x200,
			/// <summary>Whether this story is <a href="https://telegram.org/blog/protected-content-delete-by-date-and-more">protected</a> and thus cannot be forwarded; clients should also prevent users from saving attached media (i.e. videos should only be streamed, photos should be kept in RAM, et cetera).</summary>
			noforwards = 0x400,
			/// <summary>Indicates whether the story was edited.</summary>
			edited = 0x800,
			/// <summary>Whether this story can only be viewed by our contacts</summary>
			contacts = 0x1000,
			/// <summary>Whether this story can only be viewed by a select list of our contacts</summary>
			selected_contacts = 0x2000,
			/// <summary>Field <see cref="media_areas"/> has a value</summary>
			has_media_areas = 0x4000,
			/// <summary>Field <see cref="sent_reaction"/> has a value</summary>
			has_sent_reaction = 0x8000,
			/// <summary>indicates whether we sent this story.</summary>
			out_ = 0x10000,
			/// <summary>Field <see cref="fwd_from"/> has a value</summary>
			has_fwd_from = 0x20000,
			/// <summary>Field <see cref="from_id"/> has a value</summary>
			has_from_id = 0x40000,
		}

		/// <summary>ID of the story.</summary>
		public override int ID => id;
	}

	/// <summary>Full list of active (or active and hidden) <a href="https://corefork.telegram.org/api/stories#watching-stories">stories</a>.		<para>See <a href="https://corefork.telegram.org/type/stories.AllStories"/></para>		<para>Derived classes: <see cref="Stories_AllStoriesNotModified"/>, <see cref="Stories_AllStories"/></para></summary>
	public abstract partial class Stories_AllStoriesBase : IObject { }
	/// <summary>The list of active (or active and hidden) <a href="https://corefork.telegram.org/api/stories#watching-stories">stories</a> has not changed.		<para>See <a href="https://corefork.telegram.org/constructor/stories.allStoriesNotModified"/></para></summary>
	[TLDef(0x1158FE3E)]
	public sealed partial class Stories_AllStoriesNotModified : Stories_AllStoriesBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>State to use to ask for updates</summary>
		public string state;
		/// <summary>Current <a href="https://corefork.telegram.org/api/stories#stealth-mode">stealth mode</a> information</summary>
		public StoriesStealthMode stealth_mode;

		[Flags] public enum Flags : uint
		{
		}
	}
	/// <summary>Full list of active (or active and hidden) <a href="https://corefork.telegram.org/api/stories#watching-stories">stories</a>.		<para>See <a href="https://corefork.telegram.org/constructor/stories.allStories"/></para></summary>
	[TLDef(0x6EFC5E81)]
	public sealed partial class Stories_AllStories : Stories_AllStoriesBase, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of active (or active and hidden) stories</summary>
		public int count;
		/// <summary>State to use for pagination</summary>
		public string state;
		/// <summary>Stories</summary>
		public PeerStories[] peer_stories;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>Current <a href="https://corefork.telegram.org/api/stories#stealth-mode">stealth mode</a> information</summary>
		public StoriesStealthMode stealth_mode;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether more results can be fetched as <a href="https://corefork.telegram.org/api/stories#watching-stories">described here »</a>.</summary>
			has_more = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>List of <a href="https://corefork.telegram.org/api/stories#pinned-or-archived-stories">stories</a>		<para>See <a href="https://corefork.telegram.org/constructor/stories.stories"/></para></summary>
	[TLDef(0x63C3DD0A)]
	public sealed partial class Stories_Stories : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of stories that can be fetched</summary>
		public int count;
		/// <summary>Stories</summary>
		public StoryItemBase[] stories;
		/// <summary>IDs of pinned stories.</summary>
		[IfFlag(0)] public int[] pinned_to_top;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="pinned_to_top"/> has a value</summary>
			has_pinned_to_top = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary><a href="https://corefork.telegram.org/api/stories">Story</a> view date and reaction information		<para>See <a href="https://corefork.telegram.org/type/StoryView"/></para>		<para>Derived classes: <see cref="StoryView"/>, <see cref="StoryViewPublicForward"/>, <see cref="StoryViewPublicRepost"/></para></summary>
	public abstract partial class StoryViewBase : IObject { }
	/// <summary><a href="https://corefork.telegram.org/api/stories">Story</a> view date and reaction information		<para>See <a href="https://corefork.telegram.org/constructor/storyView"/></para></summary>
	[TLDef(0xB0BDEAC5)]
	public sealed partial class StoryView : StoryViewBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The user that viewed the story</summary>
		public long user_id;
		/// <summary>When did the user view the story</summary>
		public DateTime date;
		/// <summary>If present, contains the reaction that the user left on the story</summary>
		[IfFlag(2)] public Reaction reaction;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/block">completely blocked</a> this user, including from viewing more of our stories.</summary>
			blocked = 0x1,
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/block">blocked</a> this user from viewing more of our stories.</summary>
			blocked_my_stories_from = 0x2,
			/// <summary>Field <see cref="reaction"/> has a value</summary>
			has_reaction = 0x4,
		}
	}
	/// <summary>A certain peer has forwarded the story as a message to a public chat or channel.		<para>See <a href="https://corefork.telegram.org/constructor/storyViewPublicForward"/></para></summary>
	[TLDef(0x9083670B)]
	public sealed partial class StoryViewPublicForward : StoryViewBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The message with the forwarded story.</summary>
		public MessageBase message;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/block">completely blocked</a> this user, including from viewing more of our stories.</summary>
			blocked = 0x1,
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/block">blocked</a> this user from viewing more of our stories.</summary>
			blocked_my_stories_from = 0x2,
		}
	}
	/// <summary>A certain peer has reposted the story.		<para>See <a href="https://corefork.telegram.org/constructor/storyViewPublicRepost"/></para></summary>
	[TLDef(0xBD74CF49)]
	public sealed partial class StoryViewPublicRepost : StoryViewBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The peer that reposted the story.</summary>
		public Peer peer_id;
		/// <summary>The reposted story.</summary>
		public StoryItemBase story;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/block">completely blocked</a> this user, including from viewing more of our stories.</summary>
			blocked = 0x1,
			/// <summary>Whether we have <a href="https://corefork.telegram.org/api/block">blocked</a> this user from viewing more of our stories.</summary>
			blocked_my_stories_from = 0x2,
		}
	}

	/// <summary>Reaction and view counters for a <a href="https://corefork.telegram.org/api/stories">story</a>		<para>See <a href="https://corefork.telegram.org/constructor/stories.storyViewsList"/></para></summary>
	[TLDef(0x59D78FC5)]
	public sealed partial class Stories_StoryViewsList : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of results that can be fetched</summary>
		public int count;
		/// <summary>Total number of story views</summary>
		public int views_count;
		/// <summary>Total number of story forwards/reposts</summary>
		public int forwards_count;
		/// <summary>Number of reactions that were added to the story</summary>
		public int reactions_count;
		/// <summary>Story view date and reaction information</summary>
		public StoryViewBase[] views;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>Offset for pagination</summary>
		[IfFlag(0)] public string next_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Reaction and view counters for a list of <a href="https://corefork.telegram.org/api/stories">stories</a>		<para>See <a href="https://corefork.telegram.org/constructor/stories.storyViews"/></para></summary>
	[TLDef(0xDE9EED1D)]
	public sealed partial class Stories_StoryViews : IObject
	{
		/// <summary>View date and reaction information of multiple stories</summary>
		public StoryViews[] views;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
	}


	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories#story-links">story deep link</a>.		<para>See <a href="https://corefork.telegram.org/constructor/exportedStoryLink"/></para></summary>
	[TLDef(0x3FC9053B)]
	public sealed partial class ExportedStoryLink : IObject
	{
		/// <summary>The <a href="https://corefork.telegram.org/api/stories#story-links">story deep link</a>.</summary>
		public string link;
	}

	/// <summary>Information about the current <a href="https://corefork.telegram.org/api/stories#stealth-mode">stealth mode</a> session.		<para>See <a href="https://corefork.telegram.org/constructor/storiesStealthMode"/></para></summary>
	[TLDef(0x712E27FD)]
	public sealed partial class StoriesStealthMode : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>The date up to which stealth mode will be active.</summary>
		[IfFlag(0)] public DateTime active_until_date;
		/// <summary>The date starting from which the user will be allowed to re-enable stealth mode again.</summary>
		[IfFlag(1)] public DateTime cooldown_until_date;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="active_until_date"/> has a value</summary>
			has_active_until_date = 0x1,
			/// <summary>Field <see cref="cooldown_until_date"/> has a value</summary>
			has_cooldown_until_date = 0x2,
		}
	}
	/// <summary><a href="https://corefork.telegram.org/api/stories#watching-stories">Active story list</a> of a specific peer.		<para>See <a href="https://corefork.telegram.org/constructor/stories.peerStories"/></para></summary>
	[TLDef(0xCAE68768)]
	public sealed partial class Stories_PeerStories : IObject, IPeerResolver
	{
		/// <summary>Stories</summary>
		public PeerStories stories;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Contains info about the original poster of a reposted story.		<para>See <a href="https://corefork.telegram.org/constructor/storyFwdHeader"/></para></summary>
	[TLDef(0xB826E150)]
	public sealed partial class StoryFwdHeader : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer that originally posted the story; will be empty for stories forwarded from a user with forwards privacy enabled, in which case <c>from_name</c> will be set, instead.</summary>
		[IfFlag(0)] public Peer from;
		/// <summary>Will be set for stories forwarded from a user with forwards privacy enabled, in which case <c>from</c> will also be empty.</summary>
		[IfFlag(1)] public string from_name;
		/// <summary>, contains the story ID</summary>
		[IfFlag(2)] public int story_id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="from"/> has a value</summary>
			has_from = 0x1,
			/// <summary>Field <see cref="from_name"/> has a value</summary>
			has_from_name = 0x2,
			/// <summary>Field <see cref="story_id"/> has a value</summary>
			has_story_id = 0x4,
			/// <summary>Whether the story media was modified before reposting it (for example by overlaying a round video with a reaction).</summary>
			modified = 0x8,
		}
	}
	/// <summary>How a certain peer reacted to or interacted with a story		<para>See <a href="https://corefork.telegram.org/type/StoryReaction"/></para>		<para>Derived classes: <see cref="StoryReaction"/>, <see cref="StoryReactionPublicForward"/>, <see cref="StoryReactionPublicRepost"/></para></summary>
	public abstract partial class StoryReactionBase : IObject { }
	/// <summary>How a certain peer reacted to a story		<para>See <a href="https://corefork.telegram.org/constructor/storyReaction"/></para></summary>
	[TLDef(0x6090D6D5)]
	public sealed partial class StoryReaction : StoryReactionBase
	{
		/// <summary>The peer</summary>
		public Peer peer_id;
		/// <summary>Reaction date</summary>
		public DateTime date;
		/// <summary>The reaction</summary>
		public Reaction reaction;
	}
	/// <summary>A certain peer has forwarded the story as a message to a public chat or channel.		<para>See <a href="https://corefork.telegram.org/constructor/storyReactionPublicForward"/></para></summary>
	[TLDef(0xBBAB2643)]
	public sealed partial class StoryReactionPublicForward : StoryReactionBase
	{
		/// <summary>The message with the forwarded story.</summary>
		public MessageBase message;
	}
	/// <summary>A certain peer has reposted the story.		<para>See <a href="https://corefork.telegram.org/constructor/storyReactionPublicRepost"/></para></summary>
	[TLDef(0xCFCD0F13)]
	public sealed partial class StoryReactionPublicRepost : StoryReactionBase
	{
		/// <summary>The peer that reposted the story.</summary>
		public Peer peer_id;
		/// <summary>The reposted story.</summary>
		public StoryItemBase story;
	}

	/// <summary>List of peers that reacted to or intercated with a specific <a href="https://corefork.telegram.org/api/stories">story</a>		<para>See <a href="https://corefork.telegram.org/constructor/stories.storyReactionsList"/></para></summary>
	[TLDef(0xAA5F789C)]
	public sealed partial class Stories_StoryReactionsList : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of reactions matching query</summary>
		public int count;
		/// <summary>List of peers that reacted to or interacted with a specific story</summary>
		public StoryReactionBase[] reactions;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>If set, indicates the next offset to use to load more results by invoking <see cref="SchemaExtensions.Stories_GetStoryReactionsList">Stories_GetStoryReactionsList</see>.</summary>
		[IfFlag(0)] public string next_offset;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Stories found using <a href="https://corefork.telegram.org/api/stories#searching-stories">global story search »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/stories.foundStories"/></para></summary>
	[TLDef(0xE2DE7737)]
	public sealed partial class Stories_FoundStories : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of results found for the query.</summary>
		public int count;
		/// <summary>Matching stories.</summary>
		public FoundStory[] stories;
		/// <summary>Offset used to fetch the next page, if not set this is the final page.</summary>
		[IfFlag(0)] public string next_offset;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="next_offset"/> has a value</summary>
			has_next_offset = 0x1,
		}
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
}