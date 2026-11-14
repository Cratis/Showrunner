// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.for_StartedTour.when_observing_my_tours;

/// <summary>
/// A person who never started a tour has an empty answer, not an error.
/// </summary>
public class a_person_who_started_nothing : a_my_tours_query
{
    async Task Because() => await Ask();

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_give_no_tours() => Tours.ShouldBeEmpty();
}
#endif
