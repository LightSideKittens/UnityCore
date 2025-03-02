using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>A contact of the current user that is registered in the system.		<para>See <a href="https://corefork.telegram.org/constructor/contact"/></para></summary>
    [TLDef(0x145ADE0B)]
    public sealed partial class Contact : IObject
    {
        /// <summary>User identifier</summary>
        public long user_id;
        /// <summary>Current user is in the user's contact list</summary>
        public bool mutual;
    }
    
	/// <summary>Successfully imported contact.		<para>See <a href="https://corefork.telegram.org/constructor/importedContact"/></para></summary>
	[TLDef(0xC13E3C50)]
	public sealed partial class ImportedContact : IObject
	{
		/// <summary>User identifier</summary>
		public long user_id;
		/// <summary>The contact's client identifier (passed to one of the <see cref="InputContact"/> constructors)</summary>
		public long client_id;
	}

	/// <summary>Contact status: online / offline.		<para>See <a href="https://corefork.telegram.org/constructor/contactStatus"/></para></summary>
	[TLDef(0x16D9703B)]
	public sealed partial class ContactStatus : IObject
	{
		/// <summary>User identifier</summary>
		public long user_id;
		/// <summary>Online status</summary>
		public UserStatus status;
	}

	/// <summary>The current user's contact list and info on users.		<para>See <a href="https://corefork.telegram.org/constructor/contacts.contacts"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/contacts.contactsNotModified">contacts.contactsNotModified</a></remarks>
	[TLDef(0xEAE87E42)]
	public sealed partial class Contacts_Contacts : IObject
	{
		/// <summary>Contact list</summary>
		public Contact[] contacts;
		/// <summary>Number of contacts that were saved successfully</summary>
		public int saved_count;
		/// <summary>User list</summary>
		public Dictionary<long, User> users;
	}

	/// <summary>Info on successfully imported contacts.		<para>See <a href="https://corefork.telegram.org/constructor/contacts.importedContacts"/></para></summary>
	[TLDef(0x77D01C3B)]
	public sealed partial class Contacts_ImportedContacts : IObject
	{
		/// <summary>List of successfully imported contacts</summary>
		public ImportedContact[] imported;
		/// <summary>Popular contacts</summary>
		public PopularContact[] popular_invites;
		/// <summary>List of contact ids that could not be imported due to system limitation and will need to be imported at a later date.</summary>
		public long[] retry_contacts;
		/// <summary>List of users</summary>
		public Dictionary<long, User> users;
	}

	/// <summary>Full list of blocked users.		<para>See <a href="https://corefork.telegram.org/constructor/contacts.blocked"/></para></summary>
	[TLDef(0x0ADE1591)]
	public partial class Contacts_Blocked : IObject, IPeerResolver
	{
		/// <summary>List of blocked users</summary>
		public PeerBlocked[] blocked;
		/// <summary>Blocked chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>List of users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Incomplete list of blocked users.		<para>See <a href="https://corefork.telegram.org/constructor/contacts.blockedSlice"/></para></summary>
	[TLDef(0xE1664194)]
	public sealed partial class Contacts_BlockedSlice : Contacts_Blocked
	{
		/// <summary>Total number of elements in the list</summary>
		public int count;
	}

	/// <summary>Users found by name substring and auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/contacts.found"/></para></summary>
	[TLDef(0xB3134D9D)]
	public sealed partial class Contacts_Found : IObject, IPeerResolver
	{
		/// <summary>Personalized results</summary>
		public Peer[] my_results;
		/// <summary>List of found user identifiers</summary>
		public Peer[] results;
		/// <summary>Found chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>List of users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	
	/// <summary>Resolved peer		<para>See <a href="https://corefork.telegram.org/constructor/contacts.resolvedPeer"/></para></summary>
	[TLDef(0x7F077AD9)]
	public sealed partial class Contacts_ResolvedPeer : IObject
	{
		/// <summary>The peer</summary>
		public Peer peer;
		/// <summary>Chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the result</summary>
		public IPeerInfo UserOrChat => peer?.UserOrChat(users, chats);
	}

	/// <summary>Top peers		<para>See <a href="https://corefork.telegram.org/type/contacts.TopPeers"/></para>		<para>Derived classes: <see cref="Contacts_TopPeers"/>, <see cref="Contacts_TopPeersDisabled"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/contacts.topPeersNotModified">contacts.topPeersNotModified</a></remarks>
	public abstract partial class Contacts_TopPeersBase : IObject { }
	/// <summary>Top peers		<para>See <a href="https://corefork.telegram.org/constructor/contacts.topPeers"/></para></summary>
	[TLDef(0x70B772A8)]
	public sealed partial class Contacts_TopPeers : Contacts_TopPeersBase, IPeerResolver
	{
		/// <summary>Top peers by top peer category</summary>
		public TopPeerCategoryPeers[] categories;
		/// <summary>Chats</summary>
		public Dictionary<long, ChatBase> chats;
		/// <summary>Users</summary>
		public Dictionary<long, User> users;
		/// <summary>returns a <see cref="User"/> or <see cref="ChatBase"/> for the given Peer</summary>
		public IPeerInfo UserOrChat(Peer peer) => peer?.UserOrChat(users, chats);
	}
	/// <summary>Top peers disabled		<para>See <a href="https://corefork.telegram.org/constructor/contacts.topPeersDisabled"/></para></summary>
	[TLDef(0xB52C939D)]
	public sealed partial class Contacts_TopPeersDisabled : Contacts_TopPeersBase { }

	/// <summary>Birthday information of a contact.		<para>See <a href="https://corefork.telegram.org/constructor/contactBirthday"/></para></summary>
	[TLDef(0x1D998733)]
	public sealed partial class ContactBirthday : IObject
	{
		/// <summary>User ID.</summary>
		public long contact_id;
		/// <summary>Birthday information.</summary>
		public Birthday birthday;
	}

	/// <summary>Birthday information of our contacts.		<para>See <a href="https://corefork.telegram.org/constructor/contacts.contactBirthdays"/></para></summary>
	[TLDef(0x114FF30D)]
	public sealed partial class Contacts_ContactBirthdays : IObject
	{
		/// <summary>Birthday info</summary>
		public ContactBirthday[] contacts;
		/// <summary>User information</summary>
		public Dictionary<long, User> users;
	}
	
}