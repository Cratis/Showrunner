// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_an_issue_is_described : a_read_model<ReportedIssue>
{
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_be_the_issue() => _issue.Id.ShouldEqual(_issueId);
    [Fact] void should_be_manual() => _issue.Kind.ShouldEqual(IssueKind.Manual);
    [Fact] void should_have_the_summary_as_title() => _issue.Title.ShouldEqual(new IssueTitle("The invoice is not sent"));
    [Fact] void should_be_reported() => _issue.State.ShouldEqual(IssueState.Reported);
    [Fact] void should_be_reported_when_it_happened() => _issue.ReportedAt.ShouldEqual(At(0));
    [Fact] void should_have_its_last_activity_when_it_was_reported() => _issue.LastActivityAt.ShouldEqual(At(0));
    [Fact] void should_not_wait_on_the_reporter() => _issue.WaitingOnReporter.ShouldBeFalse();
    [Fact] void should_not_be_linked_to_a_known_issue() => _issue.KnownIssueTitle.ShouldEqual(KnownIssueTitle.NotSet);
    [Fact] void should_not_be_linked_to_a_work_item() => _issue.WorkItem.ShouldEqual(Unset.WorkItem);
    [Fact] void should_not_be_fixed_in_a_release() => _issue.FixedInVersion.ShouldEqual(ReleaseVersion.NotSet);
    [Fact] void should_belong_to_the_reporter() => _issue.ReporterSubject.ShouldEqual(Reporters.Subject);
}
#endif
