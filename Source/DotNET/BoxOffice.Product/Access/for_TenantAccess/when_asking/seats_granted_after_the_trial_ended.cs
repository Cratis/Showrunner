// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Product.Access.for_TenantAccess.when_asking;

public class seats_granted_after_the_trial_ended : a_tenant_in_the_product
{
    bool _readOnly;

    async Task Because()
    {
        var trial = SeatCapacity.Reduce(new OperatorOpenedTrial(new(Tenants.Day(-1)), 3), null, ContextAt(Tenants.At(0)));
        var granted = SeatCapacity.Reduce(new OperatorSetSeatCapacity(5, EntitlementEnd.Never, SeatCapacitySource.Purchase), trial, ContextAt(Tenants.At(1)));
        Answer(granted);
        _readOnly = await new TenantAccess(_stores, _clock).IsReadOnly(Tenants.Tenant);
    }

    [Fact] void should_open_the_tenant_again() => _readOnly.ShouldBeFalse();
}
#endif
