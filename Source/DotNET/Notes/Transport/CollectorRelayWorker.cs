// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Tenancy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// The HTTP transport's background work: every <see cref="NotesOptions.RelayInterval"/> sends the notes in the outbox of
/// each tenant namespace to the collector and applies the answers it has. It does nothing unless
/// <see cref="NotesOptions.Transport"/> is <see cref="NotesTransport.Http"/>.
/// </summary>
/// <remarks>
/// Nothing it does is lost by failing: the collector holds the position and the answers, so a round that fails is logged
/// and tried again after a delay that doubles with each failure up to two minutes. The host keeps running.
/// </remarks>
/// <param name="tenants">Opens the tenant namespaces.</param>
/// <param name="relay">Sends the notes.</param>
/// <param name="applier">Applies the answers.</param>
/// <param name="options">Notes' options.</param>
/// <param name="time">The clock.</param>
/// <param name="logger">The logger.</param>
public class CollectorRelayWorker(
    TenantEventStores tenants,
    OutboxRelay relay,
    ResponseApplier applier,
    IOptions<NotesOptions> options,
    TimeProvider time,
    ILogger<CollectorRelayWorker> logger) : BackgroundService
{
    static readonly TimeSpan _longestDelay = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Runs one round: sends the notes of every namespace, then applies the answers.
    /// </summary>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        foreach (var store in await tenants.All(cancellationToken))
        {
            await relay.Relay(store, cancellationToken);
        }

        await applier.Apply(cancellationToken);
    }

    /// <summary>
    /// Gets how long to wait before the next round.
    /// </summary>
    /// <param name="failures">How many rounds in a row failed.</param>
    /// <returns>The delay.</returns>
    public TimeSpan DelayAfter(int failures)
    {
        var interval = options.Value.RelayInterval;
        if (failures == 0)
        {
            return interval;
        }

        var factor = Math.Pow(2, Math.Min(failures, 10));
        return TimeSpan.FromTicks(Math.Min((long)(interval.Ticks * factor), _longestDelay.Ticks));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled || options.Value.Transport != NotesTransport.Http)
        {
            return;
        }

        var failures = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnce(stoppingToken);
                failures = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                failures++;
                logger.RelayFailed(exception, (int)DelayAfter(failures).TotalSeconds);
            }

            try
            {
                await Task.Delay(DelayAfter(failures), time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
