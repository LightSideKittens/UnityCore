// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types.Enums
{
    /// <summary>Type of the media</summary>
    [Newtonsoft.Json.JsonConverter(typeof(EnumConverter<InputPaidMediaType>))]
    public enum InputPaidMediaType
    {
        /// <summary>The paid media to send is a photo.<br/><br/><i>(<see cref="InputPaidMedia"/> can be cast into <see cref="InputPaidMediaPhoto"/>)</i></summary>
        Photo = 1,
        /// <summary>The paid media to send is a video.<br/><br/><i>(<see cref="InputPaidMedia"/> can be cast into <see cref="InputPaidMediaVideo"/>)</i></summary>
        Video,
    }
}
