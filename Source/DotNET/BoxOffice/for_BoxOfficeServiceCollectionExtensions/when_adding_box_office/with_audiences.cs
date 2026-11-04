// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class with_audiences : Specification
{
    BoxOfficeOptions _boxOffice;
    OfferingOptions _offering;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddBoxOffice(options =>
        {
            options.ProductAudience = "studio";
            options.OperatorAudience = "direct";
        });
        using var provider = services.BuildServiceProvider();
        _boxOffice = provider.GetRequiredService<IOptions<BoxOfficeOptions>>().Value;
        _offering = provider.GetRequiredService<IOptions<OfferingOptions>>().Value;
    }

    [Fact] void should_keep_the_audience_of_the_product() => _boxOffice.ProductAudience.ShouldEqual("studio");
    [Fact] void should_report_to_the_audience_of_the_operator() => _offering.OperatorAudience.ShouldEqual("direct");
}
#endif
