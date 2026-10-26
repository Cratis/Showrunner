// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Issues.for_Issue;

public class when_the_current_grant_lapses : a_read_model<Issue>
{
    static readonly ValidUntil _validUntil = new(Start.AddMinutes(30));
    Issue _issue;

    async Task Because()
    {
        await Happened(IssueEvents.Described(Reporters.Subject), new ReporterGrantedAccess(_validUntil), new AccessGrantLapsed(_validUntil));
        _issue = _scenario.Instance!;
    }

    [Fact] void should_not_be_granted() => _issue.AccessGranted.ShouldBeFalse();
}
#endif
