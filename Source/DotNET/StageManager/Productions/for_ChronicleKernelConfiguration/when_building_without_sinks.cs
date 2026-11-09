// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_building_without_sinks : Specification
{
    const string StorageFile = "Cratis__Chronicle__Storage__ConnectionDetailsFile";
    ChronicleKernelConfiguration _external;
    ChronicleKernelConfiguration _managed;

    void Because()
    {
        _external = ChronicleKernelConfiguration.Build(ProductionSamples.External(), ProductionSamples.Names);
        _managed = ChronicleKernelConfiguration.Build(ProductionSamples.Managed(), ProductionSamples.Names);
    }

    [Fact] void should_only_set_the_storage_type_and_its_connection_file() => _external.EnvironmentVariables.Keys.ShouldContainOnly(["Cratis__Chronicle__Storage__Type", StorageFile]);
    [Fact] void should_store_in_mongodb() => _external.EnvironmentVariables["Cratis__Chronicle__Storage__Type"].ShouldEqual("MongoDB");
    [Fact] void should_mount_only_the_database_secret() => _external.Connections.Sources.Select(source => source.SecretName).ShouldContainOnly(["ledger-mongo"]);
    [Fact] void should_leave_the_neo4j_settings_unset() => _external.EnvironmentVariables.Keys.Any(key => key.Contains("Neo4j", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_set_the_same_settings_for_a_managed_database() => _managed.EnvironmentVariables.Keys.ShouldContainOnly(["Cratis__Chronicle__Storage__Type", StorageFile]);
    [Fact] void should_mount_the_connection_the_operator_composes_for_a_managed_database() => _managed.Connections.Sources.Select(source => source.SecretName).ShouldContainOnly(["ledger-mongodb-connection"]);
}
#endif
