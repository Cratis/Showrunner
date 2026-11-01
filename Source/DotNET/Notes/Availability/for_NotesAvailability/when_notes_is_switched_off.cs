// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Availability.for_NotesAvailability;

/// <summary>
/// The answer is the same for everyone, so it needs no sign-in: the button asks before anyone has signed in.
/// </summary>
public class when_notes_is_switched_off : a_query<NotesAvailability>
{
    protected override bool Enabled => false;

    protected override ClaimsPrincipal Principal => Givers.Anonymous();

    async Task Because() => _result = await _scenario.Perform(nameof(NotesAvailability.Availability));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_be_off() => ((NotesAvailability)_result.Data).IsEnabled.ShouldBeFalse();
}
#endif
