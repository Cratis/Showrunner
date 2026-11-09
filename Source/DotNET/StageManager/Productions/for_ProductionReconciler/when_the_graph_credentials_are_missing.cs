// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_graph_credentials_are_missing : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Graph = new() { Mode = ProductionModes.External, CredentialsSecretName = "shared-graph" };
        return spec;
    }

    void Establish() => _cluster.SecretValues["apps/shared-graph/server"] = "bolt://shared:7687";

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_missing_key() => Condition(ProductionConditions.GraphReady).Message.ShouldContain("'password'");
}
#endif
