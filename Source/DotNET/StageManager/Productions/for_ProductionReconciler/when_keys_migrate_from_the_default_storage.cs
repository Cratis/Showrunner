// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_keys_migrate_from_the_default_storage : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        spec.Vault.MigrateFromDefaultStorage = true;
        return spec;
    }

    void Establish()
    {
        _cluster.StatefulSetsReady = true;
        _cluster.SecretValues["apps/ledger-vault-chronicle/token"] = "e2e-vault-token";
    }

    Task Because() => Reconcile();

    [Fact] void should_tell_chronicle_to_serve_the_keys_of_its_database() => Environment("Cratis__Chronicle__Compliance__Encryption__MigrateFromDefaultStorage")!.Value.ShouldEqual("true");
}
#endif
