// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringService.given;

/// <summary>
/// A product that declares a compute meter, and the means to start the metering of it.
/// </summary>
public class a_service_over_an_offering : Specification
{
    protected ProductOffering _offering;

    void Establish() => _offering = new ProductOfferingBuilder()
        .Product(ProductId.New())
        .Meter("compute", "Compute", "minutes")
        .Build();

    protected MeteringService ServiceFor(params IMeter[] meters) => new(
        Substitute.For<IChronicleClient>(),
        Options.Create(new ChronicleClientOptions()),
        _offering,
        meters,
        Substitute.For<IMeteringStore>(),
        new TenantMetering(
            Substitute.For<IMeteringStore>(),
            meters,
            Options.Create(new MeteringOptions()),
            new Cratis.Metrics.UnkeyedMeter<TenantMetering>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantMetering>.Instance),
        Options.Create(new MeteringOptions()),
        TimeProvider.System,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<MeteringService>.Instance);

    protected static IMeter MeterFor(MeterId id) => new EventMeter<SpecEvent>(id, (_, _) => null);
}
#endif
