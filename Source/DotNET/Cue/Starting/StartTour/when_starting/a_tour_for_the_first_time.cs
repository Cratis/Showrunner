// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// The first start of a tour is recorded, for the person.
/// </summary>
public class a_tour_for_the_first_time : a_starting_viewer
{
    async Task Because() => _result = await _scenario.Execute(new StartTour(EventModeling));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_person_started_it() =>
        await _scenario.ShouldHaveAppendedEvent<StartTour, TourStarted>(
            ViewerId.For(People.Me),
            e => e.Tour == EventModeling && e.Viewer == People.Me);
}
#endif
