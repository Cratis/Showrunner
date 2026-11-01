// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_rollout_becomes_ready : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Phase = StageApplicationPhases.Deploying,
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.NewImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-1)) }
        };
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(status: status), Applications.Ready(Applications.NewImage)));
    }

    [Fact] void should_be_running() => Assert.Equal(StageApplicationPhases.Running, _result.Status.Phase);
    [Fact] void should_report_the_running_version() => Assert.Equal(("1.1.0", Applications.NewImage), (_result.Status.RunningVersion, _result.Status.RunningImage));
    [Fact] void should_clear_the_rollout() => Assert.Null(_result.Status.Rollout);
    [Fact] void should_record_the_version_in_the_history() => Assert.Equal(("1.1.0", StageRolloutOutcomes.RolledOut), (_result.Status.Versions[^1].Version, _result.Status.Versions[^1].Outcome));
    [Fact] void should_keep_the_earlier_version_in_the_history() => Assert.Equal(2, _result.Status.Versions.Count);
    [Fact] void should_be_ready() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Ready).Status);
    [Fact] void should_no_longer_be_progressing() => Assert.Equal("False", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Progressing).Status);
}
#endif
