// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote.when_observing_my_notes;

public class and_someone_else_is_signed_in : a_query<MyNote>
{
    protected override ClaimsPrincipal Principal => Givers.SignedInAs(Givers.SomeoneElse);

    void Establish() => Given(NoteId.New(), NoteEvents.Given());

    async Task Because() => _result = await _scenario.Perform(nameof(MyNote.MyNotes));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_not_see_the_givers_notes() => ((IEnumerable<MyNote>)_result.Data).ShouldBeEmpty();
}
#endif
