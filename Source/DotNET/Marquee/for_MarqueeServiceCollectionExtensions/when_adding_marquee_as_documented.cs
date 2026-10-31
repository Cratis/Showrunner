// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.for_MarqueeServiceCollectionExtensions;

/// <summary>
/// The options the tutorial in Documentation/Marquee/add-announcements-to-a-product.md sets are accepted, and are what
/// the product ends up with.
/// </summary>
public class when_adding_marquee_as_documented : Specification
{
    MarqueeOptions _options;

    void Because()
    {
        var provider = new ServiceCollection()
            .AddMarquee(marquee =>
            {
                marquee.Product = "ledger";
                marquee.AuthoringStores = ["Direct"];
            })
            .BuildServiceProvider();
        _options = provider.GetRequiredService<IOptions<MarqueeOptions>>().Value;
    }

    [Fact] void should_be_the_product_given() => _options.Product.Value.ShouldEqual("ledger");

    [Fact] void should_subscribe_to_the_authoring_store_given() => _options.AuthoringStores.Select(_ => _.Value).ShouldContainOnly("Direct");
}
#endif
