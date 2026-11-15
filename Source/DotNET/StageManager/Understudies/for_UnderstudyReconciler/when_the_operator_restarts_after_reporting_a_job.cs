// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_operator_restarts_after_reporting_a_job : an_understudy_reconciliation
{
    void Establish()
    {
        SeededBackups.Run(Data, "ledger", "Studio", "acme", _now.AddMinutes(-9), BackupType.Events);
        _cluster.Jobs.Add(Job("ledger-backups-backup-1", UnderstudyJobKinds.Backup, true, _now.AddMinutes(-10), _now.AddMinutes(-8)));
        _understudy.Status = new() { ReportedJobs = ["ledger-backups-backup-1"] };
    }

    async Task Because() => await Reconcile();

    [Fact] void should_not_publish_the_fact_again() => _sink.Facts.ShouldBeEmpty();
}
#endif
