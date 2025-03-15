// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types.Passport
{
    /// <summary>This object represents the data of an identity document.</summary>
    public partial class IdDocumentData : IDecryptedValue
    {
        /// <summary>Document number</summary>
        [Newtonsoft.Json.JsonProperty("document_no")]
        
        public string DocumentNo { get; set; } = default!;

        /// <summary><em>Optional.</em> Date of expiry, in DD.MM.YYYY format</summary>
        [Newtonsoft.Json.JsonProperty("expiry_date")]
        public string? ExpiryDate { get; set; }
    }
}
