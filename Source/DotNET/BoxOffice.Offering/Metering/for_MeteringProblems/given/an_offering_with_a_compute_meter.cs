// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems.given;

/// <summary>
/// A product that declares a compute meter.
/// </summary>
public class an_offering_with_a_compute_meter : Specification
{
    protected ProductOffering _offering;

    void Establish() => _offering = new ProductOfferingBuilder()
        .Product(ProductId.New())
        .Meter("compute", "Compute", "minutes")
        .Build();

    protected static IMeter MeterFor(MeterId id) => new EventMeter<SpecEvent>(id, (_, _) => null);
}
#endif
