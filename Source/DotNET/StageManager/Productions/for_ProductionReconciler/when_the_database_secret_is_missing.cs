// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_database_secret_is_missing : a_production_reconciliation
{
    void Establish() => _cluster.SecretValues.Remove("apps/ledger-mongo/connectionString");

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_secret() => Condition(ProductionConditions.DatabaseReady).Message.ShouldContain("'ledger-mongo'");
    [Fact] void should_report_no_connection_secret() => _production.Status.ConnectionSecretName.ShouldEqual(string.Empty);
}
#endif
