// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_a_gate_reads_whether_a_module_is_usable : a_tenant_with_entitlements
{
    AvailabilityResult _full;
    AvailabilityResult _trial;
    AvailabilityResult _preview;
    AvailabilityResult _none;

    async Task Because()
    {
        _full = await Evaluate("entitlements.modules.analytics.usable");
        _trial = await Evaluate("entitlements.modules.forecasting.usable");
        _preview = await Evaluate("entitlements.modules.reports.usable");
        _none = await Evaluate("entitlements.modules.planning.usable");
    }

    [Fact] void should_be_usable_in_full() => _full.IsAvailable.ShouldBeTrue();
    [Fact] void should_be_usable_on_a_trial() => _trial.IsAvailable.ShouldBeTrue();
    [Fact] void should_not_be_usable_as_a_preview() => _preview.Outcome.ShouldEqual(AvailabilityOutcome.Unavailable);
    [Fact] void should_not_be_usable_without_the_module() => _none.Outcome.ShouldEqual(AvailabilityOutcome.Unavailable);
}
#endif
