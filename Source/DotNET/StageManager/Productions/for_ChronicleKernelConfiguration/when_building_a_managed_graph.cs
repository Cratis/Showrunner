// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_building_a_managed_graph : Specification
{
    const string Neo4j = "Cratis__Chronicle__Neo4j__";
    ChronicleKernelConfiguration _configuration;

    void Because()
    {
        var spec = ProductionSamples.External();
        spec.Graph = new() { Mode = ProductionModes.Managed };
        _configuration = ChronicleKernelConfiguration.Build(spec, ProductionSamples.Names);
    }

    [Fact] void should_read_the_server_from_its_file() => _configuration.EnvironmentVariables[$"{Neo4j}ServerFile"].ShouldEqual($"/run/secrets/chronicle/{Neo4j}Server");
    [Fact] void should_read_the_password_from_its_file() => _configuration.EnvironmentVariables[$"{Neo4j}PasswordFile"].ShouldEqual($"/run/secrets/chronicle/{Neo4j}Password");
    [Fact] void should_authenticate_as_neo4j() => _configuration.EnvironmentVariables[$"{Neo4j}UserName"].ShouldEqual("neo4j");
    [Fact] void should_use_one_database_on_community() => _configuration.EnvironmentVariables[$"{Neo4j}DatabaseMode"].ShouldEqual("Single");
    [Fact] void should_set_no_prefix() => _configuration.EnvironmentVariables.ContainsKey($"{Neo4j}DatabasePrefix").ShouldBeFalse();
    [Fact] void should_mount_the_server_and_password_from_the_generated_secret() =>
        _configuration.Connections.Sources.Single(source => source.SecretName == "ledger-neo4j").Items!.ShouldContainOnly([new("server", $"{Neo4j}Server"), new("password", $"{Neo4j}Password")]);
    [Fact] void should_never_set_the_server_or_password_as_values() => _configuration.EnvironmentVariables.Keys.Any(key => key == $"{Neo4j}Server" || key == $"{Neo4j}Password" || key == $"{Neo4j}PasswordSecret").ShouldBeFalse();
}
#endif
