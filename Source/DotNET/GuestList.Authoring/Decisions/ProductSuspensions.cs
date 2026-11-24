// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// The parts an operator has suspended for a product, and the history of every suspension and resume, as the operator's
/// screen shows them and as the commands that change them read them.
/// </summary>
/// <remarks>
/// It is a self-reducing read model over the stream of the product. Suspending a part that is suspended at that level and
/// resuming one that is not change nothing, so the commands do not decide twice and the outbox publishes a decision once.
/// </remarks>
/// <param name="Product">The product.</param>
/// <param name="Suspended">The parts suspended now, by level, tenant, user and part.</param>
/// <param name="History">Every suspension and resume, oldest first.</param>
[ReadModel]
public record ProductSuspensions(Product Product, IEnumerable<HeldSuspension> Suspended, IEnumerable<SuspensionHistoryEntry> History)
{
    /// <summary>
    /// Observes the suspensions of a product.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <returns>The suspensions of the product, again every time they change.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<ProductSuspensions>> ForProduct(Product product, IReadModels readModels) =>
        readModels.Observe<ProductSuspensions>(suspensions => suspensions.Product == product);

    /// <summary>
    /// Gets whether a part is suspended at a place.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="user">The user.</param>
    /// <param name="part">The part.</param>
    /// <returns><see langword="true"/> when it is; otherwise <see langword="false"/>.</returns>
    public bool IsSuspended(SuspensionLevel level, TenantId tenant, UserId user, PartPath part) =>
        Suspended.Any(held => held.IsAt(level, tenant, user, part));

    /// <summary>
    /// Suspends a part, unless it is suspended at that level.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductSuspensions Reduce(PartSuspensionDecided @event, ProductSuspensions? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), [], []);
        var history = state.History.Append(new SuspensionHistoryEntry(true, @event.Level, @event.TenantId, @event.UserId, @event.Part, context.Occurred));
        if (state.IsSuspended(@event.Level, @event.TenantId, @event.UserId, @event.Part))
        {
            return state with { History = [.. history] };
        }

        var held = new HeldSuspension(@event.Level, @event.TenantId, @event.UserId, @event.Part, context.Occurred);
        return state with { Suspended = Ordered([.. state.Suspended, held]), History = [.. history] };
    }

    /// <summary>
    /// Ends the suspension of a part, at its own level only.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static ProductSuspensions Reduce(PartResumeDecided @event, ProductSuspensions? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), [], []);
        var history = state.History.Append(new SuspensionHistoryEntry(false, @event.Level, @event.TenantId, @event.UserId, @event.Part, context.Occurred));
        return state with
        {
            Suspended = [.. state.Suspended.Where(held => !held.IsAt(@event.Level, @event.TenantId, @event.UserId, @event.Part))],
            History = [.. history]
        };
    }

    static IEnumerable<HeldSuspension> Ordered(IEnumerable<HeldSuspension> suspensions) =>
    [
        .. suspensions
            .OrderBy(held => held.Level)
            .ThenBy(held => held.TenantId.Value, StringComparer.Ordinal)
            .ThenBy(held => held.UserId.Value, StringComparer.Ordinal)
            .ThenBy(held => held.Part.Value, StringComparer.Ordinal)
    ];
}

/// <summary>
/// One suspension or resume an operator decided.
/// </summary>
/// <param name="IsSuspended"><see langword="true"/> when the part was suspended; <see langword="false"/> when it was resumed.</param>
/// <param name="Level">Whom the decision is about.</param>
/// <param name="TenantId">The tenant, for the tenant and user levels; empty otherwise.</param>
/// <param name="UserId">The user, for the user level; empty otherwise.</param>
/// <param name="Part">The path of the part.</param>
/// <param name="DecidedAt">When the operator decided it.</param>
public record SuspensionHistoryEntry(bool IsSuspended, SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part, DateTimeOffset DecidedAt);
