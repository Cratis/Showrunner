// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTours.when_reducing;

/// <summary>
/// The tours are kept in the order the person started them.
/// </summary>
public class two_tours : a_read_model<StartedTours>
{
    StartedTours _started;

    async Task Because()
    {
        await Happened(new TourStarted("brainstorming", People.Me), new TourStarted("event-modeling", People.Me));
        _started = _scenario.Instance;
    }

    [Fact] void should_keep_them_in_the_order_they_were_started() => _started.Tours.Select(_ => _.Value).ShouldContainOnly("brainstorming", "event-modeling");
}
#endif
