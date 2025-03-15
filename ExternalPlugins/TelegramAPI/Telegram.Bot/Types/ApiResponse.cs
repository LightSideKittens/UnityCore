using Telegram.Bot.Types;

namespace Telegram.Bot.Exceptions
{
    /// <summary>Represents a Bot API response</summary>
    public class ApiResponse
    {
        /// <summary>Gets a value indicating whether the request was successful.</summary>
        public bool Ok { get; set; }

        /// <summary>Gets the error message.</summary>
        public string? Description { get; set; }

        /// <summary>Gets the error code.</summary>
        public int ErrorCode { get; set; }

        /// <summary>Contains information about why a request was unsuccessful.</summary>
        public ResponseParameters? Parameters { get; set; }
    }
}

namespace Telegram.Bot.Types
{
    /// <summary>Represents a successful Bot API response with result</summary>
    /// <typeparam name="TResult">Expected type of operation result</typeparam>
    public class ApiResponse<TResult> : Exceptions.ApiResponse
    {
        /// <summary>Gets the result object.</summary>
        public TResult? Result { get; set; }
    }
}
