// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding the collecting side of Notes to a host.
/// </summary>
public static class NoteCollectingHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the collecting side of Notes, which the builder of applications hosts: it reads the applications' notes into
    /// the namespace each names, and answers them.
    /// </summary>
    /// <remarks>
    /// Arc discovers the collecting commands, and Chronicle its events and reactors, from the
    /// <c>Cratis.Showrunner.Notes.Collecting</c> assembly the host references: Cratis type discovery includes referenced
    /// packages whose names start with <c>Cratis</c>. Call it next to <c>AddCratis</c>; the order does not matter.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of host application builder.</typeparam>
    /// <param name="builder">The builder to add the collecting side to.</param>
    /// <param name="configure">Configures the collecting side, typically <c>NoteCollectingOptions.AppStores</c>.</param>
    /// <returns>The builder for continuation.</returns>
    public static TBuilder AddNoteCollecting<TBuilder>(this TBuilder builder, Action<Cratis.Showrunner.Notes.Collecting.NoteCollectingOptions> configure)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddNoteCollecting(configure);
        return builder;
    }
}
