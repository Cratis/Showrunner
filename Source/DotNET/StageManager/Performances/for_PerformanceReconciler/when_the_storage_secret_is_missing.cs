// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;
using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_storage_secret_is_missing : a_reconciliation
{
    protected override Performance CreatePerformance() => PerformanceSamples.PublishedPerformance();

    ResourceCondition Condition => _performance.Status.Conditions.Single(condition => condition.Type == "StorageConnection");

    void Establish() => _cluster.SecretValues.Remove("apps/crm-storage/connectionString");

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_say_which_condition_fails() => Condition.Status.ShouldEqual("False");
    [Fact] void should_say_why() => Condition.Reason.ShouldEqual("SecretNotFound");
    [Fact] void should_apply_nothing() => _cluster.Applied.Count.ShouldEqual(0);
}
#endif
