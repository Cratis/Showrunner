// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Cue.for_CueOptions.when_asking_whether_a_tour_exists;

/// <summary>
/// Only the tours the application named exist.
/// </summary>
public class with_named_tours : Specification
{
    CueOptions _options;

    void Establish() => _options = new() { Tours = ["event-modeling"] };

    [Fact] void should_have_a_named_tour() => _options.Has("event-modeling").ShouldBeTrue();

    [Fact] void should_not_have_another_tour() => _options.Has("brainstorming").ShouldBeFalse();
}
#endif
