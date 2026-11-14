// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// A tour without a name is refused, whatever the application accepts.
/// </summary>
public class no_tour_at_all : a_starting_viewer
{
    protected override IEnumerable<string> Tours => [];

    async Task Because() => _result = await _scenario.Execute(new StartTour(TourId.NotSet));

    [Fact] void should_not_be_valid() => _result.IsValid.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
