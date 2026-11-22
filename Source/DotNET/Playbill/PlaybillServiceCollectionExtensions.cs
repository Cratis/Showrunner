// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Playbill;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Playbill to an application's services.
/// </summary>
public static class PlaybillServiceCollectionExtensions
{
    /// <summary>
    /// Adds Playbill: the release history embedded in the application at build time, and the commands and queries that
    /// record which releases each person has seen. Arc discovers the commands and queries and Chronicle the events, so
    /// the application also has Arc and Chronicle, which is where what people have seen is kept. Serve the history with
    /// <c>MapPlaybill</c>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Optionally configures <see cref="PlaybillOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddPlaybill(this IServiceCollection services, Action<PlaybillOptions>? configure = null)
    {
        var options = services.AddOptions<PlaybillOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        // The history is read once and never changes while the application runs.
        services.TryAddSingleton<IReleaseNoteFiles>(provider =>
            new EmbeddedReleaseNoteFiles(provider.GetRequiredService<IOptions<PlaybillOptions>>().Value.ResolveAssemblies()));
        services.TryAddSingleton<IReleaseNotes, ReleaseNotes>();

        // Whoever signs in is told apart by their subject claim, unless the application says otherwise.
        services.TryAddTransient<ISignedInViewer, SignedInViewer>();
        services.TryAddTransient<ISeenReleases, SeenReleases>();
        return services;
    }
}
