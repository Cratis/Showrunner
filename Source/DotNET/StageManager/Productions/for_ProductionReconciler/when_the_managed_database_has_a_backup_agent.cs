// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_managed_database_has_a_backup_agent : a_production_reconciliation
{
    PerconaServerMongoDB _database;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Database.Backup.Enabled = true;
        return spec;
    }

    async Task Because()
    {
        await Reconcile();
        _database = _cluster.AppliedNamed<PerconaServerMongoDB>("ledger-mongodb")!;
    }

    [Fact] void should_enable_the_backup_agent() => _database.Spec["backup"]!["enabled"]!.GetValue<bool>().ShouldBeTrue();
    [Fact] void should_run_the_pinned_agent_image() => _database.Spec["backup"]!["image"]!.GetValue<string>().ShouldEqual(ProductionDatabaseBackup.DefaultImage);
}
#endif
