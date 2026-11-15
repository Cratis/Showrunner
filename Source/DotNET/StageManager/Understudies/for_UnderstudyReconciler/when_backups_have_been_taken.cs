// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_backups_have_been_taken : an_understudy_reconciliation
{
    void Establish()
    {
        SeededBackups.Run(Data, "ledger", "Studio", "acme", _now.AddHours(-2), BackupType.Events, BackupType.ReadModels);
        SeededBackups.Run(Data, "ledger", "Studio", "acme", _now.AddHours(-1), BackupType.Events, BackupType.ReadModels);
        SeededBackups.Run(Data, "ledger", "Studio", "globex", _now.AddHours(-1), BackupType.Events, BackupType.ReadModels);
        SeededBackups.Run(Data, "ledger", "Studio", null, _now.AddHours(-1), BackupType.EventStore);
        SeededBackups.Run(Data, "ledger", null, null, _now.AddMinutes(-30), BackupType.Cluster);
    }

    async Task Because() => await Reconcile();

    LastBackupStatus Last(BackupType type) => _understudy.Status.LastBackups.Single(last => last.Type == type);

    [Fact] void should_report_the_last_backup_of_each_type() => _understudy.Status.LastBackups.Select(last => last.Type).ShouldContainOnly(
        [BackupType.Events, BackupType.ReadModels, BackupType.EventStore, BackupType.Cluster]);
    [Fact] void should_say_when_it_was_taken() => Last(BackupType.Events).TakenAt.ShouldEqual(_now.AddHours(-1));
    [Fact] void should_say_when_the_production_level_backup_was_taken() => Last(BackupType.Cluster).TakenAt.ShouldEqual(_now.AddMinutes(-30));
    [Fact] void should_count_the_latest_run_of_each_namespace_only() => Last(BackupType.Events).Files.ShouldEqual(2);
    [Fact] void should_sum_the_size_of_the_latest_runs() => Last(BackupType.Events).SizeBytes.ShouldBeGreaterThan(0L);
    [Fact] void should_be_healthy() => _phase.ShouldEqual(UnderstudyPhase.Healthy);
    [Fact] void should_say_the_backups_are_healthy() => Condition(UnderstudyConditions.BackupsHealthy).Status.ShouldEqual("True");
}
#endif
