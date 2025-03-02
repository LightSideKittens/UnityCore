using System.Collections.Generic;
using System;

namespace TL
{
#pragma warning disable CS1574
    	/// <summary>Phone call		<para>See <a href="https://corefork.telegram.org/type/PhoneCall"/></para>		<para>Derived classes: <see cref="PhoneCallEmpty"/>, <see cref="PhoneCallWaiting"/>, <see cref="PhoneCallRequested"/>, <see cref="PhoneCallAccepted"/>, <see cref="PhoneCall"/>, <see cref="PhoneCallDiscarded"/></para></summary>
	public abstract partial class PhoneCallBase : IObject
	{
		/// <summary>Call ID</summary>
		public virtual long ID => default;
		/// <summary>Access hash</summary>
		public virtual long AccessHash => default;
		/// <summary>Date</summary>
		public virtual DateTime Date => default;
		/// <summary>Admin ID</summary>
		public virtual long AdminId => default;
		/// <summary>Participant ID</summary>
		public virtual long ParticipantId => default;
		/// <summary>Phone call protocol info</summary>
		public virtual PhoneCallProtocol Protocol => default;
		public virtual InputGroupCall ConferenceCall => default;
	}
	/// <summary>Empty constructor		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallEmpty"/></para></summary>
	[TLDef(0x5366C915)]
	public sealed partial class PhoneCallEmpty : PhoneCallBase
	{
		/// <summary>Call ID</summary>
		public long id;

		/// <summary>Call ID</summary>
		public override long ID => id;
	}
	/// <summary>Incoming phone call		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallWaiting"/></para></summary>
	[TLDef(0xEED42858)]
	public sealed partial class PhoneCallWaiting : PhoneCallBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Call ID</summary>
		public long id;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>Date</summary>
		public DateTime date;
		/// <summary>Admin ID</summary>
		public long admin_id;
		/// <summary>Participant ID</summary>
		public long participant_id;
		/// <summary>Phone call protocol info</summary>
		public PhoneCallProtocol protocol;
		/// <summary>When was the phone call received</summary>
		[IfFlag(0)] public DateTime receive_date;
		[IfFlag(8)] public InputGroupCall conference_call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="receive_date"/> has a value</summary>
			has_receive_date = 0x1,
			/// <summary>Is this a video call</summary>
			video = 0x40,
			/// <summary>Field <see cref="conference_call"/> has a value</summary>
			has_conference_call = 0x100,
		}

		/// <summary>Call ID</summary>
		public override long ID => id;
		/// <summary>Access hash</summary>
		public override long AccessHash => access_hash;
		/// <summary>Date</summary>
		public override DateTime Date => date;
		/// <summary>Admin ID</summary>
		public override long AdminId => admin_id;
		/// <summary>Participant ID</summary>
		public override long ParticipantId => participant_id;
		/// <summary>Phone call protocol info</summary>
		public override PhoneCallProtocol Protocol => protocol;
		public override InputGroupCall ConferenceCall => conference_call;
	}
	/// <summary>Requested phone call		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallRequested"/></para></summary>
	[TLDef(0x45361C63)]
	public sealed partial class PhoneCallRequested : PhoneCallBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Phone call ID</summary>
		public long id;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>When was the phone call created</summary>
		public DateTime date;
		/// <summary>ID of the creator of the phone call</summary>
		public long admin_id;
		/// <summary>ID of the other participant of the phone call</summary>
		public long participant_id;
		/// <summary><a href="https://corefork.telegram.org/api/end-to-end/voice-calls">Parameter for key exchange</a></summary>
		public byte[] g_a_hash;
		/// <summary>Call protocol info to be passed to libtgvoip</summary>
		public PhoneCallProtocol protocol;
		[IfFlag(8)] public InputGroupCall conference_call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is a video call</summary>
			video = 0x40,
			/// <summary>Field <see cref="conference_call"/> has a value</summary>
			has_conference_call = 0x100,
		}

		/// <summary>Phone call ID</summary>
		public override long ID => id;
		/// <summary>Access hash</summary>
		public override long AccessHash => access_hash;
		/// <summary>When was the phone call created</summary>
		public override DateTime Date => date;
		/// <summary>ID of the creator of the phone call</summary>
		public override long AdminId => admin_id;
		/// <summary>ID of the other participant of the phone call</summary>
		public override long ParticipantId => participant_id;
		/// <summary>Call protocol info to be passed to libtgvoip</summary>
		public override PhoneCallProtocol Protocol => protocol;
		public override InputGroupCall ConferenceCall => conference_call;
	}
	/// <summary>An accepted phone call		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallAccepted"/></para></summary>
	[TLDef(0x22FD7181)]
	public sealed partial class PhoneCallAccepted : PhoneCallBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>ID of accepted phone call</summary>
		public long id;
		/// <summary>Access hash of phone call</summary>
		public long access_hash;
		/// <summary>When was the call accepted</summary>
		public DateTime date;
		/// <summary>ID of the call creator</summary>
		public long admin_id;
		/// <summary>ID of the other user in the call</summary>
		public long participant_id;
		/// <summary>B parameter for <a href="https://corefork.telegram.org/api/end-to-end/voice-calls">secure E2E phone call key exchange</a></summary>
		public byte[] g_b;
		/// <summary>Protocol to use for phone call</summary>
		public PhoneCallProtocol protocol;
		[IfFlag(8)] public InputGroupCall conference_call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is a video call</summary>
			video = 0x40,
			/// <summary>Field <see cref="conference_call"/> has a value</summary>
			has_conference_call = 0x100,
		}

		/// <summary>ID of accepted phone call</summary>
		public override long ID => id;
		/// <summary>Access hash of phone call</summary>
		public override long AccessHash => access_hash;
		/// <summary>When was the call accepted</summary>
		public override DateTime Date => date;
		/// <summary>ID of the call creator</summary>
		public override long AdminId => admin_id;
		/// <summary>ID of the other user in the call</summary>
		public override long ParticipantId => participant_id;
		/// <summary>Protocol to use for phone call</summary>
		public override PhoneCallProtocol Protocol => protocol;
		public override InputGroupCall ConferenceCall => conference_call;
	}
	/// <summary>Phone call		<para>See <a href="https://corefork.telegram.org/constructor/phoneCall"/></para></summary>
	[TLDef(0x3BA5940C)]
	public sealed partial class PhoneCall : PhoneCallBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Call ID</summary>
		public long id;
		/// <summary>Access hash</summary>
		public long access_hash;
		/// <summary>Date of creation of the call</summary>
		public DateTime date;
		/// <summary>User ID of the creator of the call</summary>
		public long admin_id;
		/// <summary>User ID of the other participant in the call</summary>
		public long participant_id;
		/// <summary><a href="https://corefork.telegram.org/api/end-to-end/voice-calls">Parameter for key exchange</a></summary>
		public byte[] g_a_or_b;
		/// <summary><a href="https://corefork.telegram.org/api/end-to-end/voice-calls">Key fingerprint</a></summary>
		public long key_fingerprint;
		/// <summary>Call protocol info to be passed to libtgvoip</summary>
		public PhoneCallProtocol protocol;
		/// <summary>List of endpoints the user can connect to exchange call data</summary>
		public PhoneConnectionBase[] connections;
		/// <summary>When was the call actually started</summary>
		public DateTime start_date;
		/// <summary>Custom JSON-encoded call parameters to be passed to tgcalls.</summary>
		[IfFlag(7)] public DataJSON custom_parameters;
		[IfFlag(8)] public InputGroupCall conference_call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether P2P connection to the other peer is allowed</summary>
			p2p_allowed = 0x20,
			/// <summary>Whether this is a video call</summary>
			video = 0x40,
			/// <summary>Field <see cref="custom_parameters"/> has a value</summary>
			has_custom_parameters = 0x80,
			/// <summary>Field <see cref="conference_call"/> has a value</summary>
			has_conference_call = 0x100,
		}

		/// <summary>Call ID</summary>
		public override long ID => id;
		/// <summary>Access hash</summary>
		public override long AccessHash => access_hash;
		/// <summary>Date of creation of the call</summary>
		public override DateTime Date => date;
		/// <summary>User ID of the creator of the call</summary>
		public override long AdminId => admin_id;
		/// <summary>User ID of the other participant in the call</summary>
		public override long ParticipantId => participant_id;
		/// <summary>Call protocol info to be passed to libtgvoip</summary>
		public override PhoneCallProtocol Protocol => protocol;
		public override InputGroupCall ConferenceCall => conference_call;
	}
	/// <summary>Indicates a discarded phone call		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallDiscarded"/></para></summary>
	[TLDef(0xF9D25503)]
	public sealed partial class PhoneCallDiscarded : PhoneCallBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Call ID</summary>
		public long id;
		/// <summary>Why was the phone call discarded</summary>
		[IfFlag(0)] public PhoneCallDiscardReason reason;
		/// <summary>Duration of the phone call in seconds</summary>
		[IfFlag(1)] public int duration;
		[IfFlag(8)] public InputGroupCall conference_call;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="reason"/> has a value</summary>
			has_reason = 0x1,
			/// <summary>Field <see cref="duration"/> has a value</summary>
			has_duration = 0x2,
			/// <summary>Whether the server required the user to <see cref="SchemaExtensions.Phone_SetCallRating">Phone_SetCallRating</see> the call</summary>
			need_rating = 0x4,
			/// <summary>Whether the server required the client to <see cref="SchemaExtensions.Phone_SaveCallDebug">Phone_SaveCallDebug</see> the libtgvoip call debug data</summary>
			need_debug = 0x8,
			/// <summary>Whether the call was a video call</summary>
			video = 0x40,
			/// <summary>Field <see cref="conference_call"/> has a value</summary>
			has_conference_call = 0x100,
		}

		/// <summary>Call ID</summary>
		public override long ID => id;
		public override InputGroupCall ConferenceCall => conference_call;
	}

	/// <summary>Phone call connection		<para>See <a href="https://corefork.telegram.org/type/PhoneConnection"/></para>		<para>Derived classes: <see cref="PhoneConnection"/>, <see cref="PhoneConnectionWebrtc"/></para></summary>
	public abstract partial class PhoneConnectionBase : IObject
	{
		/// <summary>Endpoint ID</summary>
		public virtual long ID => default;
		/// <summary>IP address of endpoint</summary>
		public virtual string Ip => default;
		/// <summary>IPv6 address of endpoint</summary>
		public virtual string Ipv6 => default;
		/// <summary>Port ID</summary>
		public virtual int Port => default;
	}
	/// <summary>Identifies an endpoint that can be used to connect to the other user in a phone call		<para>See <a href="https://corefork.telegram.org/constructor/phoneConnection"/></para></summary>
	[TLDef(0x9CC123C7)]
	public sealed partial class PhoneConnection : PhoneConnectionBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Endpoint ID</summary>
		public long id;
		/// <summary>IP address of endpoint</summary>
		public string ip;
		/// <summary>IPv6 address of endpoint</summary>
		public string ipv6;
		/// <summary>Port ID</summary>
		public int port;
		/// <summary>Our peer tag</summary>
		public byte[] peer_tag;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether TCP should be used</summary>
			tcp = 0x1,
		}

		/// <summary>Endpoint ID</summary>
		public override long ID => id;
		/// <summary>IP address of endpoint</summary>
		public override string Ip => ip;
		/// <summary>IPv6 address of endpoint</summary>
		public override string Ipv6 => ipv6;
		/// <summary>Port ID</summary>
		public override int Port => port;
	}
	/// <summary>WebRTC connection parameters		<para>See <a href="https://corefork.telegram.org/constructor/phoneConnectionWebrtc"/></para></summary>
	[TLDef(0x635FE375)]
	public sealed partial class PhoneConnectionWebrtc : PhoneConnectionBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Endpoint ID</summary>
		public long id;
		/// <summary>IP address</summary>
		public string ip;
		/// <summary>IPv6 address</summary>
		public string ipv6;
		/// <summary>Port</summary>
		public int port;
		/// <summary>Username</summary>
		public string username;
		/// <summary>Password</summary>
		public string password;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether this is a TURN endpoint</summary>
			turn = 0x1,
			/// <summary>Whether this is a STUN endpoint</summary>
			stun = 0x2,
		}

		/// <summary>Endpoint ID</summary>
		public override long ID => id;
		/// <summary>IP address</summary>
		public override string Ip => ip;
		/// <summary>IPv6 address</summary>
		public override string Ipv6 => ipv6;
		/// <summary>Port</summary>
		public override int Port => port;
	}

	/// <summary>Protocol info for libtgvoip		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallProtocol"/></para></summary>
	[TLDef(0xFC878FC8)]
	public sealed partial class PhoneCallProtocol : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Minimum layer for remote libtgvoip</summary>
		public int min_layer;
		/// <summary>Maximum layer for remote libtgvoip</summary>
		public int max_layer;
		/// <summary>When using <see cref="SchemaExtensions.Phone_RequestCall">Phone_RequestCall</see> and <see cref="SchemaExtensions.Phone_AcceptCall">Phone_AcceptCall</see>, specify all library versions supported by the client. <br/>The server will merge and choose the best library version supported by both peers, returning only the best value in the result of the callee's <see cref="SchemaExtensions.Phone_AcceptCall">Phone_AcceptCall</see> and in the <see cref="PhoneCallAccepted"/> update received by the caller.</summary>
		public string[] library_versions;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether to allow P2P connection to the other participant</summary>
			udp_p2p = 0x1,
			/// <summary>Whether to allow connection to the other participants through the reflector servers</summary>
			udp_reflector = 0x2,
		}
	}
	
    /// <summary>A VoIP phone call		<para>See <a href="https://corefork.telegram.org/constructor/phone.phoneCall"/></para></summary>
    [TLDef(0xEC82E140)]
    public sealed partial class Phone_PhoneCall : IObject
    {
        /// <summary>The VoIP phone call</summary>
        public PhoneCallBase phone_call;
        /// <summary>VoIP phone call participants</summary>
        public Dictionary<long, User> users;
    }
    
    /// <summary>Why was the phone call discarded?		<para>See <a href="https://corefork.telegram.org/type/PhoneCallDiscardReason"/></para>		<para>Derived classes: <see cref="PhoneCallDiscardReasonMissed"/>, <see cref="PhoneCallDiscardReasonDisconnect"/>, <see cref="PhoneCallDiscardReasonHangup"/>, <see cref="PhoneCallDiscardReasonBusy"/></para></summary>
    public abstract partial class PhoneCallDiscardReason : IObject { }
    /// <summary>The phone call was missed		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallDiscardReasonMissed"/></para></summary>
    [TLDef(0x85E42301)]
    public sealed partial class PhoneCallDiscardReasonMissed : PhoneCallDiscardReason { }
    /// <summary>The phone call was disconnected		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallDiscardReasonDisconnect"/></para></summary>
    [TLDef(0xE095C1A0)]
    public sealed partial class PhoneCallDiscardReasonDisconnect : PhoneCallDiscardReason { }
    /// <summary>The phone call was ended normally		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallDiscardReasonHangup"/></para></summary>
    [TLDef(0x57ADC690)]
    public sealed partial class PhoneCallDiscardReasonHangup : PhoneCallDiscardReason { }
    /// <summary>The phone call was discarded because the user is busy in another call		<para>See <a href="https://corefork.telegram.org/constructor/phoneCallDiscardReasonBusy"/></para></summary>
    [TLDef(0xFAF7E8C9)]
    public sealed partial class PhoneCallDiscardReasonBusy : PhoneCallDiscardReason { }
    /// <summary><para>See <a href="https://corefork.telegram.org/constructor/phoneCallDiscardReasonAllowGroupCall"/></para></summary>
    [TLDef(0xAFE2B839)]
    public sealed partial class PhoneCallDiscardReasonAllowGroupCall : PhoneCallDiscardReason
    {
	    public byte[] encrypted_key;
    }
    
    /// <summary>Info about RTMP streams in a group call or livestream		<para>See <a href="https://corefork.telegram.org/constructor/phone.groupCallStreamChannels"/></para></summary>
    [TLDef(0xD0E482B2)]
    public sealed partial class Phone_GroupCallStreamChannels : IObject
    {
	    /// <summary>RTMP streams</summary>
	    public GroupCallStreamChannel[] channels;
    }

    /// <summary>RTMP URL and stream key to be used in streaming software		<para>See <a href="https://corefork.telegram.org/constructor/phone.groupCallStreamRtmpUrl"/></para></summary>
    [TLDef(0x2DBF3432)]
    public sealed partial class Phone_GroupCallStreamRtmpUrl : IObject
    {
	    /// <summary>RTMP URL</summary>
	    public string url;
	    /// <summary>Stream key</summary>
	    public string key;
    }

    	/// <summary>Contains info about a group call, and partial info about its participants.		<para>See <a href="https://corefork.telegram.org/constructor/phone.groupCall"/></para></summary>
	[TLDef(0x9E727AAD)]
	public sealed partial class Phone_GroupCall : IObject, IPeerResolver
	{
		/// <summary>Info about the group call</summary>
		public GroupCallBase call;
		/// <summary>A partial list of participants.</summary>
		public GroupCallParticipant[] participants;
		/// <summary>Next offset to use when fetching the remaining participants using <see cref="SchemaExtensions.Phone_GetGroupParticipants">Phone_GetGroupParticipants</see></summary>
		public string participants_next_offset;
		/// <summary>Chats mentioned in the participants vector</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in the participants vector</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>Info about the participants of a group call or livestream		<para>See <a href="https://corefork.telegram.org/constructor/phone.groupParticipants"/></para></summary>
	[TLDef(0xF47751B6)]
	public sealed partial class Phone_GroupParticipants : IObject, IPeerResolver
	{
		/// <summary>Number of participants</summary>
		public int count;
		/// <summary>List of participants</summary>
		public GroupCallParticipant[] participants;
		/// <summary>If not empty, the specified list of participants is partial, and more participants can be fetched specifying this parameter as <c>offset</c> in <see cref="SchemaExtensions.Phone_GetGroupParticipants">Phone_GetGroupParticipants</see>.</summary>
		public string next_offset;
		/// <summary>Mentioned chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Mentioned users</summary>
		public Dictionary<long, User> users;
		/// <summary>Version info</summary>
		public int version;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>A list of peers that can be used to join a group call, presenting yourself as a specific user/channel.		<para>See <a href="https://corefork.telegram.org/constructor/phone.joinAsPeers"/></para></summary>
	[TLDef(0xAFE5623F)]
	public sealed partial class Phone_JoinAsPeers : IObject, IPeerResolver
	{
		/// <summary>Peers</summary>
		public Peer[] peers;
		/// <summary>Chats mentioned in the peers vector</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users mentioned in the peers vector</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}

	/// <summary>An invite to a group call or livestream		<para>See <a href="https://corefork.telegram.org/constructor/phone.exportedGroupCallInvite"/></para></summary>
	[TLDef(0x204BD158)]
	public sealed partial class Phone_ExportedGroupCallInvite : IObject
	{
		/// <summary>Invite link</summary>
		public string link;
	}

}