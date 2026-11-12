// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A grant for a tenant is recorded in the tenant's own namespace, in the tenant's stream, as the operator decided it.
/// </summary>
public class a_tenant_grant : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToTenant(Guests.Plus));
        await ReadRecorded();
    }

    [Fact] void should_record_one_fact_for_the_tenant() => _recordedForAcme.Count.ShouldEqual(1);
    [Fact] void should_record_a_grant() => _recordedForAcme.Single().Content.ShouldEqual(new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus));
    [Fact] void should_record_it_in_the_tenants_stream() => _recordedForAcme.Single().Context.EventSourceId.ShouldEqual(GuestListStreams.Tenant);
    [Fact] void should_keep_the_time_the_operator_decided() => _recordedForAcme.Single().Context.Occurred.ShouldEqual(Guests.At(1));
    [Fact] void should_record_nothing_in_the_products_own_namespace() => _recordedForTheProduct.ShouldBeEmpty();
}
#endif
