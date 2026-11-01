// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_a_new_version_fails_to_become_ready : a_reconciliation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.PublishedPerformance();
        performance.Spec.Application.Published!.Version = "1.5.0";
        performance.Spec.Application.Published.PackageConfigMapRef.Name = "crm-package-1-5-0";
        performance.Status = Releases.RunningV140();
        performance.Status.Rollout = new()
        {
            Version = "1.5.0",
            Image = "cratis/stage:5.3.0",
            PackageConfigMap = "crm-package-1-5-0",
            StartedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        return performance;
    }

    V1Deployment Application => _cluster.AppliedOf<V1Deployment>().Single(deployment => deployment.Metadata.Name == "crm-app");

    Task Because() => Reconcile();

    [Fact] void should_record_the_new_version_as_failed() => _performance.Status.Versions.Single(record => record.Version == "1.5.0").Outcome.ShouldEqual(RolloutOutcome.Failed);
    [Fact] void should_record_the_restored_version() => _performance.Status.Versions[^1].Outcome.ShouldEqual(RolloutOutcome.RolledBack);
    [Fact] void should_report_the_restored_version_as_running() => _performance.Status.RunningVersion.ShouldEqual("1.4.0");
    [Fact] void should_have_tried_the_new_package() => Application.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "package").ConfigMap.Name.ShouldEqual("crm-package-1-5-0");
}
#endif
