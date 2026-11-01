// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_rollout_is_still_waiting : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Phase = StageApplicationPhases.Deploying,
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.NewImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-2)) }
        };
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(status: status), Applications.Updating(Applications.NewImage)));
    }

    [Fact] void should_keep_deploying() => Assert.Equal(StageApplicationPhases.Deploying, _result.Status.Phase);
    [Fact] void should_keep_the_rollout() => Assert.Equal("1.1.0", _result.Status.Rollout!.TargetVersion);
    [Fact] void should_keep_applying_the_new_image() => Assert.Equal(Applications.NewImage, _result.Apply!.Deployment.Spec.Template.Spec.Containers[0].Image);
    [Fact] void should_keep_reporting_the_old_version_as_running() => Assert.Equal("1.0.0", _result.Status.RunningVersion);
    [Fact] void should_keep_the_old_pod_serving() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Ready).Status);
    [Fact] void should_not_add_to_the_history() => Assert.Single(_result.Status.Versions);
}
#endif
