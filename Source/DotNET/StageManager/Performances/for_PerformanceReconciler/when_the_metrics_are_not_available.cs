// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_metrics_are_not_available : a_reconciliation
{
    Task Because() => Reconcile();

    [Fact] void should_not_claim_the_usage_was_measured() => _performance.Status.ResourceUsage.Measured.ShouldBeFalse();
}
#endif
