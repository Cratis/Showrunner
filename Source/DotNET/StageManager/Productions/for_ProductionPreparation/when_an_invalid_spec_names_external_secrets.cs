// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionPreparation;

public class when_an_invalid_spec_names_external_secrets : Specification
{
    IProductionCluster _cluster;
    Production _production;
    ProductionStatusBuilder _status;
    PreparedProduction? _prepared;

    void Establish()
    {
        _cluster = Substitute.For<IProductionCluster>();
        _production = ProductionSamples.Production(ProductionSamples.External());
        _production.Spec.Version = "";
        _production.Spec.Graph = new() { Mode = ProductionModes.External, CredentialsSecretName = "graph" };
        _status = new(_production, TimeProvider.System);
    }

    async Task Because() => _prepared = await new ProductionPreparation(_cluster).PrepareAsync(_production, _status, CancellationToken.None);

    [Fact] void should_not_prepare() => _prepared.ShouldBeNull();
    [Fact] void should_block() => _status.Phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_skip_database_and_graph_secret_lookups() => _cluster.DidNotReceive().GetSecretValueAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    [Fact] void should_still_check_operators() => _cluster.Received().HasDefinitionAsync(CertManagerCertificate.DefinitionName, Arg.Any<CancellationToken>());
}
#endif
