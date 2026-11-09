// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_running_three_replicas : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Replicas = 3;
        return spec;
    }

    Task Because() => Reconcile();

    [Fact] void should_run_three_replicas() => Chronicle!.Spec.Replicas.ShouldEqual(3);
    [Fact] void should_form_one_cluster_through_mongodb() => Environment("Cratis__Chronicle__Clustering__Type")!.Value.ShouldEqual("MongoDB");
    [Fact] void should_share_one_cluster_id() => Environment("Cratis__Chronicle__Clustering__ClusterId")!.Value.ShouldEqual("ledger");
    [Fact] void should_advertise_the_pod_address() => Environment("Cratis__Chronicle__Clustering__AdvertisedIP")!.ValueFrom.FieldRef.FieldPath.ShouldEqual("status.podIP");
    [Fact] void should_start_the_replicas_together() => Chronicle!.Spec.PodManagementPolicy.ShouldEqual("Parallel");
}
#endif
