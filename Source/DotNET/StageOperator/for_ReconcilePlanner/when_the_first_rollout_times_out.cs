// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_first_rollout_times_out : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _afterwards = null!;

    void Because()
    {
        var status = new StageApplicationStatus
        {
            Phase = StageApplicationPhases.Deploying,
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.BrokenImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-6)) }
        };
        var observed = new ObservedState(new ObservedDeployment(Applications.BrokenImage, 1, 0, 1, 1, 1, false), true);
        var application = Applications.Create(image: Applications.BrokenImage, status: status);
        _result = ReconcilePlanner.Plan(Applications.Context(application, observed));

        application.Status = _result.Status;
        _afterwards = ReconcilePlanner.Plan(Applications.Context(application, observed, Applications.Now.AddMinutes(1)));
    }

    [Fact] void should_fail() => Assert.Equal(StageApplicationPhases.Failed, _result.Status.Phase);
    [Fact] void should_record_the_failure() => Assert.Equal(StageRolloutOutcomes.Failed, _result.Status.Versions.Single().Outcome);
    [Fact] void should_end_the_rollout() => Assert.Null(_result.Status.Rollout);
    [Fact] void should_not_start_the_same_rollout_again() => Assert.Null(_afterwards.Status.Rollout);
    [Fact] void should_stay_failed() => Assert.Equal(StageApplicationPhases.Failed, _afterwards.Status.Phase);
}
#endif
