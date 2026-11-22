// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// Someone who is not signed in has no record to keep, so the command is refused.
/// </summary>
public class a_viewer_who_is_not_signed_in : a_marking_viewer
{
    protected override ClaimsPrincipal Principal => People.Anonymous;

    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("1.1.0"));

    [Fact] void should_not_succeed() => _result.IsSuccess.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
