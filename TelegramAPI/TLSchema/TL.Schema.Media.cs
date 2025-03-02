using System;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Coordinates and size of a clicable rectangular area on top of a story.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaCoordinates"/></para></summary>
    [TLDef(0xCFC9E002)]
    public sealed partial class MediaAreaCoordinates : IObject
    {
        /// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
        public Flags flags;
        /// <summary>The abscissa of the rectangle's center, as a percentage of the media width (0-100).</summary>
        public double x;
        /// <summary>The ordinate of the rectangle's center, as a percentage of the media height (0-100).</summary>
        public double y;
        /// <summary>The width of the rectangle, as a percentage of the media width (0-100).</summary>
        public double w;
        /// <summary>The height of the rectangle, as a percentage of the media height (0-100).</summary>
        public double h;
        /// <summary>Clockwise rotation angle of the rectangle, in degrees (0-360).</summary>
        public double rotation;
        /// <summary>The radius of the rectangle corner rounding, as a percentage of the media width.</summary>
        [IfFlag(0)] public double radius;

        [Flags] public enum Flags : uint
        {
            /// <summary>Field <see cref="radius"/> has a value</summary>
            has_radius = 0x1,
        }
    }
    
 	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories#media-areas">story media area »</a>		<para>See <a href="https://corefork.telegram.org/type/MediaArea"/></para>		<para>Derived classes: <see cref="MediaAreaVenue"/>, <see cref="InputMediaAreaVenue"/>, <see cref="MediaAreaGeoPoint"/>, <see cref="MediaAreaSuggestedReaction"/>, <see cref="MediaAreaChannelPost"/>, <see cref="InputMediaAreaChannelPost"/>, <see cref="MediaAreaUrl"/>, <see cref="MediaAreaWeather"/></para></summary>
 	public abstract partial class MediaArea : IObject { }
 	/// <summary>Represents a location tag attached to a <a href="https://corefork.telegram.org/api/stories">story</a>, with additional venue information.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaVenue"/></para></summary>
 	[TLDef(0xBE82DB9C)]
 	public sealed partial class MediaAreaVenue : MediaArea
 	{
 		/// <summary>The size and location of the media area corresponding to the location sticker on top of the story media.</summary>
 		public MediaAreaCoordinates coordinates;
 		/// <summary>Coordinates of the venue</summary>
 		public GeoPoint geo;
 		/// <summary>Venue name</summary>
 		public string title;
 		/// <summary>Address</summary>
 		public string address;
 		/// <summary>Venue provider: currently only "foursquare" needs to be supported.</summary>
 		public string provider;
 		/// <summary>Venue ID in the provider's database</summary>
 		public string venue_id;
 		/// <summary>Venue type in the provider's database</summary>
 		public string venue_type;
 	}
 	/// <summary>Represents a geolocation tag attached to a <a href="https://corefork.telegram.org/api/stories">story</a>.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaGeoPoint"/></para></summary>
 	[TLDef(0xCAD5452D)]
 	public sealed partial class MediaAreaGeoPoint : MediaArea
 	{
 		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
 		public Flags flags;
 		/// <summary>The size and position of the media area corresponding to the location sticker on top of the story media.</summary>
 		public MediaAreaCoordinates coordinates;
 		/// <summary>Coordinates of the geolocation tag.</summary>
 		public GeoPoint geo;
 		/// <summary>Optional textual representation of the address.</summary>
 		[IfFlag(0)] public GeoPointAddress address;
 
 		[Flags] public enum Flags : uint
 		{
 			/// <summary>Field <see cref="address"/> has a value</summary>
 			has_address = 0x1,
 		}
 	}
 	/// <summary>Represents a reaction bubble.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaSuggestedReaction"/></para></summary>
 	[TLDef(0x14455871)]
 	public sealed partial class MediaAreaSuggestedReaction : MediaArea
 	{
 		/// <summary>Extra bits of information, use <c>flags.HasFlag(...)</c> to test for those</summary>
 		public Flags flags;
 		/// <summary>The coordinates of the media area corresponding to the reaction button.</summary>
 		public MediaAreaCoordinates coordinates;
 		/// <summary>The reaction that should be sent when this area is clicked.</summary>
 		public Reaction reaction;
 
 		[Flags] public enum Flags : uint
 		{
 			/// <summary>Whether the reaction bubble has a dark background.</summary>
 			dark = 0x1,
 			/// <summary>Whether the reaction bubble is mirrored (see <a href="https://corefork.telegram.org/api/stories#reactions">here »</a> for more info).</summary>
 			flipped = 0x2,
 		}
 	}
 	/// <summary>Represents a channel post.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaChannelPost"/></para></summary>
 	[TLDef(0x770416AF)]
 	public sealed partial class MediaAreaChannelPost : MediaArea
 	{
 		/// <summary>The size and location of the media area corresponding to the location sticker on top of the story media.</summary>
 		public MediaAreaCoordinates coordinates;
 		/// <summary>The channel that posted the message</summary>
 		public long channel_id;
 		/// <summary>ID of the channel message</summary>
 		public int msg_id;
 	}
 	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories#urls">URL media area</a>.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaUrl"/></para></summary>
 	[TLDef(0x37381085)]
 	public sealed partial class MediaAreaUrl : MediaArea
 	{
 		/// <summary>The size and location of the media area corresponding to the URL button on top of the story media.</summary>
 		public MediaAreaCoordinates coordinates;
 		/// <summary>URL to open when clicked.</summary>
 		public string url;
 	}
 	/// <summary>Represents a <a href="https://corefork.telegram.org/api/stories#weather">weather widget »</a>.		<para>See <a href="https://corefork.telegram.org/constructor/mediaAreaWeather"/></para></summary>
 	[TLDef(0x49A6549C)]
 	public sealed partial class MediaAreaWeather : MediaArea
 	{
 		/// <summary>The size and location of the media area corresponding to the widget on top of the story media.</summary>
 		public MediaAreaCoordinates coordinates;
 		/// <summary>Weather emoji, should be rendered as an <a href="https://corefork.telegram.org/api/animated-emojis">animated emoji</a>.</summary>
 		public string emoji;
 		/// <summary>Temperature in degrees Celsius.</summary>
 		public double temperature_c;
 		/// <summary>ARGB background color.</summary>
 		public int color;
 	}
 	/// <summary><para>See <a href="https://corefork.telegram.org/constructor/mediaAreaStarGift"/></para></summary>
 	[TLDef(0x5787686D)]
 	public sealed partial class MediaAreaStarGift : MediaArea
 	{
 		public MediaAreaCoordinates coordinates;
 		public string slug;
 	}
 
   
}