using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Object contains info on an encrypted chat.		<para>See <a href="https://corefork.telegram.org/type/EncryptedChat"/></para>		<para>Derived classes: <see cref="EncryptedChatEmpty"/>, <see cref="EncryptedChatWaiting"/>, <see cref="EncryptedChatRequested"/>, <see cref="EncryptedChat"/>, <see cref="EncryptedChatDiscarded"/></para></summary>
    public abstract partial class EncryptedChatBase : IObject
    {
        /// <summary>Chat ID</summary>
        public virtual int ID => default;
        /// <summary>Checking sum depending on user ID</summary>
        public virtual long AccessHash => default;
        /// <summary>Date of chat creation</summary>
        public virtual DateTime Date => default;
        /// <summary>Chat creator ID</summary>
        public virtual long AdminId => default;
        /// <summary>ID of second chat participant</summary>
        public virtual long ParticipantId => default;
    }
    
    /// <summary>Empty constructor.		<para>See <a href="https://corefork.telegram.org/constructor/encryptedChatEmpty"/></para></summary>
	[TLDef(0xAB7EC0A0)]
	public sealed partial class EncryptedChatEmpty : EncryptedChatBase
	{
		/// <summary>Chat ID</summary>
		public int id;

		/// <summary>Chat ID</summary>
		public override int ID => id;
	}
	/// <summary>Chat waiting for approval of second participant.		<para>See <a href="https://corefork.telegram.org/constructor/encryptedChatWaiting"/></para></summary>
	[TLDef(0x66B25953)]
	public sealed partial class EncryptedChatWaiting : EncryptedChatBase
	{
		/// <summary>Chat ID</summary>
		public int id;
		/// <summary>Checking sum depending on user ID</summary>
		public long access_hash;
		/// <summary>Date of chat creation</summary>
		public DateTime date;
		/// <summary>Chat creator ID</summary>
		public long admin_id;
		/// <summary>ID of second chat participant</summary>
		public long participant_id;

		/// <summary>Chat ID</summary>
		public override int ID => id;
		/// <summary>Checking sum depending on user ID</summary>
		public override long AccessHash => access_hash;
		/// <summary>Date of chat creation</summary>
		public override DateTime Date => date;
		/// <summary>Chat creator ID</summary>
		public override long AdminId => admin_id;
		/// <summary>ID of second chat participant</summary>
		public override long ParticipantId => participant_id;
	}
	/// <summary>Request to create an encrypted chat.		<para>See <a href="https://corefork.telegram.org/constructor/encryptedChatRequested"/></para></summary>
	[TLDef(0x48F1D94C)]
	public sealed partial class EncryptedChatRequested : EncryptedChatBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary><a href="https://corefork.telegram.org/api/folders#peer-folders">Peer folder ID, for more info click here</a></summary>
		[IfFlag(0)] public int folder_id;
		/// <summary>Chat ID</summary>
		public int id;
		/// <summary>Check sum depending on user ID</summary>
		public long access_hash;
		/// <summary>Chat creation date</summary>
		public DateTime date;
		/// <summary>Chat creator ID</summary>
		public long admin_id;
		/// <summary>ID of second chat participant</summary>
		public long participant_id;
		/// <summary><c>A = g ^ a mod p</c>, see <a href="https://en.wikipedia.org/wiki/Diffie%E2%80%93Hellman_key_exchange">Wikipedia</a></summary>
		public byte[] g_a;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="folder_id"/> has a value</summary>
			has_folder_id = 0x1,
		}

		/// <summary>Chat ID</summary>
		public override int ID => id;
		/// <summary>Check sum depending on user ID</summary>
		public override long AccessHash => access_hash;
		/// <summary>Chat creation date</summary>
		public override DateTime Date => date;
		/// <summary>Chat creator ID</summary>
		public override long AdminId => admin_id;
		/// <summary>ID of second chat participant</summary>
		public override long ParticipantId => participant_id;
	}
	/// <summary>Encrypted chat		<para>See <a href="https://corefork.telegram.org/constructor/encryptedChat"/></para></summary>
	[TLDef(0x61F0D4C7)]
	public sealed partial class EncryptedChat : EncryptedChatBase
	{
		/// <summary>Chat ID</summary>
		public int id;
		/// <summary>Check sum dependent on the user ID</summary>
		public long access_hash;
		/// <summary>Date chat was created</summary>
		public DateTime date;
		/// <summary>Chat creator ID</summary>
		public long admin_id;
		/// <summary>ID of the second chat participant</summary>
		public long participant_id;
		/// <summary><c>B = g ^ b mod p</c>, if the currently authorized user is the chat's creator,<br/>or <c>A = g ^ a mod p</c> otherwise<br/>See <a href="https://en.wikipedia.org/wiki/Diffie%E2%80%93Hellman_key_exchange">Wikipedia</a> for more info</summary>
		public byte[] g_a_or_b;
		/// <summary>64-bit fingerprint of received key</summary>
		public long key_fingerprint;

		/// <summary>Chat ID</summary>
		public override int ID => id;
		/// <summary>Check sum dependent on the user ID</summary>
		public override long AccessHash => access_hash;
		/// <summary>Date chat was created</summary>
		public override DateTime Date => date;
		/// <summary>Chat creator ID</summary>
		public override long AdminId => admin_id;
		/// <summary>ID of the second chat participant</summary>
		public override long ParticipantId => participant_id;
	}
	/// <summary>Discarded or deleted chat.		<para>See <a href="https://corefork.telegram.org/constructor/encryptedChatDiscarded"/></para></summary>
	[TLDef(0x1E1C7C45)]
	public sealed partial class EncryptedChatDiscarded : EncryptedChatBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Chat ID</summary>
		public int id;

		[Flags] public enum Flags : uint
		{
			/// <summary>Whether both users of this secret chat should also remove all of its messages</summary>
			history_deleted = 0x1,
		}

		/// <summary>Chat ID</summary>
		public override int ID => id;
	}

	/// <summary>Creates an encrypted chat.		<para>See <a href="https://corefork.telegram.org/constructor/inputEncryptedChat"/></para></summary>
	[TLDef(0xF141B5E1)]
	public sealed partial class InputEncryptedChat : IObject
	{
		/// <summary>Chat ID</summary>
		public int chat_id;
		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Checking sum from constructor <see cref="EncryptedChat"/>, <see cref="EncryptedChatWaiting"/> or <see cref="EncryptedChatRequested"/></summary>
		public long access_hash;
	}

	/// <summary>Encrypted file.		<para>See <a href="https://corefork.telegram.org/constructor/encryptedFile"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/encryptedFileEmpty">encryptedFileEmpty</a></remarks>
	[TLDef(0xA8008CD8)]
	public sealed partial class EncryptedFile : IObject
	{
		/// <summary>File ID</summary>
		public long id;
		/// <summary>Checking sum depending on user ID</summary>
		public long access_hash;
		/// <summary>File size in bytes</summary>
		public long size;
		/// <summary>Number of data center</summary>
		public int dc_id;
		/// <summary>32-bit fingerprint of key used for file encryption</summary>
		public int key_fingerprint;
	}

	/// <summary>Object sets encrypted file for attachment		<para>See <a href="https://corefork.telegram.org/type/InputEncryptedFile"/></para>		<para>Derived classes: <see cref="InputEncryptedFileUploaded"/>, <see cref="InputEncryptedFile"/>, <see cref="InputEncryptedFileBigUploaded"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/inputEncryptedFileEmpty">inputEncryptedFileEmpty</a></remarks>
	public abstract partial class InputEncryptedFileBase : IObject
	{
		/// <summary>Random file ID created by client</summary>
		public abstract long ID { get; set; }
	}
	/// <summary>Sets new encrypted file saved by parts using upload.saveFilePart method.		<para>See <a href="https://corefork.telegram.org/constructor/inputEncryptedFileUploaded"/></para></summary>
	[TLDef(0x64BD0306)]
	public sealed partial class InputEncryptedFileUploaded : InputEncryptedFileBase
	{
		/// <summary>Random file ID created by client</summary>
		public long id;
		/// <summary>Number of saved parts</summary>
		public int parts;
		/// <summary>In case <a href="https://en.wikipedia.org/wiki/MD5">md5-HASH</a> of the (already encrypted) file was transmitted, file content will be checked prior to use</summary>
		public string md5_checksum;
		/// <summary>32-bit fingerprint of the key used to encrypt a file</summary>
		public int key_fingerprint;

		/// <summary>Random file ID created by client</summary>
		public override long ID { get => id; set => id = value; }
	}
	/// <summary>Sets forwarded encrypted file for attachment.		<para>See <a href="https://corefork.telegram.org/constructor/inputEncryptedFile"/></para></summary>
	[TLDef(0x5A17B5E5)]
	public sealed partial class InputEncryptedFile : InputEncryptedFileBase
	{
		/// <summary>File ID, value of <strong>id</strong> parameter from <see cref="EncryptedFile"/></summary>
		public long id;
		/// <summary>⚠ <b>REQUIRED FIELD</b>. See <see href="https://wiz0u.github.io/WTelegramClient/FAQ#access-hash">how to obtain it</see><br/>Checking sum, value of <strong>access_hash</strong> parameter from <see cref="EncryptedFile"/></summary>
		public long access_hash;

		/// <summary>File ID, value of <strong>id</strong> parameter from <see cref="EncryptedFile"/></summary>
		public override long ID { get => id; set => id = value; }
	}
	/// <summary>Assigns a new big encrypted file (over 10 MB in size), saved in parts using the method <see cref="SchemaExtensions.Upload_SaveBigFilePart">Upload_SaveBigFilePart</see>.		<para>See <a href="https://corefork.telegram.org/constructor/inputEncryptedFileBigUploaded"/></para></summary>
	[TLDef(0x2DC173C8)]
	public sealed partial class InputEncryptedFileBigUploaded : InputEncryptedFileBase
	{
		/// <summary>Random file id, created by the client</summary>
		public long id;
		/// <summary>Number of saved parts</summary>
		public int parts;
		/// <summary>32-bit imprint of the key used to encrypt the file</summary>
		public int key_fingerprint;

		/// <summary>Random file id, created by the client</summary>
		public override long ID { get => id; set => id = value; }
	}

	/// <summary>Object contains encrypted message.		<para>See <a href="https://corefork.telegram.org/type/EncryptedMessage"/></para>		<para>Derived classes: <see cref="EncryptedMessage"/>, <see cref="EncryptedMessageService"/></para></summary>
	public abstract partial class EncryptedMessageBase : IObject
	{
		/// <summary>Random message ID, assigned by the author of message</summary>
		public virtual long RandomId => default;
		/// <summary>ID of encrypted chat</summary>
		public virtual int ChatId => default;
		/// <summary>Date of sending</summary>
		public virtual DateTime Date => default;
		/// <summary>TL-serialization of <see cref="DecryptedMessageBase"/> type, encrypted with the key created at chat initialization</summary>
		public virtual byte[] Bytes => default;
	}
	/// <summary>Encrypted message.		<para>See <a href="https://corefork.telegram.org/constructor/encryptedMessage"/></para></summary>
	[TLDef(0xED18C118)]
	public sealed partial class EncryptedMessage : EncryptedMessageBase
	{
		/// <summary>Random message ID, assigned by the author of message</summary>
		public long random_id;
		/// <summary>ID of encrypted chat</summary>
		public int chat_id;
		/// <summary>Date of sending</summary>
		public DateTime date;
		/// <summary>TL-serialization of <see cref="DecryptedMessageBase"/> type, encrypted with the key created at chat initialization</summary>
		public byte[] bytes;
		/// <summary>Attached encrypted file</summary>
		public EncryptedFile file;

		/// <summary>Random message ID, assigned by the author of message</summary>
		public override long RandomId => random_id;
		/// <summary>ID of encrypted chat</summary>
		public override int ChatId => chat_id;
		/// <summary>Date of sending</summary>
		public override DateTime Date => date;
		/// <summary>TL-serialization of <see cref="DecryptedMessageBase"/> type, encrypted with the key created at chat initialization</summary>
		public override byte[] Bytes => bytes;
	}
	/// <summary>Encrypted service message		<para>See <a href="https://corefork.telegram.org/constructor/encryptedMessageService"/></para></summary>
	[TLDef(0x23734B06)]
	public sealed partial class EncryptedMessageService : EncryptedMessageBase
	{
		/// <summary>Random message ID, assigned by the author of message</summary>
		public long random_id;
		/// <summary>ID of encrypted chat</summary>
		public int chat_id;
		/// <summary>Date of sending</summary>
		public DateTime date;
		/// <summary>TL-serialization of the <see cref="DecryptedMessageBase"/> type, encrypted with the key created at chat initialization</summary>
		public byte[] bytes;

		/// <summary>Random message ID, assigned by the author of message</summary>
		public override long RandomId => random_id;
		/// <summary>ID of encrypted chat</summary>
		public override int ChatId => chat_id;
		/// <summary>Date of sending</summary>
		public override DateTime Date => date;
		/// <summary>TL-serialization of the <see cref="DecryptedMessageBase"/> type, encrypted with the key created at chat initialization</summary>
		public override byte[] Bytes => bytes;
	}
	
	/// <summary>Message without file attachments sent to an encrypted file.		<para>See <a href="https://corefork.telegram.org/constructor/messages.sentEncryptedMessage"/></para></summary>
	[TLDef(0x560F8935)]
	public partial class Messages_SentEncryptedMessage : IObject
	{
		/// <summary>Date of sending</summary>
		public DateTime date;
	}
	/// <summary>Message with a file enclosure sent to a protected chat		<para>See <a href="https://corefork.telegram.org/constructor/messages.sentEncryptedFile"/></para></summary>
	[TLDef(0x9493FF32, inheritBefore = true)]
	public sealed partial class Messages_SentEncryptedFile : Messages_SentEncryptedMessage
	{
		/// <summary>Attached file</summary>
		public EncryptedFile file;
	}
}