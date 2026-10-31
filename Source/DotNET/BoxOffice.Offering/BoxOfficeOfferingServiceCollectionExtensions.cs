// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for declaring what a product sells.
/// </summary>
public static class BoxOfficeOfferingServiceCollectionExtensions
{
    /// <summary>
    /// Declares the product's offering and reports it to the operator when the product starts, unless the operator
    /// already has exactly this declaration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="declare">Declares the modules, meters and seat licensing.</param>
    /// <returns>The service collection for continuation.</returns>
    /// <exception cref="InvalidOffering">Thrown when the declaration cannot be sold, for example a module requires one the product does not declare. The product does not start.</exception>
    public static IServiceCollection AddBoxOfficeOffering(this IServiceCollection services, Action<ProductOfferingBuilder> declare)
    {
        var builder = new ProductOfferingBuilder();
        declare(builder);
        var offering = builder.Build();

        services.TryAddSingleton(offering);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<OfferingReporter>();
        services.AddOptions<OfferingOptions>();
        services.AddHostedService<OfferingReportingService>();
        return services;
    }
}
