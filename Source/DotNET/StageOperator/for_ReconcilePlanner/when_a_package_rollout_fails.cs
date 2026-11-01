// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_a_package_rollout_fails : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _restored = null!;

    void Because()
    {
        var status = Applications.Running(version: "1.0.0", image: Applications.OldImage, package: "orders-package-1-0-0") with
        {
            Rollout = new StageApplicationRollout { TargetVersion = "1.1.0", TargetImage = Applications.OldImage, TargetPackage = "orders-package-1-1-0", StartedAt = Rfc3339.Format(Applications.Now.AddMinutes(-6)) }
        };
        var application = Applications.Create(version: "1.1.0", image: Applications.OldImage, status: status, package: "orders-package-1-1-0");
        _result = ReconcilePlanner.Plan(Applications.Context(application, new ObservedState(new ObservedDeployment(Applications.OldImage, 2, 1, 1, 6, 6, false, "orders-package-1-1-0"), true)));

        application.Status = _result.Status;
        _restored = ReconcilePlanner.Plan(Applications.Context(application, Applications.Ready(Applications.OldImage, "orders-package-1-0-0")));
    }

    [Fact] void should_roll_back_to_the_previous_package() => Assert.Equal(("orders-package-1-0-0", true), (_result.Status.Rollout!.TargetPackage, _result.Status.Rollout.IsRollback));
    [Fact] void should_apply_the_previous_package_at_once() => Assert.Equal("orders-package-1-0-0", _result.Apply!.Deployment.Spec.Template.Spec.Volumes.Single().ConfigMap.Name);
    [Fact] void should_record_the_failed_package() => Assert.Equal(("orders-package-1-1-0", StageRolloutOutcomes.RolledBack), (_result.Status.Versions[^1].Package, _result.Status.Versions[^1].Outcome));
    [Fact] void should_run_the_previous_package_again() => Assert.Equal(("1.0.0", "orders-package-1-0-0"), (_restored.Status.RunningVersion, _restored.Status.RunningPackage));
    [Fact] void should_hold_the_failed_package_back() => Assert.Equal("orders-package-1-0-0", _restored.Apply!.Deployment.Spec.Template.Spec.Volumes.Single().ConfigMap.Name);
}
#endif
