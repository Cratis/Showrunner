// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Viewers.for_ViewerId.when_deriving;

/// <summary>
/// Different people get different ids.
/// </summary>
public class two_subjects : Specification
{
    [Fact] void should_give_different_ids() => ViewerId.For(People.Me).ShouldNotEqual(ViewerId.For(People.SomebodyElse));

    [Fact] void should_give_a_compliance_subject_for_the_same_person_every_time() => ViewerId.SubjectOf(People.Me).ShouldEqual(ViewerId.SubjectOf(People.Me));
}
#endif
