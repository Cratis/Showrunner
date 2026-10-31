// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Metrics;
using Cratis.Showrunner.BoxOffice.Offering.Metering;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for measuring a product's compute.
/// </summary>
public static class BoxOfficeMeteringServiceCollectionExtensions
{
    /// <summary>
    /// Measures the product's meters per tenant and day while the product runs, and reports each day once it is closed.
    /// The totals are kept in the product's MongoDB database, which has to be registered as an <c>IMongoDatabase</c>.
    /// Needs <c>AddBoxOfficeOffering</c>, because only a declared meter can be measured.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Optionally changes when a day closes and how often tenants are measured.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddBoxOfficeMetering(this IServiceCollection services, Action<MeteringOptions>? configure = null)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(typeof(IMeter<>), typeof(UnkeyedMeter<>));
        services.TryAddSingleton<IMeteringStore, MongoMeteringStore>();
        services.TryAddSingleton<TenantMetering>();
        services.AddOptions<MeteringOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        if (!services.Any(_ => _.ImplementationType == typeof(MeteringService)))
        {
            services.AddHostedService<MeteringService>();
        }

        return services;
    }

    /// <summary>
    /// Registers a meter that measures a declared meter from the product's events.
    /// </summary>
    /// <typeparam name="TMeter">The type of meter.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddMeter<TMeter>(this IServiceCollection services)
        where TMeter : class, Cratis.Showrunner.BoxOffice.Offering.Metering.IMeter
    {
        services.AddSingleton<Cratis.Showrunner.BoxOffice.Offering.Metering.IMeter, TMeter>();
        return services;
    }

    /// <summary>
    /// Registers a meter that measures one kind of event of the product with a function.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to read.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="meter">The key of the declared meter.</param>
    /// <param name="measure">Measures an event; returns <see langword="null"/> when it used none of the meter.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddMeter<TEvent>(this IServiceCollection services, MeterId meter, Func<TEvent, EventContext, MeterReading?> measure)
        where TEvent : class
    {
        services.AddSingleton<Cratis.Showrunner.BoxOffice.Offering.Metering.IMeter>(new EventMeter<TEvent>(meter, measure));
        return services;
    }
}
