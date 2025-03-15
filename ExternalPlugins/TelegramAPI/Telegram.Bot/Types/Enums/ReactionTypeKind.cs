// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types.Enums
{
    /// <summary>Type of the reaction</summary>
    [Newtonsoft.Json.JsonConverter(typeof(EnumConverter<ReactionTypeKind>))]
    public enum ReactionTypeKind
    {
        /// <summary>The reaction is based on an emoji.<br/><br/><i>(<see cref="ReactionType"/> can be cast into <see cref="ReactionTypeEmoji"/>)</i></summary>
        Emoji = 1,
        /// <summary>The reaction is based on a custom emoji.<br/><br/><i>(<see cref="ReactionType"/> can be cast into <see cref="ReactionTypeCustomEmoji"/>)</i></summary>
        CustomEmoji,
        /// <summary>The reaction is paid.<br/><br/><i>(<see cref="ReactionType"/> can be cast into <see cref="ReactionTypePaid"/>)</i></summary>
        Paid,
    }
}
