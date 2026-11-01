// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_lifetime_has_not_ended : a_reconciliation
{
    DateTimeOffset _until;

    protected override Performance CreatePerformance()
    {
        _until = DateTimeOffset.UtcNow.AddDays(30);
        var performance = PerformanceSamples.PublishedPerformance();
        performance.Spec.Lifetime = new() { Until = _until };
        return performance;
    }

    void Establish() => _cluster.DeploymentsReady = true;

    Task Because() => Reconcile();

    [Fact] void should_run_as_usual() => _phase.ShouldEqual(PerformancePhase.Ready);
    [Fact] void should_report_when_it_expires() => _performance.Status.ExpiresAt.ShouldEqual(_until);
    [Fact] void should_not_say_when_data_is_kept() => _performance.Status.DataRetainedUntil.ShouldBeNull();
    [Fact] void should_not_remove_the_route() => _cluster.Deleted.Count.ShouldEqual(0);
}
#endif
