// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types.Passport;

namespace Telegram.Bot.Requests
{
    /// <summary>Informs a user that some of the Telegram Passport elements they provided contains errors. The user will not be able to re-submit their Passport to you until the errors are fixed (the contents of the field for which you returned the error must change).<br/>Use this if the data submitted by the user doesn't satisfy the standards your service requires for any reason. For example, if a birthday date seems invalid, a submitted document is blurry, a scan shows evidence of tampering, etc. Supply some details in the error message to make sure the user knows how to correct the issues.<para>Returns: </para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class SetPassportDataErrorsRequest : RequestBase<bool>, IUserTargetable
    {
        /// <summary>Informs a user that some of the Telegram Passport elements they provided contains errors. The user will not be able to re-submit their Passport to you until the errors are fixed (the contents of the field for which you returned the error must change).<br/>Use this if the data submitted by the user doesn't satisfy the standards your service requires for any reason. For example, if a birthday date seems invalid, a submitted document is blurry, a scan shows evidence of tampering, etc. Supply some details in the error message to make sure the user knows how to correct the issues.<para>Returns: </para></summary>
        public SetPassportDataErrorsRequest() : base("setPassportDataErrors")
        {
        }

        /// <summary>User identifier</summary>
        [Newtonsoft.Json.JsonProperty("user_id")]
        
        public long UserId { get; set; }

        /// <summary>A array describing the errors</summary>
        
        public IEnumerable<PassportElementError> Errors { get; set; }
    }
}
