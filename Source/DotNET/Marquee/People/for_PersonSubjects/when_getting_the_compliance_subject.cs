// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.People.for_PersonSubjects;

/// <summary>
/// The compliance subject is a one-way hash of the person: stable, different for each person and not the subject itself.
/// </summary>
public class when_getting_the_compliance_subject : Specification
{
    Subject _first;
    Subject _again;
    Subject _other;

    void Because()
    {
        _first = PersonSubjects.For(Viewers.Me);
        _again = PersonSubjects.For(Viewers.Me);
        _other = PersonSubjects.For(Viewers.SomebodyElse);
    }

    [Fact] void should_be_the_same_for_the_same_person() => _again.ShouldEqual(_first);

    [Fact] void should_differ_for_another_person() => _other.ShouldNotEqual(_first);

    [Fact] void should_not_reveal_the_subject() => _first.Value.ShouldNotContain(Viewers.Me.Value);
}
#endif
