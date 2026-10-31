// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Marquee;
using Cratis.Showrunner.Marquee.Receiving;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Marquee's services to a service collection.
/// </summary>
public static class MarqueeServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services Marquee's commands and queries use, and validates the options when the host starts.
    /// </summary>
    /// <remarks>
    /// Marquee answers <see cref="AudienceKind.HoldersOfModule"/> and <see cref="AudienceKind.OnPlan"/> with the
    /// <see cref="IAudienceEntitlements"/> in the service collection. Add the one Box Office supplies before or after
    /// this call and it is used; without one, those audiences never match.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures Marquee, at least <see cref="MarqueeOptions.Product"/>.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddMarquee(this IServiceCollection services, Action<MarqueeOptions> configure)
    {
        services
            .AddOptions<MarqueeOptions>()
            .Configure(configure)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Product.Value), "Marquee needs the product it shows announcements for: set MarqueeOptions.Product in AddMarquee.")
            .Validate(options => options.AuthoringStores.Any(), "Marquee needs at least one authoring store: set MarqueeOptions.AuthoringStores in AddMarquee.")
            .Validate(options => options.PollingInterval > TimeSpan.Zero, "Marquee polls at an interval: set MarqueeOptions.PollingInterval to more than zero.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IAudienceEntitlements>(NoAudienceEntitlements.Instance);
        services.TryAddSingleton<MarqueeInbox>();
        services.TryAddSingleton<MarqueeSubscriptions>();
        services.TryAddSingleton<IStandingAnnouncements, ProductWideAnnouncements>();
        services.TryAddTransient<ISignedInPerson, SignedInPerson>();
        services.TryAddTransient<ICurrentTenant, CurrentTenant>();
        services.AddHostedService<MarqueeWorker>();
        return services;
    }
}
