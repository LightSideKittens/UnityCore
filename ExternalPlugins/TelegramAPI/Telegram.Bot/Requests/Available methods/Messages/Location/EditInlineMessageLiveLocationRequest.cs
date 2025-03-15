// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Requests
{
    /// <summary>Use this method to edit live location messages. A location can be edited until its <see cref="LivePeriod">LivePeriod</see> expires or editing is explicitly disabled by a call to <see cref="TelegramBotClientExtensions.StopMessageLiveLocation">StopMessageLiveLocation</see>.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class EditInlineMessageLiveLocationRequest : RequestBase<bool>, IBusinessConnectable
    {
        /// <summary>Use this method to edit live location messages. A location can be edited until its <see cref="LivePeriod">LivePeriod</see> expires or editing is explicitly disabled by a call to <see cref="TelegramBotClientExtensions.StopMessageLiveLocation">StopMessageLiveLocation</see>.<para>Returns: </para></summary>
        public EditInlineMessageLiveLocationRequest() : base("editMessageLiveLocation")
        {
        }

        /// <summary>Identifier of the inline message</summary>
        [Newtonsoft.Json.JsonProperty("inline_message_id")]
        
        public string InlineMessageId { get; set; }

        /// <summary>Latitude of new location</summary>
        
        public double Latitude { get; set; }

        /// <summary>Longitude of new location</summary>
        
        public double Longitude { get; set; }

        /// <summary>New period in seconds during which the location can be updated, starting from the message send date. If 0x7FFFFFFF is specified, then the location can be updated forever. Otherwise, the new value must not exceed the current <see cref="LivePeriod">LivePeriod</see> by more than a day, and the live location expiration date must remain within the next 90 days. If not specified, then <see cref="LivePeriod">LivePeriod</see> remains unchanged</summary>
        [Newtonsoft.Json.JsonProperty("live_period")]
        public int? LivePeriod { get; set; }

        /// <summary>The radius of uncertainty for the location, measured in meters; 0-1500</summary>
        [Newtonsoft.Json.JsonProperty("horizontal_accuracy")]
        public double? HorizontalAccuracy { get; set; }

        /// <summary>Direction in which the user is moving, in degrees. Must be between 1 and 360 if specified.</summary>
        public int? Heading { get; set; }

        /// <summary>The maximum distance for proximity alerts about approaching another chat member, in meters. Must be between 1 and 100000 if specified.</summary>
        [Newtonsoft.Json.JsonProperty("proximity_alert_radius")]
        public int? ProximityAlertRadius { get; set; }

        /// <summary>An object for a new <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }

        /// <summary>Unique identifier of the business connection on behalf of which the message to be edited was sent</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }
    }
}
