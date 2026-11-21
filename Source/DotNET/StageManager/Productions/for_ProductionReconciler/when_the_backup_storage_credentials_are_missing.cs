// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_backup_storage_credentials_are_missing : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Database.Backup.Enabled = true;
        spec.Database.Backup.Storage = new() { Bucket = "ledger-backups", Endpoint = "objects.example.com", CredentialsSecretName = "ledger-backup-credentials" };
        return spec;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_secret() => Condition(ProductionConditions.DatabaseReady).Message.ShouldContain("'ledger-backup-credentials'");
    [Fact] void should_name_the_missing_key() => Condition(ProductionConditions.DatabaseReady).Message.ShouldContain("'AWS_ACCESS_KEY_ID'");
}
#endif
