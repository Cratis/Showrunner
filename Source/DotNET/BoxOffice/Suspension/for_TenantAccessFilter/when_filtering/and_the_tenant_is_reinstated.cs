// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.given;

namespace Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.when_filtering;

public class and_the_tenant_is_reinstated : a_tenant_access_filter
{
    async Task Because()
    {
        _entitlements.WhyReadOnly().Returns(ReadOnlyReason.TenantSuspended);
        await Filter(typeof(AnOrdinaryChange));
        _refused = _result;

        _entitlements.WhyReadOnly().Returns(ReadOnlyReason.None);
        await Filter(typeof(AnOrdinaryChange));
    }

    CommandResult _refused;

    [Fact] void should_have_refused_it_while_suspended() => _refused.ShouldNotBeAuthorized();
    [Fact] void should_let_it_through_at_once_when_reinstated() => _result.ShouldBeAuthorized();
}
#endif
