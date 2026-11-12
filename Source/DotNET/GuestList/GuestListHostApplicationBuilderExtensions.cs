// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding GuestList to an application.
/// </summary>
public static class GuestListHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds GuestList, the audiences a product holds for its tenants, users and the system, to the application.
    /// </summary>
    /// <remarks>
    /// Arc discovers GuestList's queries, and Chronicle its events and reducers, from the <c>Cratis.Showrunner.GuestList</c>
    /// assembly the application references: Cratis type discovery includes referenced packages whose names start with
    /// <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add GuestList to.</param>
    /// <param name="configure">Configures GuestList, at least <c>GuestListOptions.Product</c>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddGuestList<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.GuestList.GuestListOptions> configure)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddGuestList(configure);
        return builder;
    }
}
