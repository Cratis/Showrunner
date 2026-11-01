// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_application_is_up_to_date : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _crashed = null!;

    void Because()
    {
        var application = Applications.Create(version: "1.0.0", image: Applications.OldImage, status: Applications.Running());
        _result = ReconcilePlanner.Plan(Applications.Context(application, Applications.Ready(Applications.OldImage)));
        _crashed = ReconcilePlanner.Plan(Applications.Context(application, new ObservedState(new ObservedDeployment(Applications.OldImage, 1, 0, 1, 5, 5, false), true)));
    }

    [Fact] void should_be_running() => Assert.Equal(StageApplicationPhases.Running, _result.Status.Phase);
    [Fact] void should_start_no_rollout() => Assert.Null(_result.Status.Rollout);
    [Fact] void should_keep_applying_the_objects_so_drift_is_repaired() => Assert.NotNull(_result.Apply);
    [Fact] void should_be_ready() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Ready).Status);
    [Fact] void should_not_be_ready_without_a_ready_pod() => Assert.Equal("False", _crashed.Status.Conditions.Single(c => c.Type == ConditionTypes.Ready).Status);
    [Fact] void should_keep_the_phase_when_a_pod_is_not_ready() => Assert.Equal(StageApplicationPhases.Running, _crashed.Status.Phase);
    [Fact] void should_keep_the_transition_time_of_unchanged_conditions() => Assert.Equal(_result.Status.Conditions.Single(c => c.Type == ConditionTypes.Progressing).LastTransitionTime, ReconcilePlanner.Plan(Applications.Context(Applications.Create(version: "1.0.0", image: Applications.OldImage, status: _result.Status), Applications.Ready(Applications.OldImage), Applications.Now.AddHours(1))).Status.Conditions.Single(c => c.Type == ConditionTypes.Progressing).LastTransitionTime);
}
#endif
