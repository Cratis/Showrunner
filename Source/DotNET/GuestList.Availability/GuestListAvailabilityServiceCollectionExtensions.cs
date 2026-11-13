// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability;
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList;
using Cratis.Showrunner.GuestList.Availability;
using Cratis.Showrunner.GuestList.Availability.Declaration;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for feeding the Choreography availability engine from GuestList.
/// </summary>
public static class GuestListAvailabilityServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Choreography availability engine and feeds it the audiences GuestList recorded: an
    /// <see cref="IAudienceSource"/> over the local read models, a watcher that tells open sessions when a grant changes,
    /// and the audience lag metric. Call <c>AddGuestList</c> as well; this adds nothing to a host that does not call it.
    /// </summary>
    /// <remarks>
    /// Registrations the host made before this call are kept: its own <see cref="IAudienceSource"/>,
    /// <see cref="IDeclaredAudiences"/> or <see cref="IUndeclaredAudienceObserver"/> win. Without its own
    /// <see cref="IDeclaredAudiences"/> the application declares what <see cref="GuestListOptions.DeclaredAudiences"/> names.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures the availability engine; see <see cref="AvailabilityOptions"/>.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddGuestListAvailability(this IServiceCollection services, Action<AvailabilityOptions>? configure = null)
    {
        services.AddMetrics();
        services.AddAvailability(configure);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<AudienceChangeFeed>();
        services.TryAddSingleton<IAudienceSource, GuestListAudienceSource>();
        services.TryAddSingleton<IDeclaredAudiences>(provider => DeclaredBy(provider.GetRequiredService<IOptions<GuestListOptions>>().Value));
        services.TryAddSingleton<IUndeclaredAudienceObserver, LoggingUndeclaredAudienceObserver>();
        services.TryAddSingleton<GuestListAvailabilityMetrics>();
        services.AddHostedService<AudienceChangeWatcher>();

        services.TryAddSingleton<IAvailabilityManifest>(new InMemoryAvailabilityManifest());
        services.AddOptions<GuestListOptions>().PostConfigure<AvailabilityOptions>((guestList, engine) =>
        {
            if (guestList.Environment == EnvironmentName.NotSet && !string.IsNullOrWhiteSpace(engine.EnvironmentName))
            {
                guestList.Environment = engine.EnvironmentName;
            }
        });
        services.TryAddSingleton<AvailabilityReporter>();
        services.AddHostedService<AvailabilityReportingService>();
        return services;
    }

    static DeclaredAudiences DeclaredBy(GuestListOptions options) =>
        new((options.DeclaredAudiences ?? []).Select(audience => new Cratis.Choreography.Availability.Audiences.AudienceName(audience.Value)));
}
