// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

/// <summary>
/// A lapse names the grant that ended, so a late lapse of a revoked grant does not end the grant given after it.
/// </summary>
public class when_an_earlier_grant_lapses_after_a_new_grant : a_read_model<Issue>
{
    static readonly ValidUntil _earlier = new(Start.AddMinutes(30));
    static readonly ValidUntil _later = new(Start.AddDays(1));
    Issue _issue;

    async Task Because()
    {
        await Happened(
            IssueEvents.Described(Reporters.Subject),
            new ReporterGrantedAccess(_earlier),
            new ReporterRevokedAccess(),
            new ReporterGrantedAccess(_later),
            new AccessGrantLapsed(_earlier));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_still_be_granted() => _issue.AccessGranted.ShouldBeTrue();
    [Fact] void should_end_with_the_later_grant() => _issue.AccessValidUntil.ShouldEqual(_later);
}
#endif
