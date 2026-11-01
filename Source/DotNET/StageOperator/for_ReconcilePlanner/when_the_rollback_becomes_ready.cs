// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_rollback_becomes_ready : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _afterwards = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Phase = StageApplicationPhases.RollingBack,
            Versions =
            [
                .. Applications.Running().Versions,
                new StageApplicationVersion { Version = "1.1.0", Image = Applications.BrokenImage, Outcome = StageRolloutOutcomes.RolledBack, Reason = "did not become ready" }
            ],
            Rollout = new StageApplicationRollout { TargetVersion = "1.0.0", TargetImage = Applications.OldImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-1)), IsRollback = true }
        };
        var application = Applications.Create(image: Applications.BrokenImage, status: status);
        _result = ReconcilePlanner.Plan(Applications.Context(application, Applications.Ready(Applications.OldImage)));

        application.Status = _result.Status;
        _afterwards = ReconcilePlanner.Plan(Applications.Context(application, Applications.Ready(Applications.OldImage), Applications.Now.AddMinutes(10)));
    }

    [Fact] void should_run_the_previous_version_again() => Assert.Equal((StageApplicationPhases.Running, "1.0.0", Applications.OldImage), (_result.Status.Phase, _result.Status.RunningVersion, _result.Status.RunningImage));
    [Fact] void should_end_the_rollout() => Assert.Null(_result.Status.Rollout);
    [Fact] void should_stay_degraded_so_the_failure_is_visible() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Degraded).Status);
    [Fact] void should_keep_the_rolled_back_entry_last() => Assert.Equal(StageRolloutOutcomes.RolledBack, _result.Status.Versions[^1].Outcome);
    [Fact] void should_hold_the_failed_version_back_instead_of_rolling_it_out_again() => Assert.Equal((Applications.OldImage, null), (_afterwards.Apply!.Deployment.Spec.Template.Spec.Containers[0].Image, _afterwards.Status.Rollout));
    [Fact] void should_keep_running_the_previous_version_while_held_back() => Assert.Equal((StageApplicationPhases.Running, "1.0.0"), (_afterwards.Status.Phase, _afterwards.Status.RunningVersion));
}
#endif
