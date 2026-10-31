// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Marquee to an application.
/// </summary>
public static class MarqueeHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Marquee, the announcements a product shows its tenants, to the application.
    /// </summary>
    /// <remarks>
    /// Arc discovers Marquee's commands and queries, and Chronicle its events and reducers, from the
    /// <c>Cratis.Showrunner.Marquee</c> assembly the application references: Cratis type discovery includes referenced
    /// packages whose names start with <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add Marquee to.</param>
    /// <param name="configure">Configures Marquee, at least <c>MarqueeOptions.Product</c>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddMarquee<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Marquee.MarqueeOptions> configure)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddMarquee(configure);
        return builder;
    }
}
