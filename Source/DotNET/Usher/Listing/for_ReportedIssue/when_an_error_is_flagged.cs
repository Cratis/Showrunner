// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Issues;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_an_error_is_flagged : a_read_model<ReportedIssue>
{
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Flagged(Reporters.Identity));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_be_an_error() => _issue.Kind.ShouldEqual(IssueKind.Error);
    [Fact] void should_have_the_error_message_as_title() => _issue.Title.ShouldEqual(new IssueTitle("Object reference not set to an instance of an object."));
    [Fact] void should_be_reported() => _issue.State.ShouldEqual(IssueState.Reported);
    [Fact] void should_belong_to_the_reporter() => _issue.ReportedBy.ShouldEqual(Reporters.Identity);
}
#endif
