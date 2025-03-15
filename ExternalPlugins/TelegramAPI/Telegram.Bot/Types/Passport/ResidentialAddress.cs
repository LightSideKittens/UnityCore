// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types.Passport
{
    /// <summary>This object represents a residential address.</summary>
    public partial class ResidentialAddress : IDecryptedValue
    {
        /// <summary>First line for the address</summary>
        [Newtonsoft.Json.JsonProperty("street_line1")]
        
        public string StreetLine1 { get; set; } = default!;

        /// <summary><em>Optional.</em> Second line for the address</summary>
        [Newtonsoft.Json.JsonProperty("street_line2")]
        public string? StreetLine2 { get; set; }

        /// <summary>City</summary>
        
        public string City { get; set; } = default!;

        /// <summary><em>Optional.</em> State</summary>
        public string? State { get; set; }

        /// <summary>ISO 3166-1 alpha-2 country code</summary>
        [Newtonsoft.Json.JsonProperty("country_code")]
        
        public string CountryCode { get; set; } = default!;

        /// <summary>Address post code</summary>
        [Newtonsoft.Json.JsonProperty("post_code")]
        
        public string PostCode { get; set; } = default!;
    }
}
