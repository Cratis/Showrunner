// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;
using Cratis.Showrunner.Notes.Withdrawing.WithdrawNote;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Publishing;

/// <summary>
/// Publishes what the giver did to the receiving side: each private fact becomes its public event in the outbox of the
/// namespace it happened in, under the same note id. The receiving side reads the outbox through its own subscription.
/// </summary>
/// <remarks>
/// <para>
/// The <c>TenantId</c> of every public event is the namespace the private fact was recorded in. The public event keeps
/// the private fact's correlation id, compliance subject and the time it happened.
/// </para>
/// <para>
/// Chronicle delivers an event at least once: a failed partition is recovered by delivering it again. Each publication
/// is appended with an idempotency key derived from the delivery, so a delivery that is repeated - recovered, retried
/// by another instance, or replayed - finds its publication already in the outbox and appends nothing. The reactor is
/// also <see cref="OnceOnlyAttribute">not replayable</see>, so a replay does not even try. A refused append throws, so
/// the partition fails and the publication is retried rather than lost.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the outbox of the namespace the fact was recorded in.</param>
/// <param name="logger">The logger.</param>
/// <param name="options">Notes' options; with the HTTP transport <see cref="NotesOptions.CollectorTenantId"/> names the tenant.</param>
[Reactor(Id)]
[OnceOnly]
public class OutboxPublisher(IChronicleClient client, ILogger<OutboxPublisher> logger, IOptions<NotesOptions> options) : IReactor
{
    /// <summary>
    /// The stable identity of the reactor; its position in the event log and its idempotency keys hang on it.
    /// </summary>
    public const string Id = "Cratis.Showrunner.Notes.Publishing.OutboxPublisher";

    /// <summary>
    /// Publishes a note about what was on screen as <c>NoteOnContextGiven</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task GaveNoteOnContext(GiverGaveNoteOnContext @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a note about slices as <c>NoteOnSlicesGiven</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task GaveNoteOnSlices(GiverGaveNoteOnSlices @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a note about the screen as <c>NoteOnScreenGiven</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task GaveNoteOnScreen(GiverGaveNoteOnScreen @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a note about a component as <c>NoteOnComponentGiven</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task GaveNoteOnComponent(GiverGaveNoteOnComponent @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a suggested property change as <c>ComponentPropertyChangeSuggested</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task SuggestedPropertyChange(GiverSuggestedComponentPropertyChange @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    /// <summary>
    /// Publishes a withdrawn note as <c>NoteWithdrawn</c>.
    /// </summary>
    /// <param name="event">The private event.</param>
    /// <param name="context">The context of the private event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task WithdrewNote(GiverWithdrewNote @event, EventContext context, ReactorDelivery delivery) =>
        Publish(PublicEvents.For(@event, TenantOf(context)), context, delivery);

    TenantId TenantOf(EventContext context) =>
        options.Value is { Transport: NotesTransport.Http, CollectorTenantId.Value.Length: > 0 } http ? http.CollectorTenantId : new(context.Namespace.Value);

    async Task Publish(object publicEvent, EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        using var audience = string.IsNullOrWhiteSpace(options.Value.CollectorAudience) ? null : EventTags.Audience(options.Value.CollectorAudience);
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            context.EventSourceId,
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Publish)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred,
            subject: context.Subject);

        if (result.HasIdempotencyConflicts)
        {
            // The delivery already published a different public event - an earlier version of Notes mapped the fact
            // differently. The fact is published; publishing it again would tell the receiving side twice.
            logger.AlreadyPublishedDifferently(publicEvent.GetType().Name, context.EventSourceId, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new PublicationRefused(publicEvent.GetType(), context.EventSourceId, context.Namespace, result);
        }
    }
}
