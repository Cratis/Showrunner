// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_history_grows : Specification
{
    ReconcilePlan _result = null!;

    void Because()
    {
        var versions = Enumerable.Range(0, 20).Select(i => new StageApplicationVersion { Version = $"0.{i}.0", Image = $"image-{i}", Outcome = StageRolloutOutcomes.RolledOut }).ToList();
        var status = new StageApplicationStatus
        {
            Phase = StageApplicationPhases.Deploying,
            RunningVersion = "0.19.0",
            RunningImage = "image-19",
            Versions = versions,
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.NewImage, StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-1)) }
        };
        _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(status: status), Applications.Ready(Applications.NewImage)));
    }

    [Fact] void should_keep_at_most_twenty_entries() => Assert.Equal(20, _result.Status.Versions.Count);
    [Fact] void should_drop_the_oldest() => Assert.Equal("0.1.0", _result.Status.Versions[0].Version);
    [Fact] void should_keep_the_newest_last() => Assert.Equal("1.1.0", _result.Status.Versions[^1].Version);
}
#endif
