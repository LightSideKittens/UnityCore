using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace Telegram.Bot.Polling
{
    /// <summary>A very simple <see cref="IUpdateHandler"/> implementation</summary>
    /// <remarks>Constructs a new <see cref="DefaultUpdateHandler"/> with the specified callback functions</remarks>
    [PublicAPI]
    public class DefaultUpdateHandler : IUpdateHandler
    {
        private readonly Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler1;
        private readonly Func<ITelegramBotClient, Exception, HandleErrorSource, CancellationToken, Task> errorHandler1;

        /// <summary>Constructs a new <see cref="DefaultUpdateHandler"/> with the specified callback functions</summary>
        /// <param name="updateHandler">The function to invoke when an update is received</param>
        /// <param name="errorHandler">The function to invoke when an error occurs</param>
        public DefaultUpdateHandler(
            Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
            Func<ITelegramBotClient, Exception, CancellationToken, Task> errorHandler)
            : this(updateHandler, (bot, ex, s, ct) => errorHandler(bot, ex, ct))
        { }

        /// <summary>A very simple <see cref="IUpdateHandler"/> implementation</summary>
        /// <remarks>Constructs a new <see cref="DefaultUpdateHandler"/> with the specified callback functions</remarks>
        /// <param name="updateHandler">The function to invoke when an update is received</param>
        /// <param name="errorHandler">The function to invoke when an error occurs</param>
        public DefaultUpdateHandler(Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
            Func<ITelegramBotClient, Exception, HandleErrorSource, CancellationToken, Task> errorHandler)
        {
            updateHandler1 = updateHandler;
            errorHandler1 = errorHandler;
        }

        /// <inheritdoc/>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            => await updateHandler1(botClient, update, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
            => await errorHandler1(botClient, exception, source, cancellationToken).ConfigureAwait(false);
    }
}
