// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// The audiences an operator has granted for a product, as the operator's screen shows them and as the commands that change
/// them read them.
/// </summary>
/// <remarks>
/// It is a self-reducing read model over the stream of the product. Granting an audience that is held and withdrawing one
/// that is not held change nothing, so the commands do not decide twice and the outbox publishes a decision once.
/// </remarks>
/// <param name="Product">The product.</param>
/// <param name="Grants">The audiences held, by level, tenant, user and name.</param>
[ReadModel]
public record ProductGuestList(Product Product, IEnumerable<HeldAudience> Grants)
{
    /// <summary>
    /// Observes the audiences granted for a product.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <returns>The guest list of the product, again every time it changes.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<ProductGuestList>> ForProduct(Product product, IReadModels readModels) =>
        readModels.Observe<ProductGuestList>(list => list.Product == product);

    /// <summary>
    /// Gets whether an audience is held.
    /// </summary>
    /// <param name="audience">The audience, with the level, tenant and user it is held at.</param>
    /// <returns><see langword="true"/> when it is; otherwise <see langword="false"/>.</returns>
    public bool Holds(HeldAudience audience) => Grants.Any(grant => grant.IsSameAs(audience));

    /// <summary>
    /// Holds an audience, unless it is held.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductGuestList Reduce(AudienceGrantDecided @event, ProductGuestList? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), []);
        var held = new HeldAudience(@event.Level, @event.TenantId, @event.UserId, @event.Audience, @event.ClientId ?? SystemClientId.NotSet, @event.Kind);
        return state.Holds(held) ? state : state with { Grants = Ordered([.. state.Grants, held]) };
    }

    /// <summary>
    /// Stops holding an audience, at its own level only.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductGuestList Reduce(AudienceWithdrawalDecided @event, ProductGuestList? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), []);
        var held = new HeldAudience(@event.Level, @event.TenantId, @event.UserId, @event.Audience, @event.ClientId ?? SystemClientId.NotSet, @event.Kind);
        return state with { Grants = [.. state.Grants.Where(grant => !grant.IsSameAs(held))] };
    }

    static IEnumerable<HeldAudience> Ordered(IEnumerable<HeldAudience> grants) =>
    [
        .. grants
            .OrderBy(grant => grant.Level)
            .ThenBy(grant => grant.TenantId.Value, StringComparer.Ordinal)
            .ThenBy(grant => grant.UserId.Value, StringComparer.Ordinal)
            .ThenBy(grant => grant.ClientId?.Value ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(grant => grant.Audience.Value, StringComparer.Ordinal)
    ];
}
