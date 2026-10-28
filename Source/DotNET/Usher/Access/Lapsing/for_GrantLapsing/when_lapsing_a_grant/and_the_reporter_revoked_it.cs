// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.given;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.when_lapsing_a_grant;

public class and_the_reporter_revoked_it : grant_lapsing
{
    static readonly ValidUntil _validUntil = new(Now.AddMinutes(-5));

    async Task Because()
    {
        // The grant was nominated before the reporter revoked it.
        await HappenedOn(_issueId, new ReporterGrantedAccess(_validUntil), new ReporterRevokedAccess());
        await _lapsing.Lapse(_eventStore, _issueId);
        await ReadEventsOf(_issueId);
    }

    [Fact] void should_not_lapse_it() => _events.ShouldNotContain(_ => _.Content is AccessGrantLapsed);
}
#endif
