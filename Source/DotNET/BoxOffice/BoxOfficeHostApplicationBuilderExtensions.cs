// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Box Office to a host application.
/// </summary>
public static class BoxOfficeHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Box Office, which honors what the operator of a product decides about a tenant, to the application.
    /// </summary>
    /// <remarks>
    /// Arc discovers Box Office's commands, queries and command filter, and Chronicle its events and reducers, from the
    /// <c>Cratis.Showrunner.BoxOffice</c> assembly the application references: Cratis type discovery includes referenced
    /// packages whose names start with <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter. The
    /// product defines the <c>BoxOffice.ManageSeats</c> and <c>BoxOffice.ManageCompute</c> authorization policies to say who may assign and release seats and who may set compute budgets and concurrency limits.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add Box Office to.</param>
    /// <param name="configure">Configures Box Office; the defaults suit a product whose operator is <c>Direct</c>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddBoxOffice<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.BoxOffice.BoxOfficeOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddBoxOffice(configure);
        return builder;
    }
}
