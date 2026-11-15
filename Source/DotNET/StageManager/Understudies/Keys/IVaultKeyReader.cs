// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Keys;

/// <summary>
/// Reads the keys Chronicle keeps in Vault. What it returns is plaintext, so it lives in memory only: callers encrypt it at once and never write it to a volume or a log.
/// </summary>
public interface IVaultKeyReader
{
    /// <summary>
    /// Reads every secret under a path, in any depth.
    /// </summary>
    /// <param name="path">The path under the mount, such as <c>Studio/acme</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The secrets.</returns>
    Task<IReadOnlyList<VaultSecret>> ReadTreeAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Takes a Raft snapshot of the whole Vault.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The snapshot.</returns>
    Task<byte[]> TakeSnapshotAsync(CancellationToken cancellationToken);
}
