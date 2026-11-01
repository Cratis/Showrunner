// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_rollback_times_out : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var status = Applications.Running() with
        {
            Phase = StageApplicationPhases.RollingBack,
            Versions =
            [
                .. Applications.Running().Versions,
                new StageApplicationVersion { Version = "1.1.0", Image = Applications.BrokenImage, Outcome = StageRolloutOutcomes.RolledBack }
            ],
            Rollout = new StageApplicationRollout { TargetVersion = "1.0.0", TargetImage = Applications.OldImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-10)), IsRollback = true }
        };
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(image: Applications.BrokenImage, status: status), Applications.Updating(Applications.OldImage)));
    }

    [Fact] void should_fail() => Assert.Equal(StageApplicationPhases.Failed, _result.Status.Phase);
    [Fact] void should_end_the_rollout() => Assert.Null(_result.Status.Rollout);
    [Fact] void should_say_the_rollback_failed() => Assert.Equal("RollbackFailed", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Degraded).Reason);
}
#endif
