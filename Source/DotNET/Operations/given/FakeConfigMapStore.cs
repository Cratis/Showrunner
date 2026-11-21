// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An in-memory <see cref="IConfigMapStore"/>.
/// </summary>
public sealed class FakeConfigMapStore : IConfigMapStore
{
    /// <summary>
    /// Gets the values by ConfigMap and key.
    /// </summary>
    public Dictionary<(string ConfigMap, string Key), string> Values { get; } = [];

    /// <summary>
    /// Gets the writes, in order.
    /// </summary>
    public List<(string ConfigMap, string Key, string Value)> Writes { get; } = [];

    /// <inheritdoc/>
    public Task<string?> Read(string configMap, string key, CancellationToken cancellationToken) =>
        Task.FromResult(Values.TryGetValue((configMap, key), out var value) ? value : null);

    /// <inheritdoc/>
    public Task Write(string configMap, string key, string value, CancellationToken cancellationToken)
    {
        Values[(configMap, key)] = value;
        Writes.Add((configMap, key, value));
        return Task.CompletedTask;
    }
}
#endif
