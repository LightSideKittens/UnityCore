// GENERATED FILE - DO NOT MODIFY MANUALLY

using System.ComponentModel;
using Telegram.Bot.Types.Payments;

namespace Telegram.Bot.Requests
{
    /// <summary>Returns the bot's Telegram Star transactions in chronological order.<para>Returns: A <see cref="StarTransactions"/> object.</para></summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class GetStarTransactionsRequest : RequestBase<StarTransactions>
    {
        /// <summary>Returns the bot's Telegram Star transactions in chronological order.<para>Returns: A <see cref="StarTransactions"/> object.</para></summary>
        public GetStarTransactionsRequest() : base("getStarTransactions")
        {
        }

        /// <summary>Number of transactions to skip in the response</summary>
        public int? Offset { get; set; }

        /// <summary>The maximum number of transactions to be retrieved. Values between 1-100 are accepted. Defaults to 100.</summary>
        public int? Limit { get; set; }
    }
}
