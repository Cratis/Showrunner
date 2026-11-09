// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_database_mode_changes : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec() => ProductionSamples.Managed();

    void Establish() => _production.Status = new() { Database = "MongoDB/External", Graph = "None/Community" };

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_refuse_the_change() => Condition(ProductionConditions.DatabaseModeUnchanged).Reason.ShouldEqual("ModeChangeRefused");
    [Fact] void should_name_the_field() => Condition(ProductionConditions.DatabaseModeUnchanged).Message.ShouldContain("database.mode from External to Managed");
    [Fact] void should_keep_what_it_runs_with() => _production.Status.Database.ShouldEqual("MongoDB/External");
}
#endif
