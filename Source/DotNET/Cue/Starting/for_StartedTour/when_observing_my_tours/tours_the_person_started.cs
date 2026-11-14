// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTour.when_observing_my_tours;

/// <summary>
/// The person sees the tours they started, in the order they started them.
/// </summary>
public class tours_the_person_started : a_my_tours_query
{
    void Establish() => Given(People.Me, new TourStarted("brainstorming", People.Me), new TourStarted("event-modeling", People.Me));

    async Task Because() => await Ask();

    [Fact] void should_give_the_tours_in_the_order_they_were_started() => Tours.ShouldEqual(["brainstorming", "event-modeling"]);
}
#endif
