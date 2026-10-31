// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class with_a_portal_without_a_key : Specification
{
    Exception _exception;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddBoxOffice(options => options.CustomerPortalUrl = new("https://portal.example.com"));
        using var provider = services.BuildServiceProvider();
        try
        {
            _ = provider.GetRequiredService<IOptions<BoxOfficeOptions>>().Value;
        }
        catch (OptionsValidationException exception)
        {
            _exception = exception;
        }
    }

    [Fact] void should_refuse_the_options() => _exception.ShouldBeOfExactType<OptionsValidationException>();
}
#endif
