// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;
using Cratis.Showrunner.StageManager.Productions.given;
using Cratis.Showrunner.StageManager.Resources;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

/// <summary>
/// A Performance whose Chronicle comes from the Production 'ledger', which configures the sinks of <see cref="ProductionSamples.WithSinks"/>.
/// </summary>
public abstract class given_a_performance_on_a_production : a_reconciliation
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
        _cluster.Productions["apps/ledger"] = ProductionSamples.WithSinks();
    }

    protected ResourceCondition SinksMapped => _performance.Status.Conditions.Single(condition => condition.Type == "SinksMapped");
}
#endif
