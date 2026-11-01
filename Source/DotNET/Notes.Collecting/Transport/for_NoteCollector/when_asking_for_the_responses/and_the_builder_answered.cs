// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_responses;

public class and_the_builder_answered : a_note_collector
{
    NoteId _otherPublicationsNote;
    IReadOnlyList<NoteResponse> _all;
    IReadOnlyList<NoteResponse> _afterTheFirst;
    IReadOnlyList<NoteResponse> _afterTheLast;

    async Task Because()
    {
        _otherPublicationsNote = NoteId.New();
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 1)]);
        await _tenantStore.EventLog.Append(_otherPublicationsNote, Received("pub-other"));
        await _tenantStore.EventLog.Append(_noteId, new OperatorAcknowledgedNote());
        await _tenantStore.EventLog.Append(_otherPublicationsNote, new OperatorAcknowledgedNote());
        await _tenantStore.EventLog.Append(_noteId, new OperatorAddressedNote("2.0.0"));
        await _tenantStore.EventLog.Append(_noteId, new OperatorDeclinedNote("Not now."));

        _all = (await _collector.Responses(PublicationName, _publication, 0)).Responses;
        _afterTheFirst = (await _collector.Responses(PublicationName, _publication, _all[0].Cursor)).Responses;
        _afterTheLast = (await _collector.Responses(PublicationName, _publication, _all[^1].Cursor)).Responses;
    }

    static Receiving.NoteOnContextReceived Received(string publication) =>
        new("Ledger", new("Ledger", "1.2.3", publication), Collecting.given.ReceivedNotes.Giver, Collecting.given.ReceivedNotes.Context, NoteKind.Problem, "Another application's note.");

    [Fact] void should_answer_the_notes_of_the_publication_only() => _all.Count.ShouldEqual(3);
    [Fact] void should_say_which_note() => _all.All(_ => _.NoteId == _noteId.Value).ShouldBeTrue();
    [Fact] void should_say_what_was_done_in_order() => _all.Select(_ => _.Kind).ShouldContainOnly(NoteResponseKind.Acknowledged, NoteResponseKind.Addressed, NoteResponseKind.Declined);
    [Fact] void should_carry_the_version() => _all[1].Version.ShouldEqual("2.0.0");
    [Fact] void should_carry_the_reason() => _all[2].Reason.ShouldEqual("Not now.");
    [Fact] void should_use_cursors_above_zero_that_only_grow() => (_all[0].Cursor > 0 && _all[0].Cursor < _all[1].Cursor && _all[1].Cursor < _all[2].Cursor).ShouldBeTrue();
    [Fact] void should_answer_only_what_is_after_the_cursor() => _afterTheFirst.Select(_ => _.Kind).ShouldContainOnly(NoteResponseKind.Addressed, NoteResponseKind.Declined);
    [Fact] void should_answer_nothing_after_the_last() => _afterTheLast.Count.ShouldEqual(0);
}
#endif
