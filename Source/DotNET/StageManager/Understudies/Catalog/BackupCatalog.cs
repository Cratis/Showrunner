// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// Lists the backups of a Production from the manifests in object storage alone. A backup without a manifest is not in the catalog, because it is not complete.
/// </summary>
/// <param name="storage">The storage.</param>
public class BackupCatalog(IObjectStorage storage)
{
    /// <summary>
    /// Lists every backup run of a Production, newest first, with what the policy says about each.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="policy">The retention policy that decides each run's verdict.</param>
    /// <param name="now">The current time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The runs.</returns>
    public async Task<IReadOnlyList<CatalogEntry>> ListAsync(string production, RetentionPolicy policy, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var manifests = new List<(BackupManifest Manifest, string Key)>();
        await foreach (var item in storage.ListAsync(BackupLayout.ProductionPrefix(production), cancellationToken))
        {
            if (!item.Key.EndsWith($"/{BackupManifest.FileName}", StringComparison.Ordinal))
            {
                continue;
            }

            await using var stream = await storage.OpenReadAsync(item.Key, cancellationToken);
            if (stream is not null)
            {
                manifests.Add((BackupManifest.FromJson(stream), item.Key));
            }
        }

        var entries = new List<CatalogEntry>();
        foreach (var scope in manifests.GroupBy(item => (item.Manifest.EventStore, item.Manifest.Namespace)))
        {
            var runs = scope.ToList();
            var verdicts = policy.Evaluate([.. runs.Select(run => run.Manifest.TakenAt)], now);
            entries.AddRange(runs.Select((run, index) => new CatalogEntry(run.Manifest, run.Key, verdicts[index])));
        }

        return [.. entries.OrderByDescending(entry => entry.Manifest.TakenAt)];
    }
}
