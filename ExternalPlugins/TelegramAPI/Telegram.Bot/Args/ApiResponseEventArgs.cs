using System.Net.Http;

namespace Telegram.Bot.Args
{
    /// <summary>Provides data for ApiResponseReceived event</summary>
    public class ApiResponseEventArgs
    {
        /// <summary>Provides data for ApiResponseReceived event</summary>
        /// <param name="responseMessage">HTTP response received from API</param>
        /// <param name="apiRequestEventArgs">Event arguments of this request</param>
        public ApiResponseEventArgs(HttpResponseMessage responseMessage, ApiRequestEventArgs apiRequestEventArgs)
        {
            ResponseMessage = responseMessage;
            ApiRequestEventArgs = apiRequestEventArgs;
        }

        /// <summary>HTTP response received from API</summary>
        public HttpResponseMessage ResponseMessage { get; }

        /// <summary>Event arguments of this request</summary>
        public ApiRequestEventArgs ApiRequestEventArgs { get; }
    }
}
