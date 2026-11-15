// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Keys;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A Vault that holds the secrets it is given, and records what it was asked.
/// </summary>
public class FakeVaultKeyReader : IVaultKeyReader
{
    public Dictionary<string, List<VaultSecret>> Secrets { get; } = new(StringComparer.Ordinal);

    public List<string> Asked { get; } = [];

    public string Snapshot { get; set; } = "raft snapshot";

    public Task<IReadOnlyList<VaultSecret>> ReadTreeAsync(string path, CancellationToken cancellationToken)
    {
        Asked.Add(path);
        return Task.FromResult<IReadOnlyList<VaultSecret>>(Secrets.TryGetValue(path, out var secrets) ? secrets : []);
    }

    public Task<byte[]> TakeSnapshotAsync(CancellationToken cancellationToken)
    {
        Asked.Add("snapshot");
        return Task.FromResult(Encoding.UTF8.GetBytes(Snapshot));
    }
}
#endif
