// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_egress_rules_are_removed : a_reconciliation
{
    void Establish() => _cluster.DeploymentsReady = true;

    Task Because() => Reconcile();

    [Fact] void should_apply_no_egress_policy() => _cluster.AppliedOf<V1NetworkPolicy>().ShouldBeEmpty();
    [Fact] void should_delete_the_policy_that_was_left() => _cluster.Deleted.ShouldContainOnly(["V1NetworkPolicy/crm-egress"]);
}
#endif
