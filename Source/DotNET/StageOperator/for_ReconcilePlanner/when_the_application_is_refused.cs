// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_application_is_refused : Specification
{
    ReconcilePlan _result = null!;

    void Because() => _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(withAuthentication: false, withFinalizer: false)));

    [Fact] void should_fail() => Assert.Equal(StageApplicationPhases.Failed, _result.Status.Phase);
    [Fact] void should_apply_nothing() => Assert.Null(_result.Apply);
    [Fact] void should_not_take_a_finalizer_for_data_it_never_created() => Assert.False(_result.EnsureFinalizer);
    [Fact] void should_say_authentication_is_missing() => Assert.Equal("AuthenticationMissing", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Degraded).Reason);
    [Fact] void should_not_start_a_rollout() => Assert.Null(_result.Status.Rollout);
}
#endif
