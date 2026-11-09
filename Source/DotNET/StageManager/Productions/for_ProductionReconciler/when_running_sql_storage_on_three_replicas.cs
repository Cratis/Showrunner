// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_running_sql_storage_on_three_replicas : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec() => new()
    {
        Version = "19.148.0",
        Replicas = 3,
        Database = new() { Kind = StorageKinds.PostgreSql, Mode = ProductionModes.External, ConnectionSecretName = "ledger-mongo" }
    };

    Task Because() => Reconcile();

    [Fact] void should_store_in_postgresql() => Environment("Cratis__Chronicle__Storage__Type")!.Value.ShouldEqual("PostgreSql");
    [Fact] void should_form_one_cluster_through_sql() => Environment("Cratis__Chronicle__Clustering__Type")!.Value.ShouldEqual("Sql");
}
#endif
