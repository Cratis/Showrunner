// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_managed_database_is_ready_without_its_users : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec() => ProductionSamples.Managed();

    void Establish() => _cluster.MongoDBState = ManagedMongoDB.ReadyState;

    Task Because() => Reconcile();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_say_the_users_are_not_found() => Condition(ProductionConditions.DatabaseReady).Reason.ShouldEqual("UsersNotFound");
    [Fact] void should_name_the_users_secret() => Condition(ProductionConditions.DatabaseReady).Message.ShouldContain("ledger-mongodb-users");
    [Fact] void should_not_write_the_database_connection_secret() => _cluster.AppliedNamed<V1Secret>("ledger-mongodb-connection").ShouldBeNull();
    [Fact] void should_make_chronicle_wait() => Condition(ProductionConditions.ChronicleReady).Reason.ShouldEqual("WaitingForDependencies");
}
#endif
