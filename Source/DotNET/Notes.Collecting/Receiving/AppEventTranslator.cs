// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting.Receiving;

/// <summary>
/// Records what a user told an application: turns a public event the application sent into the matching private
/// <c>…Received</c> fact and appends it to the event log of the namespace that the event's <c>TenantId</c> names, under the
/// same note id and with the same compliance subject.
/// </summary>
/// <remarks>
/// <para>
/// An application is trusted to name its tenant - it is one of the configured application stores - but not to name a note
/// that exists. An event for a namespace the collecting store does not have, or a withdrawal of a note that was never
/// received in it, is logged and skipped: it changes nothing and failing on it would stop the application's later events
/// behind it.
/// </para>
/// <para>
/// The private fact keeps the time the application recorded it as its <c>Occurred</c>, its correlation id and the
/// compliance subject of the giver, so erasing the giver erases what they wrote here too.
/// </para>
/// <para>
/// Chronicle's forwarder delivers an event at least once, so the same event can arrive in the inbox twice, and the
/// translator can be handed the same inbox event again after a failure. The private fact is appended with an idempotency
/// key derived from the event itself - the application store, the note, the event type, when it occurred, its correlation
/// id and its content - so every copy of the same event yields one private fact and a redelivered note is never recorded
/// twice.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log of the tenant namespace.</param>
/// <param name="logger">The logger.</param>
public class AppEventTranslator(IChronicleClient client, ILogger<AppEventTranslator> logger)
{
    /// <summary>
    /// Derives the idempotency key of an event from an application: the same for every copy of the event the forwarder
    /// delivers.
    /// </summary>
    /// <param name="applicationEvent">The public event the application sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="appStore">The application store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object applicationEvent, EventContext context, EventStoreName appStore)
    {
        var identity = string.Join(
            '\n',
            appStore.Value,
            context.EventSourceId.Value,
            applicationEvent.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(applicationEvent, applicationEvent.GetType()));
        return new($"notes-collecting-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Translates and records one event from an application.
    /// </summary>
    /// <param name="applicationEvent">The public event the application sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="appStore">The application store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="NoteFactNotRecorded">The event log refused the private fact.</exception>
    public async Task Translate(object applicationEvent, EventContext context, EventStoreName appStore)
    {
        if (PrivateEvents.For(applicationEvent, new AppStoreName(appStore.Value)) is not { } translated)
        {
            logger.NotAnEventFromAnApplication(applicationEvent.GetType().Name, appStore);
            return;
        }

        var (tenantId, privateEvent) = translated;

        var eventType = applicationEvent.GetType().Name;
        if (string.IsNullOrWhiteSpace(tenantId.Value))
        {
            logger.NoTenant(eventType, context.EventSourceId, appStore);
            return;
        }

        var inbox = await client.GetEventStore(context.EventStore, context.Namespace);
        var namespaces = await inbox.GetNamespaces();
        var found = TenantNamespaces.Find(namespaces, tenantId);
        if (found.Match == TenantNamespaceMatch.Ambiguous)
        {
            logger.AmbiguousTenant(eventType, context.EventSourceId, tenantId, appStore);
            return;
        }

        if (found.Namespace is not { } @namespace)
        {
            logger.UnknownTenant(eventType, context.EventSourceId, tenantId, appStore);
            return;
        }

        var tenant = await client.GetEventStore(context.EventStore, @namespace);
        await Record(
            tenant,
            @namespace,
            tenantId,
            privateEvent,
            context.EventSourceId,
            KeyFor(applicationEvent, context, appStore),
            context.CorrelationId,
            context.Occurred,
            context.Subject,
            appStore,
            eventType);
    }

    /// <summary>
    /// Appends the private fact of an event from an application to the event log of the tenant, once: the idempotency key
    /// decides what a repeat is. A withdrawal of a note that was never received in the tenant is logged and skipped.
    /// Every path an application's events take - the inbox of the same kernel and the collector's HTTP endpoint - records
    /// through it, so they record the same facts.
    /// </summary>
    /// <param name="tenant">The collecting store in the tenant's namespace.</param>
    /// <param name="namespace">The tenant's namespace.</param>
    /// <param name="tenantId">The tenant, as the event names it.</param>
    /// <param name="privateEvent">The private fact, from <see cref="PrivateEvents.For"/>.</param>
    /// <param name="noteId">The note.</param>
    /// <param name="key">What makes every copy of the event the same.</param>
    /// <param name="correlationId">The correlation the application recorded the event under.</param>
    /// <param name="occurred">When the application recorded the event.</param>
    /// <param name="subject">The compliance subject of the giver.</param>
    /// <param name="appStore">The application store the event came from.</param>
    /// <param name="eventType">The name of the public event, for the log.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="NoteFactNotRecorded">The event log refused the private fact.</exception>
    public async Task Record(
        IEventStore tenant,
        EventStoreNamespaceName @namespace,
        TenantId tenantId,
        object privateEvent,
        EventSourceId noteId,
        IdempotencyKey key,
        Cratis.Execution.CorrelationId correlationId,
        DateTimeOffset occurred,
        Subject subject,
        EventStoreName appStore,
        string eventType)
    {
        if (privateEvent is NoteWithdrawalReceived && !await HasReceivedNote(tenant, noteId))
        {
            logger.UnknownNote(eventType, noteId, tenantId, appStore);
            return;
        }

        var result = await tenant.EventLog.AppendOnce(
            noteId,
            privateEvent,
            key,
            correlationId: correlationId,
            occurred: occurred,
            subject: subject);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(eventType, noteId, tenantId, appStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new NoteFactNotRecorded(privateEvent.GetType(), noteId, @namespace, result);
        }
    }

    static async Task<bool> HasReceivedNote(IEventStore tenant, EventSourceId noteId)
    {
        Type[] received =
        [
            typeof(NoteOnContextReceived),
            typeof(NoteOnSlicesReceived),
            typeof(NoteOnScreenReceived),
            typeof(NoteOnComponentReceived),
            typeof(ComponentPropertyChangeReceived)
        ];
        var openers = received.Select(tenant.EventTypes.GetEventTypeFor).ToArray();
        var opened = await tenant.EventLog.GetForEventSourceIdAndEventTypes(noteId, openers);
        return opened.Count > 0;
    }
}
