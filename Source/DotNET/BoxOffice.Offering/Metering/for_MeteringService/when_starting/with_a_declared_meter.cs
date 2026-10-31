// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringService.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringService.when_starting;

public class with_a_declared_meter : a_service_over_an_offering
{
    Exception? _result;

    void Because() => _result = Catch.Exception(() => ServiceFor(MeterFor("compute")));

    [Fact] void should_start_the_product() => _result.ShouldBeNull();
}
#endif
