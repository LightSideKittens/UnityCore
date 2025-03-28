// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a service message about a video chat scheduled in the chat.</summary>
    public partial class VideoChatScheduled
    {
        /// <summary>Point in time when the video chat is supposed to be started by a chat administrator</summary>
        [Newtonsoft.Json.JsonProperty("start_date")]
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime StartDate { get; set; }

        /// <summary>Implicit conversion to DateTime (StartDate)</summary>
        public static implicit operator DateTime(VideoChatScheduled self) => self.StartDate;
        /// <summary>Implicit conversion from DateTime (StartDate)</summary>
        public static implicit operator VideoChatScheduled(DateTime startDate) => new() { StartDate = startDate };
    }
}
