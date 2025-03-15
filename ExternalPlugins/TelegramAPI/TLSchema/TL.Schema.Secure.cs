using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Secure <a href="https://corefork.telegram.org/passport">passport</a> file, for more info <a href="https://corefork.telegram.org/passport/encryption#inputsecurefile">see the passport docs »</a>		<para>See <a href="https://corefork.telegram.org/constructor/secureFile"/></para></summary>
    /// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/secureFileEmpty">secureFileEmpty</a></remarks>
    [TLDef(0x7D09C27E)]
    public sealed partial class SecureFile : IObject
    {
        /// <summary>ID</summary>
        public long id;
        /// <summary>Access hash</summary>
        public long access_hash;
        /// <summary>File size</summary>
        public long size;
        /// <summary>DC ID</summary>
        public int dc_id;
        /// <summary>Date of upload</summary>
        public DateTime date;
        /// <summary>File hash</summary>
        public byte[] file_hash;
        /// <summary>Secret</summary>
        public byte[] secret;
    }
    
    	/// <summary>Secure <a href="https://corefork.telegram.org/passport">passport</a> data, for more info <a href="https://corefork.telegram.org/passport/encryption#securedata">see the passport docs »</a>		<para>See <a href="https://corefork.telegram.org/constructor/secureData"/></para></summary>
	[TLDef(0x8AEABEC3)]
	public sealed partial class SecureData : IObject
	{
		/// <summary>Data</summary>
		public byte[] data;
		/// <summary>Data hash</summary>
		public byte[] data_hash;
		/// <summary>Secret</summary>
		public byte[] secret;
	}

	/// <summary>Plaintext verified <a href="https://corefork.telegram.org/passport/encryption#secureplaindata">passport data</a>.		<para>See <a href="https://corefork.telegram.org/type/SecurePlainData"/></para>		<para>Derived classes: <see cref="SecurePlainPhone"/>, <see cref="SecurePlainEmail"/></para></summary>
	public abstract partial class SecurePlainData : IObject { }
	/// <summary>Phone number to use in <a href="https://corefork.telegram.org/passport">telegram passport</a>: <a href="https://corefork.telegram.org/passport/encryption#secureplaindata">it must be verified, first »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/securePlainPhone"/></para></summary>
	[TLDef(0x7D6099DD)]
	public sealed partial class SecurePlainPhone : SecurePlainData
	{
		/// <summary>Phone number</summary>
		public string phone;
	}
	/// <summary>Email address to use in <a href="https://corefork.telegram.org/passport">telegram passport</a>: <a href="https://corefork.telegram.org/passport/encryption#secureplaindata">it must be verified, first »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/securePlainEmail"/></para></summary>
	[TLDef(0x21EC5A5F)]
	public sealed partial class SecurePlainEmail : SecurePlainData
	{
		/// <summary>Email address</summary>
		public string email;
	}

	/// <summary>Secure value type		<para>See <a href="https://corefork.telegram.org/type/SecureValueType"/></para></summary>
	public enum SecureValueType : uint
	{
		///<summary>Personal details</summary>
		PersonalDetails = 0x9D2A81E3,
		///<summary>Passport</summary>
		Passport = 0x3DAC6A00,
		///<summary>Driver's license</summary>
		DriverLicense = 0x06E425C4,
		///<summary>Identity card</summary>
		IdentityCard = 0xA0D0744B,
		///<summary>Internal <a href="https://corefork.telegram.org/passport">passport</a></summary>
		InternalPassport = 0x99A48F23,
		///<summary>Address</summary>
		Address = 0xCBE31E26,
		///<summary>Utility bill</summary>
		UtilityBill = 0xFC36954E,
		///<summary>Bank statement</summary>
		BankStatement = 0x89137C0D,
		///<summary>Rental agreement</summary>
		RentalAgreement = 0x8B883488,
		///<summary>Internal registration <a href="https://corefork.telegram.org/passport">passport</a></summary>
		PassportRegistration = 0x99E3806A,
		///<summary>Temporary registration</summary>
		TemporaryRegistration = 0xEA02EC33,
		///<summary>Phone</summary>
		Phone = 0xB320AADB,
		///<summary>Email</summary>
		Email = 0x8E3CA7EE,
	}

	/// <summary>Secure value		<para>See <a href="https://corefork.telegram.org/constructor/secureValue"/></para></summary>
	[TLDef(0x187FA0CA)]
	public sealed partial class SecureValue : IObject
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Secure <a href="https://corefork.telegram.org/passport">passport</a> value type</summary>
		public SecureValueType type;
		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">Telegram Passport</a> element data</summary>
		[IfFlag(0)] public SecureData data;
		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">passport</a> file with the front side of the document</summary>
		[IfFlag(1)] public SecureFile front_side;
		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">passport</a> file with the reverse side of the document</summary>
		[IfFlag(2)] public SecureFile reverse_side;
		/// <summary>Encrypted <a href="https://corefork.telegram.org/passport">passport</a> file with a selfie of the user holding the document</summary>
		[IfFlag(3)] public SecureFile selfie;
		/// <summary>Array of encrypted <a href="https://corefork.telegram.org/passport">passport</a> files with translated versions of the provided documents</summary>
		[IfFlag(6)] public SecureFile[] translation;
		/// <summary>Array of encrypted <a href="https://corefork.telegram.org/passport">passport</a> files with photos the of the documents</summary>
		[IfFlag(4)] public SecureFile[] files;
		/// <summary>Plaintext verified <a href="https://corefork.telegram.org/passport">passport</a> data</summary>
		[IfFlag(5)] public SecurePlainData plain_data;
		/// <summary>Data hash</summary>
		public byte[] hash;

		[Flags] public enum Flags : uint
		{
			/// <summary>Field <see cref="data"/> has a value</summary>
			has_data = 0x1,
			/// <summary>Field <see cref="front_side"/> has a value</summary>
			has_front_side = 0x2,
			/// <summary>Field <see cref="reverse_side"/> has a value</summary>
			has_reverse_side = 0x4,
			/// <summary>Field <see cref="selfie"/> has a value</summary>
			has_selfie = 0x8,
			/// <summary>Field <see cref="files"/> has a value</summary>
			has_files = 0x10,
			/// <summary>Field <see cref="plain_data"/> has a value</summary>
			has_plain_data = 0x20,
			/// <summary>Field <see cref="translation"/> has a value</summary>
			has_translation = 0x40,
		}
	}


	/// <summary>Secure value hash		<para>See <a href="https://corefork.telegram.org/constructor/secureValueHash"/></para></summary>
	[TLDef(0xED1ECDB0)]
	public sealed partial class SecureValueHash : IObject
	{
		/// <summary>Secure value type</summary>
		public SecureValueType type;
		/// <summary>Hash</summary>
		public byte[] hash;
	}

	/// <summary>Secure value error		<para>See <a href="https://corefork.telegram.org/type/SecureValueError"/></para>		<para>Derived classes: <see cref="SecureValueErrorData"/>, <see cref="SecureValueErrorFrontSide"/>, <see cref="SecureValueErrorReverseSide"/>, <see cref="SecureValueErrorSelfie"/>, <see cref="SecureValueErrorFile"/>, <see cref="SecureValueErrorFiles"/>, <see cref="SecureValueError"/>, <see cref="SecureValueErrorTranslationFile"/>, <see cref="SecureValueErrorTranslationFiles"/></para></summary>
	public abstract partial class SecureValueErrorBase : IObject
	{
		/// <summary>The section of the user's Telegram Passport which has the error, one of <see cref="SecureValueType.PersonalDetails"/>, <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/>, <see cref="SecureValueType.Address"/></summary>
		public virtual SecureValueType Type => default;
		/// <summary>Error message</summary>
		public virtual string Text => default;
	}
	/// <summary>Represents an issue in one of the data fields that was provided by the user. The error is considered resolved when the field's value changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorData"/></para></summary>
	[TLDef(0xE8A40BD9)]
	public sealed partial class SecureValueErrorData : SecureValueErrorBase
	{
		/// <summary>The section of the user's Telegram Passport which has the error, one of <see cref="SecureValueType.PersonalDetails"/>, <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/>, <see cref="SecureValueType.Address"/></summary>
		public SecureValueType type;
		/// <summary>Data hash</summary>
		public byte[] data_hash;
		/// <summary>Name of the data field which has the error</summary>
		public string field;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>The section of the user's Telegram Passport which has the error, one of <see cref="SecureValueType.PersonalDetails"/>, <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/>, <see cref="SecureValueType.Address"/></summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Represents an issue with the front side of a document. The error is considered resolved when the file with the front side of the document changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorFrontSide"/></para></summary>
	[TLDef(0x00BE3DFA)]
	public sealed partial class SecureValueErrorFrontSide : SecureValueErrorBase
	{
		/// <summary>One of <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/></summary>
		public SecureValueType type;
		/// <summary>File hash</summary>
		public byte[] file_hash;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>One of <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/></summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Represents an issue with the reverse side of a document. The error is considered resolved when the file with reverse side of the document changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorReverseSide"/></para></summary>
	[TLDef(0x868A2AA5)]
	public sealed partial class SecureValueErrorReverseSide : SecureValueErrorBase
	{
		/// <summary>One of <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/></summary>
		public SecureValueType type;
		/// <summary>File hash</summary>
		public byte[] file_hash;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>One of <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/></summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Represents an issue with the selfie with a document. The error is considered resolved when the file with the selfie changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorSelfie"/></para></summary>
	[TLDef(0xE537CED6)]
	public sealed partial class SecureValueErrorSelfie : SecureValueErrorBase
	{
		/// <summary>One of <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/></summary>
		public SecureValueType type;
		/// <summary>File hash</summary>
		public byte[] file_hash;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>One of <see cref="SecureValueType.Passport"/>, <see cref="SecureValueType.DriverLicense"/>, <see cref="SecureValueType.IdentityCard"/>, <see cref="SecureValueType.InternalPassport"/></summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Represents an issue with a document scan. The error is considered resolved when the file with the document scan changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorFile"/></para></summary>
	[TLDef(0x7A700873)]
	public partial class SecureValueErrorFile : SecureValueErrorBase
	{
		/// <summary>One of <see cref="SecureValueType.UtilityBill"/>, <see cref="SecureValueType.BankStatement"/>, <see cref="SecureValueType.RentalAgreement"/>, <see cref="SecureValueType.PassportRegistration"/>, <see cref="SecureValueType.TemporaryRegistration"/></summary>
		public SecureValueType type;
		/// <summary>File hash</summary>
		public byte[] file_hash;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>One of <see cref="SecureValueType.UtilityBill"/>, <see cref="SecureValueType.BankStatement"/>, <see cref="SecureValueType.RentalAgreement"/>, <see cref="SecureValueType.PassportRegistration"/>, <see cref="SecureValueType.TemporaryRegistration"/></summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Represents an issue with a list of scans. The error is considered resolved when the list of files containing the scans changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorFiles"/></para></summary>
	[TLDef(0x666220E9)]
	public partial class SecureValueErrorFiles : SecureValueErrorBase
	{
		/// <summary>One of <see cref="SecureValueType.UtilityBill"/>, <see cref="SecureValueType.BankStatement"/>, <see cref="SecureValueType.RentalAgreement"/>, <see cref="SecureValueType.PassportRegistration"/>, <see cref="SecureValueType.TemporaryRegistration"/></summary>
		public SecureValueType type;
		/// <summary>File hash</summary>
		public byte[][] file_hash;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>One of <see cref="SecureValueType.UtilityBill"/>, <see cref="SecureValueType.BankStatement"/>, <see cref="SecureValueType.RentalAgreement"/>, <see cref="SecureValueType.PassportRegistration"/>, <see cref="SecureValueType.TemporaryRegistration"/></summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Secure value error		<para>See <a href="https://corefork.telegram.org/constructor/secureValueError"/></para></summary>
	[TLDef(0x869D758F)]
	public sealed partial class SecureValueError : SecureValueErrorBase
	{
		/// <summary>Type of element which has the issue</summary>
		public SecureValueType type;
		/// <summary>Hash</summary>
		public byte[] hash;
		/// <summary>Error message</summary>
		public string text;

		/// <summary>Type of element which has the issue</summary>
		public override SecureValueType Type => type;
		/// <summary>Error message</summary>
		public override string Text => text;
	}
	/// <summary>Represents an issue with one of the files that constitute the translation of a document. The error is considered resolved when the file changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorTranslationFile"/></para></summary>
	[TLDef(0xA1144770)]
	public sealed partial class SecureValueErrorTranslationFile : SecureValueErrorFile
	{
	}
	/// <summary>Represents an issue with the translated version of a document. The error is considered resolved when a file with the document translation changes.		<para>See <a href="https://corefork.telegram.org/constructor/secureValueErrorTranslationFiles"/></para></summary>
	[TLDef(0x34636DD8)]
	public sealed partial class SecureValueErrorTranslationFiles : SecureValueErrorFiles
	{
	}

	/// <summary>Encrypted credentials required to decrypt <a href="https://corefork.telegram.org/passport">telegram passport</a> data.		<para>See <a href="https://corefork.telegram.org/constructor/secureCredentialsEncrypted"/></para></summary>
	[TLDef(0x33F0EA47)]
	public sealed partial class SecureCredentialsEncrypted : IObject
	{
		/// <summary>Encrypted JSON-serialized data with unique user's payload, data hashes and secrets required for EncryptedPassportElement decryption and authentication, as described in <a href="https://corefork.telegram.org/passport#decrypting-data">decrypting data »</a></summary>
		public byte[] data;
		/// <summary>Data hash for data authentication as described in <a href="https://corefork.telegram.org/passport#decrypting-data">decrypting data »</a></summary>
		public byte[] hash;
		/// <summary>Secret, encrypted with the bot's public RSA key, required for data decryption as described in <a href="https://corefork.telegram.org/passport#decrypting-data">decrypting data »</a></summary>
		public byte[] secret;
	}
	
		/// <summary>KDF algorithm to use for computing telegram <a href="https://corefork.telegram.org/passport">passport</a> hash		<para>See <a href="https://corefork.telegram.org/type/SecurePasswordKdfAlgo"/></para>		<para>Derived classes: <see cref="SecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000"/>, <see cref="SecurePasswordKdfAlgoSHA512"/></para></summary>
	/// <remarks>a <see langword="null"/> value means <a href="https://corefork.telegram.org/constructor/securePasswordKdfAlgoUnknown">securePasswordKdfAlgoUnknown</a></remarks>
	public abstract partial class SecurePasswordKdfAlgo : IObject
	{
		/// <summary>Salt</summary>
		public byte[] salt;
	}
	/// <summary>PBKDF2 with SHA512 and 100000 iterations KDF algo		<para>See <a href="https://corefork.telegram.org/constructor/securePasswordKdfAlgoPBKDF2HMACSHA512iter100000"/></para></summary>
	[TLDef(0xBBF2DDA0)]
	public sealed partial class SecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000 : SecurePasswordKdfAlgo { }
	/// <summary>SHA512 KDF algo		<para>See <a href="https://corefork.telegram.org/constructor/securePasswordKdfAlgoSHA512"/></para></summary>
	[TLDef(0x86471D92)]
	public sealed partial class SecurePasswordKdfAlgoSHA512 : SecurePasswordKdfAlgo { }

	/// <summary>Secure settings		<para>See <a href="https://corefork.telegram.org/constructor/secureSecretSettings"/></para></summary>
	[TLDef(0x1527BCAC)]
	public sealed partial class SecureSecretSettings : IObject
	{
		/// <summary>Secure KDF algo</summary>
		public SecurePasswordKdfAlgo secure_algo;
		/// <summary>Secure secret</summary>
		public byte[] secure_secret;
		/// <summary>Secret ID</summary>
		public long secure_secret_id;
	}


	/// <summary>Required secure file type		<para>See <a href="https://corefork.telegram.org/type/SecureRequiredType"/></para>		<para>Derived classes: <see cref="SecureRequiredType"/>, <see cref="SecureRequiredTypeOneOf"/></para></summary>
	public abstract partial class SecureRequiredTypeBase : IObject { }
	/// <summary>Required type		<para>See <a href="https://corefork.telegram.org/constructor/secureRequiredType"/></para></summary>
	[TLDef(0x829D99DA)]
	public sealed partial class SecureRequiredType : SecureRequiredTypeBase
	{
		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
		public Flags flags;
		/// <summary>Secure value type</summary>
		public SecureValueType type;

		[Flags] public enum Flags : uint
		{
			/// <summary>Native names</summary>
			native_names = 0x1,
			/// <summary>Is a selfie required</summary>
			selfie_required = 0x2,
			/// <summary>Is a translation required</summary>
			translation_required = 0x4,
		}
	}
	/// <summary>One of		<para>See <a href="https://corefork.telegram.org/constructor/secureRequiredTypeOneOf"/></para></summary>
	[TLDef(0x027477B4)]
	public sealed partial class SecureRequiredTypeOneOf : SecureRequiredTypeBase
	{
		/// <summary>Secure required value types</summary>
		public SecureRequiredTypeBase[] types;
	}
}