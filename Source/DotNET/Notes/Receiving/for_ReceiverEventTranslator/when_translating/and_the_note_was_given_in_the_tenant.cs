// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

public class and_the_note_was_given_in_the_tenant : a_receiver_event_translator
{
    async Task Because()
    {
        await Given();
        await _translator.Translate(new NoteAcknowledged(Tenant.Value), InboxContext(), Collecting);
        await _translator.Translate(new NoteAddressed(Tenant.Value, "2.0.0"), InboxContext(1), Collecting);
        await _translator.Translate(new NoteDeclined(Tenant.Value, "Out of scope."), InboxContext(2), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_the_acknowledgement() => _recorded.Select(_ => _.Content).OfType<ReceiverAcknowledgedNote>().Count().ShouldEqual(1);
    [Fact] void should_record_the_version() => _recorded.Select(_ => _.Content).OfType<ReceiverAddressedNote>().Single().Version.ShouldEqual(new AddressedInVersion("2.0.0"));
    [Fact] void should_record_the_reason() => _recorded.Select(_ => _.Content).OfType<ReceiverDeclinedNote>().Single().Reason.ShouldEqual(new DeclineReason("Out of scope."));
    [Fact] void should_keep_when_it_happened() => _recorded.Single(_ => _.Content is ReceiverAcknowledgedNote).Context.Occurred.ShouldEqual(Occurred);
}
#endif
