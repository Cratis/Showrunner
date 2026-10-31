// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Portal;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_EntitlementSummary;

public class a_summary_over_entitlements : some_entitlements
{
    protected ICustomerPortal _portal;
    protected ProductOffering _offering;

    void Establish()
    {
        _portal = Substitute.For<ICustomerPortal>();
        _offering = new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Module("core", "Core", module => module.Availability(ModuleAvailability.Included))
            .Module("analytics", "Analytics", module => module.Requires("core"))
            .Module("agents", "Agents")
            .Module("reports", "Reports")
            .Build();
    }

    protected Task<EntitlementSummary> Summarize() =>
        EntitlementSummary.CurrentEntitlements(_entitlements, _readModels, _offering, _portal, _clock);
}
#endif
