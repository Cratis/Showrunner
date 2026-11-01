// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_only_the_package_changes : Specification
{
    ReconcilePlan _result = null!;
    ReconcilePlan _unchanged = null!;
    ReconcilePlan _ready = null!;

    void Because()
    {
        var status = Applications.Running(version: "1.0.0", image: Applications.OldImage, package: "orders-package-1-0-0");
        var changed = Applications.Create(version: "1.0.0", image: Applications.OldImage, status: status, package: "orders-package-1-0-1");
        _result = ReconcilePlanner.Plan(Applications.Context(changed, Applications.Ready(Applications.OldImage, "orders-package-1-0-0")));

        var same = Applications.Create(version: "1.0.0", image: Applications.OldImage, status: status, package: "orders-package-1-0-0");
        _unchanged = ReconcilePlanner.Plan(Applications.Context(same, Applications.Ready(Applications.OldImage, "orders-package-1-0-0")));

        changed.Status = _result.Status;
        _ready = ReconcilePlanner.Plan(Applications.Context(changed, Applications.Ready(Applications.OldImage, "orders-package-1-0-1")));
    }

    [Fact] void should_start_a_rollout() => Assert.Equal(("orders-package-1-0-1", false), (_result.Status.Rollout!.TargetPackage, _result.Status.Rollout.IsRollback));
    [Fact] void should_apply_the_new_package() => Assert.Equal("orders-package-1-0-1", _result.Apply!.Deployment.Spec.Template.Spec.Volumes.Single().ConfigMap.Name);
    [Fact] void should_not_roll_out_when_the_package_is_the_same() => Assert.Null(_unchanged.Status.Rollout);
    [Fact] void should_complete_when_the_pod_reads_the_new_package() => Assert.Equal(("orders-package-1-0-1", null), (_ready.Status.RunningPackage, _ready.Status.Rollout));
    [Fact] void should_remember_the_package_in_the_history() => Assert.Equal("orders-package-1-0-1", _ready.Status.Versions[^1].Package);
}
#endif
