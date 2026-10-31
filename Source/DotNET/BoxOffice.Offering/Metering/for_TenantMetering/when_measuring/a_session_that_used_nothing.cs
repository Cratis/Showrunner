// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_session_that_used_nothing : a_tenant_being_measured
{
    async Task Because()
    {
        var metering = new TenantMetering(
            _store,
            [new EventMeter<SessionEnded>("compute", (_, _) => null)],
            Microsoft.Extensions.Options.Options.Create(new MeteringOptions()),
            new Cratis.Metrics.UnkeyedMeter<TenantMetering>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantMetering>.Instance);
        await SessionEnded(_noon, 30);
        await metering.Measure(_eventStore, _tenant, new(2026, 10, 31, 6, 0, 0, TimeSpan.Zero));
        await ReadReported();
    }

    [Fact] void should_report_nothing() => _reported.Count.ShouldEqual(0);
}
#endif
