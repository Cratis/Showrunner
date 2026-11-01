// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_a_tenant_starts_on_the_default_offering : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Happened(new TenantStartedOnDefaultOffering("Starter", 3, [new ModuleId("core")]));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_have_the_seats_of_the_offer() => _capacity.On(Tenants.Today).ShouldEqual(3);
    [Fact] void should_not_be_a_trial() => _capacity.IsTrialOver(Tenants.Today).ShouldBeFalse();
    [Fact] void should_not_end() => _capacity.On(Tenants.Day(10000)).ShouldEqual(3);
}
#endif
