// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.given;

namespace Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.when_filtering;

public class and_a_suspended_tenant_runs_an_allowed_command : a_tenant_access_filter
{
    void Establish() => _entitlements.WhyReadOnly().Returns(ReadOnlyReason.TenantSuspended);

    async Task Because() => await Filter(typeof(ReportingAProblem));

    [Fact] void should_let_the_command_through() => _result.ShouldBeAuthorized();
    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
}
#endif
