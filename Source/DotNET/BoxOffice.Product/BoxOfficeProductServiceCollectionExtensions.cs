// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice;
using Cratis.Showrunner.BoxOffice.Product;
using Cratis.Showrunner.BoxOffice.Product.Access;
using Cratis.Showrunner.BoxOffice.Product.Activity;
using Cratis.Showrunner.BoxOffice.Product.Licensing;
using Cratis.Showrunner.BoxOffice.Product.Provisioning;
using Cratis.Showrunner.BoxOffice.Product.SignIns;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding what a managed product does for its operator to a service collection.
/// </summary>
public static class BoxOfficeProductServiceCollectionExtensions
{
    /// <summary>
    /// Adds Box Office and what a managed product does for its operator: it creates the tenants the operator registers and
    /// reports them ready, answers whether a person may take a seat and whether a tenant is read-only, and reports who was
    /// active.
    /// </summary>
    /// <remarks>
    /// This adds Box Office itself, so a product calls this instead of <c>AddBoxOffice</c>, and configures Box Office through
    /// <see cref="BoxOfficeProductOptions.ConfigureBoxOffice"/>. The product takes part in provisioning by registering its own
    /// <see cref="IProvisionTenants"/>.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures the <see cref="BoxOfficeProductOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddBoxOfficeProduct(this IServiceCollection services, Action<BoxOfficeProductOptions>? configure = null)
    {
        services
            .AddOptions<BoxOfficeProductOptions>()
            .Configure(options => configure?.Invoke(options))
            .Validate(options => options.SignInPath.StartsWith('/') && !options.SignInPath.StartsWith("//", StringComparison.Ordinal) && options.SignInPath.IndexOfAny(['?', '#']) < 0, "AuthProxy calls the product at a path: BoxOfficeProductOptions.SignInPath must start with a single '/' and carry no query or fragment.")
            .ValidateOnStart();

        services.AddBoxOffice();
        services
            .AddOptions<BoxOfficeOptions>()
            .Configure<IOptions<BoxOfficeProductOptions>>((boxOffice, product) =>
            {
                foreach (var configuration in product.Value.BoxOfficeConfigurations)
                {
                    configuration(boxOffice);
                }
            });

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ITenantStores, TenantStores>();
        services.TryAddSingleton<ISeatLicenses, SeatLicenses>();
        services.TryAddSingleton<ITenantAccess, TenantAccess>();
        services.TryAddSingleton<ActivityReporter>();
        services.TryAddSingleton<IReportActivity>(provider => provider.GetRequiredService<ActivityReporter>());
        services.TryAddSingleton<ProvisioningSubscriptions>();
        services.TryAddScoped<TenantRegistrations>();
        services.TryAddScoped<SignInCalls>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, SignInCallbackStartupFilter>());
        services.AddHostedService<BoxOfficeProductWorker>();
        return services;
    }
}
