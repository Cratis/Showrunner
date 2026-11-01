// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AddressNote.when_executing;

public class and_the_note_is_addressed_in_a_later_version : a_collecting_command<AddressNote>
{
    void Establish() => Given(ReceivedNotes.OnContext, new OperatorAddressedNote("2.0.0"));

    async Task Because() => _result = await _scenario.Execute(new AddressNote(_noteId, "2.1.0"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
