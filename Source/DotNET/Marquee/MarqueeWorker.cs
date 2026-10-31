// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.Marquee.Receiving;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee;

/// <summary>
/// Marquee's background work, every <see cref="MarqueeOptions.PollingInterval"/> from the start of the host: opens the
/// product's own namespace, subscribes it to the authoring stores and observes their inbox.
/// </summary>
/// <remarks>
/// Everything it acts on is durable - the subscriptions and the inbox position live in Chronicle - so a restart loses
/// nothing: the first round after it catches up. A round that fails is logged and the next round tries again.
/// </remarks>
/// <param name="client">The Chronicle client.</param>
/// <param name="chronicle">The Chronicle client options, which name the product's event store.</param>
/// <param name="subscriptions">Connects the product to the authoring stores.</param>
/// <param name="options">Marquee's options.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class MarqueeWorker(
    IChronicleClient client,
    IOptions<ChronicleClientOptions> chronicle,
    MarqueeSubscriptions subscriptions,
    IOptions<MarqueeOptions> options,
    TimeProvider time,
    ILogger<MarqueeWorker> logger) : BackgroundService
{
    /// <summary>
    /// Runs one round of the background work.
    /// </summary>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        var eventStore = await client.GetEventStore(chronicle.Value.EventStore, MarqueeStream.Namespace).WaitAsync(cancellationToken);
        await subscriptions.Observe(eventStore);
        await subscriptions.Subscribe(eventStore);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnce(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.RoundFailed(exception);
            }

            try
            {
                await Task.Delay(options.Value.PollingInterval, time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
