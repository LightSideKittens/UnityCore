// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>This object describes the position on faces where a mask should be placed by default.</summary>
    public partial class MaskPosition
    {
        /// <summary>The part of the face relative to which the mask should be placed. One of <see cref="MaskPositionPoint.Forehead">Forehead</see>, <see cref="MaskPositionPoint.Eyes">Eyes</see>, <see cref="MaskPositionPoint.Mouth">Mouth</see>, or <see cref="MaskPositionPoint.Chin">Chin</see>.</summary>
        
        public MaskPositionPoint Point { get; set; }

        /// <summary>Shift by X-axis measured in widths of the mask scaled to the face size, from left to right. For example, choosing -1.0 will place mask just to the left of the default mask position.</summary>
        [Newtonsoft.Json.JsonProperty("x_shift")]
        
        public double XShift { get; set; }

        /// <summary>Shift by Y-axis measured in heights of the mask scaled to the face size, from top to bottom. For example, 1.0 will place the mask just below the default mask position.</summary>
        [Newtonsoft.Json.JsonProperty("y_shift")]
        
        public double YShift { get; set; }

        /// <summary>Mask scaling coefficient. For example, 2.0 means double size.</summary>
        
        public double Scale { get; set; }
    }
}
