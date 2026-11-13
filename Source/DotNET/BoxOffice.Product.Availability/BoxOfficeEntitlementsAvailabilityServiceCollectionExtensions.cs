// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Context.Providers;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Product.Availability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for letting an availability gate read Box Office entitlements.
/// </summary>
public static class BoxOfficeEntitlementsAvailabilityServiceCollectionExtensions
{
    /// <summary>
    /// Supplies what Box Office entitles the tenant to as the host-provided context value <c>entitlements</c>.
    /// </summary>
    /// <remarks>
    /// Call <c>AddBoxOfficeProduct</c> (or <c>AddBoxOffice</c>) as well. This registers <see cref="EntitlementsHostContextValues"/>
    /// as the <see cref="IHostContextValues"/> of the host unless the host registered its own; a host that supplies other
    /// values delegates <see cref="EntitlementsHostContextValues.Name"/> to <see cref="EntitlementsHostContextValues"/> from its own.
    /// The entitlements are read from the scope of the request being answered, through <see cref="IHttpContextAccessor"/>,
    /// so a request never sees another tenant's.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddBoxOfficeEntitlementsAvailability(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton(provider =>
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            return new EntitlementsHostContextValues(
                () => (accessor.HttpContext ?? throw new InvalidOperationException("Box Office entitlements are read for a request, and there is none."))
                    .RequestServices.GetRequiredService<IEntitlements>(),
                provider.GetService<ProductOffering>());
        });
        services.TryAddSingleton<IHostContextValues>(provider => provider.GetRequiredService<EntitlementsHostContextValues>());
        return services;
    }
}
