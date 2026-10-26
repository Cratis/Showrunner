// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Receiving;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Tenancy;

/// <summary>
/// Usher's background work, every <see cref="UsherOptions.PollingInterval"/> from the start of the host: opens every
/// tenant namespace, subscribes to the desk stores and observes their inboxes in each namespace, and lapses the grants
/// of access whose end has passed.
/// </summary>
/// <remarks>
/// Everything it acts on is durable - the namespaces, the grants and the subscriptions live in Chronicle - so a restart
/// loses nothing: the first round after it catches up. A round that fails is logged and the next round tries again.
/// </remarks>
/// <param name="tenants">Opens the tenant namespaces.</param>
/// <param name="desks">Connects the application to the desk stores.</param>
/// <param name="lapsing">Lapses grants whose end has passed.</param>
/// <param name="options">Usher's options.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class UsherWorker(
    TenantEventStores tenants,
    DeskSubscriptions desks,
    GrantLapsing lapsing,
    IOptions<UsherOptions> options,
    TimeProvider time,
    ILogger<UsherWorker> logger) : BackgroundService
{
    /// <summary>
    /// Runs one round of the background work.
    /// </summary>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        var stores = await tenants.All(cancellationToken);
        foreach (var store in stores)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await desks.Observe(store);
        }

        await desks.Subscribe(stores[0]);

        foreach (var store in stores)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await lapsing.LapseDueGrants(store);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LapsingFailed(exception, store.Namespace);
            }
        }
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
