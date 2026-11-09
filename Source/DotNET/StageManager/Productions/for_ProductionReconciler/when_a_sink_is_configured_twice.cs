// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_sink_is_configured_twice : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Sinks.Configurations.Add(new() { Name = "reporting", Kind = StorageKinds.PostgreSql, ConnectionSecretRef = new() { Name = "ledger-mongo" } });
        spec.Sinks.Configurations.Add(new() { Name = "Reporting", Kind = StorageKinds.SqlServer, ConnectionSecretRef = new() { Name = "ledger-mongo" } });
        spec.Sinks.Configurations.Add(new() { Name = "archive", Kind = "Oracle", ConnectionSecretRef = new() { Name = "ledger-mongo" } });
        return spec;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_duplicate() => Condition("SinkConfigured.reporting").Message.ShouldContain("The sink 'Reporting' is configured twice");
    [Fact] void should_name_the_unknown_kind() => Condition("SinkConfigured.archive").Message.ShouldContain("unknown kind 'Oracle'");
    [Fact] void should_keep_the_spec_valid_otherwise() => Condition(ProductionConditions.SpecValid).Status.ShouldEqual("True");
}
#endif
