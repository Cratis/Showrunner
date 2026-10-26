// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantToLapse;

public class when_a_grant_is_given : a_read_model<GrantToLapse>
{
    static readonly ValidUntil _validUntil = new(Start.AddHours(1));

    async Task Because() => await Happened(IssueEvents.Described(Reporters.Subject), new ReporterGrantedAccess(_validUntil));

    [Fact] void should_have_the_grant() => _scenario.Instance.ShouldEqual(new GrantToLapse(_issueId, _validUntil));
}
#endif
