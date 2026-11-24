// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// The parts of a product an operator can suspend, as the product last reported them for each environment: every module,
/// feature, slice, command, query and screen, so the operator picks the part to suspend from the list.
/// </summary>
/// <remarks>
/// It is a self-reducing read model over the stream of the product. The newest report of an environment replaces the one
/// before, so a part the product no longer has leaves the list. Until a product reports, it holds nothing about it.
/// </remarks>
/// <param name="Product">The product.</param>
/// <param name="Environments">What the product last reported in each environment.</param>
[ReadModel]
public record ProductParts(Product Product, IEnumerable<EnvironmentParts> Environments)
{
    /// <summary>
    /// Observes the parts of a product.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <returns>The parts of the product, again every time it reports.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<ProductParts>> ForProduct(Product product, IReadModels readModels) =>
        readModels.Observe<ProductParts>(parts => parts.Product == product);

    /// <summary>
    /// Holds the parts a product reported, in place of what it reported for that environment before.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductParts Reduce(PartsDeclared @event, ProductParts? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), []);
        var declared = new EnvironmentParts(
            @event.Environment,
            context.Occurred,
            [.. @event.Parts.OrderBy(part => part.Path.Value, StringComparer.Ordinal)]);

        return state with
        {
            Environments = [.. state.Environments.Where(environment => environment.Environment != @event.Environment).Append(declared).OrderBy(environment => environment.Environment.Value, StringComparer.Ordinal)]
        };
    }
}
