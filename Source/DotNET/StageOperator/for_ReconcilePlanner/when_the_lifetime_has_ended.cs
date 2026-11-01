// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_lifetime_has_ended : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _shortlyAfter = null!;
    ReconcilePlan _afterRetention = null!;
    ReconcilePlan _dataGone = null!;
    ReconcilePlan _extended = null!;
    DateTimeOffset _until;

    void Because()
    {
        _until = Applications.Now.AddMinutes(-1);
        var application = Applications.Create(status: Applications.Running(), permanent: false, until: _until);
        var running = Applications.Ready(Applications.NewImage);
        _result = ReconcilePlanner.Plan(Applications.Context(application, running));
        _shortlyAfter = ReconcilePlanner.Plan(Applications.Context(application, running, Applications.Now.AddDays(3)));
        _afterRetention = ReconcilePlanner.Plan(Applications.Context(application, running, Applications.Now.AddDays(8)));

        application.Status = _afterRetention.Status;
        _dataGone = ReconcilePlanner.Plan(Applications.Context(application, new ObservedState(null, false), Applications.Now.AddDays(9)));

        var extended = Applications.Create(version: "1.0.0", image: Applications.OldImage, status: _result.Status, permanent: false, until: Applications.Now.AddDays(30));
        _extended = ReconcilePlanner.Plan(Applications.Context(extended, new ObservedState(null, true)));
    }

    [Fact] void should_be_expired() => Assert.Equal(StageApplicationPhases.Expired, _result.Status.Phase);
    [Fact] void should_stop_the_application() => Assert.True(_result.StopWorkloads);
    [Fact] void should_free_the_host() => Assert.True(_result.DeleteIngress);
    [Fact] void should_apply_nothing() => Assert.Null(_result.Apply);
    [Fact] void should_keep_the_data() => Assert.False(_result.DeleteData);
    [Fact] void should_say_that_it_expired() => Assert.Equal("True", _result.Status.Conditions.Single(c => c.Type == ConditionTypes.Expired).Status);
    [Fact] void should_report_when_it_expired() => Assert.Equal(Rfc3339.Format(_until), _result.Status.ExpiresAt);
    [Fact] void should_keep_the_data_for_the_retention_period() => Assert.Equal(Rfc3339.Format(_until.AddDays(7)), _result.Status.DataRetainedUntil);
    [Fact] void should_look_again_when_the_retention_ends() => Assert.Equal(TimeSpan.FromDays(7) - TimeSpan.FromMinutes(1), _result.RequeueAfter);
    [Fact] void should_still_keep_the_data_inside_the_retention_period() => Assert.False(_shortlyAfter.DeleteData);
    [Fact] void should_delete_the_data_when_the_retention_has_passed() => Assert.True(_afterRetention.DeleteData);
    [Fact] void should_say_the_data_is_removed() => Assert.Contains("removed", _afterRetention.Status.Conditions.Single(c => c.Type == ConditionTypes.Expired).Message);
    [Fact] void should_leave_a_finished_application_alone() => Assert.False(_dataGone.DeleteData || _dataGone.StopWorkloads || _dataGone.DeleteIngress);
    [Fact] void should_stay_expired_once_the_data_is_gone() => Assert.Equal(StageApplicationPhases.Expired, _dataGone.Status.Phase);
    [Fact] void should_run_again_when_the_lifetime_is_extended() => Assert.NotEqual(StageApplicationPhases.Expired, _extended.Status.Phase);
    [Fact] void should_apply_the_objects_again_when_extended() => Assert.NotNull(_extended.Apply);
}
#endif
