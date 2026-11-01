// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Notes.for_PublishedApplications;

public class when_describing_the_application : Specification
{
    PublishedApplication _explicit;
    PublishedApplication _resolved;

    void Because()
    {
        _explicit = PublishedApplications.Of(new NotesOptions { ApplicationName = "Ledger", PublicationId = "pub-7f3a", ApplicationVersion = "4.5.6" });
        _resolved = PublishedApplications.Of(new NotesOptions { ApplicationName = "Ledger", PublicationId = "pub-7f3a" });
    }

    [Fact] void should_name_the_application() => _explicit.ApplicationName.ShouldEqual(new ApplicationName("Ledger"));
    [Fact] void should_name_the_publication() => _explicit.PublicationId.ShouldEqual(new PublicationId("pub-7f3a"));
    [Fact] void should_use_the_configured_version() => _explicit.ApplicationVersion.ShouldEqual(new ApplicationVersion("4.5.6"));
    [Fact] void should_resolve_a_version_from_the_entry_assembly_when_none_is_configured() => _resolved.ApplicationVersion.ShouldNotEqual(ApplicationVersion.NotSet);
    [Fact] void should_leave_out_build_metadata() => _resolved.ApplicationVersion.Value.ShouldNotContain("+");
}
#endif
