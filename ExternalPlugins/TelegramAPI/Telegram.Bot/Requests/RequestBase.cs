using System.Net.Http;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Requests
{
    /// <summary>Represents an API request</summary>
    /// <typeparam name="TResponse">Type of result expected in result</typeparam>
    public abstract class RequestBase<TResponse> : IRequest<TResponse>
    {
        /// <summary>Represents an API request</summary>
        /// <typeparam name="TResponse">Type of result expected in result</typeparam>
        /// <param name="methodName">Bot API method</param>
        protected RequestBase(string methodName)
        {
            MethodName = methodName;
        }

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Post;

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        public string MethodName { get; }

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsWebhookResponse { get; set; }

        /// <summary><see href="https://core.telegram.org/bots/api#making-requests-when-getting-updates"/></summary>
        [Newtonsoft.Json.JsonProperty]
        internal string? Method => IsWebhookResponse ? MethodName : default;

        /// <summary>Generate content of HTTP message</summary>
        /// <returns>Content of HTTP request</returns>
        public virtual HttpContent? ToHttpContent() =>
#if NET6_0_OR_GREATER
        System.Net.Http.Json.JsonContent.Create(this, GetType(), options: JsonBotAPI.Options);
#else
            new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(this, GetType(), JsonBotAPI.Settings), System.Text.Encoding.UTF8, "application/json");
#endif
    }

    /// <summary>Represents a request that doesn't require any parameters</summary>
    public abstract class ParameterlessRequest<TResult> : RequestBase<TResult>
    {
        /// <summary>Represents a request that doesn't require any parameters</summary>
        /// <param name="methodName">Name of request method</param>
        protected ParameterlessRequest(string methodName) : base(methodName)
        {
        }

        /// <inheritdoc/>
        public override HttpContent? ToHttpContent() => IsWebhookResponse ? base.ToHttpContent() : default;
    }
}
