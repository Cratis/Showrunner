// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_a_database_is_not_claimed : an_understudy_reconciliation
{
    void Establish() => Data.Seed("ledger/plan.json", new PlanReport(_now, [], ["legacy", "scratch"], ["admin"]).ToJson());

    async Task Because() => await Reconcile();

    [Fact] void should_report_it_in_the_status() => _understudy.Status.Unclaimed.ShouldContainOnly(["legacy", "scratch"]);
}
#endif
