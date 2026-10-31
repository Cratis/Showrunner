// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.when_building;

public class a_module_that_requires_a_module_the_product_has : an_offering_being_declared
{
    void Because()
    {
        _builder
            .Module("analytics", "Analytics", module => module.Requires("core").Owns("Acme.Analytics").TrialDays(14))
            .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns("Acme.Core"));
        Build();
    }

    [Fact] void should_accept_the_offering() => _invalid.ShouldBeNull();
    [Fact] void should_keep_both_modules() => _offering.Modules.Count.ShouldEqual(2);
    [Fact] void should_keep_what_the_module_requires() => _offering.Modules.Single(_ => _.Id.Value == "analytics").Requires.ShouldContainOnly((ModuleId)"core");
    [Fact] void should_keep_the_trial_days() => _offering.Modules.Single(_ => _.Id.Value == "analytics").DefaultTrialDays.ShouldEqual(14);
    [Fact] void should_make_a_module_paid_unless_told_otherwise() => _offering.Modules.Single(_ => _.Id.Value == "analytics").Availability.ShouldEqual(ModuleAvailability.Paid);
}
#endif
