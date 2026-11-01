// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_a_new_version_replaces_a_failed_one : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Versions =
            [
                .. Applications.Running().Versions,
                new StageApplicationVersion { Version = "1.1.0", Image = Applications.BrokenImage, Outcome = StageRolloutOutcomes.RolledBack }
            ]
        };
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(version: "1.2.0", image: Applications.NewImage, status: status), Applications.Ready(Applications.OldImage)));
    }

    [Fact] void should_roll_the_new_version_out() => Assert.Equal(("1.2.0", Applications.NewImage, false), (_result.Status.Rollout!.TargetVersion, _result.Status.Rollout.TargetImage, _result.Status.Rollout.IsRollback));
    [Fact] void should_apply_the_new_image() => Assert.Equal(Applications.NewImage, _result.Apply!.Deployment.Spec.Template.Spec.Containers[0].Image);
}
#endif
