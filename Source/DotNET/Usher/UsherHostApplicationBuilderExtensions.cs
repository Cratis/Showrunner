// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Usher to an application.
/// </summary>
public static class UsherHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Usher, issue reporting and support conversations, to the application.
    /// </summary>
    /// <remarks>
    /// Arc discovers Usher's commands, queries and read models, and Chronicle its events and reducers, from the
    /// <c>Cratis.Showrunner.Usher</c> assembly the application references: Cratis type discovery includes referenced
    /// packages whose names start with <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add Usher to.</param>
    /// <param name="configure">Configures Usher, at least <c>UsherOptions.ApplicationName</c>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddUsher<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Usher.UsherOptions> configure)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddUsher(configure);
        return builder;
    }
}
