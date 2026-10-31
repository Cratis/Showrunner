// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.for_MarqueeServiceCollectionExtensions;

/// <summary>
/// Marquee refuses to start without the product it shows announcements for.
/// </summary>
public class when_adding_marquee_without_a_product : Specification
{
    Exception _exception;

    void Because()
    {
        var provider = new ServiceCollection().AddMarquee(_ => { }).BuildServiceProvider();
        _exception = Catch.Exception(() => _ = provider.GetRequiredService<IOptions<MarqueeOptions>>().Value);
    }

    [Fact] void should_fail_validation() => _exception.ShouldBeOfExactType<OptionsValidationException>();

    [Fact] void should_say_what_is_missing() => _exception.Message.ShouldContain("MarqueeOptions.Product");
}
#endif
