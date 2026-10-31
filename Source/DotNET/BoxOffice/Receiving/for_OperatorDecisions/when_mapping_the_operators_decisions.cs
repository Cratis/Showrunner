// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorDecisions;

public class when_mapping_the_operators_decisions : Specification
{
    static readonly TenantId _tenant = Tenants.Tenant;

    (TenantId TenantId, object Fact)?[] _mapped;

    void Because() => _mapped =
    [
        OperatorDecisions.ForTenant(new TrialOpened(_tenant, new(Tenants.Day(14)), 3)),
        OperatorDecisions.ForTenant(new TrialEndMoved(_tenant, new(Tenants.Day(30)))),
        OperatorDecisions.ForTenant(new TrialClosed(_tenant, TrialClosure.Expired)),
        OperatorDecisions.ForTenant(new SeatCapacitySet(_tenant, 5, EntitlementEnd.Never, SeatCapacitySource.Grant)),
        OperatorDecisions.ForTenant(new ModuleGranted(_tenant, "analytics", EntitlementEnd.Never)),
        OperatorDecisions.ForTenant(new ModuleTrialGranted(_tenant, "analytics", new(Tenants.Day(7)))),
        OperatorDecisions.ForTenant(new ModulePreviewOpened(_tenant, "analytics", EntitlementEnd.Never)),
        OperatorDecisions.ForTenant(new ModuleRevoked(_tenant, "analytics")),
        OperatorDecisions.ForTenant(new AllowanceSet(_tenant, "compute", 600, AllowancePeriod.Month, OverageBehavior.Block)),
        OperatorDecisions.ForTenant(new TenantSuspended(_tenant, SuspensionCause.PaymentOverdue)),
        OperatorDecisions.ForTenant(new TenantReinstated(_tenant)),
    ];

    [Fact] void should_map_every_decision_about_a_tenant() => _mapped.ShouldEachConformTo(_ => _ is not null);
    [Fact] void should_name_the_tenant_each_time() => _mapped.ShouldEachConformTo(_ => _!.Value.TenantId == _tenant);
    [Fact] void should_map_each_decision_to_a_different_fact() => _mapped.Select(_ => _!.Value.Fact.GetType()).Distinct().Count().ShouldEqual(_mapped.Length);
    [Fact] void should_subscribe_to_each_decision_it_maps() => OperatorDecisions.ForTenants.Length.ShouldEqual(_mapped.Length);
    [Fact] void should_map_no_event_about_a_tenant_it_does_not_receive() => OperatorDecisions.ForTenant(new TenantProvisioned(_tenant)).ShouldBeNull();
    [Fact] void should_map_an_offer_as_a_decision_about_the_product() => OperatorDecisions.ForProduct(new OfferPublished("Team", [], 5, [])).ShouldBeOfExactType<OperatorPublishedOffer>();
    [Fact] void should_map_a_withdrawn_offer() => OperatorDecisions.ForProduct(new OfferWithdrawn()).ShouldBeOfExactType<OperatorWithdrewOffer>();
    [Fact] void should_not_treat_a_tenant_decision_as_one_about_the_product() => OperatorDecisions.ForProduct(new TenantReinstated(_tenant)).ShouldBeNull();
    [Fact] void should_subscribe_to_every_decision() => OperatorDecisions.All.Count().ShouldEqual(13);
}
#endif
