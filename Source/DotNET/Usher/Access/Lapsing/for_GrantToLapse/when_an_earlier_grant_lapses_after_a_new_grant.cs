// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantToLapse;

public class when_an_earlier_grant_lapses_after_a_new_grant : a_read_model<GrantToLapse>
{
    static readonly ValidUntil _earlier = new(Start.AddHours(1));
    static readonly ValidUntil _later = new(Start.AddDays(1));

    async Task Because() => await Happened(
        new ReporterGrantedAccess(_earlier),
        new ReporterRevokedAccess(),
        new ReporterGrantedAccess(_later),
        new AccessGrantLapsed(_earlier));

    [Fact] void should_keep_the_new_grant() => _scenario.Instance.ShouldEqual(new GrantToLapse(_issueId, _later));
}
#endif
