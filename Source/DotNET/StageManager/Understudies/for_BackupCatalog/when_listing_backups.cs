// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupCatalog;

public class when_listing_backups : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    InMemoryObjectStorage _storage;
    IReadOnlyList<CatalogEntry> _entries;

    void Establish()
    {
        _storage = new();
        SeededBackups.Run(_storage, "ledger", "Studio", "acme", _now.AddHours(-2), BackupType.Events, BackupType.ReadModels);
        SeededBackups.Run(_storage, "ledger", "Studio", "acme", _now.AddHours(-1), BackupType.Events, BackupType.ReadModels);
        SeededBackups.Run(_storage, "ledger", "Studio", null, _now.AddHours(-1), BackupType.EventStore);
        SeededBackups.Run(_storage, "ledger", null, null, _now.AddHours(-1), BackupType.Cluster);
        SeededBackups.Run(_storage, "other", "Studio", "acme", _now.AddHours(-1), BackupType.Events);

        // A run that never got its manifest: it is not a backup.
        _storage.Seed(BackupLayout.RunPrefix("ledger", "Studio", "acme", _now.AddMinutes(-5)) + "Events.archive.gz", [1, 2, 3]);
        _storage.Seed("ledger/plan.json", [123]);
    }

    async Task Because() => _entries = await new BackupCatalog(_storage).ListAsync("ledger", new RetentionPolicy(), _now, CancellationToken.None);

    [Fact] void should_list_every_run_of_the_production_from_its_manifest() => _entries.Count.ShouldEqual(4);
    [Fact] void should_list_the_newest_first() => _entries.Select(entry => entry.Manifest.TakenAt).ShouldEqual(_entries.Select(entry => entry.Manifest.TakenAt).OrderDescending());
    [Fact] void should_not_list_the_runs_of_another_production() => _entries.All(entry => entry.Manifest.Production == "ledger").ShouldBeTrue();
    [Fact] void should_not_list_a_run_without_a_manifest() => _entries.Any(entry => entry.Manifest.TakenAt == _now.AddMinutes(-5)).ShouldBeFalse();
    [Fact] void should_list_runs_of_every_level() => _entries.Select(entry => (entry.Manifest.EventStore, entry.Manifest.Namespace)).Distinct().Count().ShouldEqual(3);
    [Fact] void should_know_the_files_of_a_run_from_its_manifest() => _entries.Single(entry => entry.Manifest.Namespace == "acme" && entry.Manifest.TakenAt == _now.AddHours(-1)).Manifest.Entries.Select(item => item.Type).ShouldContainOnly([BackupType.Events, BackupType.ReadModels]);
    [Fact] void should_say_what_the_policy_keeps() => _entries.All(entry => entry.Verdict.Keep).ShouldBeTrue();
    [Fact] void should_say_where_the_files_of_a_run_are() => _entries[0].RunPrefix.ShouldEqual(_entries[0].ManifestKey[..^"manifest.json".Length]);
}
#endif
