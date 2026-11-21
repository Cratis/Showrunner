// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.Operations.Link.Protocol;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Keeps the link's state in Kubernetes: the identity - credential, pinned key, the configuration with its secrets - in a Secret,
/// and the outbox, memory and status, which hold no secret, in a ConfigMap, both named <see cref="DirectLinkOptions.StateName"/>.
/// </summary>
/// <param name="configMaps">The ConfigMap store.</param>
/// <param name="secrets">The Secret store.</param>
/// <param name="name">The name of the ConfigMap and the Secret.</param>
public sealed class ConfigMapLinkStore(IConfigMapStore configMaps, ISecretStore secrets, string name) : ILinkStore
{
    const string IdentityKey = "identity";
    const string OutboxKey = "outbox";
    const string MemoryKey = "memory";
    const string StatusKey = "status";

    /// <inheritdoc/>
    public async Task<LinkIdentity?> LoadIdentity(CancellationToken cancellationToken) =>
        await secrets.Read(name, IdentityKey, cancellationToken) is { } json ? JsonSerializer.Deserialize<LinkIdentity>(json, WatchdogJson.Options) : null;

    /// <inheritdoc/>
    public Task SaveIdentity(LinkIdentity identity, CancellationToken cancellationToken) =>
        secrets.Write(name, IdentityKey, JsonSerializer.Serialize(identity, WatchdogJson.Options), cancellationToken);

    /// <inheritdoc/>
    public async Task<OutboxSnapshot> LoadOutbox(CancellationToken cancellationToken) =>
        await configMaps.Read(name, OutboxKey, cancellationToken) is { } json ? JsonSerializer.Deserialize<OutboxSnapshot>(json, WatchdogJson.Options) ?? OutboxSnapshot.Empty : OutboxSnapshot.Empty;

    /// <inheritdoc/>
    public Task SaveOutbox(OutboxSnapshot outbox, CancellationToken cancellationToken) =>
        configMaps.Write(name, OutboxKey, JsonSerializer.Serialize(outbox, WatchdogJson.Options), cancellationToken);

    /// <inheritdoc/>
    public async Task<LinkMemory> LoadMemory(CancellationToken cancellationToken) =>
        await configMaps.Read(name, MemoryKey, cancellationToken) is { } json ? JsonSerializer.Deserialize<LinkMemory>(json, WatchdogJson.Options) ?? LinkMemory.Empty : LinkMemory.Empty;

    /// <inheritdoc/>
    public Task SaveMemory(LinkMemory memory, CancellationToken cancellationToken) =>
        configMaps.Write(name, MemoryKey, JsonSerializer.Serialize(memory, WatchdogJson.Options), cancellationToken);

    /// <inheritdoc/>
    public Task SaveStatus(LinkStatus status, CancellationToken cancellationToken) =>
        configMaps.Write(name, StatusKey, JsonSerializer.Serialize(status, WatchdogJson.Options), cancellationToken);
}
