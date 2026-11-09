// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_graph_mode_changes : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Graph = new() { Mode = ProductionModes.External, Edition = GraphEditions.Enterprise, CredentialsSecretName = "shared-graph" };
        return spec;
    }

    void Establish()
    {
        _production.Status = new() { Database = "MongoDB/External", Graph = "Managed/Community" };
        _cluster.SecretValues["apps/shared-graph/server"] = "bolt://shared:7687";
        _cluster.SecretValues["apps/shared-graph/password"] = "e2e-graph-password";
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_mode() => Condition(ProductionConditions.GraphModeUnchanged).Message.ShouldContain("graph.mode from Managed to External");
    [Fact] void should_name_the_edition() => Condition(ProductionConditions.GraphModeUnchanged).Message.ShouldContain("graph.edition from Community to Enterprise");
    [Fact] void should_accept_the_database() => Condition(ProductionConditions.DatabaseModeUnchanged).Status.ShouldEqual("True");
}
#endif
