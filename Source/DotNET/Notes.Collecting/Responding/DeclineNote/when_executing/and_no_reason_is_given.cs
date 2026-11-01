// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote.when_executing;

public class and_no_reason_is_given : a_collecting_command<DeclineNote>
{
    void Establish() => Given(ReceivedNotes.OnContext);

    async Task Because() => _result = await _scenario.Execute(new DeclineNote(_noteId, DeclineReason.NotSet));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
