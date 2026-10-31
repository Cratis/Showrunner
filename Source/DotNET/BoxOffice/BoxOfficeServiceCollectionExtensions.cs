// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offers;
using Cratis.Showrunner.BoxOffice.Receiving;
using Cratis.Showrunner.BoxOffice.Tenancy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Box Office to the services of an application.
/// </summary>
public static class BoxOfficeServiceCollectionExtensions
{
    /// <summary>
    /// Adds Box Office, which honors what the operator of a product decides about a tenant, to the services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">Configures Box Office; the defaults suit a product whose operator is <c>Direct</c>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddBoxOffice(this IServiceCollection services, Action<BoxOfficeOptions>? configure = null)
    {
        services
            .AddOptions<BoxOfficeOptions>()
            .Configure(options => configure?.Invoke(options))
            .Validate(options => options.OperatorStores.Any(), "Box Office needs at least one operator store: set BoxOfficeOptions.OperatorStores in AddBoxOffice.")
            .Validate(options => options.PollingInterval > TimeSpan.Zero, "Box Office looks for new tenants at an interval: BoxOfficeOptions.PollingInterval must be more than zero.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<TenantEventStores>();
        services.TryAddSingleton<OperatorEventTranslator>();
        services.TryAddSingleton<OperatorSubscriptions>();
        services.TryAddSingleton<ProductOffers>();
        services.TryAddScoped<IEntitlements, TenantEntitlements>();
        services.AddHostedService<BoxOfficeWorker>();
        return services;
    }
}
