// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Receiving;

/// <summary>
/// Records what the desk did with an issue: turns a public event the desk sent into the matching private
/// <c>Support…</c> fact and appends it to the event log of the tenant namespace that the event's <c>TenantId</c> names,
/// under the same issue id.
/// </summary>
/// <remarks>
/// <para>
/// The desk is trusted to name the tenant - it is one of the configured desk stores - but not to name an issue that
/// exists. An event for a namespace the application does not have, or for an issue that was never reported in it, is
/// logged and skipped: it changes nothing and failing on it would stop the desk's later events behind it.
/// </para>
/// <para>
/// The private fact keeps the time the desk recorded it as its <c>Occurred</c>, and its correlation id.
/// </para>
/// <para>
/// Chronicle's forwarder delivers an event at least once, so the same desk event can arrive in the inbox twice, and
/// the translator can be handed the same inbox event again after a failure. The private fact is appended with an
/// idempotency key derived from the desk event itself - the desk store, the issue, the event type, when it occurred,
/// its correlation id and its content - so every copy of the same desk event yields one private fact.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log of the tenant namespace.</param>
/// <param name="logger">The logger.</param>
public class DeskEventTranslator(IChronicleClient client, ILogger<DeskEventTranslator> logger)
{
    static readonly Type[] _issueOpeners = [typeof(ReporterDescribedIssue), typeof(ReporterFlaggedError)];

    /// <summary>
    /// Derives the idempotency key of a desk event: the same for every copy of the event the forwarder delivers.
    /// </summary>
    /// <param name="deskEvent">The public event the desk sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="deskStore">The desk store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object deskEvent, EventContext context, EventStoreName deskStore)
    {
        var identity = string.Join(
            '\n',
            deskStore.Value,
            context.EventSourceId.Value,
            deskEvent.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(deskEvent, deskEvent.GetType()));
        return new($"usher-desk-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Translates and records one event from the desk.
    /// </summary>
    /// <param name="deskEvent">The public event the desk sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="deskStore">The desk store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="SupportFactNotRecorded">The event log refused the private fact.</exception>
    public async Task Translate(object deskEvent, EventContext context, EventStoreName deskStore)
    {
        if (PrivateEvents.For(deskEvent) is not { } translated)
        {
            logger.NotAnEventFromTheDesk(deskEvent.GetType().Name, deskStore);
            return;
        }

        var (tenantId, privateEvent) = translated;

        var eventType = deskEvent.GetType().Name;
        if (string.IsNullOrWhiteSpace(tenantId.Value))
        {
            logger.NoTenant(eventType, context.EventSourceId, deskStore);
            return;
        }

        var inbox = await client.GetEventStore(context.EventStore, context.Namespace);
        var namespaces = await inbox.GetNamespaces();
        var @namespace = namespaces.FirstOrDefault(_ => string.Equals(_.Value, tenantId.Value, StringComparison.OrdinalIgnoreCase));
        if (@namespace is null)
        {
            logger.UnknownTenant(eventType, context.EventSourceId, tenantId, deskStore);
            return;
        }

        var tenant = await client.GetEventStore(context.EventStore, @namespace);
        var openers = _issueOpeners.Select(tenant.EventTypes.GetEventTypeFor).ToArray();
        var opened = await tenant.EventLog.GetForEventSourceIdAndEventTypes(context.EventSourceId, openers);
        if (opened.Count == 0)
        {
            logger.UnknownIssue(eventType, context.EventSourceId, tenantId, deskStore);
            return;
        }

        var result = await tenant.EventLog.AppendOnce(
            context.EventSourceId,
            privateEvent,
            KeyFor(deskEvent, context, deskStore),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(eventType, context.EventSourceId, tenantId, deskStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new SupportFactNotRecorded(privateEvent.GetType(), context.EventSourceId, @namespace, result);
        }
    }
}
