// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_building_without_sinks : Specification
{
    ChronicleKernelConfiguration _configuration;

    void Because() => _configuration = ChronicleKernelConfiguration.Build(ProductionSamples.Managed());

    [Fact] void should_only_set_the_storage_type() => _configuration.EnvironmentVariables.Keys.ShouldContainOnly(["Cratis__Chronicle__Storage__Type"]);
    [Fact] void should_store_in_mongodb() => _configuration.EnvironmentVariables["Cratis__Chronicle__Storage__Type"].ShouldEqual("MongoDB");
    [Fact] void should_mount_no_connection_of_its_own() => _configuration.Connections.Sources.Count.ShouldEqual(0);
}
#endif
