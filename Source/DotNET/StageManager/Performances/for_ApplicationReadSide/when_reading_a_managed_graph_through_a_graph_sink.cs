// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationReadSide;

public class when_reading_a_managed_graph_through_a_graph_sink : Specification
{
    ApplicationReadSide _readSide;

    void Because() => _readSide = ApplicationReadSide.For(new() { EventStores = ["Crm", "Other"] }, ["people", "organization"], ProductionSamples.WithSinks(), ProductionSamples.Names);

    [Fact] void should_want_the_graph_because_a_sink_refers_to_it() => _readSide.GraphWanted.ShouldBeTrue();
    [Fact] void should_bind_the_graph() => _readSide.Graph.ShouldNotBeNull();
    [Fact] void should_name_the_managed_server_of_the_production() => _readSide.Graph!.Server.ShouldEqual(ProductionSamples.Names.Neo4jServer);
    [Fact] void should_not_take_the_server_from_a_secret() => _readSide.Graph!.ServerSecret.ShouldBeNull();
    [Fact] void should_share_the_credential() => _readSide.Graph!.SharedCredential.ShouldBeTrue();
    [Fact] void should_read_the_first_event_store() => _readSide.Graph!.EventStore.ShouldEqual("Crm");
    [Fact] void should_give_the_server_as_a_plain_value() => _readSide.EnvironmentVariables["Cratis__Neo4j__Server"].ShouldEqual(ProductionSamples.Names.Neo4jServer);
    [Fact] void should_give_the_event_store_as_a_plain_value() => _readSide.EnvironmentVariables["Cratis__Neo4j__EventStore"].ShouldEqual("Crm");
    [Fact] void should_take_only_the_password_from_a_secret() => _readSide.SecretEnvironmentVariables.Keys.ShouldContainOnly(["Cratis__Neo4j__Password"]);
    [Fact] void should_not_bind_a_mongodb_sink() => _readSide.MongoDBSinks.Count.ShouldEqual(0);
    [Fact] void should_find_the_graph_sink_with_a_server_secret_unreadable() => _readSide.Unreadable.ShouldContainOnly(["organization"]);
    [Fact] void should_have_no_mount() => _readSide.Mount.ShouldBeNull();
}
#endif
