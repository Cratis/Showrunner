// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_version_changes : a_production_reconciliation
{
    async Task Because()
    {
        await Reconcile();
        _production.Spec.Version = "19.150.0";
        await Reconcile();
    }

    [Fact] void should_keep_one_statefulset() => _cluster.AppliedOf<V1StatefulSet>().Count(statefulSet => statefulSet.Metadata.Name == "ledger-chronicle").ShouldEqual(1);
    [Fact] void should_run_the_new_version() => ChronicleContainer.Image.ShouldEqual("cratis/chronicle:19.150.0");
    [Fact] void should_roll_the_replicas() => Chronicle!.Spec.UpdateStrategy.Type.ShouldEqual("RollingUpdate");
    [Fact] void should_keep_the_credentials() => _cluster.AppliedOf<V1Secret>().Count(secret => secret.Metadata.Name == "ledger-chronicle-credentials").ShouldEqual(1);
}
#endif
