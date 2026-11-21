// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// An <see cref="IChronicleReader"/> that builds the real reader on first use. A reader that cannot be built, for example because the
/// certificate authority is unusable, then fails the read the check is making and becomes a finding, and does not crash the watchdog
/// before it has told anyone.
/// </summary>
/// <param name="create">Builds the reader.</param>
public sealed class LazyChronicleReader(Func<IChronicleReader> create) : IChronicleReader
{
    readonly Lazy<IChronicleReader> _reader = new(create, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> EventStores(TimeSpan timeout, CancellationToken cancellationToken) => _reader.Value.EventStores(timeout, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> Namespaces(string eventStore, TimeSpan timeout, CancellationToken cancellationToken) => _reader.Value.Namespaces(eventStore, timeout, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChronicleFailedPartition>> FailedPartitions(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken) =>
        _reader.Value.FailedPartitions(eventStore, @namespace, timeout, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChronicleObserver>> Observers(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken) =>
        _reader.Value.Observers(eventStore, @namespace, timeout, cancellationToken);
}
