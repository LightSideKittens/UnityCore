// GENERATED FILE - DO NOT MODIFY MANUALLY

using Telegram.Bot.Serialization;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Types
{
    /// <summary>This object describes the type of a background. Currently, it can be one of<br/><see cref="BackgroundTypeFill"/>, <see cref="BackgroundTypeWallpaper"/>, <see cref="BackgroundTypePattern"/>, <see cref="BackgroundTypeChatTheme"/></summary>
    [Newtonsoft.Json.JsonConverter(typeof(PolymorphicJsonConverter<BackgroundType>))]
    [CustomJsonPolymorphic("type")]
    [CustomJsonDerivedType(typeof(BackgroundTypeFill), "fill")]
    [CustomJsonDerivedType(typeof(BackgroundTypeWallpaper), "wallpaper")]
    [CustomJsonDerivedType(typeof(BackgroundTypePattern), "pattern")]
    [CustomJsonDerivedType(typeof(BackgroundTypeChatTheme), "chat_theme")]
    public abstract partial class BackgroundType
    {
        /// <summary>Type of the background</summary>
        
        public abstract BackgroundTypeKind Type { get; }
    }

    /// <summary>The background is automatically filled based on the selected colors.</summary>
    public partial class BackgroundTypeFill : BackgroundType
    {
        /// <summary>Type of the background, always <see cref="BackgroundTypeKind.Fill"/></summary>
        public override BackgroundTypeKind Type => BackgroundTypeKind.Fill;

        /// <summary>The background fill</summary>
        
        public BackgroundFill Fill { get; set; } = default!;

        /// <summary>Dimming of the background in dark themes, as a percentage; 0-100</summary>
        [Newtonsoft.Json.JsonProperty("dark_theme_dimming")]
        
        public int DarkThemeDimming { get; set; }
    }

    /// <summary>The background is a wallpaper in the JPEG format.</summary>
    public partial class BackgroundTypeWallpaper : BackgroundType
    {
        /// <summary>Type of the background, always <see cref="BackgroundTypeKind.Wallpaper"/></summary>
        public override BackgroundTypeKind Type => BackgroundTypeKind.Wallpaper;

        /// <summary>Document with the wallpaper</summary>
        
        public Document Document { get; set; } = default!;

        /// <summary>Dimming of the background in dark themes, as a percentage; 0-100</summary>
        [Newtonsoft.Json.JsonProperty("dark_theme_dimming")]
        
        public int DarkThemeDimming { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the wallpaper is downscaled to fit in a 450x450 square and then box-blurred with radius 12</summary>
        [Newtonsoft.Json.JsonProperty("is_blurred")]
        public bool IsBlurred { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the background moves slightly when the device is tilted</summary>
        [Newtonsoft.Json.JsonProperty("is_moving")]
        public bool IsMoving { get; set; }
    }

    /// <summary>The background is a .PNG or .TGV (gzipped subset of SVG with MIME type “application/x-tgwallpattern”) pattern to be combined with the background fill chosen by the user.</summary>
    public partial class BackgroundTypePattern : BackgroundType
    {
        /// <summary>Type of the background, always <see cref="BackgroundTypeKind.Pattern"/></summary>
        public override BackgroundTypeKind Type => BackgroundTypeKind.Pattern;

        /// <summary>Document with the pattern</summary>
        
        public Document Document { get; set; } = default!;

        /// <summary>The background fill that is combined with the pattern</summary>
        
        public BackgroundFill Fill { get; set; } = default!;

        /// <summary>Intensity of the pattern when it is shown above the filled background; 0-100</summary>
        
        public int Intensity { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the background fill must be applied only to the pattern itself. All other pixels are black in this case. For dark themes only</summary>
        [Newtonsoft.Json.JsonProperty("is_inverted")]
        public bool IsInverted { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the background moves slightly when the device is tilted</summary>
        [Newtonsoft.Json.JsonProperty("is_moving")]
        public bool IsMoving { get; set; }
    }

    /// <summary>The background is taken directly from a built-in chat theme.</summary>
    public partial class BackgroundTypeChatTheme : BackgroundType
    {
        /// <summary>Type of the background, always <see cref="BackgroundTypeKind.ChatTheme"/></summary>
        public override BackgroundTypeKind Type => BackgroundTypeKind.ChatTheme;

        /// <summary>Name of the chat theme, which is usually an emoji</summary>
        [Newtonsoft.Json.JsonProperty("theme_name")]
        
        public string ThemeName { get; set; } = default!;
    }

    /// <summary>This object represents a chat background.</summary>
    public partial class ChatBackground
    {
        /// <summary>Type of the background</summary>
        
        public BackgroundType Type { get; set; } = default!;

        /// <summary>Implicit conversion to BackgroundType (Type)</summary>
        public static implicit operator BackgroundType(ChatBackground self) => self.Type;
        /// <summary>Implicit conversion from BackgroundType (Type)</summary>
        public static implicit operator ChatBackground(BackgroundType type) => new() { Type = type };
    }
}
