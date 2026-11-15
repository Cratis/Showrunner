// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPruner;

public class when_a_failed_run_left_files_without_a_manifest : Specification
{
    static readonly DateTimeOffset _now = new(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);

    InMemoryObjectStorage _storage;
    PruneResult _result;
    string _old;
    string _recent;

    void Establish()
    {
        _storage = new();
        _old = BackupLayout.RunPrefix("ledger", "Studio", "acme", _now.AddDays(-3)) + "Events.archive.gz";
        _recent = BackupLayout.RunPrefix("ledger", "Studio", "acme", _now.AddMinutes(-20)) + "Events.archive.gz";
        _storage.Seed(_old, [1], _now.AddDays(-3));
        _storage.Seed(_recent, [1], _now.AddMinutes(-20));
        _storage.Seed("ledger/plan.json", [1], _now.AddDays(-9));
    }

    async Task Because() => _result = await new BackupPruner(_storage, new BackupCatalog(_storage)).PruneAsync("ledger", new RetentionPolicy(), _now, CancellationToken.None);

    [Fact] void should_remove_the_orphaned_run() => _storage.Has(_old).ShouldBeFalse();
    [Fact] void should_count_it() => _result.Orphans.ShouldEqual(1);
    [Fact] void should_leave_a_run_that_may_still_be_writing() => _storage.Has(_recent).ShouldBeTrue();
    [Fact] void should_leave_the_plan_report() => _storage.Has("ledger/plan.json").ShouldBeTrue();
}
#endif
