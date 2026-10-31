// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.given;

namespace Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.when_filtering;

public class and_the_tenant_is_suspended : a_tenant_access_filter
{
    void Establish() => _entitlements.WhyReadOnly().Returns(ReadOnlyReason.TenantSuspended);

    async Task Because() => await Filter(typeof(AnOrdinaryChange));

    [Fact] void should_refuse_the_command() => _result.ShouldNotBeAuthorized();
    [Fact] void should_say_the_tenant_is_suspended() => _result.AuthorizationFailureReason.ShouldEqual("TenantSuspended");
}
#endif
