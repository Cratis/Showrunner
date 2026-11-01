// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_rollout_times_out_with_a_previous_version : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Phase = StageApplicationPhases.Deploying,
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.BrokenImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-6)) }
        };
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(image: Applications.BrokenImage, status: status), Applications.Updating(Applications.BrokenImage)));
    }

    [Fact] void should_start_rolling_back() => Assert.Equal(StageApplicationPhases.RollingBack, _result.Status.Phase);
    [Fact] void should_roll_back_to_the_previous_image() => Assert.Equal((Applications.OldImage, "1.0.0", true), (_result.Status.Rollout!.TargetImage, _result.Status.Rollout.TargetVersion, _result.Status.Rollout.IsRollback));
    [Fact] void should_apply_the_previous_image_at_once() => Assert.Equal(Applications.OldImage, _result.Apply!.Deployment.Spec.Template.Spec.Containers[0].Image);
    [Fact] void should_record_the_failed_version_as_rolled_back() => Assert.Equal(("1.1.0", StageRolloutOutcomes.RolledBack), (_result.Status.Versions[^1].Version, _result.Status.Versions[^1].Outcome));
    [Fact] void should_say_why() => Assert.Contains("did not become ready", _result.Status.Versions[^1].Reason);
    [Fact] void should_report_the_application_as_degraded() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Degraded).Status);
}
#endif
