// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTours.when_reducing;

/// <summary>
/// The tour is counted once.
/// </summary>
public class the_same_tour_started_twice : a_read_model<StartedTours>
{
    StartedTours _started;

    async Task Because()
    {
        await Happened(new TourStarted("event-modeling", People.Me), new TourStarted("event-modeling", People.Me));
        _started = _scenario.Instance;
    }

    [Fact] void should_have_started_the_tour_once() => _started.Tours.Count().ShouldEqual(1);
}
#endif
