// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Contracts.Audiences;

namespace Cratis.Showrunner.Marquee.Contracts.for_Audience.when_checking_membership;

/// <summary>
/// A host without an entitlement source still gets every audience that does not need one, and module and plan
/// audiences reach nobody.
/// </summary>
public class and_there_are_no_entitlements : Specification
{
    bool _everyone;
    bool _tenants;
    bool _holder;
    bool _onPlan;

    async Task Because()
    {
        TenantId tenant = "acme";
        _everyone = await Audience.Everyone().Includes(tenant, NoAudienceEntitlements.Instance);
        _tenants = await Audience.Tenants(tenant).Includes(tenant, NoAudienceEntitlements.Instance);
        _holder = await Audience.HoldersOfModule("reports").Includes(tenant, NoAudienceEntitlements.Instance);
        _onPlan = await Audience.OnPlan("pro").Includes(tenant, NoAudienceEntitlements.Instance);
    }

    [Fact] void should_still_include_everyone() => Assert.True(_everyone);
    [Fact] void should_still_include_named_tenants() => Assert.True(_tenants);
    [Fact] void should_not_include_module_holders() => Assert.False(_holder);
    [Fact] void should_not_include_a_plan() => Assert.False(_onPlan);
}
#endif
