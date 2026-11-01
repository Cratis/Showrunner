// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote.when_observing_my_notes;

public class and_no_one_is_signed_in : a_query<MyNote>
{
    protected override ClaimsPrincipal Principal => Givers.Anonymous();

    void Establish() => Given(NoteId.New(), NoteEvents.Given());

    async Task Because() => _result = await _scenario.Perform(nameof(MyNote.MyNotes));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
