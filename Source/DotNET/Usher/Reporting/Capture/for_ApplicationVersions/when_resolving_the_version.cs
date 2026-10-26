// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ApplicationVersions;

public class when_resolving_the_version : Specification
{
    ApplicationVersion _configured;
    ApplicationVersion _fromTheAssembly;
    ApplicationVersion _withoutAnAssembly;
    string _informationalVersion;

    void Establish() => _informationalVersion = typeof(UsherOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    void Because()
    {
        _configured = ApplicationVersions.Resolve(new UsherOptions { ApplicationVersion = "4.12.1" });
        _fromTheAssembly = ApplicationVersions.FromAssembly(typeof(UsherOptions).Assembly);
        _withoutAnAssembly = ApplicationVersions.FromAssembly(null);
    }

    [Fact] void should_use_the_configured_version() => _configured.Value.ShouldEqual("4.12.1");
    [Fact] void should_use_the_informational_version_without_build_metadata() => _fromTheAssembly.Value.ShouldEqual(_informationalVersion.Split('+')[0]);
    [Fact] void should_have_no_version_without_an_assembly() => _withoutAnAssembly.ShouldEqual(ApplicationVersion.NotSet);
}
#endif
