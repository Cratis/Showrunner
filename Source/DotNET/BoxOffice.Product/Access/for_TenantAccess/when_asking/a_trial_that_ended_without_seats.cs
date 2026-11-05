// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.given;

namespace Cratis.Showrunner.BoxOffice.Product.Access.for_TenantAccess.when_asking;

public class a_trial_that_ended_without_seats : a_tenant_in_the_product
{
    bool _readOnly;

    async Task Because()
    {
        TrialEndedYesterday();
        _readOnly = await new TenantAccess(_stores, _clock).IsReadOnly(Tenants.Tenant);
    }

    [Fact] void should_be_read_only() => _readOnly.ShouldBeTrue();
}
#endif
