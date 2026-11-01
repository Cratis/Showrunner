// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_a_failed_version_is_still_asked_for : a_reconciliation
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

    async Task Because()
    {
        await Reconcile();
        await Reconcile();
    }

    [Fact] void should_deploy_the_previous_package() => Application.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "package").ConfigMap.Name.ShouldEqual("crm-package-1-4-0");
    [Fact] void should_say_the_rollout_did_not_succeed() => _performance.Status.Conditions.Single(condition => condition.Type == "RolloutSucceeded").Status.ShouldEqual("False");
    [Fact] void should_not_record_the_failure_twice() => _performance.Status.Versions.Count.ShouldEqual(3);
}
#endif
