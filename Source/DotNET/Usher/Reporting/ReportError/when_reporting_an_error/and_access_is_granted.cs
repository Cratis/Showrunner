// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.when_reporting_an_error;

public class and_access_is_granted : an_issue_command<ReportError>
{
    async Task Because() => _result = await _scenario.Execute(Reports.Error(_issueId) with { GrantAccess = true, AccessDuration = AccessDuration.SevenDays });

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_error_then_the_identity_then_the_grant() =>
        _scenario.AppendedEvents.Select(_ => _.Event.Content.GetType()).ShouldEqual([typeof(ReporterFlaggedError), typeof(ReporterSharedIdentity), typeof(ReporterGrantedAccess)]);

    [Fact]
    async Task should_grant_access_for_seven_days_from_now() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterGrantedAccess>(_issueId, e => e.ValidUntil.Value == new DateTimeOffset(2026, 11, 2, 12, 0, 0, TimeSpan.Zero));
}
#endif
