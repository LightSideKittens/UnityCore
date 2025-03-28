// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Serialization;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>This object describes the way a background is filled based on the selected colors. Currently, it can be one of<br/><see cref="BackgroundFillSolid"/>, <see cref="BackgroundFillGradient"/>, <see cref="BackgroundFillFreeformGradient"/></summary>
    [Newtonsoft.Json.JsonConverter(typeof(PolymorphicJsonConverter<BackgroundFill>))]
    [CustomJsonPolymorphic("type")]
    [CustomJsonDerivedType(typeof(BackgroundFillSolid), "solid")]
    [CustomJsonDerivedType(typeof(BackgroundFillGradient), "gradient")]
    [CustomJsonDerivedType(typeof(BackgroundFillFreeformGradient), "freeform_gradient")]
    public abstract partial class BackgroundFill
    {
        /// <summary>Type of the background fill</summary>
        
        public abstract BackgroundFillType Type { get; }
    }

    /// <summary>The background is filled using the selected color.</summary>
    public partial class BackgroundFillSolid : BackgroundFill
    {
        /// <summary>Type of the background fill, always <see cref="BackgroundFillType.Solid"/></summary>
        public override BackgroundFillType Type => BackgroundFillType.Solid;

        /// <summary>The color of the background fill in the RGB24 format</summary>
        
        public int Color { get; set; }
    }

    /// <summary>The background is a gradient fill.</summary>
    public partial class BackgroundFillGradient : BackgroundFill
    {
        /// <summary>Type of the background fill, always <see cref="BackgroundFillType.Gradient"/></summary>
        public override BackgroundFillType Type => BackgroundFillType.Gradient;

        /// <summary>Top color of the gradient in the RGB24 format</summary>
        [Newtonsoft.Json.JsonProperty("top_color")]
        
        public int TopColor { get; set; }

        /// <summary>Bottom color of the gradient in the RGB24 format</summary>
        [Newtonsoft.Json.JsonProperty("bottom_color")]
        
        public int BottomColor { get; set; }

        /// <summary>Clockwise rotation angle of the background fill in degrees; 0-359</summary>
        [Newtonsoft.Json.JsonProperty("rotation_angle")]
        
        public int RotationAngle { get; set; }
    }

    /// <summary>The background is a freeform gradient that rotates after every message in the chat.</summary>
    public partial class BackgroundFillFreeformGradient : BackgroundFill
    {
        /// <summary>Type of the background fill, always <see cref="BackgroundFillType.FreeformGradient"/></summary>
        public override BackgroundFillType Type => BackgroundFillType.FreeformGradient;

        /// <summary>A list of the 3 or 4 base colors that are used to generate the freeform gradient in the RGB24 format</summary>
        
        public int[] Colors { get; set; } = default!;
    }
}
