// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Tenancy;
using Cratis.Showrunner.Notes.Transport;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Notes' services to a service collection.
/// </summary>
public static class NotesServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services Notes' commands and queries use, and validates the options when the host starts.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures Notes, at least <see cref="NotesOptions.ApplicationName"/> and <see cref="NotesOptions.PublicationId"/>.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddNotes(this IServiceCollection services, Action<NotesOptions> configure)
    {
        services
            .AddOptions<NotesOptions>()
            .Configure(configure)
            .Validate(options => !options.Enabled || options.ApplicationName != ApplicationName.NotSet, "Notes needs an application name: set NotesOptions.ApplicationName in AddNotes.")
            .Validate(options => !options.Enabled || options.PublicationId != PublicationId.NotSet, "Notes needs the publication the application runs as: set NotesOptions.PublicationId in AddNotes.")
            .Validate(options => !options.Enabled || options.Transport != NotesTransport.SameKernel || options.CollectingStores.Any(), "Notes needs at least one collecting store: set NotesOptions.CollectingStores in AddNotes.")
            .Validate(options => options.PollingInterval > TimeSpan.Zero, "Notes polls at an interval: set NotesOptions.PollingInterval to more than zero.")
            .Validate(options => !NotesOptionsValidation.HttpProblems(options).Any(), "Notes' HTTP transport is not configured: check NotesOptions.CollectorUrl, CollectorToken or CollectorTokenFile, RelayInterval and RelayBatchSize.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddTransient<ISignedInGiver, SignedInGiver>();
        services.TryAddSingleton<TenantEventStores>();
        services.TryAddSingleton<ReceiverEventTranslator>();
        services.TryAddSingleton<ReceiverSubscriptions>();
        services.AddHostedService<NotesWorker>();

        services.TryAddSingleton<CollectorToken>();
        services.TryAddSingleton<ICollectorClient>(provider => new HttpCollectorClient(
            new HttpClient(),
            provider.GetRequiredService<CollectorToken>(),
            provider.GetRequiredService<IOptions<NotesOptions>>()));
        services.TryAddSingleton<OutboxRelay>();
        services.TryAddSingleton<ResponseApplier>();
        services.AddHostedService<CollectorRelayWorker>();
        return services;
    }
}
