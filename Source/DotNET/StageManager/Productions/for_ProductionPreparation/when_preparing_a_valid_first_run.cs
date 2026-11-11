// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionPreparation;

public class when_preparing_a_valid_first_run : Specification
{
    FakeProductionCluster _cluster;
    Production _production;
    ProductionStatusBuilder _status;
    PreparedProduction? _prepared;

    void Establish()
    {
        _cluster = new();
        _production = ProductionSamples.Production(ProductionSamples.Managed());
        _status = new(_production, TimeProvider.System);
    }

    async Task Because() => _prepared = await new ProductionPreparation(_cluster).PrepareAsync(_production, _status, CancellationToken.None);

    [Fact] void should_prepare_the_production() => _prepared.ShouldNotBeNull();
    [Fact] void should_not_change_the_cluster() => _cluster.Applied.Count.ShouldEqual(0);
    [Fact] void should_treat_the_initial_database_mode_as_unchanged() => _status.Conditions.Single(condition => condition.Type == ProductionConditions.DatabaseModeUnchanged).Reason.ShouldEqual("Unchanged");
    [Fact] void should_keep_condition_order() => _status.Conditions.Select(condition => condition.Type).ShouldEqual(new[] { ProductionConditions.SpecValid, ProductionConditions.VaultSupported, ProductionConditions.DatabaseModeUnchanged, ProductionConditions.GraphModeUnchanged, ProductionConditions.CertManagerInstalled });
}
#endif
