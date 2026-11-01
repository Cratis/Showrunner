// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnContext;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen;
using Cratis.Showrunner.Notes.Giving.GiveNoteOnSlices;
using Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange;
using Cratis.Showrunner.Notes.Tenancy;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Receiving;

/// <summary>
/// Records what the receiving side did with a note: turns a public event it sent into the matching private
/// <c>Receiver…</c> fact and appends it to the event log of the tenant namespace that the event's <c>TenantId</c> names,
/// under the same note id.
/// </summary>
/// <remarks>
/// <para>
/// The receiving side is trusted to name the tenant - it is one of the configured collecting stores - but not to name a
/// note that exists. An event for a namespace the application does not have, or for a note that was never given in it, is
/// logged and skipped: it changes nothing and failing on it would stop the receiving side's later events behind it.
/// </para>
/// <para>
/// The private fact keeps the time the receiving side recorded it as its <c>Occurred</c>, and its correlation id.
/// </para>
/// <para>
/// Chronicle's forwarder delivers an event at least once, so the same event can arrive in the inbox twice, and the
/// translator can be handed the same inbox event again after a failure. The private fact is appended with an idempotency
/// key derived from the event itself - the collecting store, the note, the event type, when it occurred, its correlation
/// id and its content - so every copy of the same event yields one private fact.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log of the tenant namespace.</param>
/// <param name="logger">The logger.</param>
public class ReceiverEventTranslator(IChronicleClient client, ILogger<ReceiverEventTranslator> logger)
{
    static readonly Type[] _noteOpeners =
    [
        typeof(GiverGaveNoteOnContext),
        typeof(GiverGaveNoteOnSlices),
        typeof(GiverGaveNoteOnScreen),
        typeof(GiverGaveNoteOnComponent),
        typeof(GiverSuggestedComponentPropertyChange)
    ];

    /// <summary>
    /// Derives the idempotency key of an event from the receiving side: the same for every copy of the event the
    /// forwarder delivers.
    /// </summary>
    /// <param name="receiverEvent">The public event the receiving side sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="collectingStore">The collecting store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object receiverEvent, EventContext context, EventStoreName collectingStore)
    {
        var identity = string.Join(
            '\n',
            collectingStore.Value,
            context.EventSourceId.Value,
            receiverEvent.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(receiverEvent, receiverEvent.GetType()));
        return new($"notes-receiver-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Translates and records one event from the receiving side.
    /// </summary>
    /// <param name="receiverEvent">The public event the receiving side sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="collectingStore">The collecting store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="ReceiverFactNotRecorded">The event log refused the private fact.</exception>
    public async Task Translate(object receiverEvent, EventContext context, EventStoreName collectingStore)
    {
        if (PrivateEvents.For(receiverEvent) is not { } translated)
        {
            logger.NotAnEventFromTheReceiver(receiverEvent.GetType().Name, collectingStore);
            return;
        }

        var (tenantId, privateEvent) = translated;

        var eventType = receiverEvent.GetType().Name;
        if (string.IsNullOrWhiteSpace(tenantId.Value))
        {
            logger.NoTenant(eventType, context.EventSourceId, collectingStore);
            return;
        }

        var inbox = await client.GetEventStore(context.EventStore, context.Namespace);
        var namespaces = await inbox.GetNamespaces();
        var found = TenantNamespaces.Find(namespaces, tenantId);
        if (found.Match == TenantNamespaceMatch.Ambiguous)
        {
            logger.AmbiguousTenant(eventType, context.EventSourceId, tenantId, collectingStore);
            return;
        }

        if (found.Namespace is not { } @namespace)
        {
            logger.UnknownTenant(eventType, context.EventSourceId, tenantId, collectingStore);
            return;
        }

        var tenant = await client.GetEventStore(context.EventStore, @namespace);
        var openers = _noteOpeners.Select(tenant.EventTypes.GetEventTypeFor).ToArray();
        var opened = await tenant.EventLog.GetForEventSourceIdAndEventTypes(context.EventSourceId, openers);
        if (opened.Count == 0)
        {
            logger.UnknownNote(eventType, context.EventSourceId, tenantId, collectingStore);
            return;
        }

        var result = await tenant.EventLog.AppendOnce(
            context.EventSourceId,
            privateEvent,
            KeyFor(receiverEvent, context, collectingStore),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(eventType, context.EventSourceId, tenantId, collectingStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new ReceiverFactNotRecorded(privateEvent.GetType(), context.EventSourceId, @namespace, result);
        }
    }
}
