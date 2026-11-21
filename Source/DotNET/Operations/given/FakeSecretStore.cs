// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An in-memory <see cref="ISecretStore"/>.
/// </summary>
public sealed class FakeSecretStore : ISecretStore
{
    /// <summary>
    /// Gets the values by secret and key.
    /// </summary>
    public Dictionary<(string Secret, string Key), string> Values { get; } = [];

    /// <inheritdoc/>
    public Task<string?> Read(string secret, string key, CancellationToken cancellationToken) =>
        Task.FromResult(Values.TryGetValue((secret, key), out var value) ? value : null);

    /// <inheritdoc/>
    public Task Write(string secret, string key, string value, CancellationToken cancellationToken)
    {
        Values[(secret, key)] = value;
        return Task.CompletedTask;
    }
}
#endif
