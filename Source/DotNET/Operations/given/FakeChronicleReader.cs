// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IChronicleReader"/> the specifications fill in with event stores, namespaces, failed partitions and observers.
/// </summary>
public sealed class FakeChronicleReader : IChronicleReader
{
    int _inFlight;

    /// <summary>
    /// Gets the failed partitions by event store and namespace.
    /// </summary>
    public Dictionary<string, Dictionary<string, List<ChronicleFailedPartition>>> Stores { get; } = [];

    /// <summary>
    /// Gets the observers by event store and namespace.
    /// </summary>
    public Dictionary<(string Store, string Namespace), List<ChronicleObserver>> ObserversOf { get; } = [];

    /// <summary>
    /// Gets the namespaces whose queries fail.
    /// </summary>
    public HashSet<(string Store, string Namespace)> Broken { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether listing the event stores fails, as rejected credentials would.
    /// </summary>
    public bool RejectCredentials { get; set; }

    /// <summary>
    /// Gets or sets what happens at the start of every request, for example the clock moving to simulate a slow Chronicle.
    /// </summary>
    public Action? OnRequest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether every namespace answers with data that is not shaped as expected.
    /// </summary>
    public bool MalformedNamespaces { get; set; }

    /// <summary>
    /// Gets how many times the event stores were listed.
    /// </summary>
    public int ListedStores { get; private set; }

    /// <summary>
    /// Gets the most requests that were in flight at once.
    /// </summary>
    public int MostInFlight { get; private set; }

    /// <summary>
    /// Gets the timeouts of the requests.
    /// </summary>
    public System.Collections.Concurrent.ConcurrentQueue<TimeSpan> Timeouts { get; } = [];

    /// <summary>
    /// Creates a failed partition that failed the given time ago.
    /// </summary>
    /// <param name="observer">The observer.</param>
    /// <param name="failingFor">How long ago the first attempt was.</param>
    /// <param name="now">The current time.</param>
    /// <param name="quarantined">Whether it ran out of retries.</param>
    /// <param name="resolved">Whether it was resolved.</param>
    /// <returns>The partition.</returns>
    public static ChronicleFailedPartition Partition(string observer, TimeSpan failingFor, DateTimeOffset now, bool quarantined = false, bool resolved = false) =>
        new(observer, resolved, quarantined, [now - failingFor + TimeSpan.FromSeconds(2), now - failingFor]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> EventStores(TimeSpan timeout, CancellationToken cancellationToken)
    {
        OnRequest?.Invoke();
        ListedStores++;
        Timeouts.Enqueue(timeout);
        return RejectCredentials
            ? throw new HttpRequestException("Response status code does not indicate success: 400 (Bad Request).")
            : Task.FromResult<IReadOnlyList<string>>([.. Stores.Keys]);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> Namespaces(string eventStore, TimeSpan timeout, CancellationToken cancellationToken)
    {
        OnRequest?.Invoke();
        Timeouts.Enqueue(timeout);
        return Task.FromResult<IReadOnlyList<string>>([.. Stores[eventStore].Keys]);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChronicleFailedPartition>> FailedPartitions(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken)
    {
        await Enter(timeout);
        try
        {
            return MalformedNamespaces ? throw new InvalidOperationException("a Chronicle answer has no observerId") : Broken.Contains((eventStore, @namespace)) ? throw new InvalidOperationException("Event store does not exist.") : Stores[eventStore][@namespace];
        }
        finally
        {
            Interlocked.Decrement(ref _inFlight);
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChronicleObserver>> Observers(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken)
    {
        Timeouts.Enqueue(timeout);
        return Task.FromResult<IReadOnlyList<ChronicleObserver>>(ObserversOf.TryGetValue((eventStore, @namespace), out var observers) ? observers : []);
    }

    async Task Enter(TimeSpan timeout)
    {
        OnRequest?.Invoke();
        Timeouts.Enqueue(timeout);
        var now = Interlocked.Increment(ref _inFlight);
        lock (Timeouts)
        {
            MostInFlight = Math.Max(MostInFlight, now);
        }

        await Task.Yield();
    }
}
#endif
