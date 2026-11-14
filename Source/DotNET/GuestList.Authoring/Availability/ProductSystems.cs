// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// The systems that call a product, such as integrations, MCP clients and API callers, as the product last reported them for
/// each environment it reports from. It is what an operator picks from to grant an audience to one system, see
/// <see cref="AudienceLevel.NamedSystem"/>.
/// </summary>
/// <remarks>
/// It is built from <see cref="SystemsDeclared"/>, which the product publishes once per change; each report replaces the
/// earlier list of its environment. Until a product has reported, it has no <see cref="Environments"/> and nothing is claimed
/// about it. A grant to a system the product no longer reports stays in the guest list: listing says who can be picked, it
/// does not decide who holds what.
/// </remarks>
/// <param name="Product">The product.</param>
/// <param name="Environments">The last list of each environment, in the order of the environments' names.</param>
[ReadModel]
public record ProductSystems(Product Product, IEnumerable<EnvironmentSystems> Environments)
{
    /// <summary>
    /// Observes the systems that call a product.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/>.</param>
    /// <returns>An observable of the systems the product reported; it holds one item once the product has reported.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<ProductSystems>> ForProduct(Product product, IReadModels readModels) =>
        readModels.Observe<ProductSystems>(systems => systems.Product == product);

    /// <summary>
    /// Takes a report of the systems as the last of its environment.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductSystems Reduce(SystemsDeclared @event, ProductSystems? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), []);
        var declared = new EnvironmentSystems(
            @event.Environment,
            context.Occurred,
            [.. @event.Systems.OrderBy(system => system.ClientId.Value, StringComparer.Ordinal)]);

        return state with
        {
            Environments = [.. state.Environments.Where(environment => environment.Environment != @event.Environment).Append(declared).OrderBy(environment => environment.Environment.Value, StringComparer.Ordinal)]
        };
    }
}
