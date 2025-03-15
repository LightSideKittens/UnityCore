// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using Telegram.Bot.Serialization;
using Telegram.Bot.Types.Enums;
using LabeledPrice = Telegram.Bot.Types.Payments.LabeledPrice;

namespace Telegram.Bot.Types.InlineQueryResults
{
    /// <summary>This object represents the content of a message to be sent as a result of an inline query. Telegram clients currently support the following 5 types:<br/><see cref="InputTextMessageContent"/>, <see cref="InputLocationMessageContent"/>, <see cref="InputVenueMessageContent"/>, <see cref="InputContactMessageContent"/>, <see cref="InputInvoiceMessageContent"/></summary>
    [Newtonsoft.Json.JsonConverter(typeof(PolymorphicJsonConverter<InputMessageContent>))]
    [CustomJsonPolymorphic()]
    [CustomJsonDerivedType(typeof(InputTextMessageContent))]
    [CustomJsonDerivedType(typeof(InputLocationMessageContent))]
    [CustomJsonDerivedType(typeof(InputVenueMessageContent))]
    [CustomJsonDerivedType(typeof(InputContactMessageContent))]
    [CustomJsonDerivedType(typeof(InputInvoiceMessageContent))]
    public abstract partial class InputMessageContent{}

    /// <summary>Represents the <see cref="InputMessageContent">content</see> of a text message to be sent as the result of an inline query.</summary>
    public partial class InputTextMessageContent : InputMessageContent
    {
        /// <summary>Text of the message to be sent, 1-4096 characters</summary>
        [Newtonsoft.Json.JsonProperty("message_text")]
        
        public string MessageText { get; set; }

        /// <summary><em>Optional</em>. Mode for parsing entities in the message text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</summary>
        [Newtonsoft.Json.JsonProperty("parse_mode")]
        public ParseMode ParseMode { get; set; }

        /// <summary><em>Optional</em>. List of special entities that appear in message text, which can be specified instead of <see cref="ParseMode">ParseMode</see></summary>
        public MessageEntity[]? Entities { get; set; }

        /// <summary><em>Optional</em>. Link preview generation options for the message</summary>
        [Newtonsoft.Json.JsonProperty("link_preview_options")]
        public LinkPreviewOptions? LinkPreviewOptions { get; set; }

        /// <summary>Initializes an instance of <see cref="InputTextMessageContent"/></summary>
        /// <param name="messageText">Text of the message to be sent, 1-4096 characters</param>
        
        public InputTextMessageContent(string messageText) => MessageText = messageText;

        /// <summary>Instantiates a new <see cref="InputTextMessageContent"/></summary>
        public InputTextMessageContent() { }
    }

    /// <summary>Represents the <see cref="InputMessageContent">content</see> of a location message to be sent as the result of an inline query.</summary>
    public partial class InputLocationMessageContent : InputMessageContent
    {
        /// <summary>Latitude of the location in degrees</summary>
        
        public double Latitude { get; set; }

        /// <summary>Longitude of the location in degrees</summary>
        
        public double Longitude { get; set; }

        /// <summary><em>Optional</em>. The radius of uncertainty for the location, measured in meters; 0-1500</summary>
        [Newtonsoft.Json.JsonProperty("horizontal_accuracy")]
        public double? HorizontalAccuracy { get; set; }

        /// <summary><em>Optional</em>. Period in seconds during which the location can be updated, should be between 60 and 86400, or 0x7FFFFFFF for live locations that can be edited indefinitely.</summary>
        [Newtonsoft.Json.JsonProperty("live_period")]
        public int? LivePeriod { get; set; }

        /// <summary><em>Optional</em>. For live locations, a direction in which the user is moving, in degrees. Must be between 1 and 360 if specified.</summary>
        public int? Heading { get; set; }

        /// <summary><em>Optional</em>. For live locations, a maximum distance for proximity alerts about approaching another chat member, in meters. Must be between 1 and 100000 if specified.</summary>
        [Newtonsoft.Json.JsonProperty("proximity_alert_radius")]
        public int? ProximityAlertRadius { get; set; }

        /// <summary>Initializes an instance of <see cref="InputLocationMessageContent"/></summary>
        /// <param name="latitude">Latitude of the location in degrees</param>
        /// <param name="longitude">Longitude of the location in degrees</param>
        
        public InputLocationMessageContent(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>Instantiates a new <see cref="InputLocationMessageContent"/></summary>
        public InputLocationMessageContent() { }
    }

    /// <summary>Represents the <see cref="InputMessageContent">content</see> of a venue message to be sent as the result of an inline query.</summary>
    public partial class InputVenueMessageContent : InputMessageContent
    {
        /// <summary>Latitude of the venue in degrees</summary>
        
        public double Latitude { get; set; }

        /// <summary>Longitude of the venue in degrees</summary>
        
        public double Longitude { get; set; }

        /// <summary>Name of the venue</summary>
        
        public string Title { get; set; }

        /// <summary>Address of the venue</summary>
        
        public string Address { get; set; }

        /// <summary><em>Optional</em>. Foursquare identifier of the venue, if known</summary>
        [Newtonsoft.Json.JsonProperty("foursquare_id")]
        public string? FoursquareId { get; set; }

        /// <summary><em>Optional</em>. Foursquare type of the venue, if known. (For example, “arts_entertainment/default”, “arts_entertainment/aquarium” or “food/icecream”.)</summary>
        [Newtonsoft.Json.JsonProperty("foursquare_type")]
        public string? FoursquareType { get; set; }

        /// <summary><em>Optional</em>. Google Places identifier of the venue</summary>
        [Newtonsoft.Json.JsonProperty("google_place_id")]
        public string? GooglePlaceId { get; set; }

        /// <summary><em>Optional</em>. Google Places type of the venue. (See <a href="https://developers.google.com/places/web-service/supported_types">supported types</a>.)</summary>
        [Newtonsoft.Json.JsonProperty("google_place_type")]
        public string? GooglePlaceType { get; set; }

        /// <summary>Initializes an instance of <see cref="InputVenueMessageContent"/></summary>
        /// <param name="latitude">Latitude of the venue in degrees</param>
        /// <param name="longitude">Longitude of the venue in degrees</param>
        /// <param name="title">Name of the venue</param>
        /// <param name="address">Address of the venue</param>
        
        public InputVenueMessageContent(double latitude, double longitude, string title, string address)
        {
            Latitude = latitude;
            Longitude = longitude;
            Title = title;
            Address = address;
        }

        /// <summary>Instantiates a new <see cref="InputVenueMessageContent"/></summary>
        public InputVenueMessageContent() { }
    }

    /// <summary>Represents the <see cref="InputMessageContent">content</see> of a contact message to be sent as the result of an inline query.</summary>
    public partial class InputContactMessageContent : InputMessageContent
    {
        /// <summary>Contact's phone number</summary>
        [Newtonsoft.Json.JsonProperty("phone_number")]
        
        public string PhoneNumber { get; set; }

        /// <summary>Contact's first name</summary>
        [Newtonsoft.Json.JsonProperty("first_name")]
        
        public string FirstName { get; set; }

        /// <summary><em>Optional</em>. Contact's last name</summary>
        [Newtonsoft.Json.JsonProperty("last_name")]
        public string? LastName { get; set; }

        /// <summary><em>Optional</em>. Additional data about the contact in the form of a <a href="https://en.wikipedia.org/wiki/VCard">vCard</a>, 0-2048 bytes</summary>
        public string? Vcard { get; set; }

        /// <summary>Initializes an instance of <see cref="InputContactMessageContent"/></summary>
        /// <param name="phoneNumber">Contact's phone number</param>
        /// <param name="firstName">Contact's first name</param>
        
        public InputContactMessageContent(string phoneNumber, string firstName)
        {
            PhoneNumber = phoneNumber;
            FirstName = firstName;
        }

        /// <summary>Instantiates a new <see cref="InputContactMessageContent"/></summary>
        public InputContactMessageContent() { }
    }

    /// <summary>Represents the <see cref="InputMessageContent">content</see> of an invoice message to be sent as the result of an inline query.</summary>
    public partial class InputInvoiceMessageContent : InputMessageContent
    {
        /// <summary>Product name, 1-32 characters</summary>
        
        public string Title { get; set; }

        /// <summary>Product description, 1-255 characters</summary>
        
        public string Description { get; set; }

        /// <summary>Bot-defined invoice payload, 1-128 bytes. This will not be displayed to the user, use it for your internal processes.</summary>
        
        public string Payload { get; set; }

        /// <summary><em>Optional</em>. Payment provider token, obtained via <a href="https://t.me/botfather">@BotFather</a>. Pass an empty string for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("provider_token")]
        public string? ProviderToken { get; set; }

        /// <summary>Three-letter ISO 4217 currency code, see <a href="https://core.telegram.org/bots/payments#supported-currencies">more on currencies</a>. Pass “XTR” for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        
        public string Currency { get; set; }

        /// <summary>Price breakdown, a list of components (e.g. product price, tax, discount, delivery cost, delivery tax, bonus, etc.). Must contain exactly one item for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        
        public IEnumerable<LabeledPrice> Prices { get; set; }

        /// <summary><em>Optional</em>. The maximum accepted amount for tips in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). For example, for a maximum tip of <c>US$ 1.45</c> pass <c><see cref="MaxTipAmount">MaxTipAmount</see> = 145</c>. See the <em>exp</em> parameter in <a href="https://core.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies). Defaults to 0. Not supported for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("max_tip_amount")]
        public int? MaxTipAmount { get; set; }

        /// <summary><em>Optional</em>. A array of suggested amounts of tip in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). At most 4 suggested tip amounts can be specified. The suggested tip amounts must be positive, passed in a strictly increased order and must not exceed <see cref="MaxTipAmount">MaxTipAmount</see>.</summary>
        [Newtonsoft.Json.JsonProperty("suggested_tip_amounts")]
        public int[]? SuggestedTipAmounts { get; set; }

        /// <summary><em>Optional</em>. A JSON-serialized object for data about the invoice, which will be shared with the payment provider. A detailed description of the required fields should be provided by the payment provider.</summary>
        [Newtonsoft.Json.JsonProperty("provider_data")]
        public string? ProviderData { get; set; }

        /// <summary><em>Optional</em>. URL of the product photo for the invoice. Can be a photo of the goods or a marketing image for a service.</summary>
        [Newtonsoft.Json.JsonProperty("photo_url")]
        public string? PhotoUrl { get; set; }

        /// <summary><em>Optional</em>. Photo size in bytes</summary>
        [Newtonsoft.Json.JsonProperty("photo_size")]
        public int? PhotoSize { get; set; }

        /// <summary><em>Optional</em>. Photo width</summary>
        [Newtonsoft.Json.JsonProperty("photo_width")]
        public int? PhotoWidth { get; set; }

        /// <summary><em>Optional</em>. Photo height</summary>
        [Newtonsoft.Json.JsonProperty("photo_height")]
        public int? PhotoHeight { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if you require the user's full name to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_name")]
        public bool NeedName { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if you require the user's phone number to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_phone_number")]
        public bool NeedPhoneNumber { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if you require the user's email address to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_email")]
        public bool NeedEmail { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if you require the user's shipping address to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("need_shipping_address")]
        public bool NeedShippingAddress { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if the user's phone number should be sent to the provider. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("send_phone_number_to_provider")]
        public bool SendPhoneNumberToProvider { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if the user's email address should be sent to the provider. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("send_email_to_provider")]
        public bool SendEmailToProvider { get; set; }

        /// <summary><em>Optional</em>. Pass <see langword="true"/> if the final price depends on the shipping method. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</summary>
        [Newtonsoft.Json.JsonProperty("is_flexible")]
        public bool IsFlexible { get; set; }

        /// <summary>Initializes an instance of <see cref="InputInvoiceMessageContent"/></summary>
        /// <param name="title">Product name, 1-32 characters</param>
        /// <param name="description">Product description, 1-255 characters</param>
        /// <param name="payload">Bot-defined invoice payload, 1-128 bytes. This will not be displayed to the user, use it for your internal processes.</param>
        /// <param name="currency">Three-letter ISO 4217 currency code, see <a href="https://core.telegram.org/bots/payments#supported-currencies">more on currencies</a>. Pass “XTR” for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
        /// <param name="prices">Price breakdown, a list of components (e.g. product price, tax, discount, delivery cost, delivery tax, bonus, etc.). Must contain exactly one item for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
        
        public InputInvoiceMessageContent(string title, string description, string payload, string currency, IEnumerable<LabeledPrice> prices)
        {
            Title = title;
            Description = description;
            Payload = payload;
            Currency = currency;
            Prices = prices;
        }

        /// <summary>Instantiates a new <see cref="InputInvoiceMessageContent"/></summary>
        public InputInvoiceMessageContent() { }
    }
}
