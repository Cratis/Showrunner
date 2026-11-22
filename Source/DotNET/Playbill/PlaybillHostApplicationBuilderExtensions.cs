// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Playbill to an application.
/// </summary>
public static class PlaybillHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Playbill, the release notes an application shows its users, to the application.
    /// </summary>
    /// <remarks>
    /// Arc discovers Playbill's commands and queries, and Chronicle its events and reducers, from the
    /// <c>Cratis.Showrunner.Playbill</c> assembly the application references: Cratis type discovery includes referenced
    /// packages whose names start with <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter. Serve
    /// the history with <c>MapPlaybill</c>.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add Playbill to.</param>
    /// <param name="configure">Optionally configures <see cref="Cratis.Showrunner.Playbill.PlaybillOptions"/>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddPlaybill<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Playbill.PlaybillOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddPlaybill(configure);
        return builder;
    }
}
