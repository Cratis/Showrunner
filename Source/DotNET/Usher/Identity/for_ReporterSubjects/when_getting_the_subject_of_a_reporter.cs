// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Identity.for_ReporterSubjects;

public class when_getting_the_subject_of_a_reporter : Specification
{
    Subject _subject;
    Subject _again;
    Subject _sameSubjectAtAnotherProvider;

    void Because()
    {
        _subject = ReporterSubjects.For(Reporters.Identity);
        _again = ReporterSubjects.For(Reporters.Identity);
        _sameSubjectAtAnotherProvider = ReporterSubjects.For(Reporters.SameSubjectAtAnotherProvider);
    }

    [Fact] void should_be_the_same_for_the_same_reporter() => _again.ShouldEqual(_subject);
    [Fact] void should_differ_for_the_same_subject_at_another_provider() => _sameSubjectAtAnotherProvider.ShouldNotEqual(_subject);
    [Fact] void should_not_reveal_the_subject() => _subject.Value.ShouldNotContain(Reporters.Subject.Value);
}
#endif
