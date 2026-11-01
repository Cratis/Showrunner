// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Cratis.Showrunner.Notes.Collecting.Transport;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding the collecting side of Notes to a service collection.
/// </summary>
public static class NoteCollectingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services the collecting side's commands use, and validates the options when the host starts.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures the collecting side, typically <see cref="NoteCollectingOptions.AppStores"/>.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddNoteCollecting(this IServiceCollection services, Action<NoteCollectingOptions> configure)
    {
        services
            .AddOptions<NoteCollectingOptions>()
            .Configure(configure)
            .Validate(options => options.PollingInterval > TimeSpan.Zero, "Note collecting polls at an interval: set NoteCollectingOptions.PollingInterval to more than zero.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IAppStores, ConfiguredAppStores>();
        services.TryAddSingleton<TenantEventStores>();
        services.TryAddSingleton<AppEventTranslator>();
        services.TryAddSingleton<AppSubscriptions>();
        services.AddHostedService<CollectingWorker>();
        services.TryAddSingleton<NoteCollector>();
        services.TryAddSingleton<NoteCollectingCalls>();
        return services;
    }
}
