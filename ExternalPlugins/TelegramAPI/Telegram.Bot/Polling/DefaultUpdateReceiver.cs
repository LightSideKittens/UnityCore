using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Polling
{
    /// <summary>A simple <see cref="IUpdateReceiver"/>> implementation that requests new updates and handles them sequentially</summary>
    /// <remarks>Constructs a new <see cref="DefaultUpdateReceiver"/> with the specified <see cref="ITelegramBotClient"/> instance and optional <see cref="ReceiverOptions"/></remarks>
    [PublicAPI]
    public class DefaultUpdateReceiver : IUpdateReceiver
    {
        private readonly ITelegramBotClient botClient1;
        private readonly ReceiverOptions receiverOptions1;

        /// <summary>A simple <see cref="IUpdateReceiver"/>> implementation that requests new updates and handles them sequentially</summary>
        /// <remarks>Constructs a new <see cref="DefaultUpdateReceiver"/> with the specified <see cref="ITelegramBotClient"/> instance and optional <see cref="ReceiverOptions"/></remarks>
        /// <param name="botClient">The <see cref="ITelegramBotClient"/> used for making GetUpdates calls</param>
        /// <param name="receiverOptions">Options used to configure getUpdates requests</param>
        public DefaultUpdateReceiver(ITelegramBotClient botClient, ReceiverOptions? receiverOptions = default)
        {
            botClient1 = botClient;
            receiverOptions1 = receiverOptions;
        }

        private static readonly Update[] EmptyUpdates = Array.Empty<Update>();

        /// <inheritdoc/>
        public async Task ReceiveAsync(IUpdateHandler updateHandler, CancellationToken cancellationToken = default)
        {
            if (updateHandler is null) { throw new ArgumentNullException(nameof(updateHandler)); }

            var allowedUpdates = receiverOptions1?.AllowedUpdates;
            var limit = receiverOptions1?.Limit ?? 100;
            var messageOffset = receiverOptions1?.Offset ?? 0;
            var emptyUpdates = EmptyUpdates;

            if (receiverOptions1?.DropPendingUpdates is true)
            {
                try
                {
                    var updates = await botClient1.GetUpdates(-1, 1, 0, new List<UpdateType> { }.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                    messageOffset = updates.Length == 0 ? 0 : updates[^1].Id + 1;
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            }
            var request = new GetUpdatesRequest
            {
                Limit = limit,
                Offset = messageOffset,
                AllowedUpdates = allowedUpdates,
            };
            while (!cancellationToken.IsCancellationRequested)
            {
                request.Timeout = (int)botClient1.Timeout.TotalSeconds;

                var updates = emptyUpdates;
                try
                {
                    updates = await botClient1.SendRequest(request, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    try
                    {
                        await updateHandler.HandleErrorAsync(botClient1, exception, HandleErrorSource.PollingError, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }

                foreach (var update in updates)
                {
                    try
                    {
                        request.Offset = update.Id + 1;
                        await updateHandler.HandleUpdateAsync(botClient1, update, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await updateHandler.HandleErrorAsync(botClient1, ex, HandleErrorSource.HandleUpdateError, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // ignored
                        }
                    }
                }
            }
        }
    }
}

