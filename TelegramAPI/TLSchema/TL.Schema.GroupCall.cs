using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>A group call		<para>See <a href="https://corefork.telegram.org/type/GroupCall"/></para>		<para>Derived classes: <see cref="GroupCallDiscarded"/>, <see cref="GroupCall"/></para></summary>
    public abstract partial class GroupCallBase : IObject
    {
        /// <summary>Group call ID</summary>
        public virtual long ID => default;
        /// <summary>Group call access hash</summary>
        public virtual long AccessHash => default;
    }
    
	/// <summary>An ended group call		<para>See <a href="https://corefork.telegram.org/constructor/groupCallDiscarded"/></para></summary>
	[TLDef(0x7780BCB4)]
	public sealed partial class GroupCallDiscarded : GroupCallBase
	{
		/// <summary>Group call ID</summary>
		public long id;
		/// <summary>Group call access hash</summary>
		public long access_hash;
		/// <summary>Group call duration</summary>
		public int duration;

		/// <summary>Group call ID</summary>
		public override long ID => id;
		/// <summary>Group call access hash</summary>
		public override long AccessHash => access_hash;
	}
	/// <summary>Info about a group call or livestream		<para>See <a href="https://corefork.telegram.org/constructor/groupCall"/></para></summary>
	[TLDef(0xCDF8D3E3)]
	public sealed partial class GroupCall : GroupCallBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Group call ID</summary>
		public long id;
		/// <summary>Group call access hash</summary>
		public long access_hash;
		/// <summary>Participant count</summary>
		public int participants_count;
		/// <summary>Group call title</summary>
		[IfFlag(3)] public string title;
		/// <summary>DC ID to be used for livestream chunks</summary>
		[IfFlag(4)] public int stream_dc_id;
		/// <summary>When was the recording started</summary>
		[IfFlag(5)] public DateTime record_start_date;
		/// <summary>When is the call scheduled to start</summary>
		[IfFlag(7)] public DateTime schedule_date;
		/// <summary>Number of people currently streaming video into the call</summary>
		[IfFlag(10)] public int unmuted_video_count;
		/// <summary>Maximum number of people allowed to stream video into the call</summary>
		public int unmuted_video_limit;
		/// <summary>Version</summary>
		public int version;
		[IfFlag(14)] public long conference_from_call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the user should be muted upon joining the call</summary>
			join_muted = 0x2,
			/// <summary>Whether the current user can change the value of the <c>join_muted</c> flag using <see cref="SchemaExtensions.Phone_ToggleGroupCallSettings">Phone_ToggleGroupCallSettings</see></summary>
			can_change_join_muted = 0x4,
			/// <summary>Field <see cref="title"/> has a value</summary>
			has_title = 0x8,
			/// <summary>Field <see cref="stream_dc_id"/> has a value</summary>
			has_stream_dc_id = 0x10,
			/// <summary>Field <see cref="record_start_date"/> has a value</summary>
			has_record_start_date = 0x20,
			/// <summary>Specifies the ordering to use when locally sorting by date and displaying in the UI group call participants.</summary>
			join_date_asc = 0x40,
			/// <summary>Field <see cref="schedule_date"/> has a value</summary>
			has_schedule_date = 0x80,
			/// <summary>Whether we subscribed to the scheduled call</summary>
			schedule_start_subscribed = 0x100,
			/// <summary>Whether you can start streaming video into the call</summary>
			can_start_video = 0x200,
			/// <summary>Field <see cref="unmuted_video_count"/> has a value</summary>
			has_unmuted_video_count = 0x400,
			/// <summary>Whether the group call is currently being recorded</summary>
			record_video_active = 0x800,
			/// <summary>Whether RTMP streams are allowed</summary>
			rtmp_stream = 0x1000,
			/// <summary>Whether the listeners list is hidden and cannot be fetched using <see cref="SchemaExtensions.Phone_GetGroupParticipants">Phone_GetGroupParticipants</see>. The <c>phone.groupParticipants.count</c> and <c>groupCall.participants_count</c> counters will still include listeners.</summary>
			listeners_hidden = 0x2000,
			/// <summary>Field <see cref="conference_from_call"/> has a value</summary>
			has_conference_from_call = 0x4000,
		}

		/// <summary>Group call ID</summary>
		public override long ID => id;
		/// <summary>Group call access hash</summary>
		public override long AccessHash => access_hash;
	}


	/// <summary>Info about a group call participant		<para>See <a href="https://corefork.telegram.org/constructor/groupCallParticipant"/></para></summary>
	[TLDef(0xEBA636FE)]
	public sealed partial class GroupCallParticipant : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Peer information</summary>
		public Peer peer;
		/// <summary>When did this participant join the group call</summary>
		public DateTime date;
		/// <summary>When was this participant last active in the group call</summary>
		[IfFlag(3)] public DateTime active_date;
		/// <summary>Source ID</summary>
		public int source;
		/// <summary>Volume, if not set the volume is set to 100%.</summary>
		[IfFlag(7)] public int volume;
		/// <summary>Info about this participant</summary>
		[IfFlag(11)] public string about;
		/// <summary>Specifies the UI visualization order of peers with raised hands: peers with a higher rating should be showed first in the list.</summary>
		[IfFlag(13)] public long raise_hand_rating;
		/// <summary>Info about the video stream the participant is currently broadcasting</summary>
		[IfFlag(6)] public GroupCallParticipantVideo video;
		/// <summary>Info about the screen sharing stream the participant is currently broadcasting</summary>
		[IfFlag(14)] public GroupCallParticipantVideo presentation;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the participant is muted</summary>
			muted = 0x1,
			/// <summary>Whether the participant has left</summary>
			left = 0x2,
			/// <summary>Whether the participant can unmute themselves</summary>
			can_self_unmute = 0x4,
			/// <summary>Field <see cref="active_date"/> has a value</summary>
			has_active_date = 0x8,
			/// <summary>Whether the participant has just joined</summary>
			just_joined = 0x10,
			/// <summary>If set, and <see cref="UpdateGroupCallParticipants"/>.version &lt; locally stored call.version, info about this participant should be ignored. If (...), and <see cref="UpdateGroupCallParticipants"/>.version &gt; call.version+1, the participant list should be refetched using <see cref="SchemaExtensions.Phone_GetGroupParticipants">Phone_GetGroupParticipants</see>.</summary>
			versioned = 0x20,
			/// <summary>Field <see cref="video"/> has a value</summary>
			has_video = 0x40,
			/// <summary>Field <see cref="volume"/> has a value</summary>
			has_volume = 0x80,
			/// <summary>If not set, the <c>volume</c> and <c>muted_by_you</c> fields can be safely used to overwrite locally cached information; otherwise, <c>volume</c> will contain valid information only if <c>volume_by_admin</c> is set both in the cache and in the received constructor.</summary>
			min = 0x100,
			/// <summary>Whether this participant was muted by the current user</summary>
			muted_by_you = 0x200,
			/// <summary>Whether our volume can only changed by an admin</summary>
			volume_by_admin = 0x400,
			/// <summary>Field <see cref="about"/> has a value</summary>
			has_about = 0x800,
			/// <summary>Whether this participant is the current user</summary>
			self = 0x1000,
			/// <summary>Field <see cref="raise_hand_rating"/> has a value</summary>
			has_raise_hand_rating = 0x2000,
			/// <summary>Field <see cref="presentation"/> has a value</summary>
			has_presentation = 0x4000,
			/// <summary>Whether this participant is currently broadcasting video</summary>
			video_joined = 0x8000,
		}
	}
	/// <summary>Describes a group of video synchronization source identifiers		<para>See <a href="https://corefork.telegram.org/constructor/groupCallParticipantVideoSourceGroup"/></para></summary>
	[TLDef(0xDCB118B7)]
	public sealed partial class GroupCallParticipantVideoSourceGroup : IObject
	{
		/// <summary>SDP semantics</summary>
		public string semantics;
		/// <summary>Source IDs</summary>
		public int[] sources;
	}

	/// <summary>Info about a video stream		<para>See <a href="https://corefork.telegram.org/constructor/groupCallParticipantVideo"/></para></summary>
	[TLDef(0x67753AC8)]
	public sealed partial class GroupCallParticipantVideo : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Endpoint</summary>
		public string endpoint;
		/// <summary>Source groups</summary>
		public GroupCallParticipantVideoSourceGroup[] source_groups;
		/// <summary>Audio source ID</summary>
		[IfFlag(1)] public int audio_source;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether the stream is currently paused</summary>
			paused = 0x1,
			/// <summary>Field <see cref="audio_source"/> has a value</summary>
			has_audio_source = 0x2,
		}
	}

	/// <summary>Info about an RTMP stream in a group call or livestream		<para>See <a href="https://corefork.telegram.org/constructor/groupCallStreamChannel"/></para></summary>
	[TLDef(0x80EB48AF)]
	public sealed partial class GroupCallStreamChannel : IObject
	{
		/// <summary>Channel ID</summary>
		public int channel;
		/// <summary>Specifies the duration of the video segment to fetch in milliseconds, by bitshifting <c>1000</c> to the right <c>scale</c> times: <c>duration_ms := 1000 &gt;&gt; scale</c>.</summary>
		public int scale;
		/// <summary>Last seen timestamp to easily start fetching livestream chunks using <see cref="InputGroupCallStream"/></summary>
		public long last_timestamp_ms;
	}
    
}