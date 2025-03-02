using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace WTelegram
{
	internal sealed partial class Session : IDisposable
	{
		public int ApiId;
		public long UserId;
		public int MainDC;
		public Dictionary<int, DCSession> DCSessions = new Dictionary<int, DCSession>();
		public TL.DcOption[] DcOptions;

		public sealed class DCSession
		{
			public long Id;
			public long AuthKeyID;
			public byte[] AuthKey;
			public long UserId;
			public long OldSalt;
			public long Salt;
			public SortedList<DateTime, long> Salts;
			public int Seqno;
			public long ServerTicksOffset;
			public long LastSentMsgId;
			public TL.DcOption DataCenter;
			public bool WithoutUpdates;
			public int Layer;

			internal Client Client;
			internal int DcID => DataCenter?.id ?? 0;
			internal IPEndPoint EndPoint => DataCenter == null ? null : new(IPAddress.Parse(DataCenter.ip_address), DataCenter.port);
			internal void Renew() { Helpers.Log(3, $"Renewing session on DC {DcID}..."); Id = Helpers.RandomLong(); Seqno = 0; LastSentMsgId = 0; }
			public void DisableUpdates(bool disable = true) { if (WithoutUpdates != disable) { WithoutUpdates = disable; Renew(); } }

			const int MsgIdsN = 512;
			private long[] _msgIds;
			private int _msgIdsHead;
			internal bool CheckNewMsgId(long msg_id)
			{
				if (_msgIds == null)
				{
					_msgIds = new long[MsgIdsN];
					_msgIds[0] = msg_id;
					msg_id -= 300L << 32;
					for (int i = 1; i < MsgIdsN; i++) _msgIds[i] = msg_id;
					return true;
				}
				int newHead = (_msgIdsHead + 1) % MsgIdsN;
				if (msg_id > _msgIds[_msgIdsHead])
					_msgIds[_msgIdsHead = newHead] = msg_id;
				else if (msg_id <= _msgIds[newHead])
					return false;
				else
				{
					int min = 0, max = MsgIdsN - 1;
					while (min <= max)
					{
						int mid = (min + max) / 2;
						int sign = msg_id.CompareTo(_msgIds[(mid + newHead) % MsgIdsN]);
						if (sign == 0) return false;
						else if (sign < 0) max = mid - 1;
						else min = mid + 1;
					}
					_msgIdsHead = newHead;
					for (min = (min + newHead) % MsgIdsN; newHead != min;)
						_msgIds[newHead] = _msgIds[newHead = newHead == 0 ? MsgIdsN - 1 : newHead - 1];
					_msgIds[min] = msg_id;
				}
				return true;
			}
		}

		public DateTime SessionStart => _sessionStart;
		private readonly DateTime _sessionStart = DateTime.UtcNow;
		private readonly SHA256 _sha256 = SHA256.Create();
		private Stream _store;
		private byte[] _reuseKey;
		private byte[] _encrypted = new byte[16];
		private ICryptoTransform _encryptor;
		// Removed Utf8JsonWriter; using Newtonsoft.Json instead.
		private readonly MemoryStream _jsonStream = new MemoryStream(4096);

		public void Dispose()
		{
			_sha256.Dispose();
			_store.Dispose();
			_encryptor.Dispose();
			_jsonStream.Dispose();
		}

		internal static Session LoadOrCreate(Stream store, byte[] rgbKey)
		{
			using var aes = Aes.Create();
			Session session = null;
			try
			{
				var length = (int)store.Length;
				if (length > 0)
				{
					var input = new byte[length];
					if (store.Read(input, 0, length) != length)
						throw new WTException($"Can't read session block ({store.Position}, {length})");
					using var sha256 = SHA256.Create();
					using var decryptor = aes.CreateDecryptor(rgbKey, input.Take(16).ToArray());
					var utf8Json = decryptor.TransformFinalBlock(input, 16, input.Length - 16);
					if (!sha256.ComputeHash(utf8Json, 32, utf8Json.Length - 32).SequenceEqual(utf8Json.Take(32)))
						throw new WTException("Integrity check failed in session loading");
					var utf8JsonString = Encoding.UTF8.GetString(utf8Json, 32, utf8Json.Length - 32);
					session = JsonConvert.DeserializeObject<Session>(utf8JsonString, Helpers.JsonOptions);
					Helpers.Log(2, "Loaded previous session");
				}
				session ??= new Session();
				session._store = store;
				Encryption.RNG.GetBytes(session._encrypted, 0, 16);
				session._encryptor = aes.CreateEncryptor(rgbKey, session._encrypted);
				if (!session._encryptor.CanReuseTransform) session._reuseKey = rgbKey;
				return session;
			}
			catch (Exception ex)
			{
				store.Dispose();
				throw new WTException($"Exception while reading session file: {ex.Message}\nUse the correct api_hash/id/key, or delete the file to start a new session", ex);
			}
		}

		internal void Save() // must be called with lock(session)
		{
			var utf8JsonString = JsonConvert.SerializeObject(this, Helpers.JsonOptions);
			var utf8Json = Encoding.UTF8.GetBytes(utf8JsonString);
			var utf8JsonLen = utf8Json.Length;
			int encryptedLen = 64 + (utf8JsonLen & ~15);
			lock (_store)
			{
				if (encryptedLen > _encrypted.Length)
					Array.Copy(_encrypted, _encrypted = new byte[encryptedLen + 256], 16);
				_encryptor.TransformBlock(_sha256.ComputeHash(utf8Json, 0, utf8JsonLen), 0, 32, _encrypted, 16);
				_encryptor.TransformBlock(utf8Json, 0, encryptedLen - 64, _encrypted, 48);
				_encryptor.TransformFinalBlock(utf8Json, encryptedLen - 64, utf8JsonLen & 15).CopyTo(_encrypted, encryptedLen - 16);
				if (!_encryptor.CanReuseTransform)
					using (var aes = Aes.Create())
						_encryptor = aes.CreateEncryptor(_reuseKey, _encrypted.Take(16).ToArray());
				try
				{
					_store.Position = 0;
					_store.Write(_encrypted, 0, encryptedLen);
					_store.SetLength(encryptedLen);
				}
				catch (Exception ex)
				{
					Helpers.Log(4, $"{_store} raised {ex}");
				}
			}
			_jsonStream.Position = 0;
			_jsonStream.SetLength(0);
		}
	}

	internal sealed class SessionStore : FileStream
	{
		public override long Length { get; }
		public override long Position { get => base.Position; set { } }
		public override void SetLength(long value) { }
		private readonly byte[] _header = new byte[8];
		private int _nextPosition = 8;

		public SessionStore(string pathname)
			: base(pathname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1)
		{
			if (base.Read(_header, 0, 8) == 8)
			{
				var position = BinaryPrimitives.ReadInt32LittleEndian(_header);
				var length = BinaryPrimitives.ReadInt32LittleEndian(_header.AsSpan(4));
				base.Position = position;
				Length = length;
				_nextPosition = position + length;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_nextPosition > count * 3) _nextPosition = 8;
			base.Position = _nextPosition;
			base.Write(buffer, offset, count);
			BinaryPrimitives.WriteInt32LittleEndian(_header, _nextPosition);
			BinaryPrimitives.WriteInt32LittleEndian(_header.AsSpan(4), count);
			_nextPosition += count;
			base.Position = 0;
			base.Write(_header, 0, 8);
		}
	}

	internal sealed class ActionStore : MemoryStream
	{
		private readonly Action<byte[]> save;

		public ActionStore(byte[] initial, Action<byte[]> save)
			: base(initial ?? Array.Empty<byte>())
		{
			this.save = save;
		}

		public override void Write(byte[] buffer, int offset, int count) => save(buffer[offset..(offset + count)]);
		public override void SetLength(long value) { }
	}
}
