// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing;

/// <summary>
/// Publishes every audience decision to the products, as the public contract events, in the order it was taken.
/// </summary>
/// <remarks>
/// <para>
/// It is the one reactor for both facts, so a grant and a withdrawal of the same audience can not be handled in the wrong
/// order by two observers. It is not <c>[OnceOnly]</c>: each decision carries an idempotency key, so a redelivery or a
/// replay appends nothing a second time. The public event keeps the time the operator decided, which is what a product
/// orders by: the newest decision wins.
/// </para>
/// <para>
/// All decisions of a product are appended under one event source, the product, so they share one product-wide stream in the
/// outbox and arrive in the order they were taken. The tenant and the user are fields of the event.
/// </para>
/// <para>
/// Nothing here calls Direct, or anything else: the decisions are the commands' events, and the outbox is Chronicle's.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the outbox of the namespace of the event.</param>
/// <param name="logger">The logger.</param>
/// <param name="options">The <see cref="GuestListAuthoringOptions"/>; <see langword="null"/> addresses decisions to products.</param>
[Reactor(Id)]
public class GuestListOutbox(IChronicleClient client, ILogger<GuestListOutbox> logger, IOptions<GuestListAuthoringOptions>? options = null) : IReactor
{
    /// <summary>
    /// The stable identity of the reactor; its position in the event log and its idempotency keys hang on it.
    /// </summary>
    public const string Id = "Cratis.Showrunner.GuestList.Authoring.Publishing.GuestListOutbox";

    /// <summary>
    /// Reacts to an audience being granted.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Granted(AudienceGrantDecided @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(ProductOf(context), @event), context, delivery);

    /// <summary>
    /// Reacts to an audience being withdrawn.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Withdrawn(AudienceWithdrawalDecided @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(ProductOf(context), @event), context, delivery);

    static Product ProductOf(EventContext context) => new(context.EventSourceId.Value);

    async Task Publish(object publicEvent, EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        var product = context.EventSourceId.Value;
        using var routingAudience = options?.Value.AddressToProducts ?? true ? EventTags.Audience(product) : null;
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            context.EventSourceId,
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Publish)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.HasIdempotencyConflicts)
        {
            logger.AlreadyPublishedDifferently(publicEvent.GetType().Name, context.EventSourceId, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new PublicationRefused(publicEvent.GetType(), context.EventSourceId, context.Namespace, result);
        }
    }
}
