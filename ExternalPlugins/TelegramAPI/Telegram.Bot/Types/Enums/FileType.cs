using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types.Enums
{
    /// <summary>Type of a <see cref="InputFile"/></summary>
    [Newtonsoft.Json.JsonConverter(typeof(EnumConverter<FileType>))]
    public enum FileType
    {
        /// <summary>FileStream</summary>
        Stream = 1,
        /// <summary>FileId</summary>
        Id,
        /// <summary>File URL</summary>
        Url,
    }
}
