// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;

namespace Cratis.Showrunner.GuestList.Querying.for_UndeclaredGrant.when_picking;

/// <summary>
/// A grant of an audience the application does not declare is reported, whatever its level; audience names are
/// case-sensitive, so a differently cased name matches nothing.
/// </summary>
public class grants_that_match_no_declared_audience : Specification
{
    IEnumerable<UndeclaredGrant> _undeclared;

    void Because() => _undeclared = UndeclaredGrant.Among(
        [
            new GrantedAudience(AudienceLevel.System, UserId.NotSet, "Retired"),
            new GrantedAudience(AudienceLevel.Tenant, UserId.NotSet, Guests.Plus),
            new GrantedAudience(AudienceLevel.User, Guests.Alice, "plus"),
            new GrantedAudience(AudienceLevel.Tenant, UserId.NotSet, "Typo")
        ],
        [Guests.Plus, Guests.Preview]);

    [Fact] void should_report_each_undeclared_grant_once() => _undeclared.Count().ShouldEqual(3);
    [Fact] void should_not_report_a_declared_audience() => _undeclared.ShouldNotContain(_ => _.Audience == Guests.Plus);
    [Fact] void should_report_by_level_then_user_then_name() => _undeclared.Select(_ => _.Audience.Value).ShouldEqual(["Typo", "plus", "Retired"]);
    [Fact] void should_report_the_user() => _undeclared.Single(_ => _.Level == AudienceLevel.User).UserId.ShouldEqual(Guests.Alice);
}
#endif
