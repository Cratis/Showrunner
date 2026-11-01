// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_resource_is_deleted : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _afterwards = null!;
    ReconcilePlan _afterRetention = null!;

    void Because()
    {
        var deletedAt = Applications.Now.AddMinutes(-1);
        var application = Applications.Create(status: Applications.Running(), deletedAt: deletedAt);
        var running = Applications.Ready(Applications.NewImage);
        _result = ReconcilePlanner.Plan(Applications.Context(application, running));

        application.Status = _result.Status;
        _afterwards = ReconcilePlanner.Plan(Applications.Context(application, running, Applications.Now.AddDays(3)));
        _afterRetention = ReconcilePlanner.Plan(Applications.Context(application, running, Applications.Now.AddDays(8)));
    }

    [Fact] void should_be_stopped() => Assert.Equal(StageApplicationPhases.Stopped, _result.Status.Phase);
    [Fact] void should_stop_the_workloads() => Assert.True(_result.StopWorkloads);
    [Fact] void should_remove_the_ingress() => Assert.True(_result.DeleteIngress);
    [Fact] void should_apply_nothing() => Assert.Null(_result.Apply);
    [Fact] void should_hold_the_resource_with_the_finalizer() => Assert.False(_result.RemoveFinalizer);
    [Fact] void should_keep_the_data_for_the_retention_period() => Assert.False(_result.DeleteData);
    [Fact] void should_retain_the_data_from_the_time_of_deletion() => Assert.Equal(Rfc3339.Format(Applications.Now.AddMinutes(-1).AddDays(7)), _result.Status.DataRetainedUntil);
    [Fact] void should_not_move_the_retention_on_later_reconciles() => Assert.Equal(_result.Status.DataRetainedUntil, _afterwards.Status.DataRetainedUntil);
    [Fact] void should_release_nothing_before_the_retention_has_passed() => Assert.False(_afterwards.RemoveFinalizer);
    [Fact] void should_delete_the_data_when_the_retention_has_passed() => Assert.True(_afterRetention.DeleteData);
    [Fact] void should_release_the_finalizer_when_the_retention_has_passed() => Assert.True(_afterRetention.RemoveFinalizer);
    [Fact] void should_not_add_the_finalizer_to_a_deleted_resource() => Assert.False(_result.EnsureFinalizer);
}
#endif
