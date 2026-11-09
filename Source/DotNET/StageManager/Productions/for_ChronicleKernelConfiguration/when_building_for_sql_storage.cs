// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_building_for_sql_storage : Specification
{
    [Theory]
    [InlineData(StorageKinds.PostgreSql, "PostgreSql")]
    [InlineData(StorageKinds.SqlServer, "MsSql")]
    [InlineData(StorageKinds.MongoDB, "MongoDB")]
    void should_name_the_chronicle_storage_type(string kind, string type) =>
        Build(kind).EnvironmentVariables["Cratis__Chronicle__Storage__Type"].ShouldEqual(type);

    [Fact] void should_mount_the_connection_from_the_secret() =>
        Shape(Build(StorageKinds.SqlServer).Connections.Sources.Single()).ShouldEqual(new SecretSourceShape("ledger-sql", "connectionString", "Cratis__Chronicle__Storage__ConnectionDetails"));

    [Fact] void should_name_the_file_after_the_setting() =>
        Build(StorageKinds.PostgreSql).ConnectionFiles["Cratis__Chronicle__Storage__ConnectionDetails"].ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Storage__ConnectionDetails");

    [Fact] void should_point_the_kernel_at_the_file() =>
        Build(StorageKinds.PostgreSql).EnvironmentVariables["Cratis__Chronicle__Storage__ConnectionDetailsFile"].ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Storage__ConnectionDetails");

    [Fact] void should_not_put_the_connection_in_an_environment_value() =>
        Build(StorageKinds.PostgreSql).EnvironmentVariables.Keys.Any(key => key.EndsWith("ConnectionDetails", StringComparison.Ordinal)).ShouldBeFalse();

    static ChronicleKernelConfiguration Build(string kind) => ChronicleKernelConfiguration.Build(
        new() { Database = new() { Kind = kind, Mode = "External", ConnectionSecretName = "ledger-sql" } },
        ProductionSamples.Names);

    static SecretSourceShape Shape(Performances.SecretSource source) => new(source.SecretName, source.Items!.Single().Key, source.Items!.Single().Value);

    record SecretSourceShape(string Secret, string Key, string File);
}
#endif
