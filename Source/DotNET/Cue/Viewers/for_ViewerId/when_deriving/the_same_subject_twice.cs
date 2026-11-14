// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Viewers.for_ViewerId.when_deriving;

/// <summary>
/// A person always gets the same id, so their history is found again on any device.
/// </summary>
public class the_same_subject_twice : Specification
{
    ViewerId _first;
    ViewerId _second;

    void Because()
    {
        _first = ViewerId.For(People.Me);
        _second = ViewerId.For(People.Me);
    }

    [Fact] void should_be_the_same_id() => _first.ShouldEqual(_second);

    [Fact] void should_not_contain_the_subject() => _first.Value.Contains(People.Me.Value).ShouldBeFalse();
}
#endif
