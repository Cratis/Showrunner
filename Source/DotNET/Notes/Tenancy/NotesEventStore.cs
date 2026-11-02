// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Tenancy;

/// <summary>
/// Resolves the name of the event store Notes records the application's facts in.
/// </summary>
/// <remarks>
/// <see cref="NotesOptions.EventStore"/> wins when it is set. Otherwise the name is the one of the application's own
/// <see cref="IEventStore"/>, which an <c>AddCratis</c> host registers without writing the name into
/// <see cref="ChronicleClientOptions"/>. A host that registers only the client falls back to
/// <see cref="ChronicleClientOptions.EventStore"/>.
/// </remarks>
/// <param name="scopes">The <see cref="IServiceScopeFactory"/> the application's <see cref="IEventStore"/> is resolved through.</param>
/// <param name="chronicle">The <see cref="ChronicleClientOptions"/>.</param>
/// <param name="options">The <see cref="NotesOptions"/>.</param>
public class NotesEventStore(
    IServiceScopeFactory scopes,
    IOptions<ChronicleClientOptions> chronicle,
    IOptions<NotesOptions> options)
{
    readonly Lazy<EventStoreName> _name = new(() => Resolve(scopes, chronicle.Value, options.Value));

    /// <summary>
    /// Gets the name of the event store Notes records the application's facts in.
    /// </summary>
    public EventStoreName Name => _name.Value;

    /// <summary>
    /// Gets a resolver that knows only the Chronicle client options, which is what the code written before the
    /// application's event store was resolved knew.
    /// </summary>
    /// <param name="chronicle">The <see cref="ChronicleClientOptions"/>.</param>
    /// <param name="options">The <see cref="NotesOptions"/>, when the code has them.</param>
    /// <returns>The resolver.</returns>
    public static NotesEventStore ForClientOptions(IOptions<ChronicleClientOptions> chronicle, IOptions<NotesOptions>? options = null) =>
        new(new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), chronicle, options ?? Options.Create(new NotesOptions()));

    static EventStoreName Resolve(IServiceScopeFactory scopes, ChronicleClientOptions chronicle, NotesOptions options)
    {
        if (options.EventStore != EventStoreName.NotSet)
        {
            return options.EventStore;
        }

        using var scope = scopes.CreateScope();
        var applicationStore = scope.ServiceProvider.GetService<IEventStore>();
        return applicationStore is null ? chronicle.EventStore : applicationStore.Name;
    }
}
