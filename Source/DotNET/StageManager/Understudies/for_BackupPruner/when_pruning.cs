// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPruner;

/// <summary>
/// An hourly backup of one namespace for 40 days, under the default tiers and a maximum age of 35 days.
/// </summary>
public class when_pruning : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    InMemoryObjectStorage _storage;
    PruneResult _result;
    IReadOnlyList<CatalogEntry> _after;
    List<DateTimeOffset> _before;

    void Establish()
    {
        _storage = new();
        _before = [.. Enumerable.Range(0, 40 * 24).Select(hours => new DateTimeOffset(2026, 3, 15, 12, 5, 0, TimeSpan.Zero).AddHours(-hours))];
        foreach (var takenAt in _before)
        {
            SeededBackups.Run(_storage, "ledger", "Studio", "acme", takenAt, BackupType.Events);
        }

        SeededBackups.Run(_storage, "other", "Studio", "acme", _now.AddDays(-39), BackupType.Events);
    }

    async Task Because()
    {
        var catalog = new BackupCatalog(_storage);
        _result = await new BackupPruner(_storage, catalog).PruneAsync("ledger", new RetentionPolicy(), _now, CancellationToken.None);
        _after = await catalog.ListAsync("ledger", new RetentionPolicy(), _now, CancellationToken.None);
    }

    [Fact] void should_keep_what_the_tiers_keep() => _result.Kept.ShouldEqual(_after.Count);
    [Fact] void should_keep_no_more_than_the_tiers_allow() => _after.Count.ShouldBeLessThan(24 + 7 + 4 + 1);
    [Fact] void should_remove_what_no_tier_keeps() => _result.Pruned.ShouldEqual(_before.Count - _after.Count);
    [Fact] void should_leave_nothing_older_than_the_maximum_age() => _after.Any(entry => entry.Manifest.TakenAt <= _now.AddDays(-35)).ShouldBeFalse();
    [Fact] void should_keep_the_newest_backup() => _after.Any(entry => entry.Manifest.TakenAt == _before[0]).ShouldBeTrue();
    [Fact] void should_remove_the_files_of_a_pruned_run_with_its_manifest() => _storage.Keys.Count(key => key.StartsWith("ledger/", StringComparison.Ordinal)).ShouldEqual(_after.Count * 2);
    [Fact] void should_leave_another_production_alone() => _storage.Keys.Count(key => key.StartsWith("other/", StringComparison.Ordinal)).ShouldEqual(2);
}
#endif
