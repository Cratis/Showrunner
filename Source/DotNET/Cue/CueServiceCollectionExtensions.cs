// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Cue;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Cue to an application's services.
/// </summary>
public static class CueServiceCollectionExtensions
{
    /// <summary>
    /// Adds Cue: the commands and queries that record which guided tours a person has started. They are discovered by Arc
    /// and the events by Chronicle, so the application also has Arc and Chronicle, which is where the history is kept.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Optionally configures <see cref="CueOptions"/>, such as the tours the application has.</param>
    /// <returns>The <see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddCue(this IServiceCollection services, Action<CueOptions>? configure = null)
    {
        var options = services.AddOptions<CueOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        // Whoever signs in is told apart by their subject claim, unless the application says otherwise.
        services.TryAddTransient<ISignedInViewer, SignedInViewer>();
        services.TryAddTransient<IViewerTours, ViewerTours>();
        return services;
    }
}
