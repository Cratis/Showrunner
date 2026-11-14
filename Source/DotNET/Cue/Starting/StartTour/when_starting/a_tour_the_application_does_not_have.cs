// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Starting.StartTour.when_starting;

/// <summary>
/// A tour the application has not named is refused, so a client cannot fill a history with tours that do not exist.
/// </summary>
public class a_tour_the_application_does_not_have : a_starting_viewer
{
    async Task Because() => _result = await _scenario.Execute(new StartTour("a-tour-nobody-made"));

    [Fact] void should_not_succeed() => _result.IsSuccess.ShouldBeFalse();

    [Fact] void should_not_be_valid() => _result.IsValid.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
