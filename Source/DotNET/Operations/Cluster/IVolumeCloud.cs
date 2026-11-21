// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the volume sweeper may do in the cloud account: list volumes, find where one is attached, detach it and delete it.
/// A failure throws; nothing else is reachable through it.
/// </summary>
public interface IVolumeCloud
{
    /// <summary>
    /// Lists the account's block storage volumes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The volumes.</returns>
    Task<IReadOnlyList<CloudVolume>> ListVolumes(CancellationToken cancellationToken);

    /// <summary>
    /// Finds the servers a volume is attached to.
    /// </summary>
    /// <param name="volumeId">The volume id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The attachments.</returns>
    Task<IReadOnlyList<CloudAttachment>> Attachments(string volumeId, CancellationToken cancellationToken);

    /// <summary>
    /// Detaches a volume from a server.
    /// </summary>
    /// <param name="attachment">The attachment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the detach was requested.</returns>
    Task Detach(CloudAttachment attachment, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a volume.
    /// </summary>
    /// <param name="volumeId">The volume id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when deleted, <see langword="false"/> when the volume is still attached.</returns>
    Task<bool> Delete(string volumeId, CancellationToken cancellationToken);
}
