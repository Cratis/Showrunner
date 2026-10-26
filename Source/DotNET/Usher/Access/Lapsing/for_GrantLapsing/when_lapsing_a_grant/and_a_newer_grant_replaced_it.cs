// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.given;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.when_lapsing_a_grant;

public class and_a_newer_grant_replaced_it : grant_lapsing
{
    static readonly ValidUntil _earlier = new(Now.AddMinutes(-5));
    static readonly ValidUntil _later = new(Now.AddDays(1));

    async Task Because()
    {
        await HappenedOn(_issueId, new ReporterGrantedAccess(_earlier), new ReporterRevokedAccess(), new ReporterGrantedAccess(_later));
        await _lapsing.Lapse(_eventStore, new(_issueId, _earlier));
        await ReadEventsOf(_issueId);
    }

    [Fact] void should_not_lapse_anything() => _events.ShouldNotContain(_ => _.Content is AccessGrantLapsed);
}
#endif
