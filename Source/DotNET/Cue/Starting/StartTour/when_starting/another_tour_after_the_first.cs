// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// Having started one tour does not stop the person's next tour being recorded.
/// </summary>
public class another_tour_after_the_first : a_starting_viewer
{
    void Establish() => Given(new TourStarted(EventModeling, People.Me));

    async Task Because() => _result = await _scenario.Execute(new StartTour(Brainstorming));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_person_started_it() =>
        await _scenario.ShouldHaveAppendedEvent<StartTour, TourStarted>(
            ViewerId.For(People.Me),
            e => e.Tour == Brainstorming && e.Viewer == People.Me);
}
#endif
