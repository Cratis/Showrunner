// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Availability.Audiences;
using Cratis.Arc.Availability.Suspensions;
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Holding;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// Observes the <see cref="AudienceHoldings"/> read model of the product's own namespace, which holds the system's grants,
/// and of every tenant's namespace, and publishes what changes into the <see cref="AudienceChangeFeed"/>.
/// </summary>
/// <remarks>
/// It watches the product's own event store, never the operator's: a change is seen when the product has recorded it. A
/// tenant added later is picked up on the next round, which runs every <see cref="GuestListOptions.PollingInterval"/>. A
/// watch that ends is opened again on the next round, so a dropped connection to Chronicle heals itself.
/// </remarks>
/// <param name="client">The Chronicle client.</param>
/// <param name="chronicle">The Chronicle client options, which name the product's event store.</param>
/// <param name="feed">The feed to publish changes into.</param>
/// <param name="metrics">The metrics, held so they are created with the watcher.</param>
/// <param name="options">GuestList's options.</param>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
/// <param name="logger">The logger.</param>
public class AudienceChangeWatcher(
    IChronicleClient client,
    IOptions<ChronicleClientOptions> chronicle,
    AudienceChangeFeed feed,
    GuestListAvailabilityMetrics metrics,
    IOptions<GuestListOptions> options,
    TimeProvider time,
    ILogger<AudienceChangeWatcher> logger) : BackgroundService
{
    readonly Lock _lock = new();
    readonly Dictionary<EventStoreNamespaceName, IDisposable> _watching = [];

    /// <summary>
    /// Gets the metrics the watcher keeps alive.
    /// </summary>
    public GuestListAvailabilityMetrics Metrics => metrics;

    /// <summary>
    /// Watches every namespace that is not watched yet.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>An awaitable task.</returns>
    public async Task RunOnce(CancellationToken cancellationToken = default)
    {
        var productStore = await client.GetEventStore(chronicle.Value.EventStore, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);

        foreach (var @namespace in (await productStore.GetNamespaces(cancellationToken)).Append(EventStoreNamespaceName.Default).Distinct())
        {
            if (IsWatching(@namespace))
            {
                continue;
            }

            try
            {
                await Watch(@namespace, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.WatchFailed(exception, @namespace.Value);
            }
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        lock (_lock)
        {
            foreach (var subscription in _watching.Values)
            {
                subscription.Dispose();
            }

            _watching.Clear();
        }

        base.Dispose();
        GC.SuppressFinalize(this);
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

    bool IsWatching(EventStoreNamespaceName @namespace)
    {
        lock (_lock)
        {
            return _watching.ContainsKey(@namespace);
        }
    }

    async Task Watch(EventStoreNamespaceName @namespace, CancellationToken cancellationToken)
    {
        var store = await client.GetEventStore(chronicle.Value.EventStore, @namespace).WaitAsync(cancellationToken);
        var audiences = store.ReadModels.Watch<AudienceHoldings>().Subscribe(new Observer(this, @namespace));
        var suspensions = store.ReadModels.Watch<SuspensionHoldings>().Subscribe(new SuspensionObserver(this, @namespace));
        lock (_lock)
        {
            _watching[@namespace] = new Both(audiences, suspensions);
        }
    }

    async Task PublishSuspension(SuspensionChange change)
    {
        try
        {
            await feed.PublishSuspension(change);
        }
        catch (Exception exception)
        {
            logger.RoundFailed(exception);
        }
    }

    async Task Publish(AudienceChange change)
    {
        try
        {
            await feed.Publish(change);
        }
        catch (Exception exception)
        {
            logger.RoundFailed(exception);
        }
    }

    void Failed(EventStoreNamespaceName @namespace, Exception error)
    {
        logger.WatchFailed(error, @namespace.Value);
        Stopped(@namespace);
    }

    void Stopped(EventStoreNamespaceName @namespace)
    {
        lock (_lock)
        {
            if (_watching.Remove(@namespace, out var subscription))
            {
                subscription.Dispose();
            }
        }
    }

    sealed class Both(IDisposable first, IDisposable second) : IDisposable
    {
        public void Dispose()
        {
            first.Dispose();
            second.Dispose();
        }
    }

    sealed class SuspensionObserver(AudienceChangeWatcher watcher, EventStoreNamespaceName @namespace) : IObserver<ReadModelChangeset<SuspensionHoldings>>
    {
        public void OnNext(ReadModelChangeset<SuspensionHoldings> value)
        {
            if (AudienceChanges.From(@namespace, value.ModelKey.Value) is not { } change)
            {
                return;
            }

            _ = watcher.PublishSuspension(new SuspensionChange(change.Level, change.Key));
        }

        public void OnError(Exception error) => watcher.Failed(@namespace, error);

        public void OnCompleted() => watcher.Stopped(@namespace);
    }

    sealed class Observer(AudienceChangeWatcher watcher, EventStoreNamespaceName @namespace) : IObserver<ReadModelChangeset<AudienceHoldings>>
    {
        public void OnNext(ReadModelChangeset<AudienceHoldings> value)
        {
            if (AudienceChanges.From(@namespace, value.ModelKey.Value) is not { } change)
            {
                return;
            }

            _ = watcher.Publish(change);
        }

        public void OnError(Exception error) => watcher.Failed(@namespace, error);

        public void OnCompleted() => watcher.Stopped(@namespace);
    }
}
