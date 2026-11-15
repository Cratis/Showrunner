// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// Removes the backups no retention tier keeps, and everything older than the maximum age. The manifest goes first, so a run that is half removed is already
/// out of the catalog; the files that remain belong to no manifest and are removed by the next pass.
/// </summary>
/// <param name="storage">The storage.</param>
/// <param name="catalog">The catalog.</param>
public class BackupPruner(IObjectStorage storage, BackupCatalog catalog)
{
    /// <summary>
    /// How long files without a manifest are left alone, so a run that is still writing is not mistaken for a failed one.
    /// </summary>
    public static readonly TimeSpan OrphanGracePeriod = TimeSpan.FromDays(1);

    /// <summary>
    /// Prunes the backups of a Production.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="policy">The retention policy.</param>
    /// <param name="now">The current time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What was done.</returns>
    public async Task<PruneResult> PruneAsync(string production, RetentionPolicy policy, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var entries = await catalog.ListAsync(production, policy, now, cancellationToken);
        var pruned = 0;
        foreach (var entry in entries.Where(entry => !entry.Verdict.Keep))
        {
            await storage.DeleteAsync(entry.ManifestKey, cancellationToken);
            await DeleteRunAsync(entry.RunPrefix, cancellationToken);
            pruned++;
        }

        var orphans = await RemoveOrphansAsync(production, entries, now, cancellationToken);
        return new(entries.Count - pruned, pruned, orphans);
    }

    async Task DeleteRunAsync(string runPrefix, CancellationToken cancellationToken)
    {
        var keys = new List<string>();
        await foreach (var item in storage.ListAsync(runPrefix, cancellationToken))
        {
            keys.Add(item.Key);
        }

        foreach (var key in keys)
        {
            await storage.DeleteAsync(key, cancellationToken);
        }
    }

    async Task<int> RemoveOrphansAsync(string production, IReadOnlyList<CatalogEntry> entries, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var known = entries.Select(entry => entry.RunPrefix).ToHashSet(StringComparer.Ordinal);
        var orphans = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        await foreach (var item in storage.ListAsync(BackupLayout.ProductionPrefix(production), cancellationToken))
        {
            var run = BackupLayout.RunPrefixOf(item.Key);
            if (run is null || known.Contains(run) || now - item.LastModified < OrphanGracePeriod)
            {
                continue;
            }

            if (!orphans.TryGetValue(run, out var keys))
            {
                orphans[run] = keys = [];
            }

            keys.Add(item.Key);
        }

        foreach (var key in orphans.Values.SelectMany(keys => keys))
        {
            await storage.DeleteAsync(key, cancellationToken);
        }

        return orphans.Count;
    }
}
