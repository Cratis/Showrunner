// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_building_an_external_enterprise_graph : Specification
{
    const string Neo4j = "Cratis__Chronicle__Neo4j__";
    ChronicleKernelConfiguration _enterprise;
    ChronicleKernelConfiguration _community;

    static ProductionSpec Spec(string edition, string prefix)
    {
        var spec = ProductionSamples.External();
        spec.Graph = new() { Mode = ProductionModes.External, Edition = edition, CredentialsSecretName = "shared-graph", UserName = "chronicle", DatabasePrefix = prefix };
        return spec;
    }

    void Because()
    {
        _enterprise = ChronicleKernelConfiguration.Build(Spec(GraphEditions.Enterprise, string.Empty), ProductionSamples.Names);
        _community = ChronicleKernelConfiguration.Build(Spec(GraphEditions.Community, string.Empty), ProductionSamples.Names);
    }

    [Fact] void should_use_a_database_per_namespace() => _enterprise.EnvironmentVariables[$"{Neo4j}DatabaseMode"].ShouldEqual("PerNamespace");
    [Fact] void should_prefix_the_databases_with_the_production() => _enterprise.EnvironmentVariables[$"{Neo4j}DatabasePrefix"].ShouldEqual("ledger");
    [Fact] void should_use_the_user_of_the_spec() => _enterprise.EnvironmentVariables[$"{Neo4j}UserName"].ShouldEqual("chronicle");
    [Fact] void should_mount_only_the_referenced_secret_for_the_graph() => _enterprise.Connections.Sources.Select(source => source.SecretName).ShouldContainOnly(["ledger-mongo", "shared-graph"]);
    [Fact] void should_use_one_database_on_community() => _community.EnvironmentVariables[$"{Neo4j}DatabaseMode"].ShouldEqual("Single");
    [Fact] void should_use_a_given_prefix() => ChronicleKernelConfiguration.Build(Spec(GraphEditions.Enterprise, "tenant-a"), ProductionSamples.Names).EnvironmentVariables[$"{Neo4j}DatabasePrefix"].ShouldEqual("tenant-a");
}
#endif
