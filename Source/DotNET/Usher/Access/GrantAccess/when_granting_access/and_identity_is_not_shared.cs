// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.GrantAccess.when_granting_access;

public class and_identity_is_not_shared : an_issue_command<GrantAccess>
{
    void Establish() => GivenReported();

    async Task Because() => _result = await _scenario.Execute(new GrantAccess(_issueId, AccessDuration.OneHour));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_share_identity_before_granting_access() =>
        _scenario.AppendedEvents.Select(_ => _.Event.Content.GetType()).ShouldEqual([typeof(ReporterSharedIdentity), typeof(ReporterGrantedAccess)]);

    [Fact]
    async Task should_share_who_the_signed_in_reporter_is() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAccess, ReporterSharedIdentity>(_issueId, e => e.Reporter == Reporters.Reporter);

    [Fact]
    async Task should_grant_access_for_one_hour_from_now() =>
        await _scenario.ShouldHaveAppendedEvent<GrantAccess, ReporterGrantedAccess>(_issueId, e => e.ValidUntil.Value == new DateTimeOffset(2026, 10, 26, 13, 0, 0, TimeSpan.Zero));
}
#endif
