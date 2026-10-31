// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_BoxOfficeOfferingServiceCollectionExtensions.when_adding_an_offering;

public class that_is_valid : Specification
{
    ServiceCollection _services;
    ServiceProvider _provider;

    void Because()
    {
        _services = [];
        _services.AddBoxOfficeOffering(offering => offering
            .Product(ProductId.New())
            .Module("core", "Core"));
        _provider = _services.BuildServiceProvider();
    }

    [Fact] void should_make_the_offering_available() => _provider.GetRequiredService<ProductOffering>().Modules.Count.ShouldEqual(1);
    [Fact] void should_report_it_when_the_product_starts() => _services.ShouldContain(_ => _.ImplementationType == typeof(OfferingReportingService));
}
#endif
