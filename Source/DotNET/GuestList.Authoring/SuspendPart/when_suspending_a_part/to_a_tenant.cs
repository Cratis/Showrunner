// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.SuspendPart.when_suspending_a_part;

/// <summary>
/// Suspending a part for a tenant records the decision with the tenant and the part.
/// </summary>
public class to_a_tenant : a_guest_list_command<SuspendPart>
{
    async Task Because() => _result = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_suspended() =>
        await _scenario.ShouldHaveAppendedEvent<SuspendPart, PartSuspensionDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == SuspensionLevel.Tenant && e.TenantId == Guests.Acme && e.Part == Guests.FleetReports);
}
#endif
