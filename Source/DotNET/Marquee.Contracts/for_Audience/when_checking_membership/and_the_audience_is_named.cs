// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Contracts.Audiences;

namespace Cratis.Showrunner.Marquee.Contracts.for_Audience.when_checking_membership;

/// <summary>
/// An audience decides membership from its own values when it can, and asks the plugged-in entitlements only for
/// module and plan audiences.
/// </summary>
public class and_the_audience_is_named : Specification
{
    static readonly TenantId _acme = "acme";
    static readonly TenantId _globex = "globex";

    IAudienceEntitlements _entitlements = null!;
    bool _everyone;
    bool _namedTenant;
    bool _otherTenant;
    bool _holder;
    bool _onPlan;

    void Establish()
    {
        _entitlements = Substitute.For<IAudienceEntitlements>();
        _entitlements.HoldsModule(_acme, "reports").Returns(ValueTask.FromResult(true));
        _entitlements.IsOnPlan(_acme, "pro").Returns(ValueTask.FromResult(true));
    }

    async Task Because()
    {
        _everyone = await Audience.Everyone().Includes(_globex, _entitlements);
        _namedTenant = await Audience.Tenants(_acme).Includes(_acme, _entitlements);
        _otherTenant = await Audience.Tenants(_acme).Includes(_globex, _entitlements);
        _holder = await Audience.HoldersOfModule("reports").Includes(_acme, _entitlements);
        _onPlan = await Audience.OnPlan("pro").Includes(_acme, _entitlements);
    }

    [Fact] void should_include_every_tenant_for_everyone() => Assert.True(_everyone);
    [Fact] void should_include_a_named_tenant() => Assert.True(_namedTenant);
    [Fact] void should_not_include_a_tenant_that_is_not_named() => Assert.False(_otherTenant);
    [Fact] void should_ask_the_entitlements_for_a_module_holder() => Assert.True(_holder);
    [Fact] void should_ask_the_entitlements_for_a_plan() => Assert.True(_onPlan);
}
#endif
