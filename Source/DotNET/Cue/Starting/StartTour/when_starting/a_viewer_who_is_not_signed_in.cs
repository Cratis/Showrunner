// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// With nobody signed in there is nobody to remember the tour for.
/// </summary>
public class a_viewer_who_is_not_signed_in : a_starting_viewer
{
    protected override ClaimsPrincipal Principal => People.Anonymous;

    async Task Because() => _result = await _scenario.Execute(new StartTour(EventModeling));

    [Fact] void should_not_succeed() => _result.IsSuccess.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
