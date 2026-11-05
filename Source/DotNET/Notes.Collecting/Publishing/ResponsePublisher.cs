// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Publishing;

/// <summary>
/// Publishes what the builder did with a note back to the application: each private fact becomes its public event in the
/// outbox of the namespace it happened in, under the same note id. The application reads the outbox through its own
/// subscription.
/// </summary>
/// <remarks>
/// <para>
/// The <c>TenantId</c> of every public event is the namespace the private fact was recorded in, which is the namespace
/// the note arrived in, so the application routes the answer into the tenant the note was given in.
/// </para>
/// <para>
/// Chronicle delivers an event at least once: a failed partition is recovered by delivering it again. Each publication
/// is appended with an idempotency key derived from the delivery, so a delivery that is repeated finds its publication
/// already in the outbox and appends nothing. The reactor is also <see cref="OnceOnlyAttribute">not replayable</see>, so
/// a replay does not even try. A refused append throws, so the partition fails and the publication is retried rather than
/// lost.
/// </para>
/// <para>
/// A reply is addressed to the application that sent the note when that application named itself: the private fact the
/// translator recorded carries the application's <c>from</c> tag, and the public event is appended with it as its
/// audience, so the application's subscription for its own audience receives it. A note from an application that did not
/// name itself is answered unaddressed, as before audiences.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the outbox of the namespace the fact was recorded in.</param>
/// <param name="logger">The logger.</param>
[Reactor(Id)]
[OnceOnly]
public class ResponsePublisher(IChronicleClient client, ILogger<ResponsePublisher> logger) : IReactor
{
    /// <summary>
    /// The stable identity of the reactor; its position in the event log and its idempotency keys hang on it.
    /// </summary>
    public const string Id = "Cratis.Showrunner.Notes.Collecting.Publishing.ResponsePublisher";

    /// <summary>
    /// Publishes an acknowledged note as <c>NoteAcknowledged</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Acknowledged(OperatorAcknowledgedNote @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes an addressed note as <c>NoteAddressed</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Addressed(OperatorAddressedNote @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a declined note as <c>NoteDeclined</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Declined(OperatorDeclinedNote @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    static TenantId TenantOf(EventContext context) => new(context.Namespace.Value);

    async Task Publish(object publicEvent, EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        using var audience = ReplyAudience.Address(await ReplyAudience.Resolve(eventStore, context));
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            context.EventSourceId,
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Publish)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred,
            subject: context.Subject);

        if (result.HasIdempotencyConflicts)
        {
            // The delivery already published a different public event - an earlier version mapped the fact differently.
            // The fact is published; publishing it again would tell the application twice.
            logger.AlreadyPublishedDifferently(publicEvent.GetType().Name, context.EventSourceId, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new PublicationRefused(publicEvent.GetType(), context.EventSourceId, context.Namespace, result);
        }
    }
}
