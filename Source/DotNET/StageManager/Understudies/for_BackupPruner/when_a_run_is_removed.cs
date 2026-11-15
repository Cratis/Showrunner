// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPruner;

/// <summary>
/// The manifest goes first, so a run that is half removed is already out of the catalog.
/// </summary>
public class when_a_run_is_removed : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    InMemoryObjectStorage _storage;

    void Establish()
    {
        _storage = new();
        SeededBackups.Run(_storage, "ledger", "Studio", "acme", _now.AddDays(-40), BackupType.Events, BackupType.ReadModels);
    }

    async Task Because() => await new BackupPruner(_storage, new BackupCatalog(_storage)).PruneAsync("ledger", new RetentionPolicy(), _now, CancellationToken.None);

    [Fact] void should_delete_the_manifest_before_the_files() => _storage.Deletes[0].EndsWith("manifest.json", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_delete_every_file_of_the_run() => _storage.Deletes.Count.ShouldEqual(3);
    [Fact] void should_leave_nothing_of_the_run() => _storage.Keys.ShouldBeEmpty();
}
#endif
