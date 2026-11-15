// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_a_backup_job_fails : an_understudy_reconciliation
{
    void Establish() => _cluster.Jobs.Add(Job("ledger-backups-backup-2", UnderstudyJobKinds.Backup, false, _now.AddMinutes(-10), _now.AddMinutes(-8)));

    async Task Because()
    {
        await Reconcile();
        await Reconcile();
    }

    [Fact] void should_publish_that_the_backup_failed() => _sink.Payloads<BackupFailed>().Count().ShouldEqual(1);
    [Fact] void should_identify_the_job() => _sink.Payloads<BackupFailed>().Single().OperationId.Value.ShouldEqual("ledger-backups-backup-2");
    [Fact] void should_say_which_production() => _sink.Payloads<BackupFailed>().Single().Production.Value.ShouldEqual("ledger");
    [Fact] void should_say_when() => _sink.Payloads<BackupFailed>().Single().FailedAt.ShouldEqual(_now.AddMinutes(-8));
    [Fact] void should_say_what_kind_of_job() => _sink.Payloads<BackupFailed>().Single().Kind.ShouldEqual("backup");
    [Fact] void should_say_why_without_a_secret() => _sink.Payloads<BackupFailed>().Single().Reason.ShouldContain("BackoffLimitExceeded");
    [Fact] void should_publish_it_as_a_warning() => _sink.Facts.Single().Warning.ShouldBeTrue();
    [Fact] void should_not_publish_a_backup_as_taken() => _sink.Payloads<UnderstudyBackupTaken>().ShouldBeEmpty();
    [Fact] void should_be_degraded() => _phase.ShouldEqual(UnderstudyPhase.Degraded);
    [Fact] void should_say_the_backups_are_not_healthy() => Condition(UnderstudyConditions.BackupsHealthy).Reason.ShouldEqual("LatestRunFailed");
}
#endif
