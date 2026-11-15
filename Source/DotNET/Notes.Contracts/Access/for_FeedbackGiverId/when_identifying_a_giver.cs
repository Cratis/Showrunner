// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Notes.Contracts.Access.for_FeedbackGiverId;

/// <summary>
/// The application and a desk compute the same id and subject for the same user, and tell users of different providers or
/// with different subjects apart.
/// </summary>
public class when_identifying_a_giver : Specification
{
    static readonly FeedbackGiverIdentity _ada = new("github", "1234");

    FeedbackGiverId _id = FeedbackGiverId.For(_ada);
    FeedbackGiverId _again = FeedbackGiverId.For(_ada);
    FeedbackGiverId _otherCase = FeedbackGiverId.For("GitHub", "1234");
    FeedbackGiverId _otherProvider = FeedbackGiverId.For("aad", "1234");
    FeedbackGiverId _otherSubject = FeedbackGiverId.For("github", "1235");
    FeedbackGiverId _otherSubjectCase = FeedbackGiverId.For("github", "ABC");
    FeedbackGiverId _lowerSubjectCase = FeedbackGiverId.For("github", "abc");

    [Fact] void should_give_the_same_id_for_the_same_user() => Assert.Equal(_id, _again);
    [Fact] void should_ignore_the_case_of_the_provider() => Assert.Equal(_id, _otherCase);
    [Fact] void should_tell_providers_apart() => Assert.NotEqual(_id, _otherProvider);
    [Fact] void should_tell_subjects_apart() => Assert.NotEqual(_id, _otherSubject);
    [Fact] void should_keep_the_case_of_the_subject() => Assert.NotEqual(_otherSubjectCase, _lowerSubjectCase);
    [Fact] void should_use_the_lowercase_hyphenated_form_as_event_source_id() => Assert.Equal(_id.Value.ToString("D"), ((EventSourceId)_id).Value);
    [Fact] void should_give_the_same_subject_for_the_same_user() => Assert.Equal(FeedbackGiverId.SubjectOf(_ada), FeedbackGiverId.SubjectOf(new FeedbackGiverIdentity("GITHUB", "1234")));
    [Fact] void should_not_reveal_the_user_in_the_subject() => Assert.DoesNotContain("1234", FeedbackGiverId.SubjectOf(_ada).Value);
}
#endif
