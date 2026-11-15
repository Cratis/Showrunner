// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;

namespace Cratis.Showrunner.StageManager.Understudies.Keys;

/// <summary>
/// <see cref="IVaultKeyReader"/> over the HTTP API of Vault, reading a KV version 2 mount.
/// </summary>
public sealed class VaultHttpKeyReader : IVaultKeyReader, IDisposable
{
    /// <summary>
    /// The mount Chronicle keeps its keys under.
    /// </summary>
    public const string DefaultMount = "secret";

    readonly HttpClient _client;
    readonly string _mount;

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultHttpKeyReader"/> class.
    /// </summary>
    /// <param name="client">The client, whose base address is the Vault address.</param>
    /// <param name="token">The token. It is sent as a header and never logged.</param>
    /// <param name="mount">The KV mount.</param>
    public VaultHttpKeyReader(HttpClient client, string token, string mount = DefaultMount)
    {
        _client = client;
        _mount = mount;
        _client.DefaultRequestHeaders.Add("X-Vault-Token", token);
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<VaultSecret>> ReadTreeAsync(string path, CancellationToken cancellationToken)
    {
        var secrets = new List<VaultSecret>();
        await ReadAsync(path.Trim('/'), secrets, cancellationToken);
        return secrets;
    }

    /// <inheritdoc/>
    public async Task<byte[]> TakeSnapshotAsync(CancellationToken cancellationToken)
    {
        using var response = await _client.GetAsync("v1/sys/storage/raft/snapshot", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose() => _client.Dispose();

    async Task ReadAsync(string path, List<VaultSecret> secrets, CancellationToken cancellationToken)
    {
        using var listing = await _client.GetAsync($"v1/{_mount}/metadata/{path}?list=true", cancellationToken);
        if (listing.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        listing.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(await listing.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        foreach (var key in document.RootElement.GetProperty("data").GetProperty("keys").EnumerateArray().Select(element => element.GetString()!))
        {
            if (key.EndsWith('/'))
            {
                await ReadAsync($"{path}/{key.TrimEnd('/')}", secrets, cancellationToken);
            }
            else
            {
                secrets.Add(await ReadSecretAsync($"{path}/{key}", cancellationToken));
            }
        }
    }

    async Task<VaultSecret> ReadSecretAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _client.GetAsync($"v1/{_mount}/data/{path}", cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var data = document.RootElement.GetProperty("data").GetProperty("data").EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString()! : property.Value.GetRawText(), StringComparer.Ordinal);
        return new() { Path = path, Data = data };
    }
}
