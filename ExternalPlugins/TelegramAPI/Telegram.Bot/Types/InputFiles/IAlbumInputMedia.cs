
// ReSharper disable once CheckNamespace

using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>A marker for input media types that can be used in sendMediaGroup method.</summary>
    [Newtonsoft.Json.JsonConverter(typeof(PolymorphicJsonConverter<IAlbumInputMedia>))]
    [CustomJsonPolymorphic("type")]
    [CustomJsonDerivedType(typeof(InputMediaDocument), "document")]
    [CustomJsonDerivedType(typeof(InputMediaAudio), "audio")]
    [CustomJsonDerivedType(typeof(InputMediaPhoto), "photo")]
    [CustomJsonDerivedType(typeof(InputMediaVideo), "video")]
    public interface IAlbumInputMedia{}
}
