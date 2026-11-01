// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_application_is_new : Specification
{
    ReconcilePlan _result = null!;

    void Because() => _result = ReconcilePlanner.Plan(Applications.Context(Applications.Create(withFinalizer: false)));

    [Fact] void should_add_the_retention_finalizer() => Assert.True(_result.EnsureFinalizer);
    [Fact] void should_apply_the_objects_with_the_requested_image() => Assert.Equal(Applications.NewImage, _result.Apply!.Deployment.Spec.Template.Spec.Containers[0].Image);
    [Fact] void should_expose_the_application() => Assert.NotNull(_result.Apply!.Ingress);
    [Fact] void should_start_a_rollout_of_the_requested_version() => Assert.Equal(("1.1.0", Applications.NewImage, false), (_result.Status.Rollout!.TargetVersion, _result.Status.Rollout.TargetImage, _result.Status.Rollout.IsRollback));
    [Fact] void should_be_pending_until_the_deployment_exists() => Assert.Equal(StageApplicationPhases.Pending, _result.Status.Phase);
    [Fact] void should_say_that_it_is_progressing() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Progressing).Status);
    [Fact] void should_report_the_url() => Assert.Equal("https://orders.cratis.app", _result.Status.Url);
    [Fact] void should_report_no_expiry_for_a_permanent_application() => Assert.Equal(string.Empty, _result.Status.ExpiresAt);
    [Fact] void should_report_the_observed_generation() => Assert.Equal(3, _result.Status.ObservedGeneration);
    [Fact] void should_look_again_soon_to_follow_the_rollout() => Assert.Equal(TimeSpan.FromSeconds(5), _result.RequeueAfter);
    [Fact] void should_not_stop_or_delete_anything() => Assert.False(_result.StopWorkloads || _result.DeleteIngress || _result.DeleteData || _result.RemoveFinalizer);
}
#endif
