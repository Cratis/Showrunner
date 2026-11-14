// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTour.when_observing_my_tours;

/// <summary>
/// What one person started says nothing about another.
/// </summary>
public class tours_somebody_else_started : a_my_tours_query
{
    void Establish() => Given(People.SomebodyElse, new TourStarted("brainstorming", People.SomebodyElse));

    async Task Because() => await Ask();

    [Fact] void should_not_give_them_to_the_person() => Tours.ShouldBeEmpty();
}
#endif
