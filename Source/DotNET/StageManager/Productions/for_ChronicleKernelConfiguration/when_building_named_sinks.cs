// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_building_named_sinks : Specification
{
    const string Sinks = "Cratis__Chronicle__Sinks__";
    ChronicleKernelConfiguration _configuration;

    void Because() => _configuration = ChronicleKernelConfiguration.Build(ProductionSamples.WithSinks(), ProductionSamples.Names);

    [Fact] void should_set_the_default_for_read_models() => _configuration.EnvironmentVariables[$"{Sinks}ReadModels"].ShouldEqual("reporting");
    [Fact] void should_set_the_default_for_graphs() => _configuration.EnvironmentVariables[$"{Sinks}Graphs"].ShouldEqual("people");
    [Fact] void should_map_postgresql_to_sql() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__reporting__Type"].ShouldEqual("SQL");
    [Fact] void should_map_sql_server_to_sql() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__warehouse__Type"].ShouldEqual("SQL");
    [Fact] void should_keep_mongodb() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__documents__Type"].ShouldEqual("MongoDB");
    [Fact] void should_keep_neo4j() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__organization__Type"].ShouldEqual("Neo4j");
    [Fact] void should_mount_the_sql_connection_as_its_connection_details() => _configuration.ConnectionFiles.ContainsKey($"{Sinks}Configurations__reporting__ConnectionDetails").ShouldBeTrue();
    [Fact] void should_mount_the_key_the_reference_names() => _configuration.Connections.Sources.Single(source => source.SecretName == "warehouse-sql").Items!.Keys.ShouldContainOnly(["dsn"]);
    [Fact] void should_mount_the_neo4j_connection_as_its_server() => _configuration.ConnectionFiles.ContainsKey($"{Sinks}Configurations__organization__Server").ShouldBeTrue();
    [Fact] void should_point_the_kernel_at_the_storage_file() => _configuration.EnvironmentVariables["Cratis__Chronicle__Storage__ConnectionDetailsFile"].ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Storage__ConnectionDetails");
    [Fact] void should_point_the_kernel_at_the_sql_file() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__reporting__ConnectionDetailsFile"].ShouldEqual($"/run/secrets/chronicle/{Sinks}Configurations__reporting__ConnectionDetails");
    [Fact] void should_point_the_kernel_at_the_sql_server_file() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__warehouse__ConnectionDetailsFile"].ShouldEqual($"/run/secrets/chronicle/{Sinks}Configurations__warehouse__ConnectionDetails");
    [Fact] void should_point_the_kernel_at_the_mongodb_file() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__documents__ConnectionDetailsFile"].ShouldEqual($"/run/secrets/chronicle/{Sinks}Configurations__documents__ConnectionDetails");
    [Fact] void should_point_the_kernel_at_every_mounted_connection_details_file() => _configuration.EnvironmentVariables.Keys.Count(key => key.EndsWith("ConnectionDetailsFile", StringComparison.Ordinal)).ShouldEqual(4);
    [Fact] void should_point_the_kernel_at_the_neo4j_server_file() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__organization__ServerFile"].ShouldEqual($"/run/secrets/chronicle/{Sinks}Configurations__organization__Server");
    [Fact] void should_point_a_sink_on_the_managed_graph_at_its_server_file() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__people__ServerFile"].ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Neo4j__Server");
    [Fact] void should_point_a_sink_on_the_managed_graph_at_its_password_file() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__people__PasswordFile"].ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Neo4j__Password");
    [Fact] void should_run_a_sink_on_the_managed_graph_in_one_database() => _configuration.EnvironmentVariables[$"{Sinks}Configurations__people__DatabaseMode"].ShouldEqual("Single");
    [Fact] void should_mount_nothing_for_a_sink_on_the_productions_graph() => _configuration.ConnectionFiles.Keys.Any(setting => setting.Contains("__people__", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_mount_the_storage_the_graph_and_every_secret_sink() => _configuration.Connections.Sources.Count.ShouldEqual(6);
    [Fact] void should_put_no_connection_in_an_environment_value() => _configuration.EnvironmentVariables.Keys.Any(key => key.EndsWith("ConnectionDetails", StringComparison.Ordinal) || key.EndsWith("Server", StringComparison.Ordinal) || key.EndsWith("Password", StringComparison.Ordinal)).ShouldBeFalse();
}
#endif
