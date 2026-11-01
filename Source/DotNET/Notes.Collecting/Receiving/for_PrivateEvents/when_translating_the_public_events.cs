// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_PrivateEvents;

public class when_translating_the_public_events : Specification
{
    (TenantId TenantId, object Fact)? _context;
    (TenantId TenantId, object Fact)? _unknown;

    void Because()
    {
        _context = PrivateEvents.For(new NoteOnContextGiven("t", ReceivedNotes.Application, ReceivedNotes.Giver, ReceivedNotes.Context, NoteKind.Praise, "Nice."), ReceivedNotes.AppStore);
        _unknown = PrivateEvents.For("not an event", ReceivedNotes.AppStore);
    }

    [Fact] void should_name_the_tenant() => _context!.Value.TenantId.ShouldEqual(new TenantId("t"));
    [Fact] void should_name_the_application_store() => ((NoteOnContextReceived)_context!.Value.Fact).AppStore.ShouldEqual(ReceivedNotes.AppStore);
    [Fact] void should_not_translate_what_it_does_not_receive() => _unknown.ShouldBeNull();
    [Fact] void should_receive_the_six_events_of_an_application() => PrivateEvents.FromApplications.Count.ShouldEqual(6);
}
#endif
