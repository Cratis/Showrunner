// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AddressNote.when_executing;

public class and_nobody_is_signed_in : a_collecting_command<AddressNote>
{
    protected override ClaimsPrincipal Principal => Operators.Anonymous();

    void Establish() => Given(ReceivedNotes.OnContext);

    async Task Because() => _result = await _scenario.Execute(new AddressNote(_noteId, "2.0.0"));

    [Fact] void should_not_be_authorized() => _result.ShouldNotBeAuthorized();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
