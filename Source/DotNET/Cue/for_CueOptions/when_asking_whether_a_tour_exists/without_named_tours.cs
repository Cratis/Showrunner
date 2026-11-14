// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Cue.for_CueOptions.when_asking_whether_a_tour_exists;

/// <summary>
/// Without any named tours every tour exists.
/// </summary>
public class without_named_tours : Specification
{
    CueOptions _options;

    void Establish() => _options = new();

    [Fact] void should_have_any_tour() => _options.Has("brainstorming").ShouldBeTrue();
}
#endif
