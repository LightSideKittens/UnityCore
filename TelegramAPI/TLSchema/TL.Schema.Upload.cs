namespace TL
{
#pragma warning disable CS1574
    /// <summary>Contains info on file.		<para>See <a href="https://corefork.telegram.org/type/upload.File"/></para>		<para>Derived classes: <see cref="Upload_File"/>, <see cref="Upload_FileCdnRedirect"/></para></summary>
    public abstract partial class Upload_FileBase : IObject { }
    
    /// <summary>File content.		<para>See <a href="https://corefork.telegram.org/constructor/upload.file"/></para></summary>
    [TLDef(0x096A18D5)]
    public sealed partial class Upload_File : Upload_FileBase
    {
        /// <summary>File type</summary>
        public Storage_FileType type;
        /// <summary>Modification time</summary>
        public int mtime;
        /// <summary>Binary data, file content</summary>
        public byte[] bytes;
    }
    /// <summary>The file must be downloaded from a <a href="https://corefork.telegram.org/cdn">CDN DC</a>.		<para>See <a href="https://corefork.telegram.org/constructor/upload.fileCdnRedirect"/></para></summary>
    [TLDef(0xF18CDA44)]
    public sealed partial class Upload_FileCdnRedirect : Upload_FileBase
    {
        /// <summary><a href="https://corefork.telegram.org/cdn">CDN DC</a> ID</summary>
        public int dc_id;
        /// <summary>File token (see <a href="https://corefork.telegram.org/cdn">CDN files</a>)</summary>
        public byte[] file_token;
        /// <summary>Encryption key (see <a href="https://corefork.telegram.org/cdn">CDN files</a>)</summary>
        public byte[] encryption_key;
        /// <summary>Encryption IV (see <a href="https://corefork.telegram.org/cdn">CDN files</a>)</summary>
        public byte[] encryption_iv;
        /// <summary>File hashes (see <a href="https://corefork.telegram.org/cdn">CDN files</a>)</summary>
        public FileHash[] file_hashes;
    }
    
    /// <summary>Represents the download status of a CDN file		<para>See <a href="https://corefork.telegram.org/type/upload.CdnFile"/></para>		<para>Derived classes: <see cref="Upload_CdnFileReuploadNeeded"/>, <see cref="Upload_CdnFile"/></para></summary>
    public abstract partial class Upload_CdnFileBase : IObject { }
    /// <summary>The file was cleared from the temporary RAM cache of the <a href="https://corefork.telegram.org/cdn">CDN</a> and has to be re-uploaded.		<para>See <a href="https://corefork.telegram.org/constructor/upload.cdnFileReuploadNeeded"/></para></summary>
    [TLDef(0xEEA8E46E)]
    public sealed partial class Upload_CdnFileReuploadNeeded : Upload_CdnFileBase
    {
        /// <summary>Request token (see <a href="https://corefork.telegram.org/cdn">CDN</a>)</summary>
        public byte[] request_token;
    }
    /// <summary>Represent a chunk of a <a href="https://corefork.telegram.org/cdn">CDN</a> file.		<para>See <a href="https://corefork.telegram.org/constructor/upload.cdnFile"/></para></summary>
    [TLDef(0xA99FCA4F)]
    public sealed partial class Upload_CdnFile : Upload_CdnFileBase
    {
        /// <summary>The data</summary>
        public byte[] bytes;
    }
    
    /// <summary>Represents a chunk of an <a href="https://corefork.telegram.org/api/files">HTTP webfile</a> downloaded through telegram's secure MTProto servers		<para>See <a href="https://corefork.telegram.org/constructor/upload.webFile"/></para></summary>
    [TLDef(0x21E753BC)]
    public sealed partial class Upload_WebFile : IObject
    {
    	/// <summary>File size</summary>
    	public int size;
    	/// <summary>Mime type</summary>
    	public string mime_type;
    	/// <summary>File type</summary>
    	public Storage_FileType file_type;
    	/// <summary>Modified time</summary>
    	public int mtime;
    	/// <summary>Data</summary>
    	public byte[] bytes;
    }

}