// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_a_later_backup_job_succeeds_after_a_failed_one : an_understudy_reconciliation
{
    void Establish()
    {
        _cluster.Jobs.Add(Job("ledger-backups-backup-2", UnderstudyJobKinds.Backup, false, _now.AddMinutes(-70), _now.AddMinutes(-68)));
        SeededBackups.Run(Data, "ledger", "Studio", "acme", _now.AddMinutes(-9), BackupType.Events);
        _cluster.Jobs.Add(Job("ledger-backups-backup-3", UnderstudyJobKinds.Backup, true, _now.AddMinutes(-10), _now.AddMinutes(-8)));
    }

    async Task Because() => await Reconcile();

    [Fact] void should_have_reported_both() => _sink.Facts.Select(fact => fact.Type).ShouldContainOnly(["BackupFailed", "UnderstudyBackupTaken"]);
    [Fact] void should_be_healthy_again() => _phase.ShouldEqual(UnderstudyPhase.Healthy);
}
#endif
