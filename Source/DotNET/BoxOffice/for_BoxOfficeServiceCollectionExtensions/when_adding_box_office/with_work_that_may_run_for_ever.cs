// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class with_work_that_may_run_for_ever : Specification
{
    Exception _exception;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddBoxOffice(options => options.MaximumWorkDuration = TimeSpan.FromDays(30));
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
