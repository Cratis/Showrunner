// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Lifecycle.ReopenIssue;
using Cratis.Showrunner.Usher.Receiving;

namespace Cratis.Showrunner.Usher.Listing.for_ReportedIssue;

public class when_a_release_ships_the_fix_and_the_issue_is_reopened : a_read_model<ReportedIssue>
{
    ReleaseVersion _fixedIn;
    ReportedIssue _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new SupportResolvedIssue("Fixed."),
            new ReleaseShippedFix("4.12.3", "Cratis/Ledger#42"));
        _fixedIn = _scenario.Instance!.FixedInVersion;
        await Happened(new ReporterReopenedIssue("Still broken in 4.12.3."));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_show_the_version_with_the_fix() => _fixedIn.ShouldEqual(new ReleaseVersion("4.12.3"));
    [Fact] void should_clear_the_version_when_reopened() => _issue.FixedInVersion.ShouldEqual(ReleaseVersion.NotSet);
}
#endif
