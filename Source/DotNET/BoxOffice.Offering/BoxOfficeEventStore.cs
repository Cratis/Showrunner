// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering;

/// <summary>
/// Resolves the name of the event store Box Office reads and records in: the product's own.
/// </summary>
/// <remarks>
/// <see cref="OfferingOptions.EventStore"/> wins when it is set; <c>BoxOfficeOptions.EventStore</c> sets it. Otherwise the
/// name is the one of the application's own <see cref="IEventStore"/>, which an <c>AddCratis</c> host registers without
/// writing the name into <see cref="ChronicleClientOptions"/>. A host that registers only the client falls back to
/// <see cref="ChronicleClientOptions.EventStore"/>.
/// </remarks>
/// <param name="scopes">The <see cref="IServiceScopeFactory"/> the application's <see cref="IEventStore"/> is resolved through.</param>
/// <param name="chronicle">The <see cref="ChronicleClientOptions"/>.</param>
/// <param name="options">The <see cref="OfferingOptions"/>.</param>
public class BoxOfficeEventStore(
    IServiceScopeFactory scopes,
    IOptions<ChronicleClientOptions> chronicle,
    IOptions<OfferingOptions> options)
{
    readonly Lazy<EventStoreName> _name = new(() => Resolve(scopes, chronicle.Value, options.Value));

    /// <summary>
    /// Gets the name of the event store Box Office reads and records in.
    /// </summary>
    public EventStoreName Name => _name.Value;

    /// <summary>
    /// Gets a resolver that knows only the Chronicle client options, which is what the code written before the
    /// application's event store was resolved knew.
    /// </summary>
    /// <param name="chronicle">The <see cref="ChronicleClientOptions"/>.</param>
    /// <param name="options">The <see cref="OfferingOptions"/>, when the code has them.</param>
    /// <returns>The resolver.</returns>
    public static BoxOfficeEventStore ForClientOptions(IOptions<ChronicleClientOptions> chronicle, IOptions<OfferingOptions>? options = null) =>
        new(new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), chronicle, options ?? Options.Create(new OfferingOptions()));

    static EventStoreName Resolve(IServiceScopeFactory scopes, ChronicleClientOptions chronicle, OfferingOptions options)
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
