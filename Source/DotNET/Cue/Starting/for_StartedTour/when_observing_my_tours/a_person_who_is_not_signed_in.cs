// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTour.when_observing_my_tours;

/// <summary>
/// With nobody signed in the query is refused, and nobody else's tours leak.
/// </summary>
public class a_person_who_is_not_signed_in : a_my_tours_query
{
    protected override ClaimsPrincipal Principal => People.Anonymous;

    void Establish() => Given(People.Me, new TourStarted("brainstorming", People.Me));

    async Task Because() => await Ask();

    [Fact] void should_be_refused() => _result.IsAuthorized.ShouldBeFalse();

    [Fact] void should_give_no_tours() => (_result.Data is null || !((IEnumerable<StartedTour>)_result.Data).Any()).ShouldBeTrue();
}
#endif
