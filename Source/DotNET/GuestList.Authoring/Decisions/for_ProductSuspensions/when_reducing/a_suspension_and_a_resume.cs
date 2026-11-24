// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductSuspensions.when_reducing;

/// <summary>
/// A part is suspended for a tenant and for the system, and then resumed for the tenant: the resume ends the suspension of
/// the tenant only, the system's suspension holds, and the history shows every decision in the order it was taken.
/// </summary>
public class a_suspension_and_a_resume : a_product_that_reports<ProductSuspensions>
{
    async Task Because() => await Happened(
        Guests.TenantSuspension,
        Guests.SystemSuspension,
        new PartResumeDecided(SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    [Fact] void should_belong_to_the_product() => Built.Product.ShouldEqual(Guests.Product);
    [Fact] void should_still_suspend_the_part_for_the_system() => Built.IsSuspended(SuspensionLevel.System, TenantId.NotSet, UserId.NotSet, Guests.FleetReports).ShouldBeTrue();
    [Fact] void should_no_longer_suspend_the_part_for_the_tenant() => Built.IsSuspended(SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports).ShouldBeFalse();
    [Fact] void should_hold_one_suspension() => Built.Suspended.Count().ShouldEqual(1);
    [Fact] void should_show_every_decision_in_the_history() => Built.History.Select(_ => _.IsSuspended).ShouldEqual([true, true, false]);
}
#endif
