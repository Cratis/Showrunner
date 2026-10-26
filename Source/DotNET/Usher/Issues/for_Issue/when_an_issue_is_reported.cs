// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

public class when_an_issue_is_reported : a_read_model<Issue>
{
    Issue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Flagged(Reporters.Subject));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_be_the_issue() => _issue.Id.ShouldEqual(_issueId);
    [Fact] void should_belong_to_the_reporter() => _issue.ReporterSubject.ShouldEqual(Reporters.Subject);
    [Fact] void should_be_reported() => _issue.State.ShouldEqual(IssueState.Reported);
    [Fact] void should_not_have_identity_shared() => _issue.IdentityShared.ShouldBeFalse();
    [Fact] void should_have_no_open_questions() => _issue.OpenQuestions.ShouldBeEmpty();
    [Fact] void should_have_no_grant() => _issue.AccessGranted.ShouldBeFalse();
    [Fact] void should_have_no_grant_end() => _issue.AccessValidUntil.ShouldEqual(Unset.ValidUntil);
}
#endif
