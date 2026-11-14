// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// An application that names no tours accepts any tour a person starts.
/// </summary>
public class any_tour_when_the_application_names_none : a_starting_viewer
{
    protected override IEnumerable<string> Tours => [];

    async Task Because() => _result = await _scenario.Execute(new StartTour("a-tour-nobody-registered"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_person_started_it() =>
        await _scenario.ShouldHaveAppendedEvent<StartTour, TourStarted>(
            ViewerId.For(People.Me),
            e => e.Tour.Value == "a-tour-nobody-registered");
}
#endif
