// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleKernelConfiguration;

public class when_naming_the_system_database : Specification
{
    const string Setting = "Cratis__Chronicle__Storage__SystemDatabaseName";
    ChronicleKernelConfiguration _named;
    ChronicleKernelConfiguration _unnamed;

    void Because()
    {
        var spec = ProductionSamples.External();
        spec.Database.SystemDatabaseName = "System";
        _named = ChronicleKernelConfiguration.Build(spec, ProductionSamples.Names);
        _unnamed = ChronicleKernelConfiguration.Build(ProductionSamples.External(), ProductionSamples.Names);
    }

    [Fact] void should_set_the_name_for_the_kernel() => _named.EnvironmentVariables[Setting].ShouldEqual("System");
    [Fact] void should_leave_the_kernel_default_when_no_name_is_given() => _unnamed.EnvironmentVariables.ContainsKey(Setting).ShouldBeFalse();
}
#endif
