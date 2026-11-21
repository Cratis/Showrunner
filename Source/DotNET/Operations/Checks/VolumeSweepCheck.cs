// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Deletes CSI-provisioned cloud volumes that no PersistentVolume refers to any more. A pod deleted while the cloud is still
/// attaching its volume leaves the disk attached with nothing in the cluster that knows about it, so the delete fails forever and
/// the disk is billed until someone notices. The sweep finds those volumes, detaches them and deletes them, behind several fences:
/// an empty or ambiguous view of the cluster or the cloud is a "cannot run" finding and never a pass; only volumes this cluster
/// provisioned (exactly one cluster id label among the volumes that back PersistentVolumes) and that are old enough are candidates;
/// more candidates than the limit trips a fuse and deletes none; and the PersistentVolumes are read again right before acting.
/// </summary>
/// <param name="cloud">The cloud account.</param>
/// <param name="handles">The volume handles of the cluster's PersistentVolumes.</param>
/// <param name="options">The options.</param>
public sealed class VolumeSweepCheck(IVolumeCloud cloud, IVolumeHandles handles, VolumeSweepOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "volume-sweeper";

    const string TitlePrefix = "pvc-";

    IReadOnlyCollection<Finding>? _acted;

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        // The pipeline asks again to confirm a finding. A delete is an action, so it is done once per run and the confirmation sees the same result.
        return _acted ??= await Act(context, cancellationToken);
    }

    static string Volume(CloudVolume volume) => $"{volume.Id} ({volume.SizeGigabytes} GB)";

    static string Number(int count, string noun) => $"{count} {noun}";

    async Task<IReadOnlyCollection<Finding>> Act(CheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await Sweep(context, cancellationToken);
        }
        catch (VolumeSweepCannotRunException error)
        {
            return [new Finding("volume-sweeper:cannot-run", $"volume sweeper cannot run: {error.Message}", AlertSeverity.Critical)];
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding("volume-sweeper:cannot-run", $"volume sweeper cannot run: {Redaction.Summarize(error.Message)}", AlertSeverity.Critical)];
        }
    }

    async Task<IReadOnlyCollection<Finding>> Sweep(CheckContext context, CancellationToken cancellationToken)
    {
        var known = await handles.Read(options.CsiDriver, cancellationToken);
        if (known.Count == 0)
        {
            throw new VolumeSweepCannotRunException("the cluster reports no CSI persistent volumes; refusing to treat every volume as an orphan");
        }

        var volumes = await cloud.ListVolumes(cancellationToken);
        if (volumes.Count == 0)
        {
            throw new VolumeSweepCannotRunException("the cloud reports no storage at all");
        }

        var clusterId = OwnClusterId(volumes, known);
        var orphans = volumes
            .Where(volume => !known.Contains(volume.Id) && volume.Title.StartsWith(TitlePrefix, StringComparison.Ordinal) && volume.ClusterId == clusterId && context.Now - volume.Created >= options.MinimumAge)
            .ToList();
        if (orphans.Count > options.MaximumPerRun)
        {
            var text = $"volume sweeper fuse: {orphans.Count} orphaned volumes exceeds the {options.MaximumPerRun} limit; acting on none - investigate";
            return [new Finding("volume-sweeper:fuse", text)];
        }

        if (options.DryRun)
        {
            return orphans.Count == 0
                ? []
                : [new Finding("volume-sweeper:would-delete", $"volume sweeper would delete {Number(orphans.Count, "orphaned volume(s)")}: {string.Join(", ", orphans.Select(Volume))}", AlertSeverity.Information)];
        }

        return await Delete(orphans, context.TimeProvider, cancellationToken);
    }

    string OwnClusterId(IReadOnlyList<CloudVolume> volumes, IReadOnlySet<string> known)
    {
        var ids = volumes.Where(volume => known.Contains(volume.Id)).Select(volume => volume.ClusterId).OfType<string>().Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        return ids.Count == 1
            ? ids[0]
            : throw new VolumeSweepCannotRunException($"expected exactly one {options.ClusterLabel} among volumes backing PVs, found [{string.Join(", ", ids)}]");
    }

    async Task<IReadOnlyCollection<Finding>> Delete(List<CloudVolume> orphans, TimeProvider time, CancellationToken cancellationToken)
    {
        // Read the PersistentVolumes again right before acting: one created since the first read protects its volume.
        var known = await handles.Read(options.CsiDriver, cancellationToken);
        var findings = new List<Finding>();
        var deleted = new List<CloudVolume>();
        foreach (var orphan in orphans.Where(orphan => !known.Contains(orphan.Id)))
        {
            try
            {
                if (await DetachAndDelete(orphan, time, cancellationToken))
                {
                    deleted.Add(orphan);
                }
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                findings.Add(new Finding($"volume-sweeper:failed:{orphan.Id}", $"volume sweeper failed to delete {Volume(orphan)}: {Redaction.Summarize(error.Message)}"));
            }
        }

        if (deleted.Count > 0)
        {
            findings.Add(new Finding("volume-sweeper:deleted", $"volume sweeper deleted {Number(deleted.Count, "orphaned volume(s)")}: {string.Join(", ", deleted.Select(Volume))}", AlertSeverity.Information));
        }

        return findings;
    }

    async Task<bool> DetachAndDelete(CloudVolume volume, TimeProvider time, CancellationToken cancellationToken)
    {
        foreach (var attachment in await cloud.Attachments(volume.Id, cancellationToken))
        {
            if (!attachment.Started)
            {
                // A node that is busy (maintenance) is retried on the next run.
                return false;
            }

            await cloud.Detach(attachment, cancellationToken);
        }

        for (var attempt = 0; attempt < options.DetachAttempts; attempt++)
        {
            await Task.Delay(options.DetachPoll, time, cancellationToken);
            if (await cloud.Delete(volume.Id, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }
}
