using System;
using JetBrains.Annotations;
using System.IO;
using Telegram.Bot.Extensions;
using Telegram.Bot.Serialization;
using Telegram.Bot.Types.Enums;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Types
{
    /// <summary>A file to send</summary>
    [Newtonsoft.Json.JsonConverter(typeof(InputFileConverter))]
    [PublicAPI]
    public abstract class InputFile
    {
        /// <summary>Type of file to send</summary>
        public abstract FileType FileType { get; }

        /// <summary>Creates an instance of <see cref="InputFile"/> from a string containing a file's URL or file id</summary>
        /// <param name="urlOrFileId">A file's URL or a file id</param>
        /// <returns>An instance of a class that implements <see cref="InputFile"/></returns>
        public static InputFile FromString(string urlOrFileId) =>
            Uri.TryCreate(urlOrFileId, UriKind.Absolute, out var url) ? FromUri(url) : FromFileId(urlOrFileId);

        /// <summary>Creates an <see cref="InputFileStream"/> from an instance <see cref="Stream"/></summary>
        /// <param name="stream">A <see cref="Stream"/> with file data to upload</param>
        /// <param name="fileName">An optional file name. If unspecified, it may be extracted from FileStream</param>
        /// <returns>An instance of <see cref="InputFileStream"/></returns>
        public static InputFileStream FromStream(Stream stream, string? fileName = default) =>
            new(stream.ThrowIfNull(), fileName ?? Path.GetFileName((stream as FileStream)?.Name));

        /// <summary>Creates an <see cref="InputFileUrl"/> from an <see cref="Uri"/></summary>
        /// <param name="url">A URL of a file</param>
        /// <returns>An instance of <see cref="InputFileUrl"/></returns>
        public static InputFileUrl FromUri(Uri url) => new(url.ThrowIfNull());

        /// <summary>Creates an <see cref="InputFileUrl"/> from a URL passed as a <see cref="string"/></summary>
        /// <param name="url">A URL of a file</param>
        /// <returns>An instance of <see cref="InputFileUrl"/></returns>
        public static InputFileUrl FromUri(string url) => new(url.ThrowIfNull());

        /// <summary>Creates an <see cref="InputFileId"/> from a file id</summary>
        /// <param name="fileId">An ID of a file</param>
        /// <returns>An instance of <see cref="InputFileId"/></returns>
        public static InputFileId FromFileId(string fileId) => new(fileId.ThrowIfNull());

        /// <summary>Implicit operator, same as <see cref="FromStream"/></summary>
        public static implicit operator InputFile(Stream stream) => FromStream(stream);

        /// <summary>Implicit operator, same as <see cref="FromString"/></summary>
        public static implicit operator InputFile(string urlOrFileId) => FromString(urlOrFileId);

        /// <summary>Implicit operator, using <see cref="FileBase.FileId"/> property</summary>
        public static implicit operator InputFile(FileBase file) => FromFileId(file.FileId);
    }
}
