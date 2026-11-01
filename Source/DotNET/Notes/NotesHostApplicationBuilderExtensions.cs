// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding Notes to an application.
/// </summary>
public static class NotesHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Notes, product feedback from the users of the application, to the application.
    /// </summary>
    /// <remarks>
    /// Arc discovers Notes' commands, queries and read models, and Chronicle its events and reactors, from the
    /// <c>Cratis.Showrunner.Notes</c> assembly the application references: Cratis type discovery includes referenced
    /// packages whose names start with <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add Notes to.</param>
    /// <param name="configure">Configures Notes, at least <c>NotesOptions.ApplicationName</c> and <c>NotesOptions.PublicationId</c>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddNotes<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Notes.NotesOptions> configure)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddNotes(configure);
        return builder;
    }
}
