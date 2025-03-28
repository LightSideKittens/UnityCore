// GENERATED FILE - DO NOT MODIFY MANUALLY
namespace Telegram.Bot.Types.Passport
{
    /// <summary>This object represents the data to be requested.</summary>
    public partial class PassportScope
    {
        /// <summary>List of requested elements, each type may be used only once in the entire array of PassportScopeElement objects</summary>
        
        public PassportScopeElement[] Data { get; set; } = default!;

        /// <summary>Scope version, must be <em>1</em></summary>
        
        public int V { get; set; } = 1;
    }
}
