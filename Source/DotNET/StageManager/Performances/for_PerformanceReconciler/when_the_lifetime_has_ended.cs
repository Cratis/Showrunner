// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_lifetime_has_ended : a_reconciliation
{
    DateTimeOffset _until;

    protected override Performance CreatePerformance()
    {
        _until = DateTimeOffset.UtcNow.AddMinutes(-5);
        var performance = PerformanceSamples.PublishedPerformance();
        performance.Spec.Lifetime = new() { Until = _until, DataRetentionDays = 3 };
        performance.Status = Releases.RunningV140();
        return performance;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_expired() => _phase.ShouldEqual(PerformancePhase.Expired);
    [Fact] void should_remove_the_public_route() => _cluster.Deleted.ShouldContain("V1Ingress/crm");
    [Fact] void should_stop_the_application() => _cluster.Scaled["crm-app"].ShouldEqual(0);
    [Fact] void should_stop_authproxy() => _cluster.Scaled["crm-authproxy"].ShouldEqual(0);
    [Fact] void should_not_apply_anything() => _cluster.Applied.Count.ShouldEqual(0);
    [Fact] void should_say_so_in_a_condition() => _performance.Status.Conditions.Single(condition => condition.Type == "Expired").Status.ShouldEqual("True");
    [Fact] void should_report_when_it_expired() => _performance.Status.ExpiresAt.ShouldEqual(_until);
    [Fact] void should_say_until_when_the_data_is_kept() => _performance.Status.DataRetainedUntil.ShouldEqual(_until.AddDays(3));
    [Fact] void should_report_no_public_url() => _performance.Status.PublicUrl.ShouldEqual(string.Empty);
    [Fact] void should_report_no_running_version() => _performance.Status.RunningVersion.ShouldEqual(string.Empty);
    [Fact] void should_keep_the_version_history() => _performance.Status.Versions.Count.ShouldEqual(1);
}
#endif
