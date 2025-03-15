// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;

namespace Telegram.Bot.Types.ReplyMarkups
{
    /// <summary>This object represents an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a> that appears right next to the message it belongs to.</summary>
    public partial class InlineKeyboardMarkup : ReplyMarkup
    {
        /// <summary>Array of button rows, each represented by an Array of <see cref="InlineKeyboardButton"/> objects</summary>
        [Newtonsoft.Json.JsonProperty("inline_keyboard")]
        
        public IEnumerable<IEnumerable<InlineKeyboardButton>> InlineKeyboard { get; set; }

        /// <summary>Initializes an instance of <see cref="InlineKeyboardMarkup"/></summary>
        /// <param name="inlineKeyboard">Array of button rows, each represented by an Array of <see cref="InlineKeyboardButton"/> objects</param>
        
        public InlineKeyboardMarkup(IEnumerable<IEnumerable<InlineKeyboardButton>> inlineKeyboard) => InlineKeyboard = inlineKeyboard;

        /// <summary>Instantiates a new <see cref="InlineKeyboardMarkup"/></summary>
        
        public InlineKeyboardMarkup() => InlineKeyboard = new List<List<InlineKeyboardButton>>();
    }
}
