// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Herald to an application.
/// </summary>
public static class HeraldHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Herald, the email an application sends, to the application.
    /// </summary>
    /// <remarks>
    /// Chronicle discovers Herald's events and read model from the <c>Cratis.Showrunner.Herald</c> assembly the application
    /// references: Cratis type discovery includes referenced packages whose names start with <c>Cratis</c>. Call it next to
    /// <c>AddCratis</c> or <c>AddCratisChronicle</c>; the order does not matter.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add Herald to.</param>
    /// <param name="configure">Configures Herald: the templates folder, the sender and the mail server at least.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddHerald<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Herald.HeraldOptions> configure)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHerald(configure);
        return builder;
    }
}
