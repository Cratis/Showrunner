// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationReadSide;

public class when_reading_an_external_enterprise_graph : Specification
{
    ApplicationReadSide _readSide;

    void Because()
    {
        var production = ProductionSamples.External();
        production.Graph = new() { Mode = ProductionModes.External, Edition = GraphEditions.Enterprise, CredentialsSecretName = "shared-graph", UserName = "chronicle" };
        _readSide = ApplicationReadSide.For(new() { Graph = true, EventStores = ["Crm"] }, [], production, ProductionSamples.Names);
    }

    [Fact] void should_not_give_the_bolt_address_as_a_value() => _readSide.EnvironmentVariables.ContainsKey("Cratis__Neo4j__Server").ShouldBeFalse();
    [Fact] void should_take_the_bolt_address_from_the_credentials_secret() => _readSide.SecretEnvironmentVariables["Cratis__Neo4j__Server"].ShouldEqual(new ResolvedSecretRef("shared-graph", "server"));
    [Fact] void should_take_the_password_from_the_credentials_secret() => _readSide.SecretEnvironmentVariables["Cratis__Neo4j__Password"].ShouldEqual(new ResolvedSecretRef("shared-graph", "password"));
    [Fact] void should_read_a_database_per_namespace() => _readSide.EnvironmentVariables["Cratis__Neo4j__DatabaseMode"].ShouldEqual("PerNamespace");
    [Fact] void should_prefix_the_databases_as_the_kernel_does() => _readSide.EnvironmentVariables["Cratis__Neo4j__DatabasePrefix"].ShouldEqual("ledger");
    [Fact] void should_read_as_the_graph_user() => _readSide.EnvironmentVariables["Cratis__Neo4j__UserName"].ShouldEqual("chronicle");
    [Fact] void should_not_share_the_credential_as_on_community() => _readSide.Graph!.SharedCredential.ShouldBeFalse();
    [Fact] void should_need_both_keys_of_the_secret() => _readSide.Secrets.Count().ShouldEqual(2);
}
#endif
