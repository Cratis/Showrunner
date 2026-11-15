// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_a_backup_job_succeeds : an_understudy_reconciliation
{
    static readonly DateTimeOffset _started = _now.AddMinutes(-10);

    void Establish()
    {
        SeededBackups.Run(Data, "ledger", "Studio", "acme", _started.AddSeconds(5), BackupType.Events, BackupType.ReadModels);
        SeededBackups.Run(Data, "ledger", null, null, _started.AddSeconds(6), BackupType.Cluster);

        // An older run, taken by a job the operator reported before.
        SeededBackups.Run(Data, "ledger", "Studio", "acme", _now.AddHours(-3), BackupType.Events);
        _cluster.Jobs.Add(Job("ledger-backups-backup-1", UnderstudyJobKinds.Backup, true, _started, _now.AddMinutes(-8)));
    }

    async Task Because()
    {
        await Reconcile();
        await Reconcile();
    }

    [Fact] void should_publish_a_fact_for_each_run_of_the_job() => _sink.Payloads<BackupTaken>().Count().ShouldEqual(2);
    [Fact] void should_not_publish_a_run_of_another_job() => _sink.Payloads<BackupTaken>().Any(taken => taken.TakenAt == _now.AddHours(-3)).ShouldBeFalse();
    [Fact] void should_publish_it_exactly_once_however_often_it_reconciles() => _sink.Facts.Count(fact => fact.Type == "BackupTaken").ShouldEqual(2);
    [Fact] void should_identify_the_backup_by_its_place_in_the_catalog() => _sink.Payloads<BackupTaken>().Select(taken => taken.BackupId.Value).ShouldContainOnly(
        [$"ledger/Studio/acme/{BackupLayout.FormatTakenAt(_started.AddSeconds(5))}", $"ledger/_/_/{BackupLayout.FormatTakenAt(_started.AddSeconds(6))}"]);
    [Fact] void should_say_what_the_run_holds() => _sink.Payloads<BackupTaken>().Single(taken => taken.Namespace == "acme").Types.ShouldContainOnly(["Events", "ReadModels"]);
    [Fact] void should_leave_the_scope_empty_when_it_does_not_apply() => _sink.Payloads<BackupTaken>().Single(taken => taken.EventStore.Length == 0).Namespace.Length.ShouldEqual(0);
    [Fact] void should_give_each_fact_a_stable_id() => _sink.Facts.Select(fact => fact.Id).ShouldContainOnly([$"BackupTaken:ledger/Studio/acme/{BackupLayout.FormatTakenAt(_started.AddSeconds(5))}", $"BackupTaken:ledger/_/_/{BackupLayout.FormatTakenAt(_started.AddSeconds(6))}"]);
    [Fact] void should_remember_that_it_reported_the_job() => _understudy.Status.ReportedJobs.ShouldContainOnly(["ledger-backups-backup-1"]);
}
#endif
