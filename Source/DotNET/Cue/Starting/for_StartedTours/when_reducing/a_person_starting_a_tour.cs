// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTours.when_reducing;

/// <summary>
/// The tour is among those the person started.
/// </summary>
public class a_person_starting_a_tour : a_read_model<StartedTours>
{
    StartedTours _started;

    async Task Because()
    {
        await Happened(new TourStarted("event-modeling", People.Me));
        _started = _scenario.Instance;
    }

    [Fact] void should_be_for_the_person() => _started.Id.ShouldEqual(ViewerId.For(People.Me));

    [Fact] void should_have_started_the_tour() => _started.Has("event-modeling").ShouldBeTrue();

    [Fact] void should_not_have_started_another_tour() => _started.Has("brainstorming").ShouldBeFalse();
}
#endif
