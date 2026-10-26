// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.given;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.when_lapsing_a_grant;

public class and_its_end_has_passed : grant_lapsing
{
    static readonly ValidUntil _validUntil = new(Now.AddMinutes(-5));

    async Task Because()
    {
        await HappenedOn(_issueId, new ReporterGrantedAccess(_validUntil));
        await _lapsing.Lapse(_eventStore, _issueId);
        await ReadEventsOf(_issueId);
    }

    [Fact] void should_lapse_the_grant() => _events[^1].Content.ShouldEqual(new AccessGrantLapsed(_validUntil));
}
#endif
