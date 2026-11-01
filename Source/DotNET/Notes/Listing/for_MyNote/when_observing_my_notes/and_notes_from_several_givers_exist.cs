// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote.when_observing_my_notes;

public class and_notes_from_several_givers_exist : a_query<MyNote>
{
    readonly NoteId _mine = NoteId.New();
    readonly NoteId _alsoMine = NoteId.New();
    readonly NoteId _someoneElses = NoteId.New();

    void Establish()
    {
        Given(_mine, NoteEvents.Given());
        Given(_someoneElses, NoteEvents.Given(new NoteGiver(Givers.SomeoneElse, "Grace Hopper")));
        Given(_alsoMine, NoteEvents.Suggested());
    }

    async Task Because() => _result = await _scenario.Perform(nameof(MyNote.MyNotes));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_return_only_the_givers_notes() => ((IEnumerable<MyNote>)_result.Data).Select(_ => _.Id).ShouldContainOnly(_mine, _alsoMine);
}
#endif
