// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_spec_is_invalid : a_reconciliation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.AuthProxy.Enabled = false;
        return performance;
    }

    [Fact] void should_be_blocked() => _phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.Count.ShouldEqual(0);
    [Fact] void should_say_the_spec_is_not_valid() => _performance.Status.Conditions.Single(condition => condition.Type == "SpecValid").Status.ShouldEqual("False");
}
#endif
