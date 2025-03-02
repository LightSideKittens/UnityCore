using System;
using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>URL with chat statistics		<para>See <a href="https://corefork.telegram.org/constructor/statsURL"/></para></summary>
    [TLDef(0x47A971E0)]
    public sealed partial class StatsURL : IObject
    {
        /// <summary>Chat statistics</summary>
        public string url;
    }
    
	/// <summary><a href="https://corefork.telegram.org/api/stats">Channel statistics</a> date range		<para>See <a href="https://corefork.telegram.org/constructor/statsDateRangeDays"/></para></summary>
	[TLDef(0xB637EDAF)]
	public sealed partial class StatsDateRangeDays : IObject
	{
		/// <summary>Initial date</summary>
		public DateTime min_date;
		/// <summary>Final date</summary>
		public DateTime max_date;
	}

	/// <summary>Statistics value couple; initial and final value for period of time currently in consideration		<para>See <a href="https://corefork.telegram.org/constructor/statsAbsValueAndPrev"/></para></summary>
	[TLDef(0xCB43ACDE)]
	public sealed partial class StatsAbsValueAndPrev : IObject
	{
		/// <summary>Current value</summary>
		public double current;
		/// <summary>Previous value</summary>
		public double previous;
	}

	/// <summary><a href="https://corefork.telegram.org/api/stats">Channel statistics percentage</a>.<br/>Compute the percentage simply by doing <c>part * total / 100</c>		<para>See <a href="https://corefork.telegram.org/constructor/statsPercentValue"/></para></summary>
	[TLDef(0xCBCE2FE0)]
	public sealed partial class StatsPercentValue : IObject
	{
		/// <summary>Partial value</summary>
		public double part;
		/// <summary>Total value</summary>
		public double total;
	}

	/// <summary>Channel statistics graph		<para>See <a href="https://corefork.telegram.org/type/StatsGraph"/></para>		<para>Derived classes: <see cref="StatsGraphAsync"/>, <see cref="StatsGraphError"/>, <see cref="StatsGraph"/></para></summary>
	public abstract partial class StatsGraphBase : IObject { }
	/// <summary>This <a href="https://corefork.telegram.org/api/stats">channel statistics graph</a> must be generated asynchronously using <see cref="SchemaExtensions.Stats_LoadAsyncGraph">Stats_LoadAsyncGraph</see> to reduce server load		<para>See <a href="https://corefork.telegram.org/constructor/statsGraphAsync"/></para></summary>
	[TLDef(0x4A27EB2D)]
	public sealed partial class StatsGraphAsync : StatsGraphBase
	{
		/// <summary>Token to use for fetching the async graph</summary>
		public string token;
	}
	/// <summary>An error occurred while generating the <a href="https://corefork.telegram.org/api/stats">statistics graph</a>		<para>See <a href="https://corefork.telegram.org/constructor/statsGraphError"/></para></summary>
	[TLDef(0xBEDC9822)]
	public sealed partial class StatsGraphError : StatsGraphBase
	{
		/// <summary>The error</summary>
		public string error;
	}
	/// <summary><a href="https://corefork.telegram.org/api/stats">Channel statistics graph</a>		<para>See <a href="https://corefork.telegram.org/constructor/statsGraph"/></para></summary>
	[TLDef(0x8EA464B6)]
	public sealed partial class StatsGraph : StatsGraphBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Statistics data</summary>
		public DataJSON json;
		/// <summary>Zoom token</summary>
		[IfFlag(0)] public string zoom_token;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="zoom_token"/> has a value</summary>
			has_zoom_token = 0x1,
		}
	}

	/// <summary><a href="https://corefork.telegram.org/api/stats">Channel statistics</a>.		<para>See <a href="https://corefork.telegram.org/constructor/stats.broadcastStats"/></para></summary>
	[TLDef(0x396CA5FC)]
	public sealed partial class Stats_BroadcastStats : IObject
	{
		/// <summary>Period in consideration</summary>
		public StatsDateRangeDays period;
		/// <summary>Follower count change for period in consideration</summary>
		public StatsAbsValueAndPrev followers;
		/// <summary><c>total_viewcount/postcount</c>, for posts posted during the period in consideration. <br/>Note that in this case, <c>current</c> refers to the <c>period</c> in consideration (<c>min_date</c> till <c>max_date</c>), and <c>prev</c> refers to the previous period (<c>(min_date - (max_date - min_date))</c> till <c>min_date</c>).</summary>
		public StatsAbsValueAndPrev views_per_post;
		/// <summary><c>total_sharecount/postcount</c>, for posts posted during the period in consideration. <br/>Note that in this case, <c>current</c> refers to the <c>period</c> in consideration (<c>min_date</c> till <c>max_date</c>), and <c>prev</c> refers to the previous period (<c>(min_date - (max_date - min_date))</c> till <c>min_date</c>)</summary>
		public StatsAbsValueAndPrev shares_per_post;
		/// <summary><c>total_reactions/postcount</c>, for posts posted during the period in consideration. <br/>Note that in this case, <c>current</c> refers to the <c>period</c> in consideration (<c>min_date</c> till <c>max_date</c>), and <c>prev</c> refers to the previous period (<c>(min_date - (max_date - min_date))</c> till <c>min_date</c>)</summary>
		public StatsAbsValueAndPrev reactions_per_post;
		/// <summary><c>total_views/storycount</c>, for posts posted during the period in consideration. <br/>Note that in this case, <c>current</c> refers to the <c>period</c> in consideration (<c>min_date</c> till <c>max_date</c>), and <c>prev</c> refers to the previous period (<c>(min_date - (max_date - min_date))</c> till <c>min_date</c>)</summary>
		public StatsAbsValueAndPrev views_per_story;
		/// <summary><c>total_shares/storycount</c>, for posts posted during the period in consideration. <br/>Note that in this case, <c>current</c> refers to the <c>period</c> in consideration (<c>min_date</c> till <c>max_date</c>), and <c>prev</c> refers to the previous period (<c>(min_date - (max_date - min_date))</c> till <c>min_date</c>)</summary>
		public StatsAbsValueAndPrev shares_per_story;
		/// <summary><c>total_reactions/storycount</c>, for posts posted during the period in consideration. <br/>Note that in this case, <c>current</c> refers to the <c>period</c> in consideration (<c>min_date</c> till <c>max_date</c>), and <c>prev</c> refers to the previous period (<c>(min_date - (max_date - min_date))</c> till <c>min_date</c>)</summary>
		public StatsAbsValueAndPrev reactions_per_story;
		/// <summary>Percentage of subscribers with enabled notifications</summary>
		public StatsPercentValue enabled_notifications;
		/// <summary>Channel growth graph (absolute subscriber count)</summary>
		public StatsGraphBase growth_graph;
		/// <summary>Followers growth graph (relative subscriber count)</summary>
		public StatsGraphBase followers_graph;
		/// <summary>Muted users graph (relative)</summary>
		public StatsGraphBase mute_graph;
		/// <summary>Views per hour graph (absolute)</summary>
		public StatsGraphBase top_hours_graph;
		/// <summary>Interactions graph (absolute)</summary>
		public StatsGraphBase interactions_graph;
		/// <summary>IV interactions graph (absolute)</summary>
		public StatsGraphBase iv_interactions_graph;
		/// <summary>Views by source graph (absolute)</summary>
		public StatsGraphBase views_by_source_graph;
		/// <summary>New followers by source graph (absolute)</summary>
		public StatsGraphBase new_followers_by_source_graph;
		/// <summary>Subscriber language graph (pie chart)</summary>
		public StatsGraphBase languages_graph;
		/// <summary>A graph containing the number of reactions on posts categorized by emotion</summary>
		public StatsGraphBase reactions_by_emotion_graph;
		/// <summary>A graph containing the number of story views and shares</summary>
		public StatsGraphBase story_interactions_graph;
		/// <summary>A graph containing the number of reactions on stories categorized by emotion</summary>
		public StatsGraphBase story_reactions_by_emotion_graph;
		/// <summary>Detailed statistics about number of views and shares of recently sent messages and stories</summary>
		public PostInteractionCounters[] recent_posts_interactions;
	}
	/// <summary>Information about an active user in a supergroup		<para>See <a href="https://corefork.telegram.org/constructor/statsGroupTopPoster"/></para></summary>
	[TLDef(0x9D04AF9B)]
	public sealed partial class StatsGroupTopPoster : IObject
	{
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>Number of messages for <a href="https://corefork.telegram.org/api/stats">statistics</a> period in consideration</summary>
		public int messages;
		/// <summary>Average number of characters per message</summary>
		public int avg_chars;
	}

	/// <summary>Information about an active admin in a supergroup		<para>See <a href="https://corefork.telegram.org/constructor/statsGroupTopAdmin"/></para></summary>
	[TLDef(0xD7584C87)]
	public sealed partial class StatsGroupTopAdmin : IObject
	{
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>Number of deleted messages for <a href="https://corefork.telegram.org/api/stats">statistics</a> period in consideration</summary>
		public int deleted;
		/// <summary>Number of kicked users for <a href="https://corefork.telegram.org/api/stats">statistics</a> period in consideration</summary>
		public int kicked;
		/// <summary>Number of banned users for <a href="https://corefork.telegram.org/api/stats">statistics</a> period in consideration</summary>
		public int banned;
	}

	/// <summary>Information about an active supergroup inviter		<para>See <a href="https://corefork.telegram.org/constructor/statsGroupTopInviter"/></para></summary>
	[TLDef(0x535F779D)]
	public sealed partial class StatsGroupTopInviter : IObject
	{
		/// <summary>User ID</summary>
		public long user_id;
		/// <summary>Number of invitations for <a href="https://corefork.telegram.org/api/stats">statistics</a> period in consideration</summary>
		public int invitations;
	}

	/// <summary>Supergroup <a href="https://corefork.telegram.org/api/stats">statistics</a>		<para>See <a href="https://corefork.telegram.org/constructor/stats.megagroupStats"/></para></summary>
	[TLDef(0xEF7FF916)]
	public sealed partial class Stats_MegagroupStats : IObject
	{
		/// <summary>Period in consideration</summary>
		public StatsDateRangeDays period;
		/// <summary>Member count change for period in consideration</summary>
		public StatsAbsValueAndPrev members;
		/// <summary>Message number change for period in consideration</summary>
		public StatsAbsValueAndPrev messages;
		/// <summary>Number of users that viewed messages, for range in consideration</summary>
		public StatsAbsValueAndPrev viewers;
		/// <summary>Number of users that posted messages, for range in consideration</summary>
		public StatsAbsValueAndPrev posters;
		/// <summary>Supergroup growth graph (absolute subscriber count)</summary>
		public StatsGraphBase growth_graph;
		/// <summary>Members growth (relative subscriber count)</summary>
		public StatsGraphBase members_graph;
		/// <summary>New members by source graph</summary>
		public StatsGraphBase new_members_by_source_graph;
		/// <summary>Subscriber language graph (pie chart)</summary>
		public StatsGraphBase languages_graph;
		/// <summary>Message activity graph (stacked bar graph, message type)</summary>
		public StatsGraphBase messages_graph;
		/// <summary>Group activity graph (deleted, modified messages, blocked users)</summary>
		public StatsGraphBase actions_graph;
		/// <summary>Activity per hour graph (absolute)</summary>
		public StatsGraphBase top_hours_graph;
		/// <summary>Activity per day of week graph (absolute)</summary>
		public StatsGraphBase weekdays_graph;
		/// <summary>Info about most active group members</summary>
		public StatsGroupTopPoster[] top_posters;
		/// <summary>Info about most active group admins</summary>
		public StatsGroupTopAdmin[] top_admins;
		/// <summary>Info about most active group inviters</summary>
		public StatsGroupTopInviter[] top_inviters;
		/// <summary>Info about users mentioned in statistics</summary>
		public Dictionary<long, User> users;
	}
	/// <summary>Message statistics		<para>See <a href="https://corefork.telegram.org/constructor/stats.messageStats"/></para></summary>
	[TLDef(0x7FE91C14)]
	public sealed partial class Stats_MessageStats : IObject
	{
		/// <summary>Message view graph</summary>
		public StatsGraphBase views_graph;
		/// <summary>A graph containing the number of reactions on stories categorized by emotion</summary>
		public StatsGraphBase reactions_by_emotion_graph;
	}
	/// <summary>Contains <a href="https://corefork.telegram.org/api/stats">statistics</a> about a <a href="https://corefork.telegram.org/api/stories">story</a>.		<para>See <a href="https://corefork.telegram.org/constructor/stats.storyStats"/></para></summary>
	[TLDef(0x50CD067C)]
	public sealed partial class Stats_StoryStats : IObject
	{
		/// <summary>A graph containing the number of story views and shares</summary>
		public StatsGraphBase views_graph;
		/// <summary>A bar graph containing the number of story reactions categorized by "emotion" (i.e. Positive, Negative, Other, etc...)</summary>
		public StatsGraphBase reactions_by_emotion_graph;
	}
	/// <summary>Contains info about the forwards of a <a href="https://corefork.telegram.org/api/stories">story</a> as a message to public chats and reposts by public channels.		<para>See <a href="https://corefork.telegram.org/constructor/stats.publicForwards"/></para></summary>
	[TLDef(0x93037E20)]
	public sealed partial class Stats_PublicForwards : IObject, IPeerResolver
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Total number of results</summary>
		public int count;
		/// <summary>Info about the forwards of a story.</summary>
		public PublicForward[] forwards;
		/// <summary>Offset used for <a href="https://corefork.telegram.org/api/offsets">pagination</a>.</summary>
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

	/// <summary><a href="https://corefork.telegram.org/api/revenue">Channel revenue ad statistics, see here »</a> for more info.		<para>See <a href="https://corefork.telegram.org/constructor/stats.broadcastRevenueStats"/></para></summary>
	[TLDef(0x5407E297)]
	public sealed partial class Stats_BroadcastRevenueStats : IObject
	{
		/// <summary>Ad impressions graph</summary>
		public StatsGraphBase top_hours_graph;
		/// <summary>Ad revenue graph (in the smallest unit of the cryptocurrency in which revenue is calculated)</summary>
		public StatsGraphBase revenue_graph;
		/// <summary>Current balance, current withdrawable balance and overall revenue</summary>
		public BroadcastRevenueBalances balances;
		/// <summary>Current conversion rate of the cryptocurrency (<strong>not</strong> in the smallest unit) in which revenue is calculated to USD</summary>
		public double usd_rate;
	}

	/// <summary>Contains the URL to use to <a href="https://corefork.telegram.org/api/revenue#withdrawing-revenue">withdraw channel ad revenue</a>.		<para>See <a href="https://corefork.telegram.org/constructor/stats.broadcastRevenueWithdrawalUrl"/></para></summary>
	[TLDef(0xEC659737)]
	public sealed partial class Stats_BroadcastRevenueWithdrawalUrl : IObject
	{
		/// <summary>A unique URL to a Fragment page where the user will be able to specify and submit the address of the TON wallet where the funds will be sent.</summary>
		public string url;
	}
	/// <summary><a href="https://corefork.telegram.org/api/revenue">Channel ad revenue transactions »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/stats.broadcastRevenueTransactions"/></para></summary>
	[TLDef(0x87158466)]
	public sealed partial class Stats_BroadcastRevenueTransactions : IObject
	{
		/// <summary>Total number of transactions.</summary>
		public int count;
		/// <summary>Transactions</summary>
		public BroadcastRevenueTransaction[] transactions;
	}
    
}