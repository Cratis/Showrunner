// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_using_the_connection_secret_of_a_production : a_reconciliation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.Chronicle = null;
        performance.Spec.ProductionRef = new() { Name = "ledger" };
        return performance;
    }

    void Establish()
    {
        _cluster.ProductionConnectionSecrets["apps/ledger"] = "ledger-connection";
        _cluster.SecretValues["apps/ledger-connection/connectionString"] = PerformanceSamples.ConnectionString;
    }

    [Fact] void should_not_be_blocked() => _phase.ShouldEqual(PerformancePhase.Progressing);

    [Fact] void should_mount_the_secret_of_the_production() =>
        _cluster.AppliedOf<V1Deployment>().Single(deployment => deployment.Metadata.Name == "crm-app")
            .Spec.Template.Spec.Volumes.SelectMany(volume => volume.Projected.Sources).Any(source => source.Secret.Name == "ledger-connection").ShouldBeTrue();
}
#endif
