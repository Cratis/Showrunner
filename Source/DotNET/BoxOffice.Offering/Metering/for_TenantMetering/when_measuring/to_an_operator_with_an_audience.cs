// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class to_an_operator_with_an_audience : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        var closes = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        await StartTheProduct("direct").Measure(_eventStore, _tenant, closes);
        await SessionEnded(_noon.AddHours(10), 20);
        await StartTheProduct("direct").Measure(_eventStore, _tenant, closes.AddMinutes(5));
        await ReadReported();
    }

    [Fact] void should_have_reported_the_closed_day_and_the_correction() => _reported.Count.ShouldEqual(2);
    [Fact] void should_address_the_closed_day_to_the_operator() => _reported[0].Context.NamedTags.ShouldContain(new NamedTag(WellKnownTags.Audience, "direct"));
    [Fact] void should_address_the_correction_to_the_operator() => _reported[1].Context.NamedTags.ShouldContain(new NamedTag(WellKnownTags.Audience, "direct"));
}
#endif
