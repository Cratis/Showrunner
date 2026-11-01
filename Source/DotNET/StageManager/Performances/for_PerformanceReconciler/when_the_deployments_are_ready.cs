// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_deployments_are_ready : a_reconciliation
{
    void Establish() => _cluster.DeploymentsReady = true;

    [Fact] void should_be_ready() => _phase.ShouldEqual(PerformancePhase.Ready);
}
#endif
