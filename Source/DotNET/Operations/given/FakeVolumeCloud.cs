// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IVolumeCloud"/> and <see cref="IVolumeHandles"/> the specifications fill in, which records every action.
/// </summary>
public sealed class FakeVolumeCloud : IVolumeCloud, IVolumeHandles
{
    int _handleReads;

    /// <summary>
    /// Gets the volumes in the cloud account.
    /// </summary>
    public List<CloudVolume> Volumes { get; } = [];

    /// <summary>
    /// Gets the volume handles of the first read of the PersistentVolumes.
    /// </summary>
    public HashSet<string> Handles { get; } = [];

    /// <summary>
    /// Gets the volume handles that appear from the second read on, as a PV created in the meantime.
    /// </summary>
    public HashSet<string> LateHandles { get; } = [];

    /// <summary>
    /// Gets the servers a volume is attached to, by volume id.
    /// </summary>
    public Dictionary<string, List<CloudAttachment>> Attached { get; } = [];

    /// <summary>
    /// Gets or sets how many delete attempts answer "still attached" before one succeeds.
    /// </summary>
    public int AttachedForAttempts { get; set; }

    /// <summary>
    /// Gets the volumes whose deletion fails.
    /// </summary>
    public HashSet<string> FailingDeletes { get; } = [];

    /// <summary>
    /// Gets the actions that were taken, in order.
    /// </summary>
    public List<string> Actions { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether listing volumes fails.
    /// </summary>
    public bool ListingFails { get; set; }

    /// <summary>
    /// Creates a volume.
    /// </summary>
    /// <param name="id">The volume id.</param>
    /// <param name="age">How old it is.</param>
    /// <param name="now">The current time.</param>
    /// <param name="title">The title.</param>
    /// <param name="cluster">The cluster id label.</param>
    /// <returns>The volume.</returns>
    public static CloudVolume Volume(string id, TimeSpan age, DateTimeOffset now, string title = "pvc-x", string? cluster = "mine") =>
        new(id, title, now - age, 10, cluster);

    /// <inheritdoc/>
    public Task<IReadOnlySet<string>> Read(string driver, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlySet<string>>(_handleReads++ == 0 ? Handles : [.. Handles, .. LateHandles]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<CloudVolume>> ListVolumes(CancellationToken cancellationToken) =>
        ListingFails ? throw new InvalidOperationException("GET storage/normal: HTTP 500") : Task.FromResult<IReadOnlyList<CloudVolume>>(Volumes);

    /// <inheritdoc/>
    public Task<IReadOnlyList<CloudAttachment>> Attachments(string volumeId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CloudAttachment>>(Attached.TryGetValue(volumeId, out var servers) ? servers : []);

    /// <inheritdoc/>
    public Task Detach(CloudAttachment attachment, CancellationToken cancellationToken)
    {
        Actions.Add($"detach {attachment.ServerId}@{attachment.Address}");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> Delete(string volumeId, CancellationToken cancellationToken)
    {
        if (FailingDeletes.Contains(volumeId))
        {
            throw new UpCloudException("DELETE storage: HTTP 500", "INTERNAL");
        }

        Actions.Add($"delete {volumeId}");
        return Task.FromResult(Actions.Count(action => action == $"delete {volumeId}") > AttachedForAttempts);
    }
}
#endif
