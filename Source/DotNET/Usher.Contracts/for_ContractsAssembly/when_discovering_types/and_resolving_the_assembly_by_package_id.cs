// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Contracts.for_ContractsAssembly.when_discovering_types;

/// <summary>
/// Cratis.Types resolves a referenced project by the library name recorded in deps.json, which is the
/// PackageId for a packable project. An assembly name that drifts from the package id silently drops
/// every event type in this assembly from discovery - see Usher.Contracts.csproj.
/// </summary>
public class and_resolving_the_assembly_by_package_id : Specification
{
    string? _assemblyName;

    void Because() => _assemblyName = typeof(and_resolving_the_assembly_by_package_id).Assembly.GetName().Name;

    [Fact] void should_have_the_package_id_as_assembly_name() => Assert.Equal("Cratis.Showrunner.Usher.Contracts", _assemblyName);
}
#endif
