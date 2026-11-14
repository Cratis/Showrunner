// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Cue to an application.
/// </summary>
public static class CueHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Cue to the application. See <see cref="CueServiceCollectionExtensions.AddCue"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder to add to.</param>
    /// <param name="configure">Optionally configures <see cref="Cratis.Showrunner.Cue.CueOptions"/>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddCue<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Cue.CueOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddCue(configure);
        return builder;
    }
}
