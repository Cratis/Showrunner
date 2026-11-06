// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class without_audiences : Specification
{
    BoxOfficeOptions _boxOffice;
    OfferingOptions _offering;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddBoxOffice();
        using var provider = services.BuildServiceProvider();
        _boxOffice = provider.GetRequiredService<IOptions<BoxOfficeOptions>>().Value;
        _offering = provider.GetRequiredService<IOptions<OfferingOptions>>().Value;
    }

    [Fact] void should_receive_everything_the_operators_publish() => _boxOffice.ProductAudience.ShouldBeNull();
    [Fact] void should_not_address_what_it_reports() => _offering.OperatorAudience.ShouldBeNull();
}
#endif
