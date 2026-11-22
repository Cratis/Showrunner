// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Identity;
using Cratis.Showrunner.Playbill.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Playbill.for_PlaybillServiceCollectionExtensions.when_adding_playbill;

/// <summary>
/// Adding Playbill gives an application its release history, read from the assemblies it names, and the services the
/// commands and queries use.
/// </summary>
public class as_documented : Specification
{
    ServiceProvider _provider;
    IReleaseNotes _notes;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(People.Anonymous));
        services.AddPlaybill(options =>
        {
            options.Assemblies.Add(typeof(PlaybillOptions).Assembly);
            options.CurrentVersion = "1.2.0";
        });
        _provider = services.BuildServiceProvider();
        _notes = _provider.GetRequiredService<IReleaseNotes>();
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_read_the_history_embedded_in_the_named_assembly() => _notes.All.Select(_ => _.Version.Value).ShouldContain("1.1.0");

    [Fact] void should_know_the_running_version() => _notes.Current.Value.ShouldEqual("1.2.0");

    [Fact] void should_read_the_history_once() => ReferenceEquals(_provider.GetRequiredService<IReleaseNotes>(), _notes).ShouldBeTrue();

    [Fact] void should_tell_viewers_apart() => _provider.GetRequiredService<ISignedInViewer>().ShouldBeOfExactType<SignedInViewer>();
}
#endif
