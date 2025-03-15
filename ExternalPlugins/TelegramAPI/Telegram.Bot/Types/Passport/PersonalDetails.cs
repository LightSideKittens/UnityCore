// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types.Passport
{
    /// <summary>This object represents personal details.</summary>
    public partial class PersonalDetails : IDecryptedValue
    {
        /// <summary>First Name</summary>
        [Newtonsoft.Json.JsonProperty("first_name")]
        
        public string FirstName { get; set; } = default!;

        /// <summary>Last Name</summary>
        [Newtonsoft.Json.JsonProperty("last_name")]
        
        public string LastName { get; set; } = default!;

        /// <summary><em>Optional.</em> Middle Name</summary>
        [Newtonsoft.Json.JsonProperty("middle_name")]
        public string? MiddleName { get; set; }

        /// <summary>Date of birth in DD.MM.YYYY format</summary>
        [Newtonsoft.Json.JsonProperty("birth_date")]
        
        public string BirthDate { get; set; } = default!;

        /// <summary>Gender, <em>male</em> or <em>female</em></summary>
        
        public string Gender { get; set; } = default!;

        /// <summary>Citizenship (ISO 3166-1 alpha-2 country code)</summary>
        [Newtonsoft.Json.JsonProperty("country_code")]
        
        public string CountryCode { get; set; } = default!;

        /// <summary>Country of residence (ISO 3166-1 alpha-2 country code)</summary>
        [Newtonsoft.Json.JsonProperty("residence_country_code")]
        
        public string ResidenceCountryCode { get; set; } = default!;

        /// <summary>First Name in the language of the user's country of residence</summary>
        [Newtonsoft.Json.JsonProperty("first_name_native")]
        
        public string FirstNameNative { get; set; } = default!;

        /// <summary>Last Name in the language of the user's country of residence</summary>
        [Newtonsoft.Json.JsonProperty("last_name_native")]
        
        public string LastNameNative { get; set; } = default!;

        /// <summary><em>Optional.</em> Middle Name in the language of the user's country of residence</summary>
        [Newtonsoft.Json.JsonProperty("middle_name_native")]
        public string? MiddleNameNative { get; set; }
    }
}
