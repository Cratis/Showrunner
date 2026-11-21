// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.State;

/// <summary>
/// Reads and writes one value of a Secret in the watchdog's own namespace.
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Reads a value of a Secret.
    /// </summary>
    /// <param name="secret">The name of the Secret.</param>
    /// <param name="key">The key in its data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value, or <see langword="null"/> when the Secret or the key does not exist.</returns>
    Task<string?> Read(string secret, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Writes a value of a Secret, creating the Secret when it does not exist.
    /// </summary>
    /// <param name="secret">The name of the Secret.</param>
    /// <param name="key">The key in its data.</param>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the value is stored.</returns>
    Task Write(string secret, string key, string value, CancellationToken cancellationToken);
}
