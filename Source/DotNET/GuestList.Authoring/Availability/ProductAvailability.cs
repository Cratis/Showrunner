// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// What a product declares about its availability, as the product last reported it for each environment it reports from,
/// and how that changed over time. It is the operator's answer to "what can be granted, and since when".
/// </summary>
/// <remarks>
/// It is built from <see cref="AvailabilityDeclared"/>, which the product publishes once per change. The operator's host
/// subscribes to the product's published events and appends them to the event log it reads this from: the product never
/// has to be asked. Until a product has reported, it has no <see cref="Environments"/>, and nothing is claimed about it.
/// </remarks>
/// <param name="Product">The product.</param>
/// <param name="Environments">The last declaration of each environment, in the order of the environments' names.</param>
/// <param name="Changes">What each report changed, oldest first.</param>
[ReadModel]
public record ProductAvailability(Product Product, IEnumerable<EnvironmentAvailability> Environments, IEnumerable<ProductAvailabilityChange> Changes)
{
    /// <summary>
    /// Observes what a product declares.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/>.</param>
    /// <returns>An observable of what the product declares; it holds one item once the product has reported.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<ProductAvailability>> ForProduct(Product product, IReadModels readModels) =>
        readModels.Observe<ProductAvailability>(availability => availability.Product == product);

    /// <summary>
    /// Gets the audiences the product declares in an environment.
    /// </summary>
    /// <param name="primary">The environment that counts when the product reports from more than one.</param>
    /// <returns>The audiences, or <see langword="null"/> when the product has not reported from that environment.</returns>
    /// <remarks>
    /// A product that does not name its environment, and reports from no other, is taken as the primary one.
    /// </remarks>
    public IEnumerable<AudienceName>? DeclaredAudiences(EnvironmentName primary)
    {
        var environments = Environments.ToArray();
        var declaration = environments.FirstOrDefault(environment => environment.Environment == primary)
            ?? (environments.Length == 1 && environments[0].Environment == EnvironmentName.NotSet ? environments[0] : null);
        return declaration?.Audiences.Select(audience => audience.Name).ToArray();
    }

    /// <summary>
    /// Takes a report of the product's declaration as the last of its environment.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductAvailability Reduce(AvailabilityDeclared @event, ProductAvailability? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), [], []);
        var previous = state.Environments.FirstOrDefault(environment => environment.Environment == @event.Environment);
        var declared = new EnvironmentAvailability(
            @event.Environment,
            @event.Version,
            context.Occurred,
            @event.Origin,
            @event.EveryDeclaredAudienceGranted,
            Ordered(@event.Audiences, audience => audience.Name.Value),
            Ordered(@event.Availabilities, block => block.Name),
            Ordered(@event.Gates, gate => gate.Target),
            Ordered(@event.ContextValues, value => value.Name),
            Ordered(@event.Personas, persona => persona.Name));

        return state with
        {
            Environments = [.. state.Environments.Where(environment => environment.Environment != @event.Environment).Append(declared).OrderBy(environment => environment.Environment.Value, StringComparer.Ordinal)],
            Changes = [.. state.Changes, ProductAvailabilityChange.Between(previous, declared)]
        };
    }

    static IReadOnlyList<T> Ordered<T>(IEnumerable<T> items, Func<T, string> key) => [.. items.OrderBy(key, StringComparer.Ordinal)];
}
