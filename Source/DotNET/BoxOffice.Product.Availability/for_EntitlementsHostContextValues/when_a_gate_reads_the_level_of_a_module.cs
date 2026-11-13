// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_a_gate_reads_the_level_of_a_module : a_tenant_with_entitlements
{
    AvailabilityResult _full;
    AvailabilityResult _notFull;
    AvailabilityResult _preview;
    AvailabilityResult _trial;
    AvailabilityResult _none;
    AvailabilityResult _included;

    async Task Because()
    {
        _full = await Evaluate("entitlements.modules.analytics.level", ComparisonOperator.Equal, "Full");
        _notFull = await Evaluate("entitlements.modules.reports.level", ComparisonOperator.Equal, "Full");
        _preview = await Evaluate("entitlements.modules.reports.level", ComparisonOperator.Equal, "Preview");
        _trial = await Evaluate("entitlements.modules.forecasting.level", ComparisonOperator.Equal, "Trial");
        _none = await Evaluate("entitlements.modules.planning.level", ComparisonOperator.Equal, "None");
        _included = await Evaluate("entitlements.modules.core.level", ComparisonOperator.Equal, "Full");
    }

    [Fact] void should_make_a_module_in_full_available() => _full.IsAvailable.ShouldBeTrue();
    [Fact] void should_make_a_preview_unavailable_for_full() => _notFull.IsAvailable.ShouldBeFalse();
    [Fact] void should_read_a_preview() => _preview.IsAvailable.ShouldBeTrue();
    [Fact] void should_read_a_trial() => _trial.IsAvailable.ShouldBeTrue();
    [Fact] void should_read_none_for_a_module_the_tenant_has_nothing_to_do_with() => _none.IsAvailable.ShouldBeTrue();
    [Fact] void should_read_an_included_module_as_full() => _included.IsAvailable.ShouldBeTrue();
    [Fact] void should_not_ask_what_an_included_module_gives() => _entitlements.DidNotReceive().AccessTo(new Cratis.Showrunner.BoxOffice.Contracts.Entitlements.ModuleId("core"));
}
#endif
