// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantOrganization;

public class when_the_organization_is_identified : a_tenant_read_model<TenantOrganization>
{
    [Fact]
    async Task should_know_the_organization()
    {
        await Happened(new TenantOrganizationIdentified(Tenants.Organization));
        _scenario.Instance!.Organization.ShouldEqual(Tenants.Organization);
    }
}
#endif
