// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Receiving;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AddressNote.when_executing;

/// <summary>
/// A withdrawal is honored: nothing is acted on a note the giver took back.
/// </summary>
public class and_the_giver_withdrew_the_note : a_collecting_command<AddressNote>
{
    void Establish() => Given(ReceivedNotes.OnContext, new NoteWithdrawalReceived());

    async Task Because() => _result = await _scenario.Execute(new AddressNote(_noteId, "2.0.0"));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
