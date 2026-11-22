// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.for_PlaybillOptions.when_resolving_the_current_version;

/// <summary>
/// The version the application was built with is its informational version, without the build metadata.
/// </summary>
public class from_the_assembly : Specification
{
    PlaybillOptions _options;

    void Establish()
    {
        _options = new();
        _options.Assemblies.Add(typeof(PlaybillOptions).Assembly);
    }

    [Fact] void should_give_a_version() => _options.ResolveCurrentVersion().IsValid.ShouldBeTrue();

    [Fact] void should_leave_out_the_build_metadata() => _options.ResolveCurrentVersion().Value.ShouldNotContain("+");

    [Fact] void should_prefer_the_one_that_was_configured() => new PlaybillOptions { CurrentVersion = " 4.5.6 " }.ResolveCurrentVersion().Value.ShouldEqual("4.5.6");

    [Fact] void should_read_the_named_assemblies() => _options.ResolveAssemblies().ShouldContainOnly(typeof(PlaybillOptions).Assembly);
}
#endif
