// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_sink_secret_is_missing : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec() => ProductionSamples.WithSinks();

    void Establish()
    {
        _cluster.SecretValues["apps/warehouse-sql/dsn"] = "Server=sql;Database=warehouse";
        _cluster.SecretValues["apps/documents-mongo/connectionString"] = "mongodb://documents";
        _cluster.SecretValues["apps/organization-graph/server"] = "bolt://organization:7687";
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_say_which_sink_lacks_its_secret() => Condition("SinkConfigured.reporting").Message.ShouldContain("'reporting-pg'");
    [Fact] void should_give_the_reason() => Condition("SinkConfigured.reporting").Reason.ShouldEqual("SecretNotFound");
    [Fact] void should_report_the_other_sinks_configured() => Condition("SinkConfigured.warehouse").Status.ShouldEqual("True");
    [Fact] void should_report_a_sink_on_the_managed_graph_configured() => Condition("SinkConfigured.people").Status.ShouldEqual("True");
}
#endif
