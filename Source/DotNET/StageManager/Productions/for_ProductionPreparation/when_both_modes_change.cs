// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionPreparation;

public class when_both_modes_change : Specification
{
    Production _production;
    ProductionStatusBuilder _status;
    PreparedProduction? _prepared;

    void Establish()
    {
        _production = ProductionSamples.Production(ProductionSamples.Managed());
        _production.Status.Database = "PostgreSql/External";
        _production.Status.Graph = "External/Enterprise";
        _status = new(_production, TimeProvider.System);
    }

    async Task Because() => _prepared = await new ProductionPreparation(new FakeProductionCluster()).PrepareAsync(_production, _status, CancellationToken.None);

    [Fact] void should_block() => _status.Phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_not_prepare() => _prepared.ShouldBeNull();
    [Fact] void should_name_both_changed_database_fields() => _status.Conditions.Single(condition => condition.Type == ProductionConditions.DatabaseModeUnchanged).Message.ShouldEqual("Changing database.kind from PostgreSql to MongoDB and database.mode from External to Managed on a Production that runs is refused: the data stays where it is. Set it back to PostgreSql/External, or create a new Production.");
    [Fact] void should_name_both_changed_graph_fields() => _status.Conditions.Single(condition => condition.Type == ProductionConditions.GraphModeUnchanged).Message.ShouldEqual("Changing graph.mode from External to None and graph.edition from Enterprise to Community on a Production that runs is refused: the data stays where it is. Set it back to External/Enterprise, or create a new Production.");
}
#endif
