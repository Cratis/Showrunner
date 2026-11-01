// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Contracts.Tenants;
using Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting.for_DefaultOffering.when_starting_a_tenant;

public class on_a_published_default_offering : a_product_with_a_default_offering
{
    TenantStart _result;
    TenantStartedOnOffer _told;
    int _count;
    string _source;
    TenantStartedOnDefaultOffering _entitled;
    int _entitledCount;

    async Task Because()
    {
        _result = await StartingWith(Offering()).StartTenant(Tenant, Organization);
        var told = await Told();
        _count = told.Count;
        _told = (TenantStartedOnOffer)told[0].Content;
        _source = told[0].Context.EventSourceId.Value;
        var entitled = await Entitled();
        _entitledCount = entitled.Count;
        _entitled = (TenantStartedOnDefaultOffering)entitled[0].Content;
    }

    [Fact] void should_start_the_tenant() => _result.ShouldEqual(TenantStart.Started);
    [Fact] void should_tell_the_operator_once() => _count.ShouldEqual(1);
    [Fact] void should_tell_it_under_the_organization() => _source.ShouldEqual(Organization.Value.ToString());
    [Fact] void should_name_the_tenant() => _told.TenantId.ShouldEqual(Tenant);
    [Fact] void should_name_the_offer() => _told.Offer.Value.ShouldEqual("Starter");
    [Fact] void should_give_the_seats_of_the_offer() => _told.Seats.Value.ShouldEqual(3);
    [Fact] void should_give_the_modules_of_the_offer() => _told.Modules.Select(_ => _.Value).ShouldContainOnly("core");
    [Fact] void should_entitle_the_tenant_once() => _entitledCount.ShouldEqual(1);
    [Fact] void should_entitle_it_with_the_seats_of_the_offer() => _entitled.Seats.Value.ShouldEqual(3);
    [Fact] void should_entitle_it_with_the_modules_of_the_offer() => _entitled.Modules.Select(_ => _.Value).ShouldContainOnly("core");
}
#endif
