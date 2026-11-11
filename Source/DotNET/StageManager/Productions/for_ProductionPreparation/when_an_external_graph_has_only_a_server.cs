// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionPreparation;

public class when_an_external_graph_has_only_a_server : Specification
{
    FakeProductionCluster _cluster;
    Production _production;
    ProductionStatusBuilder _status;
    PreparedProduction? _prepared;

    void Establish()
    {
        _cluster = new();
        _cluster.SecretValues["apps/graph/server"] = "neo4j://example.invalid";
        _production = ProductionSamples.Production(ProductionSamples.Managed());
        _production.Spec.Graph = new() { Mode = ProductionModes.External, CredentialsSecretName = "graph" };
        _status = new(_production, TimeProvider.System);
    }

    async Task Because() => _prepared = await new ProductionPreparation(_cluster).PrepareAsync(_production, _status, CancellationToken.None);

    [Fact] void should_not_prepare() => _prepared.ShouldBeNull();
    [Fact] void should_name_only_the_missing_password_key() => _status.Conditions.Single(condition => condition.Type == ProductionConditions.GraphReady).Message.ShouldEqual("The graph credentials Secret 'graph' does not exist or has no key 'password'.");
    [Fact] void should_keep_the_reason() => _status.Conditions.Single(condition => condition.Type == ProductionConditions.GraphReady).Reason.ShouldEqual("SecretNotFound");
}
#endif
