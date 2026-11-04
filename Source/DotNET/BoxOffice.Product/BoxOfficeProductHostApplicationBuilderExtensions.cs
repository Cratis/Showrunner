// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Product;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding what a managed product does for its operator to a host application.
/// </summary>
public static class BoxOfficeProductHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Box Office and what a managed product does for its operator to the application; see
    /// <see cref="BoxOfficeProductServiceCollectionExtensions.AddBoxOfficeProduct"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add to.</param>
    /// <param name="configure">Configures the <see cref="BoxOfficeProductOptions"/>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddBoxOfficeProduct<TBuilder>(this TBuilder builder, Action<BoxOfficeProductOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddBoxOfficeProduct(configure);
        return builder;
    }
}
