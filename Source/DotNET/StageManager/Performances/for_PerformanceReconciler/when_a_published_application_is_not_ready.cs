// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_a_published_application_is_not_ready : a_reconciliation
{
    protected override Performance CreatePerformance() => PerformanceSamples.PublishedPerformance();

    Task Because() => Reconcile();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(PerformancePhase.Progressing);
    [Fact] void should_wait_for_the_rollout() => _performance.Status.Rollout.Version.ShouldEqual("1.4.0");
    [Fact] void should_not_report_a_version_as_rolled_out() => _performance.Status.Versions.Count.ShouldEqual(0);
}
#endif
