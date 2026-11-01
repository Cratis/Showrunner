// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_a_published_application_is_reconciled : a_reconciliation
{
    string[] ConditionTypes => [.. _performance.Status.Conditions.Select(condition => condition.Type)];

    protected override Performance CreatePerformance() => PerformanceSamples.PublishedPerformance();

    void Establish()
    {
        _cluster.DeploymentsReady = true;
        _cluster.Usage = new(0.5, 700_000_000);
    }

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(PerformancePhase.Ready);
    [Fact] void should_apply_the_application_and_authproxy() => string.Join(',', _cluster.AppliedOf<V1Deployment>().Select(deployment => deployment.Metadata.Name).Order(StringComparer.Ordinal)).ShouldEqual("crm-app,crm-authproxy");
    [Fact] void should_apply_the_network_policy() => _cluster.AppliedOf<V1NetworkPolicy>().Count().ShouldEqual(1);
    [Fact] void should_report_the_package_condition() => ConditionTypes.Contains("PackageAvailable").ShouldBeTrue();
    [Fact] void should_report_the_storage_condition() => ConditionTypes.Contains("StorageConnection").ShouldBeTrue();
    [Fact] void should_report_the_notes_condition() => ConditionTypes.Contains("NotesCollectorToken").ShouldBeTrue();
    [Fact] void should_report_the_rollout_condition() => ConditionTypes.Contains("RolloutSucceeded").ShouldBeTrue();
    [Fact] void should_record_the_version_as_rolled_out() => _performance.Status.Versions.Single().Outcome.ShouldEqual(RolloutOutcome.RolledOut);
    [Fact] void should_report_the_running_version() => _performance.Status.RunningVersion.ShouldEqual("1.4.0");
    [Fact] void should_report_the_public_url() => _performance.Status.PublicUrl.ShouldEqual("https://crm.example.com");
    [Fact] void should_measure_usage() => _performance.Status.ResourceUsage.Measured.ShouldBeTrue();
    [Fact] void should_not_put_a_secret_value_in_the_status() => PerformanceSamples.SecretValues.Any(StatusAsJson.Contains).ShouldBeFalse();
}
#endif
