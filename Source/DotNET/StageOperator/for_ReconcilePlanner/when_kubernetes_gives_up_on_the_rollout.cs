// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_kubernetes_gives_up_on_the_rollout : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Phase = StageApplicationPhases.Deploying,
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.BrokenImage, StartedAt = Rfc3339.Format(Applications.Now.AddSeconds(-30)) }
        };
        var observed = new ObservedState(new ObservedDeployment(Applications.BrokenImage, 2, 1, 1, 6, 6, true), true);
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(image: Applications.BrokenImage, status: status), observed));
    }

    [Fact] void should_roll_back_before_the_timeout_has_passed() => Assert.Equal(StageApplicationPhases.RollingBack, _result.Status.Phase);
}
#endif
