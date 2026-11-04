// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class with_a_blank_audience : Specification
{
    Exception _product;
    Exception _operator;

    void Because()
    {
        _product = Refused(options => options.ProductAudience = " ");
        _operator = Refused(options => options.OperatorAudience = "");
    }

    [Fact] void should_refuse_a_blank_product_audience() => _product.ShouldBeOfExactType<OptionsValidationException>();
    [Fact] void should_refuse_a_blank_operator_audience() => _operator.ShouldBeOfExactType<OptionsValidationException>();

    static Exception Refused(Action<BoxOfficeOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddBoxOffice(configure);
        using var provider = services.BuildServiceProvider();
        try
        {
            _ = provider.GetRequiredService<IOptions<BoxOfficeOptions>>().Value;
            return null!;
        }
        catch (OptionsValidationException exception)
        {
            return exception;
        }
    }
}
#endif
