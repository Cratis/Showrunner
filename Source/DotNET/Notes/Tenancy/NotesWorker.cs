// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Receiving;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Tenancy;

/// <summary>
/// Notes' background work, every <see cref="NotesOptions.PollingInterval"/> from the start of the host: opens every
/// tenant namespace, observes the inbox of each collecting store in each namespace and subscribes the application to
/// the collecting stores. With the HTTP transport it only opens the namespaces.
/// </summary>
/// <remarks>
/// Everything it acts on is durable - the namespaces, the subscriptions and the inbox positions live in Chronicle - so a
/// restart loses nothing: the first round after it catches up. A round that fails is logged and the next round tries again.
/// </remarks>
/// <param name="tenants">Opens the tenant namespaces.</param>
/// <param name="receivers">Connects the application to the collecting stores.</param>
/// <param name="options">Notes' options.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class NotesWorker(
    TenantEventStores tenants,
    ReceiverSubscriptions receivers,
    IOptions<NotesOptions> options,
    TimeProvider time,
    ILogger<NotesWorker> logger) : BackgroundService
{
    /// <summary>
    /// Runs one round of the background work.
    /// </summary>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        var stores = await tenants.All(cancellationToken);
        if (options.Value.Transport == NotesTransport.Http)
        {
            // The collector is not in this kernel: there is no inbox to observe and no outbox to subscribe. The relay
            // takes the notes over HTTP, but the namespaces still have to be open for Notes' reactors to run in them.
            return;
        }

        foreach (var store in stores)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await receivers.Observe(store);
        }

        await receivers.Subscribe(stores[0]);
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
