// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AddressNote.when_executing;

public class and_no_version_is_named : a_collecting_command<AddressNote>
{
    void Establish() => Given(ReceivedNotes.OnContext);

    async Task Because() => _result = await _scenario.Execute(new AddressNote(_noteId, AddressedInVersion.NotSet));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
