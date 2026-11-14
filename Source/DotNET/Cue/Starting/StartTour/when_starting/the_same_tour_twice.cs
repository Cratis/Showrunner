// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// Starting a tour twice appends nothing the second time.
/// </summary>
public class the_same_tour_twice : a_starting_viewer
{
    void Establish() => Given(new TourStarted(EventModeling, People.Me));

    async Task Because() => _result = await _scenario.Execute(new StartTour(EventModeling));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
