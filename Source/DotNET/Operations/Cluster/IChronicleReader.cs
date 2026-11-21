// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The only way the watchdog reads Chronicle: the query API the Workbench uses. An implementation throws when a query did not
/// succeed, so that an unknown event store or a server exception never reads as "nothing is failing".
/// </summary>
public interface IChronicleReader
{
    /// <summary>
    /// Lists the event stores the watchdog may read; Chronicle's own reserved System store is not included.
    /// </summary>
    /// <param name="timeout">How long each request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The names of the event stores.</returns>
    Task<IReadOnlyList<string>> EventStores(TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the namespaces of an event store.
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="timeout">How long each request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The namespace names.</returns>
    Task<IReadOnlyList<string>> Namespaces(string eventStore, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the failed partitions of a namespace.
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="namespace">The namespace.</param>
    /// <param name="timeout">How long each request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The failed partitions.</returns>
    Task<IReadOnlyList<ChronicleFailedPartition>> FailedPartitions(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the observers of a namespace.
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="namespace">The namespace.</param>
    /// <param name="timeout">How long each request may take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The observers.</returns>
    Task<IReadOnlyList<ChronicleObserver>> Observers(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken);
}
