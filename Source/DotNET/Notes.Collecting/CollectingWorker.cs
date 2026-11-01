// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting;

/// <summary>
/// The collecting side's background work, every <see cref="NoteCollectingOptions.PollingInterval"/> from the start of the
/// host: opens every namespace of the collecting store, observes the inbox of each application store in each namespace
/// and subscribes the collecting store to the application stores.
/// </summary>
/// <remarks>
/// Everything it acts on is durable - the namespaces, the subscriptions and the inbox positions live in Chronicle - so a
/// restart loses nothing: the first round after it catches up. A round that fails is logged and the next round tries again.
/// </remarks>
/// <param name="namespaces">Opens the namespaces of the collecting store.</param>
/// <param name="subscriptions">Connects the collecting store to the application stores.</param>
/// <param name="options">The collecting options.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class CollectingWorker(
    TenantEventStores namespaces,
    AppSubscriptions subscriptions,
    IOptions<NoteCollectingOptions> options,
    TimeProvider time,
    ILogger<CollectingWorker> logger) : BackgroundService
{
    /// <summary>
    /// Runs one round of the background work.
    /// </summary>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        var stores = await namespaces.All(cancellationToken);
        foreach (var store in stores)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await subscriptions.Observe(store, cancellationToken);
        }

        await subscriptions.Subscribe(stores[0], cancellationToken);
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
