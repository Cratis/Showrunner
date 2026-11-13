// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_a_gate_reads_a_module_the_product_does_not_declare : a_tenant_with_entitlements
{
    AvailabilityResult _result;

    async Task Because() => _result = await Evaluate("entitlements.modules.unknown.usable");

    [Fact] void should_not_know_the_answer() => _result.Outcome.ShouldEqual(AvailabilityOutcome.Unknown);
    [Fact] void should_deny() => _result.IsAvailable.ShouldBeFalse();
}
#endif
