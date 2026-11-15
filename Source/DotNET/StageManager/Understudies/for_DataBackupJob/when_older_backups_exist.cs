// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob.given;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_DataBackupJob;

public class when_older_backups_exist : a_seeded_production
{
    string _tooOld;
    string _recent;

    void Establish()
    {
        _tooOld = BackupLayout.ManifestKey(BackupLayout.RunPrefix("ledger", "Studio", "acme", _now.AddDays(-40)));
        _recent = BackupLayout.ManifestKey(BackupLayout.RunPrefix("ledger", "Studio", "acme", _now.AddHours(-1)));
        SeededBackups.Run(_storage, "ledger", "Studio", "acme", _now.AddDays(-40), BackupType.Events);
        SeededBackups.Run(_storage, "ledger", "Studio", "acme", _now.AddHours(-1), BackupType.Events);
    }

    async Task Because() => await Run();

    [Fact] void should_prune_what_is_older_than_the_maximum_age() => _storage.Has(_tooOld).ShouldBeFalse();
    [Fact] void should_keep_what_a_tier_keeps() => _storage.Has(_recent).ShouldBeTrue();
    [Fact] void should_say_what_was_pruned() => _result.Prune.Pruned.ShouldEqual(1);
}
#endif
